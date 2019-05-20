using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
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
        private readonly MduFile mduFile = new MduFile();

        public MduFile MduFile => mduFile;

        internal void SyncModelTimesWithBase()
        {
            base.StartTime = StartTime;
            base.StopTime = StopTime;
            base.TimeStep = TimeStep;
        }

        private void InitializeAreaDataColumns()
        {
            MduFile.SetBridgePillarAttributes(Area.BridgePillars, BridgePillarsDataModel);
        }

        private void RestoreAreaDataColumns()
        {
            MduFile.CleanBridgePillarAttributes(Area.BridgePillars);
        }

        #region Import/Load

        private bool isLoading;
        private const int TotalImportSteps = 10;

        /// <summary> Import the WaterFlowFMModel described by the specified mdu file path. </summary>
        /// <param name="mduFilePath"> The mdu file path. </param>
        /// <param name="progressChanged"> The progressChanged delegate provided by the importer. </param>
        /// <returns>
        /// A WaterFlowFMModel describing the mdu defined at the <paramref name="mduFilePath" />
        /// </returns>
        public static WaterFlowFMModel Import(string mduFilePath, ImportProgressChangedDelegate progressChanged)
        {
            var model = new WaterFlowFMModel(mduFilePath, progressChanged)
            {
                ImportProgressChanged = null,
            };
            model.ClearOutputDirAndWaqDirProperty();

            return model;
        }

        private void OnLoad(string mduPath)
        {
            LoadStateFromMdu(mduPath);
            ImportSpatialOperationsAfterLoading();
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
            WaterFlowFMProperty sedimentFileProperty =
                ModelDefinition.Properties.FirstOrDefault(
                    p => p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SedFile));
            if (mduFileDir != null && sedimentFileProperty != null && UseMorSed &&
                File.Exists(Path.Combine(mduFileDir, sedimentFileProperty.Value.ToString())))
            {
                SedimentFile.LoadSediments(SedFilePath, this);
            }

            FireImportProgressChanged(this, "Reading grid", 4, TotalImportSteps);
            var is1D2DModel = (bool) ModelDefinition.GetModelProperty(GuiProperties.PartOf1D2DModel).Value;
            Grid = ReadGridFromNetFile(NetFilePath, is1D2DModel) ?? new UnstructuredGrid();

            UnstructuredGridFileHelper.DoIfUgrid(NetFilePath,
                                                 uGridAdaptor =>
                                                 {
                                                     bathymetryNoDataValue = uGridAdaptor.uGrid.ZCoordinateFillValue;
                                                 });

            FireImportProgressChanged(this, "Renaming sub files", 5, TotalImportSteps);
            RenameSubFilesIfApplicable();

            FireImportProgressChanged(this, "Initialize input spatial data", 6, TotalImportSteps);
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

            FireImportProgressChanged(this, "Reading model output", 8, TotalImportSteps);

            LoadRestartFile(mduFilePath);

            currentOutputDirectoryPath = PersistentOutputDirectoryPath;

            if (ModelDefinition.ContainsProperty(KnownProperties.OutputDir))
            {
                string mduOutputDir =
                    ModelDefinition.GetModelProperty(KnownProperties.OutputDir).GetValueAsString()?.Trim();

                if (!string.IsNullOrEmpty(mduOutputDir))
                {
                    // We currently assume all OutputDirectoryNames are relative.
                    string mduOutputDirPath = Path.Combine(mduFileDir, mduOutputDir);
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

            ReconnectOutputFiles(existingOutputDirectory);
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
                List<IPointValue> xyzFile = new XyzFile().Read(samplesOperation.FilePath).ToList();

                int componentValueCount =
                    coverage.Arguments.Aggregate(
                        0,
                        (totaal, arguments) =>
                            totaal == 0 ? arguments.Values.Count : totaal * arguments.Values.Count);

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
                    var importSamplesSpatialOperationExtension = operation as ImportSamplesSpatialOperationExtension;
                    if (importSamplesSpatialOperationExtension != null)
                    {
                        DelftTools.Utils.Tuple<ImportSamplesOperation, InterpolateOperation> operations =
                            importSamplesSpatialOperationExtension.CreateOperations();
                        valueConverter.SpatialOperationSet.AddOperation(operations.First);
                        valueConverter.SpatialOperationSet.AddOperation(operations.Second);
                    }
                    else
                    {
                        valueConverter.SpatialOperationSet.AddOperation(operation);
                    }
                }
            }
        }

        public ImportProgressChangedDelegate ImportProgressChanged { get; set; }

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
                mduFile.Read(mduFilePath, ModelDefinition, Area, fixedWeirProperties,
                             (name, current, total) =>
                                 FireImportProgressChanged(this, "Reading mdu - " + name, current, total),
                             BridgePillarsDataModel);
                isLoading = false;
                SyncModelTimesWithBase();
            }

            WaterFlowFMProperty netFileProperty = ModelDefinition.GetModelProperty(KnownProperties.NetFile);
            if (string.IsNullOrEmpty(netFileProperty.GetValueAsString()))
            {
                netFileProperty.Value = Name + NetFile.FullExtension;
            }

            FireImportProgressChanged(this, "Loading restart", 2, TotalImportSteps);
            LoadRestartInfo(mduFilePath);

            // sync the heat flux model, because events are off during reading
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
        }

        private static void FireImportProgressChanged(WaterFlowFMModel model, string currentStepName, int currentStep,
                                                      int totalSteps)
        {
            if (model.ImportProgressChanged == null)
            {
                return;
            }

            model.ImportProgressChanged(currentStepName, currentStep, totalSteps);
        }

        private void AssembleTracerDefinitions()
        {
            foreach (IBoundaryCondition boundaryCondition in BoundaryConditions)
            {
                var flowCondition = boundaryCondition as FlowBoundaryCondition;
                if (flowCondition != null)
                {
                    if (flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                    {
                        if (!TracerDefinitions.Contains(flowCondition.TracerName))
                        {
                            TracerDefinitions.Add(flowCondition.TracerName);
                        }

                        AddTracerToSourcesAndSink(flowCondition.TracerName);
                    }
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
                AddToIntialFractions(spatiallyVaryingSedimentProperty.SpatiallyVaryingName);
            }
        }

        #endregion

        #region Export/Save

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

        public virtual bool ExportTo(string mduPath, bool switchTo = true, bool writeExtForcings = true,
                                     bool writeFeatures = true)
        {
            string dirName = Path.GetDirectoryName(mduPath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).SetValueAsString("1");

            // make sure on save / export, restart file + mdu are up to date and could be ran standalone with correct info
            if (switchTo)
            {
                SaveRestartInfo(mduPath);
            }

            InitializeRestart(dirName);

            if (switchTo)
            {
                RenameSubFilesIfApplicable();
            }

            if (writeExtForcings)
            {
                List<string> spatVarSedPropNames =
                    SedimentFractions.Where(sf => sf.CurrentSedimentType != null).SelectMany(
                                         sf =>
                                             sf.CurrentSedimentType.Properties
                                               .OfType<ISpatiallyVaryingSedimentProperty>()
                                               .Where(p => p.IsSpatiallyVarying)).Select(p => p.SpatiallyVaryingName)
                                     .ToList();
                spatVarSedPropNames.AddRange(SedimentFractions.Where(sf => sf.CurrentFormulaType != null).SelectMany(
                                                                  sf =>
                                                                      sf.CurrentFormulaType.Properties
                                                                        .OfType<ISpatiallyVaryingSedimentProperty>()
                                                                        .Where(p => p.IsSpatiallyVarying))
                                                              .Select(p => p.SpatiallyVaryingName).ToList());
                ModelDefinition.SelectSpatialOperations(DataItems, TracerDefinitions, spatVarSedPropNames);
                ModelDefinition.Bathymetry = Bathymetry;
            }

            InitializeAreaDataColumns();

            if (RunsInIntegratedModel)
            {
                SetOutputDirAndWaqDirProperty();
            }

            var mduFileWriteConfig = new MduFileWriteConfig()
            {
                WriteExtForcings = writeExtForcings,
                WriteFeatures = writeFeatures,
                DisableFlowNodeRenumbering = DisableFlowNodeRenumbering
            };
            mduFile.Write(mduPath,
                          ModelDefinition,
                          Area,
                          fixedWeirProperties.Values,
                          mduFileWriteConfig,
                          switchTo,
                          UseMorSed ? this : null);

            RestoreAreaDataColumns();

            if (switchTo)
            {
                MduFilePath = mduPath;
                SaveOutput();
            }

            return true;
        }

        #region Implementation of IDimrModel

        public virtual Type ExporterType => typeof(WaterFlowFMFileExporter);

        #endregion Implementation of IDimrModel

        #endregion Export

        #region Output

        public const string DiaFileDataItemTag = "DiaFile";

        /// <summary>
        /// Moves all content in the source directory into the target directory.
        /// </summary>
        /// <param name="sourceDirectory"> The source directory. </param>
        /// <param name="targetDirectoryPath"> The target directory path. </param>
        /// <remarks> <paramref name="sourceDirectory" /> should exist. </remarks>
        public virtual void ConnectOutput(string outputPath)
        {
            currentOutputDirectoryPath = outputPath;
            ReconnectOutputFiles(outputPath);
            ReadDiaFile(outputPath);
            ClearOutputDirAndWaqDirProperty();
        }

        /// <summary>
        /// Disconnects the output.
        /// </summary>
        public virtual void DisconnectOutput()
        {
            if (HasOpenFunctionStores)
            {
                BeginEdit(new DefaultEditAction("Disconnecting from output files"));

                if (OutputMapFileStore != null)
                {
                    OutputMapFileStore.Close();
                    OutputMapFileStore = null;
                }

                if (OutputHisFileStore != null)
                {
                    OutputHisFileStore.Close();
                    OutputHisFileStore = null;
                }

                if (OutputClassMapFileStore != null)
                {
                    OutputClassMapFileStore.Close();
                    OutputClassMapFileStore = null;
                }

                EndEdit();
            }

            OutputSnappedFeaturesPath = null;
        }

        protected virtual void ReconnectOutputFiles(string outputDirectory)
        {
            string mapFilePath = Path.Combine(outputDirectory, ModelDefinition.MapFileName);
            string hisFilePath = Path.Combine(outputDirectory, ModelDefinition.HisFileName);
            string classMapFilePath = Path.Combine(outputDirectory, ModelDefinition.ClassMapFileName);
            string waqFilePath = Path.Combine(outputDirectory, DelwaqOutputDirectoryName);
            string snappedFolderPath = Path.Combine(outputDirectory, SnappedFeaturesDirectoryName);

            ReconnectOutputFiles(mapFilePath, hisFilePath, classMapFilePath, waqFilePath, snappedFolderPath);
        }

        /// <summary>
        /// Saves the output by either moving or copying the source output to the target output directory.
        /// </summary>
        /// <remarks> When a file is locked, we report an error and return. </remarks>
        private void SaveOutput()
        {
            if (string.IsNullOrEmpty(currentOutputDirectoryPath))
            {
                return;
            }

            var sourceOutputDirectory = new DirectoryInfo(currentOutputDirectoryPath);
            if (!sourceOutputDirectory.Exists)
            {
                currentOutputDirectoryPath = PersistentOutputDirectoryPath;
                return;
            }

            var targetOutputDirectory = new DirectoryInfo(PersistentOutputDirectoryPath);
            string sourceOutputDirectoryPath = sourceOutputDirectory.FullName;
            string targetOutputDirectoryPath = targetOutputDirectory.FullName;

            bool sourceIsWorkingDir = sourceOutputDirectoryPath == WorkingOutputDirectoryPath;

            if (OutputIsEmpty && !HasOpenFunctionStores)
            {
                CleanDirectory(PersistentOutputDirectoryPath);

                if (sourceIsWorkingDir)
                {
                    CleanDirectory(WorkingDirectoryPath);
                }

                currentOutputDirectoryPath = PersistentOutputDirectoryPath;

                return;
            }

            if (sourceOutputDirectoryPath == targetOutputDirectoryPath)
            {
                return;
            }

            //copy all files and subdirectories from source directory "output" to persistent directory "output"
            if (!FileUtils.IsDirectoryEmpty(sourceOutputDirectoryPath))
            {
                FileUtils.CreateDirectoryIfNotExists(targetOutputDirectoryPath);

                if (sourceIsWorkingDir)
                {
                    List<string> lockedFiles = GetLockedFiles(WorkingDirectoryPath).ToList();

                    if (lockedFiles.Any())
                    {
                        ReportLockedFiles(lockedFiles);
                        return;
                    }

                    CleanDirectory(targetOutputDirectoryPath);
                    MoveAllContentDirectory(sourceOutputDirectory, targetOutputDirectoryPath);
                }
                else
                {
                    CleanDirectory(targetOutputDirectoryPath);
                    FileUtils.CopyAll(sourceOutputDirectory, targetOutputDirectory, string.Empty);
                }
            }

            string waqOutputDir = Path.Combine(PersistentOutputDirectoryPath, DelwaqOutputDirectoryName);
            string snappedOutputDir = Path.Combine(PersistentOutputDirectoryPath, SnappedFeaturesDirectoryName);
            ReconnectOutputFiles(MapFilePath, HisFilePath, ClassMapFilePath, waqOutputDir, snappedOutputDir, true);

            if (sourceIsWorkingDir)
            {
                CleanDirectory(WorkingDirectoryPath);
            }

            currentOutputDirectoryPath = PersistentOutputDirectoryPath;
        }

        private void ReconnectOutputFiles(string mapFilePath, string hisFilePath, string classMapFilePath,
                                          string waqFolderPath, string snappedFolderPath, bool switchTo = false)
        {
            bool existsMapFile = File.Exists(mapFilePath);
            bool existsHisFile = File.Exists(hisFilePath);
            bool existsClassMapFile = File.Exists(classMapFilePath);
            bool existsWaqFolder = Directory.Exists(waqFolderPath);
            bool existsSnappedFolder = Directory.Exists(snappedFolderPath);

            if (!existsMapFile && !existsHisFile && !existsClassMapFile && !existsWaqFolder && !existsSnappedFolder)
            {
                return;
            }

            FireImportProgressChanged(this, "Reading output files - Reading Map file", 1, 2);
            BeginEdit(new DefaultEditAction("Reconnect output files"));

            // deal with issue that kernel doesn't understand any coordinate systems other than RD & WGS84 :
            if (existsMapFile)
            {
                ReportProgressText("Reading map file");
                ICoordinateSystem cs = UnstructuredGridFileHelper.GetCoordinateSystem(mapFilePath);

                // update map file coordinate system:
                if (CoordinateSystem != null && cs != CoordinateSystem)
                {
                    NetFile.WriteCoordinateSystem(mapFilePath, CoordinateSystem);
                }

                if (switchTo && OutputMapFileStore != null)
                {
                    OutputMapFileStore.Path = mapFilePath;
                }
                else
                {
                    OutputMapFileStore = new FMMapFileFunctionStore(this);
                    // don't change this to a property setter, because the timing is of great importance.
                    // elsewise, there will be no subscription to the read and Path triggers the Read().
                    OutputMapFileStore.Path = mapFilePath;
                }
            }

            if (existsHisFile)
            {
                ReportProgressText("Reading his file");
                FireImportProgressChanged(this, "Reading output files - Reading His file", 1, 2);
                if (switchTo && OutputHisFileStore != null)
                {
                    OutputHisFileStore.Path = hisFilePath;
                }
                else
                {
                    OutputHisFileStore = new FMHisFileFunctionStore(hisFilePath, CoordinateSystem,
                                                                    Area.ObservationPoints,
                                                                    Area.ObservationCrossSections,
                                                                    Area.Weirs.Where(
                                                                        w =>
                                                                            w.WeirFormula is
                                                                                GeneralStructureWeirFormula));
                }
            }

            if (existsClassMapFile)
            {
                ReportProgressText("Reading class map file");
                FireImportProgressChanged(this, "Reading output files - Reading Class Map file", 1, 2);
                if (switchTo && OutputClassMapFileStore != null)
                {
                    OutputClassMapFileStore.Path = classMapFilePath;
                }
                else
                {
                    OutputClassMapFileStore = new FMClassMapFileFunctionStore(classMapFilePath);
                }
            }

            if (existsWaqFolder)
            {
                DelwaqOutputDirectoryPath = waqFolderPath;
            }

            if (existsSnappedFolder)
            {
                OutputSnappedFeaturesPath = snappedFolderPath;
            }

            OutputIsEmpty = false;

            EndEdit();
        }

        private void ReadDiaFile(string outputDirectory)
        {
            ReportProgressText("Reading dia file");
            string diaFileName = string.Format("{0}.dia", Name);

            string diaFilePath = Path.Combine(outputDirectory, diaFileName);
            if (File.Exists(diaFilePath))
            {
                try
                {
                    IDataItem logDataItem = DataItems.FirstOrDefault(di => di.Tag == DiaFileDataItemTag);
                    if (logDataItem == null)
                    {
                        // add logfile dataitem if not exists
                        var textDocument = new TextDocument(true) {Name = diaFileName};
                        logDataItem = new DataItem(textDocument, DataItemRole.Output, DiaFileDataItemTag);
                        DataItems.Add(logDataItem);
                    }

                    string log = DiaFileReader.Read(diaFilePath);
                    ((TextDocument) logDataItem.Value).Content = log;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(Resources.WaterFlowFMModel_ReadDiaFile_Error_reading_log_file___0____1_,
                                    diaFileName, ex.Message);
                }
            }
            else
            {
                Log.WarnFormat(
                    Resources.WaterFlowFMModel_ReadDiaFile_Could_not_find_log_file___0__at_expected_path___1_,
                    diaFileName, diaFilePath);
            }
        }

        private void MoveAllContentDirectory(DirectoryInfo sourceDirectory, string targetDirectoryPath)
        {
            foreach (FileInfo file in sourceDirectory.EnumerateFiles())
            {
                MoveFile(file, targetDirectoryPath);
            }

            bool onSameVolume = Directory.GetDirectoryRoot(sourceDirectory.FullName)
                                         .Equals(Directory.GetDirectoryRoot(targetDirectoryPath));

            foreach (DirectoryInfo directory in sourceDirectory.EnumerateDirectories())
            {
                MoveDirectory(directory, targetDirectoryPath, onSameVolume);
            }
        }

        private static void ReportLockedFiles(IEnumerable<string> filePaths)
        {
            string separator = Environment.NewLine + "- ";
            string lockedFilesMessage = separator + string.Join(separator, filePaths);
            Log.Error("There are one or more files locked, please close the following file(s) and save again:" +
                      lockedFilesMessage);
        }

        private IEnumerable<string> GetLockedFiles(string sourceDirectoryPath)
        {
            var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

            foreach (FileInfo file in sourceDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string path = file.FullName;
                string parentDirectoryName = Path.GetFileName(Path.GetDirectoryName(path));

                // Snapped feature files are locked when the map in the GUI is open, so we ignore and copy snapped files instead.
                if (parentDirectoryName != SnappedFeaturesDirectoryName && FileUtils.IsFileLocked(path))
                {
                    yield return path;
                }
            }
        }

        private static void MoveFile(FileInfo file, string targetDirectoryPath)
        {
            string targetPath = Path.Combine(targetDirectoryPath, file.Name);
            file.MoveTo(targetPath);
        }

        private void MoveDirectory(DirectoryInfo sourceDirectoryInfo, string targetParentDirectoryPath,
                                   bool onSameVolume)
        {
            var targetDirectoryInfo =
                new DirectoryInfo(Path.Combine(targetParentDirectoryPath, sourceDirectoryInfo.Name));

            if (onSameVolume && sourceDirectoryInfo.Name != SnappedFeaturesDirectoryName)
            {
                sourceDirectoryInfo.MoveTo(targetDirectoryInfo.FullName);
            }
            else
            {
                FileUtils.CopyAll(sourceDirectoryInfo, targetDirectoryInfo, string.Empty);
            }
        }

        /// <summary>
        /// Removes all files and directories from the directory.
        /// </summary>
        /// <param name="directoryPath"> The directory path of the directory that needs to be cleaned. </param>
        private static void CleanDirectory(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                return;
            }

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                try
                {
                    directory.Delete(true);
                }
                // Do NOT remove: when File Explorer is opened in the directory, an IO exeption is thrown.
                // There is no way of checking for this case, so we have to catch it. The second time it is called, it works fine.
                // https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
                catch (IOException)
                {
                    directory.Delete(true);
                }
            }
        }

        private bool HasOpenFunctionStores =>
            OutputMapFileStore != null || OutputHisFileStore != null || OutputClassMapFileStore != null;

        private void ClearFunctionStore(ReadOnlyNetCdfFunctionStoreBase functionStore)
        {
            functionStore.Functions.Clear();
            functionStore.Close();
        }

        private void SetOutputDirAndWaqDirProperty()
        {
            WaterFlowFMProperty outputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.OutputDir);

            string existingOutputDir = outputDirProperty.GetValueAsString();
            if (!existingOutputDir.StartsWith(OutputDirectoryName))
            {
                outputDirProperty.SetValueAsString(OutputDirectoryName);
                Log.InfoFormat("Running this model requires the OutputDirectory to be overwritten to: {0}",
                               OutputDirectoryName);
            }

            if (!SpecifyWaqOutputInterval)
            {
                return;
            }

            string relativeDWaqOutputDirectory = Path.Combine(OutputDirectoryName, DelwaqOutputDirectoryName);
            WaterFlowFMProperty waqOutputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir);
            waqOutputDirProperty.SetValueAsString(relativeDWaqOutputDirectory);
        }

        private void ClearOutputDirAndWaqDirProperty()
        {
            ModelDefinition.GetModelProperty(KnownProperties.OutputDir).SetValueAsString(string.Empty);
            ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir).SetValueAsString(string.Empty);
        }

        #endregion
    }
}