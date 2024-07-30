using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel : IFileBased
    {
        private string filePath;
        private bool isOpen;

        public ImportProgressChangedDelegate ImportProgressChanged { get; set; }

        string IFileBased.Path
        {
            get => filePath;
            set
            {
                if (filePath == value)
                {
                    return;
                }

                filePath = value;

                if (filePath == null)
                {
                    return;
                }

                if (filePath.StartsWith("$") && MduFilePath != null)
                {
                    OnSave();
                }
            }
        }

        IEnumerable<string> IFileBased.Paths
        {
            get
            {
                yield return ((IFileBased)this).Path;
            }
        }

        public bool IsFileCritical => true;

        bool IFileBased.IsOpen => isOpen;

        public bool CopyFromWorkingDirectory => false;

        void IFileBased.CreateNew(string path)
        {
            MduFilePath = GetMduPathFromDeltaShellPath(path);
            ExportTo(MduFilePath);
            filePath = path;
            isOpen = true;
        }

        void IFileBased.Close()
        {
            isOpen = false;
        }

        void IFileBased.Open(string path)
        {
            isOpen = true;
        }

        void IFileBased.CopyTo(string destinationPath)
        {
            string mduPath = GetMduPathFromDeltaShellPath(destinationPath);

            string dirName = Path.GetDirectoryName(mduPath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            RenameSubFilesIfApplicable();
            ExportTo(mduPath, false);
        }

        /// <summary>
        /// Relocate to reconnects the item to the given path. Does NOT perform copyTo.
        /// </summary>
        void IFileBased.SwitchTo(string newPath)
        {
            filePath = newPath;

            string expectedMduPath = GetMduPathFromDeltaShellPath(newPath);
            var mduFileInfo = new FileInfo(expectedMduPath);
            if (!mduFileInfo.Exists && mduFileInfo.Directory?.Parent != null)
            {
                // [D3DFMIQ-450] Backwards compatibility: Older Models may not have 'input' folder
                string legacyMduPath = Path.Combine(mduFileInfo.Directory.Parent.FullName, mduFileInfo.Name);

                if (File.Exists(legacyMduPath))
                {
                    OnSwitchTo(legacyMduPath);
                    return;
                }
            }

            OnSwitchTo(expectedMduPath);
        }

        void IFileBased.Delete()
        {
            //Currently no action, implementation will be based on decision of issue [FM1D2D-2112].
        }

        internal virtual bool ExportTo(string mduPath, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true)
        {
            string dirName = Path.GetDirectoryName(mduPath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            CopyRestartFile(dirName);

            if (switchTo)
            {
                RenameSubFilesIfApplicable();
            }

            if (writeExtForcings)
            {
                List<string> spatVarSedPropNames =
                    SedimentFractions.Where(sf => sf.CurrentSedimentType != null).SelectMany(
                        sf =>
                            sf.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>()
                              .Where(p => p.IsSpatiallyVarying)).Select(p => p.SpatiallyVaryingName).ToList();
                spatVarSedPropNames.AddRange(SedimentFractions.Where(sf => sf.CurrentFormulaType != null).SelectMany(
                                                 sf =>
                                                     sf.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>()
                                                       .Where(p => p.IsSpatiallyVarying)).Select(p => p.SpatiallyVaryingName).ToList());
                ModelDefinition.SelectSpatialOperations(DataItems, TracerDefinitions, spatVarSedPropNames);
                ModelDefinition.Bathymetry = Bathymetry;
            }

            if (!IsEditing)
            {
                InitializeAreaDataColumns();
            }

            SetOutputDirProperty();
            CacheFile.Export(mduPath);

            if (switchTo)
            {
                ReloadGrid();
                mduFile.Write(mduPath, ModelDefinition, Area, Network, RoughnessSections, ChannelFrictionDefinitions, ChannelInitialConditionDefinitions, BoundaryConditions1D, LateralSourcesData, allFixedWeirsAndCorrespondingProperties, switchTo, writeExtForcings, writeFeatures, DisableFlowNodeRenumbering, UseMorSed ? this : null);
            }
            else
            {
                string workNetFile = MduFileHelper.GetSubfilePath(mduPath, ModelDefinition.GetModelProperty(KnownProperties.NetFile));
                WriteNetFile(workNetFile, Grid, Network, NetworkDiscretization, Links, Name, BedLevelLocation,
                             BedLevelZValues);
                UnstructuredGrid newGrid = new UnstructuredGrid();
                UGridFileHelper.SetUnstructuredGrid(workNetFile, newGrid); //may throw...
                bathymetryNoDataValue = UGridFileHelper.GetZCoordinateNoDataValue(workNetFile, BedLevelLocation);

                mduFile.Write(mduPath, ModelDefinition, Area, Network, RoughnessSections, ChannelFrictionDefinitions, ChannelInitialConditionDefinitions, BoundaryConditions1D, LateralSourcesData, allFixedWeirsAndCorrespondingProperties, switchTo, writeExtForcings, writeFeatures, DisableFlowNodeRenumbering, UseMorSed ? this : null, workNetFilePath: workNetFile);
            }

            if (!IsEditing)
            {
                RestoreAreaDataColumns();
            }

            if (switchTo)
            {
                MduFilePath = mduPath;
                CacheFile.UpdatePathToMduLocation(mduPath);
                SaveOutput();
            }

            return true;
        }

        internal void ImportSpatialOperationsAfterLoading()
        {
            foreach (KeyValuePair<string, IList<ISpatialOperation>> spatialOperation in ModelDefinition.SpatialOperations)
            {
                string dataItemName = spatialOperation.Key;
                IList<ISpatialOperation> spatialOperationList = spatialOperation.Value;
                IDataItem dataItem = DataItems.FirstOrDefault(di => di.Name == dataItemName);

                // when only one operation is found and it has the same name as when you would generate it from saving,
                // it will not override the operations found in the database. Assuming that we are loading a dsproj file.
                // Goes wrong when you change the file name of the quantity and you only have one quantity.
                if (spatialOperationList.Count != 1 || !(spatialOperationList[0] is ImportSamplesOperation) ||
                    dataItem == null || dataItem.ValueConverter != null || !(dataItem.Value is UnstructuredGridCoverage))
                {
                    continue;
                }

                IEnumerable<double> valuesToSet;
                var coverage = (UnstructuredGridCoverage)dataItem.Value;
                if (spatialOperationList[0] is ImportRasterSamplesOperationImportData samplesOperation)
                {
                    List<IPointValue> rasterFile = RasterFile.ReadPointValues(samplesOperation.FilePath).ToList();

                    int componentValueCount = coverage.Arguments.Aggregate(0,
                                                                           (totaal, arguments) => totaal == 0 ? arguments.Values.Count : totaal * arguments.Values.Count);

                    valuesToSet = rasterFile.Count != componentValueCount
                                      ? new InterpolateOperation().InterpolateToGrid(rasterFile, coverage, coverage.Grid)
                                      : rasterFile.Select(p => p.Value);
                }
                else
                {
                    var importSamplesOperation = (ImportSamplesOperation)spatialOperationList[0];
                    List<IPointValue> xyzFile = XyzFile.Read(importSamplesOperation.FilePath).ToList();

                    int componentValueCount = coverage.Arguments.Aggregate(0,
                                                                           (totaal, arguments) => totaal == 0 ? arguments.Values.Count : totaal * arguments.Values.Count);

                    valuesToSet = xyzFile.Count != componentValueCount
                                      ? new InterpolateOperation().InterpolateToGrid(xyzFile, coverage, coverage.Grid)
                                      : xyzFile.Select(p => p.Value);
                }

                if (valuesToSet.Any())
                {
                    coverage.SetValues(valuesToSet);
                }
            }
        }

        private void ReadFromMdu(string mduFilePath, ImportProgressChangedDelegate progressChanged = null)
        {
            ImportProgressChanged = progressChanged;

            LoadStateFromMdu(mduFilePath);

            FireImportProgressChanged(Resources.WaterFlowFMModel_ReadFromMdu_Reading_spatial_operations);
            IEventedList<IDataItem> modelDataItems = AddSpatialDataItems();
            ImportSpatialOperationsAfterCreating(modelDataItems);

            FireImportProgressChanged(Resources.WaterFlowFMModel_ReadFromMdu_Reading_sewer_roughness);
            AddSewerRoughnessIfNecessary();
            LoadOutputStateFromMdu(mduFilePath);
        }

        [InvokeRequired]
        private void FireImportProgressChanged(string currentStepName)
        {
            ImportProgressChanged?.Invoke(currentStepName, currentStep++, TOTALSTEPS);
        }

        private void LoadStateFromMdu(string mduFilePath)
        {
            // in case we're reloading into an existing flow model instance..cleanup first
            syncers.ForEach(s => s.Dispose());
            syncers.Clear();

            TracerDefinitions.Clear();

            LoadModelFromMdu(mduFilePath);

            SynchronizeModelDefinitions();

            // import SedimentFractions (these are not part of the model definition, however they are needed for SourcesAndSinks and TracerDefinitions)
            string mduFileDir = Path.GetDirectoryName(mduFilePath);
            WaterFlowFMProperty sedimentFileProperty = ModelDefinition.Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SedFile));
            if (mduFileDir != null && sedimentFileProperty != null && UseMorSed && File.Exists(Path.Combine(mduFileDir, sedimentFileProperty.Value.ToString())))
            {
                SedimentFile.LoadSediments(SedFilePath, this);
            }

            bathymetryNoDataValue = UGridFileHelper.GetZCoordinateNoDataValue(NetFilePath, BedLevelLocation);

            FireImportProgressChanged(Resources.WaterFlowFMModel_LoadStateFromMdu_Renaming_sub_files);
            RenameSubFilesIfApplicable();

            FireImportProgressChanged(Resources.WaterFlowFMModel_LoadStateFromMdu_Initialize_input_spatial_data);
            InitializeUnstructuredGridCoverages();

            FireImportProgressChanged(string.Format(Resources.WaterFlowFMModel_LoadStateFromMdu_Reading_Coordinate_system_from__0_, Path.GetFileName(NetFilePath))); 
            CoordinateSystem = UGridFileHelper.ReadCoordinateSystem(NetFilePath);

            // read depth layer definition
            DepthLayerDefinition = ModelDefinition.Kmx == 0
                                       ? new DepthLayerDefinition(DepthLayerType.Single)
                                       : new DepthLayerDefinition(ModelDefinition.Kmx);

            // find all names for tracer definitions
            FireImportProgressChanged(Resources.WaterFlowFMModel_LoadStateFromMdu_Assemble_tracer_definitions);
            AssembleTracerDefinitions();

            FireImportProgressChanged(Resources.WaterFlowFMModel_LoadStateFromMdu_Assemble_spatially_varying_sediment_properties); 
            AssembleSpatiallyVaryingSedimentProperties();

            // now that tracers and sediment fractions are imported we can complete the source and sink function
            FireImportProgressChanged(Resources.WaterFlowFMModel_LoadStateFromMdu_Populate_source_and_sinks);
            SourcesAndSinks.ForEach(sourceAndSink =>
            {
                var componentSettings = new Dictionary<string, bool>()
                {
                    { SourceAndSink.SalinityVariableName, UseSalinity },
                    { SourceAndSink.TemperatureVariableName, UseTemperature },
                    { SourceAndSink.SecondaryFlowVariableName, UseSecondaryFlow }
                };

                sourceAndSink.SedimentFractionNames.ForEach(sfn => componentSettings.Add(sfn, UseMorSed));
                sourceAndSink.TracerNames.ForEach(tn => componentSettings.Add(tn, true));
                sourceAndSink.PopulateFunctionValuesFromAttributes(componentSettings);
            });
        }

        private void LoadModelFromMdu(string mduFilePath)
        {
            MduFilePath = mduFilePath;
            string mduFileDir = Path.GetDirectoryName(mduFilePath);
            Name = Path.GetFileNameWithoutExtension(mduFilePath);
            ModelDefinition = new WaterFlowFMModelDefinition(mduFileDir, Name);
            Grid = Grid ?? new UnstructuredGrid();

            // initialize model definition from mdu file if it exists
            if (File.Exists(mduFilePath))
            {
                isLoading = true;
                var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = ModelDefinition,
                    HydroArea = Area,
                    Grid = Grid,
                    HydroNetwork = Network,
                    Discretization = NetworkDiscretization,
                    Links1D2D = Links,
                    BoundaryConditions1D = BoundaryConditions1D,
                    LateralSourcesData = LateralSourcesData,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties,
                    AllBridgePillarsAndCorrespondingProperties = BridgePillarsDataModel,
                    RoughnessSections = RoughnessSections,
                    ChannelFrictionDefinitions = ChannelFrictionDefinitions, 
                    ChannelInitialConditionDefinitions = ChannelInitialConditionDefinitions
                };
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModel, (mduStepName) => FireImportProgressChanged(Resources.WaterFlowFMModel_LoadModelFromMdu_Reading_mdu_file + Environment.NewLine + mduStepName));
                isLoading = false;
                SyncModelTimesWithBase();
                CacheFile.UpdatePathToMduLocation(mduFilePath);
            }

            WaterFlowFMProperty netFileProperty = ModelDefinition.GetModelProperty(KnownProperties.NetFile);
            if (string.IsNullOrEmpty(netFileProperty.GetValueAsString()))
            {
                netFileProperty.Value = Name + NetFile.FullExtension;
            }

            FireImportProgressChanged(Resources.WaterFlowFMModel_LoadModelFromMdu_Loading_restart);

            // sync the heat flux model, because events are off during reading
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
        }

        private void OnSave()
        {
            const string postfixExplicitWorkingDirectory = "_output";

            string previousModelDir = null;
            string previousExplicitWorkingDirectory = null;
            if (MduFilePath != MduSavePath)
            {
                previousModelDir = RecursivelyGetModelDirectoryPathFromMduFile();
                previousExplicitWorkingDirectory = previousModelDir + postfixExplicitWorkingDirectory;
            }

            if (ExportTo(MduSavePath))
            {
                /*Make sure the ModelDirectory gets updated when saving*/
                ModelDefinition.ModelDirectory = RecursivelyGetModelDirectoryPathFromMduFile();
            }

            if (previousModelDir == null)
            {
                return;
            }

            FileUtils.DeleteIfExists(previousModelDir);
            FileUtils.DeleteIfExists(previousExplicitWorkingDirectory);
        }

        private void OnSwitchTo(string mduPath)
        {
            if (MduFilePath == null) // switch from nothing: load
            {
                OnLoad(mduPath);
            }
            else // else: switch from existing: only change path
            {
                MduFilePath = mduPath;

                if (MduFile == null)
                {
                    return;
                }

                mduFile.Path = mduPath;
                SwitchFileBasedItems();
            }
        }

        private void SwitchFileBasedItems()
        {
            foreach (IFileBased windField in WindFields.OfType<IFileBased>())
            {
                string newPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), Path.GetFileName(windField.Path));
                windField.SwitchTo(newPath);
            }

            foreach (IUnsupportedFileBasedExtForceFileItem notUsedExtForceFileItem in UnsupportedFileBasedExtForceFileItems)
            {
                string newPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), Path.GetFileName(notUsedExtForceFileItem.Path));
                notUsedExtForceFileItem.SwitchTo(newPath);
            }
        }

        private void OnLoad(string mduPath)
        {
            SuspendClearOutputOnInputChange = true;
            CreateDataItemsNotAvailableInPreviousVersion();
            LoadStateFromMdu(mduPath);

            SuspendClearOutputOnInputChange = false;
            UpdateDataItemsNotCreatedInPreviousVersion();
            
            UpdateSpatialDataAfterGridSet(grid, false, false, false);
            ImportSpatialOperationsAfterLoading();
            LoadOutputStateFromMdu(mduPath);
        }

        private void UpdateDataItemsNotCreatedInPreviousVersion()
        {
            var initialWaterQuantityNameType = (InitialConditionQuantity)(int)ModelDefinition
                                                                              .GetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D).Value;
            string waterQuantityName =
                initialWaterQuantityNameType == InitialConditionQuantity.WaterLevel
                    ? WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
                    : WaterFlowFMModelDefinition.InitialWaterDepthDataItemName;

            UpdateNewDataItem(waterQuantityName, InitialWaterLevel);
            UpdateNewDataItem(WaterFlowFMModelDefinition.InfiltrationDataItemName, Infiltration);
        }

        private void UpdateNewDataItem(string quantityName, UnstructuredGridCoverage coverage)
        {
            if (AllDataItems.Any(di => di.Name.Equals(quantityName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return;
            }

            AddOrRenameDataItem(coverage, quantityName);
            UpdateSpatialDataAfterGridSet(grid, false, false, false);
            ImportSpatialOperationsAfterCreating(new EventedList<IDataItem> { GetDataItemByValue(coverage) });
        }

        private void RestoreAreaDataColumns()
        {
            MduFile.CleanBridgePillarAttributes(Area.BridgePillars);
        }

        private void RenameSubFilesIfApplicable()
        {
            foreach (KeyValuePair<WaterFlowFMProperty, string> subFile in SubFiles)
            {
                WaterFlowFMProperty waterFlowFMProperty = subFile.Key;

                if (waterFlowFMProperty.GetValueAsString().Equals(subFile.Value))
                {
                    continue;
                }

                if (waterFlowFMProperty.Equals(ModelDefinition.GetModelProperty(KnownProperties.NetFile)))
                {
                    string oldPath = NetFilePath;
                    waterFlowFMProperty.SetValueFromString(subFile.Value);
                    string newPath = NetFilePath;

                    if (!File.Exists(oldPath) ||
                        string.Equals(Path.GetFullPath(oldPath), Path.GetFullPath(newPath),
                                      StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    File.Copy(oldPath, newPath, true);
                    File.Delete(oldPath);
                }
                else
                {
                    waterFlowFMProperty.SetValueFromString(subFile.Value);
                }
            }

            ModelDefinition.ModelName = Name;
        }
    }
}