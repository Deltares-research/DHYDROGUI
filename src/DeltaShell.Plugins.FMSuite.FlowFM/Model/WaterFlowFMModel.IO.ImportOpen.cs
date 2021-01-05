using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DelftIniReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        #region Import/Load

        private bool isLoading;
        private const int TotalImportSteps = 10;
        private ImportProgressChangedDelegate importProgressChanged;

        /// <summary>
        /// Loads data from the provided <paramref name="mduFilePath"/>.
        /// </summary>
        /// <param name="mduFilePath">The path to the mdu file.</param>
        public void LoadFromMdu(string mduFilePath)
        {
            ClearSyncers();
            TracerDefinitions.Clear();

            LoadInputStateFromMdu(mduFilePath);
            LoadOutputStateFromMdu(mduFilePath);

            ImportSpatialOperationsAfterLoading();

            InitializeSyncers();
        }

        private void LoadOutputStateFromMdu(string mduFilePath)
        {
            string existingOutputDirectory = RetrieveOutputDirectory(mduFilePath);
            ReconnectOutputFiles(existingOutputDirectory);
        }

        /// <summary>
        /// Imports data from the provided <paramref name="mduFilePath"/>.
        /// </summary>
        /// <param name="mduFilePath">The path to the mdu file.</param>
        /// <param name="clearWaqOutputDirProperty">Whether or not WAQ output directory property needs to be cleared (optional).</param>
        /// <param name="progressChanged">A handle for notifying progress changes (optional).</param>
        public void ImportFromMdu(string mduFilePath, bool clearWaqOutputDirProperty = false, ImportProgressChangedDelegate progressChanged = null)
        {
            importProgressChanged = progressChanged;

            ClearSyncers();
            TracerDefinitions.Clear();

            LoadInputStateFromMdu(mduFilePath);
            AddSpatialDataItems();
            ImportSpatialOperationsAfterCreating();

            if (clearWaqOutputDirProperty)
            {
                ClearWaqOutputDirProperty();
            }

            InitializeSyncers();

            importProgressChanged = null;
        }

        /// <summary>
        /// Gets the net file path from the mdu file at <paramref name="mduFilePath"/>.
        /// </summary>
        /// <param name="mduFilePath">The mdu file path.</param>
        /// <returns>
        /// The absolute path to the net file as specified in the <paramref name="mduFilePath"/>
        /// </returns>
        /// <remarks>
        /// Note this implementation is currently strictly required for the <see cref="LoadInputStateFromMdu"/>.
        /// In any other situation you should make use of the <see cref="NetFilePath"/> property.
        /// </remarks>
        private string GetNetFilePath(string mduFilePath)
        {
            if (!File.Exists(mduFilePath))
            {
                return null;
            }

            // We need to obtain the NetFile property value from, however because
            // we explicitly load the grid before the mdu, the NetFileProperty of
            // the model will be nul, thus we need to obtain it directly from the
            // mdu. Thus we read the mdu twice. This is undesired, and should be 
            // refactored as part of a revised data access layer.
            string mduFileDir = Path.GetDirectoryName(mduFilePath);

            using (var fileStream = new FileStream(mduFilePath, FileMode.Open, FileAccess.Read))
            {
                IList<DelftIniCategory> categories = new MduDelftIniReader().ReadDelftIniFile(fileStream, filePath);
                
                DelftIniCategory geometryCategory = 
                    categories.FirstOrDefault(x => x.Name.ToLowerInvariant() == "geometry");
                DelftIniProperty netFileProperty = 
                    geometryCategory?.Properties
                                    .FirstOrDefault(x => x.Name.ToLowerInvariant() == KnownProperties.NetFile);
                
                string netFileRelativePath = netFileProperty?.Value;
                return netFileRelativePath != null ? Path.Combine(mduFileDir, netFileRelativePath) : null;
            }
        }

        private void LoadInputStateFromMdu(string mduFilePath)
        {
            // The grid is read first to ensure events utilising the grid work correctly.
            string gridPath = GetNetFilePath(mduFilePath);

            FireImportProgressChanged("Reading grid", 1, TotalImportSteps);
            Grid = ReadGridFromNetFile(gridPath) ?? new UnstructuredGrid();

            LoadModelFromMdu(mduFilePath);

            SynchronizeModelDefinitions();

            // import SedimentFractions (these are not part of the model definition, however they are needed for SourcesAndSinks and TracerDefinitions)
            string mduFileDir = Path.GetDirectoryName(mduFilePath);
            WaterFlowFMProperty sedimentFileProperty =
                ModelDefinition.Properties.FirstOrDefault(
                    p => p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SedFile));
            if (mduFileDir != null && sedimentFileProperty != null && UseMorSed &&
                File.Exists(Path.Combine(mduFileDir, sedimentFileProperty.Value.ToString())))
            {
                SedimentFile.LoadSediments(SedFilePath, this);
            }

            UnstructuredGridFileHelper.DoIfUgrid(NetFilePath, uGridAdapter => { bathymetryNoDataValue = uGridAdapter.uGrid.ZCoordinateFillValue; });

            FireImportProgressChanged("Renaming sub files", 6, TotalImportSteps);
            RenameSubFilesIfApplicable();

            FireImportProgressChanged("Initialize input spatial data", 7, TotalImportSteps);
            InitializeUnstructuredGridCoverages();

            CoordinateSystem = UnstructuredGridFileHelper.GetCoordinateSystem(NetFilePath);

            // read depth layer definition
            DepthLayerDefinition = ModelDefinition.Kmx == 0
                                       ? new DepthLayerDefinition(DepthLayerType.Single)
                                       : new DepthLayerDefinition(ModelDefinition.Kmx);

            // find all names for tracer definitions
            AssembleTracerDefinitions();

            AssembleSpatiallyVaryingSedimentProperties();

            // now that tracers and sediment fractions are imported we can complete the source and sink function
            SourcesAndSinks.ForEach(sourceAndSink =>
            {
                var componentSettings = new Dictionary<string, bool>()
                {
                    {SourceAndSink.SalinityVariableName, UseSalinity},
                    {SourceAndSink.TemperatureVariableName, UseTemperature},
                    {SourceAndSink.SecondaryFlowVariableName, UseSecondaryFlow}
                };

                sourceAndSink.SedimentFractionNames.ForEach(sfn => componentSettings.Add(sfn, UseMorSed));
                sourceAndSink.TracerNames.ForEach(tn => componentSettings.Add(tn, true));
                sourceAndSink.PopulateFunctionValuesFromAttributes(componentSettings);
            });

            FireImportProgressChanged("Reading model output", 9, TotalImportSteps);

            LoadRestartFile(mduFilePath);
        }

        private string RetrieveOutputDirectory(string mduFilePath)
        {
            currentOutputDirectoryPath = PersistentOutputDirectoryPath;

            if (ModelDefinition.ContainsProperty(KnownProperties.OutputDir))
            {
                string mduOutputDir =
                    ModelDefinition.GetModelProperty(KnownProperties.OutputDir).GetValueAsString()?.Trim();

                if (!string.IsNullOrEmpty(mduOutputDir))
                {
                    // We currently assume all OutputDirectoryNames are relative.
                    string mduOutputDirPath = Path.Combine(Path.GetDirectoryName(mduFilePath), mduOutputDir);
                    if (Directory.Exists(mduOutputDirPath))
                    {
                        currentOutputDirectoryPath = mduOutputDirPath;
                    }
                }
            }

            string existingOutputDirectory = Directory.Exists(currentOutputDirectoryPath)
                                                 ? currentOutputDirectoryPath
                                                 : Path.GetDirectoryName(
                                                     mduFilePath); // backwards Compatibility (output next to mdu file)
            return existingOutputDirectory;
        }

        private void LoadRestartFile(string mduPath)
        {
            string restartFilePath = MduFileHelper.GetSubfilePath(
                mduPath, ModelDefinition.GetModelProperty(KnownProperties.RestartFile));

            if (string.IsNullOrEmpty(restartFilePath))
            {
                RestartInput = new RestartFile();
                return;
            }

            if (!File.Exists(restartFilePath))
            {
                Log.Warn($"Restart file not found: {restartFilePath}.");
                return;
            }

            RestartInput = new RestartFile(restartFilePath);
        }

        public void ImportSpatialOperationsAfterLoading()
        {
            foreach (KeyValuePair<string, IList<ISpatialOperation>> spatialOperation in ModelDefinition
                .SpatialOperations)
            {
                string dataItemName = spatialOperation.Key;
                IList<ISpatialOperation> spatialOperationList = spatialOperation.Value;
                IDataItem dataItem = DataItems.FirstOrDefault(di => di.Name == dataItemName);

                // when only one operation is found and it has the same name as when you would generate it from saving,
                // it will not override the operations found in the database. Assuming that we are loading a dsproj file.
                // Goes wrong when you change the file name of the quantity and you only have one quantity.
                if (spatialOperationList.Count != 1 || !(spatialOperationList[0] is ImportSamplesOperation) ||
                    dataItem == null || dataItem.ValueConverter != null ||
                    !(dataItem.Value is UnstructuredGridCoverage))
                {
                    continue;
                }

                var samplesOperation = (ImportSamplesOperation) spatialOperationList[0];
                var coverage = (UnstructuredGridCoverage) dataItem.Value;
                List<IPointValue> xyzFile = XyzFile.Read(samplesOperation.FilePath).ToList();

                int componentValueCount =
                    coverage.Arguments.Aggregate(
                        0,
                        (total, arguments) =>
                            total == 0 ? arguments.Values.Count : total * arguments.Values.Count);

                IEnumerable<double> valuesToSet = xyzFile.Count != componentValueCount
                                                      ? new InterpolateOperation().InterpolateToGrid(
                                                          xyzFile, coverage, coverage.Grid)
                                                      : xyzFile.Select(p => p.Value);

                if (valuesToSet.Any())
                {
                    coverage.SetValues(valuesToSet);
                }
            }
        }

        private void ImportSpatialOperationsAfterCreating()
        {
            FireImportProgressChanged("Reading spatial operations", 9, TotalImportSteps);

            foreach (KeyValuePair<string, IList<ISpatialOperation>> spatialOperation in ModelDefinition
                .SpatialOperations)
            {
                string dataItemName = spatialOperation.Key;
                IList<ISpatialOperation> spatialOperationList = spatialOperation.Value;
                IDataItem dataItem = DataItems.FirstOrDefault(di => di.Name == dataItemName);

                if (!spatialOperationList.Any())
                {
                    continue;
                }

                if (dataItem == null)
                {
                    Log.Error("No data item found with name " + dataItemName);
                    continue;
                }

                if (dataItem.ValueConverter == null)
                {
                    dataItem.ValueConverter =
                        SpatialOperationValueConverterFactory.Create(dataItem.Value, dataItem.ValueType);
                }

                var valueConverter = dataItem.ValueConverter as SpatialOperationSetValueConverter;
                if (valueConverter == null)
                {
                    continue;
                }

                valueConverter.SpatialOperationSet.Operations.Clear();

                foreach (ISpatialOperation operation in spatialOperationList)
                {
                    // samples should directly be applied to the coverage with an interpolate operation
                    var importSamplesSpatialOperation = operation as ImportSamplesSpatialOperation;
                    if (importSamplesSpatialOperation != null)
                    {
                        Tuple<ImportSamplesOperation, InterpolateOperation> operations =
                            importSamplesSpatialOperation.CreateOperations();
                        valueConverter.SpatialOperationSet.AddOperation(operations.Item1);
                        valueConverter.SpatialOperationSet.AddOperation(operations.Item2);
                    }
                    else
                    {
                        valueConverter.SpatialOperationSet.AddOperation(operation);
                    }
                }
            }
        }

        private void LoadModelFromMdu(string mduFilePath)
        {
            MduFilePath = mduFilePath;
            string mduFileDir = Path.GetDirectoryName(mduFilePath);
            Name = Path.GetFileNameWithoutExtension(mduFilePath);
            ModelDefinition = new WaterFlowFMModelDefinition(mduFileDir, Name);

            // intialize model definition from mdu file if it exists
            if (File.Exists(mduFilePath))
            {
                isLoading = true;
                MduFile.Read(mduFilePath, ModelDefinition, Area, fixedWeirProperties,
                             (name, current, total) =>
                                 FireImportProgressChanged("Reading mdu - " + name, current, total),
                             BridgePillarsDataModel);
                isLoading = false;
                SyncModelTimesWithBase();

                CacheFile.UpdatePathToMduLocation(mduFilePath);
            }

            WaterFlowFMProperty netFileProperty = ModelDefinition.GetModelProperty(KnownProperties.NetFile);
            if (string.IsNullOrEmpty(netFileProperty.GetValueAsString()))
            {
                netFileProperty.Value = Name + NetFile.FullExtension;
            }

            FireImportProgressChanged("Loading restart", 2, TotalImportSteps);

            // sync the heat flux model, because events are off during reading
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
        }

        internal void SyncModelTimesWithBase()
        {
            base.StartTime = StartTime;
            base.StopTime = StopTime;
            base.TimeStep = TimeStep;
        }

        private void FireImportProgressChanged(string currentStepName, int currentStep, int totalSteps)
        {
            if (importProgressChanged == null)
            {
                return;
            }

            importProgressChanged(currentStepName, currentStep, totalSteps);
        }

        private void AssembleTracerDefinitions()
        {
            foreach (IBoundaryCondition boundaryCondition in BoundaryConditions)
            {
                var flowCondition = boundaryCondition as FlowBoundaryCondition;
                if (flowCondition != null && flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    if (!TracerDefinitions.Contains(flowCondition.TracerName))
                    {
                        TracerDefinitions.Add(flowCondition.TracerName);
                    }

                    AddTracerToSourcesAndSink(flowCondition.TracerName);
                }
            }

            IEnumerable<string> sp = SedimentFractions
                                     .SelectMany(sf => sf.GetAllActiveSpatiallyVaryingPropertyNames()).Distinct();
            foreach (string quantity in ModelDefinition
                                        .SpatialOperations.Keys.Except(WaterFlowFMModelDefinition.SpatialDataItemNames)
                                        .Except(sp))
            {
                if (!TracerDefinitions.Contains(quantity))
                {
                    TracerDefinitions.Add(quantity);
                }
            }
        }

        private void AssembleSpatiallyVaryingSedimentProperties()
        {
            List<ISpatiallyVaryingSedimentProperty> spatiallyVaryingSedimentProperties = SedimentFractions
                                                                                         .SelectMany(
                                                                                             f => f.CurrentSedimentType
                                                                                                   .Properties
                                                                                                   .OfType<
                                                                                                       ISpatiallyVaryingSedimentProperty
                                                                                                   >().Where(
                                                                                                       sp => sp
                                                                                                           .IsSpatiallyVarying))
                                                                                         .ToList();
            spatiallyVaryingSedimentProperties.AddRange(SedimentFractions
                                                        .Where(f => f.CurrentFormulaType != null)
                                                        .SelectMany(
                                                            f => f.CurrentFormulaType.Properties
                                                                  .OfType<ISpatiallyVaryingSedimentProperty>()
                                                                  .Where(sp => sp.IsSpatiallyVarying)));
            foreach (ISpatiallyVaryingSedimentProperty spatiallyVaryingSedimentProperty in
                spatiallyVaryingSedimentProperties)
            {
                AddToInitialFractions(spatiallyVaryingSedimentProperty.SpatiallyVaryingName);
            }
        }

        #endregion
    }
}