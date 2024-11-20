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
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        public ImportProgressChangedDelegate ImportProgressChanged { get; set; }

        [InvokeRequired]
        private void FireImportProgressChanged(string currentStepName)
        {
            ImportProgressChanged?.Invoke(currentStepName, currentStep++, TOTALSTEPS);
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
        
        private void CreateDataItemsNotAvailableInPreviousVersion()
        {
            if (GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag) == null)
            {
                AddNetworkToModel();
            }
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

            using (var ugridFile = new UGridFile(NetFilePath))
            {
                FireImportProgressChanged(string.Format(Resources.LoadStateFromMdu_Reading_Z_coordinate_NoDataValue_from__0_, Path.GetFileName(NetFilePath)));
                bathymetryNoDataValue = ugridFile.GetZCoordinateNoDataValue(BedLevelLocation);

                FireImportProgressChanged(string.Format(Resources.WaterFlowFMModel_LoadStateFromMdu_Reading_Coordinate_system_from__0_, Path.GetFileName(NetFilePath)));
                CoordinateSystem = ugridFile.ReadCoordinateSystem();
            }
            FireImportProgressChanged(Resources.WaterFlowFMModel_LoadStateFromMdu_Renaming_sub_files);
            RenameSubFilesIfApplicable();

            FireImportProgressChanged(Resources.WaterFlowFMModel_LoadStateFromMdu_Initialize_input_spatial_data);
            InitializeUnstructuredGridCoverages();

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
            Name = Path.GetFileNameWithoutExtension(mduFilePath);
            ModelDefinition = new WaterFlowFMModelDefinition(Name);
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
                MduFile.Read(mduFilePath, convertedFileObjectsForFMModel, (mduStepName) => FireImportProgressChanged(Resources.WaterFlowFMModel_LoadModelFromMdu_Reading_mdu_file + Environment.NewLine + mduStepName));
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
        
        internal void SyncModelTimesWithBase()
        {
            base.StartTime = StartTime;
            base.StopTime = StopTime;
            base.TimeStep = TimeStep;
        }
        
        private void LoadOutputStateFromMdu(string mduFilePath)
        {
            if(!File.Exists(mduFilePath)) return;
            string existingOutputDirectory = RetrieveOutputDirectory(mduFilePath);
            ReconnectOutputFiles(existingOutputDirectory);
        }
        
        private string RetrieveOutputDirectory(string mduFilePath)
        {
            currentOutputDirectoryPath = GetModelOutputDirectory();

            if (ModelDefinition.ContainsProperty(KnownProperties.OutDir))
            {
                string mduOutputDir =
                    ModelDefinition.GetModelProperty(KnownProperties.OutDir).GetValueAsString()?.Trim();

                if (!string.IsNullOrEmpty(mduOutputDir))
                {
                    // We currently assume all OutputDirectoryNames are relative.
                    string mduOutputDirPath = Path.Combine(Path.GetDirectoryName(mduFilePath), mduOutputDir);
                    if (Directory.Exists(mduOutputDirPath))
                    {
                        currentOutputDirectoryPath = mduOutputDirPath;
                    }
                }
                else
                {
                    // try default path
                    string defaultName = "DFM_OUTPUT_" + modelDefinition.ModelName;
                    string mduOutputDirPath = Path.Combine(Path.GetDirectoryName(mduFilePath), defaultName);
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
        
        private void AssembleTracerDefinitions()
        {
            foreach (var boundaryCondition in BoundaryConditions)
            {
                var flowCondition = boundaryCondition as FlowBoundaryCondition;
                if (flowCondition != null && flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    if(!TracerDefinitions.Contains(flowCondition.TracerName))
                    {
                        TracerDefinitions.Add(flowCondition.TracerName);
                    }
                    AddTracerToSourcesAndSink(flowCondition.TracerName);
                }
            }
            var sp = SedimentFractions.SelectMany(sf => sf.GetAllActiveSpatiallyVaryingPropertyNames()).Distinct();
            foreach (var quantity in ModelDefinition.SpatialOperations.Keys.Except(WaterFlowFMModelDefinition.SpatialDataItemNames).Except(sp))
            {
                if (!TracerDefinitions.Contains(quantity))
                {
                    TracerDefinitions.Add(quantity);
                }
            }
        }
        
        private void AssembleSpatiallyVaryingSedimentProperties()
        {
            var spatiallyVaryingSedimentProperties = SedimentFractions.SelectMany(f => f.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().Where(sp => sp.IsSpatiallyVarying)).ToList();
            spatiallyVaryingSedimentProperties.AddRange(SedimentFractions.Where(f=> f.CurrentFormulaType != null ).SelectMany(f => f.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().Where(sp => sp.IsSpatiallyVarying)));
            foreach (var spatiallyVaryingSedimentProperty in spatiallyVaryingSedimentProperties)
            {
                AddToIntialFractions(spatiallyVaryingSedimentProperty.SpatiallyVaryingName);
            }
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
    }
}