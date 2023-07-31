using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public partial class WaterFlowFMModelDefinition
    {
        public const string BathymetryDataItemName = "Bed Level";
        public const string InitialWaterLevelDataItemName = "Initial Water Level";
        public const string InitialSalinityDataItemName = "Initial Salinity";
        public const string InitialTemperatureDataItemName = "Initial Temperature";
        public const string RoughnessDataItemName = "Roughness";
        public const string ViscosityDataItemName = "Viscosity";
        public const string DiffusivityDataItemName = "Diffusivity";
        public const string ClassMapFilePropertyName = "ClassMapFile";
        public const string HisFilePropertyName = "HisFile";
        public const string MapFilePropertyName = "MapFile";
        public const string InitialVelocityXName = "Initial velocity X";
        public const string InitialVelocityYName = "Initial velocity Y";

        public static readonly string[] SpatialDataItemNames =
        {
            BathymetryDataItemName,
            InitialWaterLevelDataItemName,
            InitialSalinityDataItemName,
            InitialTemperatureDataItemName,
            RoughnessDataItemName,
            ViscosityDataItemName,
            DiffusivityDataItemName
        };

        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMModelDefinition));



        public readonly IDictionary<string, IList<ISpatialOperation>> SpatialOperations;

        public readonly List<string> KnownWriteOutputSnappedFeatures = new List<string>()
        {
            KnownProperties.Wrishp_crs,
            KnownProperties.Wrishp_obs,
            KnownProperties.Wrishp_thd,
            KnownProperties.Wrishp_gate,
            KnownProperties.Wrishp_emb,
            KnownProperties.Wrishp_fxw,
            KnownProperties.Wrishp_weir,
            KnownProperties.Wrishp_dryarea,
            KnownProperties.Wrishp_enc,
            KnownProperties.Wrishp_src,
            KnownProperties.Wrishp_pump
        };

        public WaterFlowFMModelDefinition()
        {
            HeatFluxModel = new HeatFluxModel();
            Properties = new EventedList<WaterFlowFMProperty>();

            ((INotifyPropertyChange)Properties).PropertyChanged += OnWaterFlowFMPropertyChanged;
            Properties.CollectionChanged += OnWaterFlowFMCollectionChanged;

            foreach (WaterFlowFMPropertyDefinition propertyDefinition in modelPropertySchema.PropertyDefinitions.Values)
            {
                SetModelProperty(propertyDefinition.MduPropertyName,
                                 new WaterFlowFMProperty(propertyDefinition, propertyDefinition.DefaultValueAsString));
            }

            foreach (WaterFlowFMPropertyDefinition propertyDefinition in morphologyModelPropertySchema
                                                                         .PropertyDefinitions.Values)
            {
                SetModelProperty(propertyDefinition.MduPropertyName,
                                 new WaterFlowFMProperty(propertyDefinition, propertyDefinition.DefaultValueAsString));
            }

            Dependencies.CompileEnabledDependencies(Properties);
            Dependencies.CompileVisibleDependencies(Properties);
            waterFlowFmPropertyChangedHandler = new Dictionary<string, Action<WaterFlowFMProperty>>
            {
                {KnownProperties.ICdtyp.ToLower(), OnIcdTypePropertyChanged},
                {KnownProperties.StopDateTime.ToLower(), OnTimePropertyChanged},
                {KnownProperties.StartDateTime.ToLower(), OnTimePropertyChanged},
                {KnownProperties.RefDate.ToLower(), OnTimePropertyChanged},
                {KnownProperties.Temperature.ToLower(), OnTemperaturePropertyChanged},
                {GuiProperties.UseMorSed.ToLower(), OnMorphologySedimentPropertyChanged},
                {GuiProperties.WriteSnappedFeatures.ToLower(), OnWriteSnappedFeaturesPropertyChanged}
            };

            SetDefaultGuiTimeProperties();
            ClearPropertySortIndices();

            Boundaries = new EventedList<Feature2D>();
            BoundaryConditionSets = new EventedList<BoundaryConditionSet>();
            WindFields = new EventedList<IWindField>();
            UnsupportedFileBasedExtForceFileItems = new EventedList<IUnsupportedFileBasedExtForceFileItem>();
            SourcesAndSinks = new EventedList<SourceAndSink>();
            Pipes = new EventedList<Feature2D>();
            SpatialOperations = new Dictionary<string, IList<ISpatialOperation>>();
            InitialTracerNames = new List<string>();
            InitialSpatiallyVaryingSedimentPropertyNames = new List<string>();
            UpdateWriteOutputSnappedFeatures();
        }

        public WaterFlowFMModelDefinition(string modelDir, string modelName) : this()
        {
            ModelDirectory = modelDir;
            ModelName = modelName;
        }

        public List<string> InitialTracerNames { get; private set; }
        public List<string> InitialSpatiallyVaryingSedimentPropertyNames { get; private set; }
        public IEventedList<WaterFlowFMProperty> Properties { get; private set; }

        /// <summary>
        /// Gets the GUI property groups from the default properties file and the Morphology properties file.
        /// </summary>
        /// <value>
        /// The GUI property groups.
        /// </value>
        public static Dictionary<string, ModelPropertyGroup> GuiPropertyGroups
        {
            get
            {
                IEnumerable<KeyValuePair<string, ModelPropertyGroup>> modelPropertyGroups =
                    modelPropertySchema?.GuiPropertyGroups?
                        .Union(morphologyModelPropertySchema.GuiPropertyGroups);

                return modelPropertyGroups?
                       .GroupBy(kvp => kvp.Key)
                       .Select(grp => grp.First())
                       .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        public string ModelDirectory { get; set; }
        public string ModelName { get; set; }
        public ICoordinateSystem CoordinateSystem { get; set; }
        public UnstructuredGridCoverage Bathymetry { get; set; }

        public IPointCloud InitialVelocityX { get; set; }
        public IPointCloud InitialVelocityY { get; set; }

        public IEventedList<IWindField> WindFields { get; private set; }

        public IList<IUnsupportedFileBasedExtForceFileItem> UnsupportedFileBasedExtForceFileItems { get; private set; }

        public HeatFluxModel HeatFluxModel { get; private set; }

        public IEventedList<Feature2D> Boundaries { get; private set; }

        public IEventedList<BoundaryConditionSet> BoundaryConditionSets { get; private set; }

        public StructureSchema<ModelPropertyDefinition> StructureSchema => structureSchemaInstance;

        public IEnumerable<IBoundaryCondition> BoundaryConditions
        {
            get
            {
                return BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions);
            }
        }

        public IEventedList<Feature2D> Pipes { get; private set; }

        public IEventedList<SourceAndSink> SourcesAndSinks { get; private set; }

        public int Kmx
        {
            get => (int)GetModelProperty(KnownProperties.Kmx).Value;
            set => GetModelProperty(KnownProperties.Kmx).Value = value;
        }

        public MapFormatType MapFormat
        {
            get
            {
                string mapFormatStringValue = GetModelProperty(KnownProperties.MapFormat).GetValueAsString();
                MapFormatType mapFormatValue;
                return Enum.TryParse(mapFormatStringValue, out mapFormatValue) ? mapFormatValue : MapFormatType.Unknown;
            }
            set => GetModelProperty(KnownProperties.MapFormat).SetValueAsString(((int)value).ToString());
        }

        public bool WriteSnappedFeatures
        {
            get => (bool)GetModelProperty(GuiProperties.WriteSnappedFeatures).Value;
            set => GetModelProperty(GuiProperties.WriteSnappedFeatures).Value = value;
        }

        public bool UseMorphologySediment
        {
            get => (bool)GetModelProperty(GuiProperties.UseMorSed).Value;
            set => GetModelProperty(GuiProperties.UseMorSed).Value = value;
        }

        public string MapFileName => GetFileNameFromProperty(MapFilePropertyName, ModelName + FileConstants.MapFileExtension);

        public string HisFileName => GetFileNameFromProperty(HisFilePropertyName, ModelName + FileConstants.HisFileExtension);

        /// <summary> Gets the relative class map file path. </summary>
        /// <value> The relative class map file path. </value>
        public string ClassMapFileName =>
            GetFileNameFromProperty(ClassMapFilePropertyName, ModelName + FileConstants.ClassMapFileExtension);

        public string RelativeComFilePath
        {
            get
            {
                string comFileName = ModelName + FileConstants.ComFileExtension;
                return Path.Combine(OutputDirectoryName, comFileName);
            }
        }

        /// <summary>
        /// Gets the name of the output directory.
        /// </summary>
        /// <value>
        /// The name of the output directory.
        /// </value>
        /// <remarks>
        /// If the property does not exist or the value of the property is null or an empty string we use the default
        /// output directory name.
        /// </remarks>
        /// <remarks> If the value of the property is a dot (.) it means output files are in the model directory. </remarks>
        public string OutputDirectoryName
        {
            get
            {
                if (!ContainsProperty(KnownProperties.OutputDir))
                {
                    return DirectoryNameConstants.OutputDirectoryName;
                }

                string mduOutputDir = GetModelProperty(KnownProperties.OutputDir).GetValueAsString()?.Trim();

                if (string.IsNullOrEmpty(mduOutputDir))
                {
                    return DirectoryNameConstants.OutputDirectoryName;
                }

                if (string.Equals(mduOutputDir, "."))
                {
                    return "";
                }

                return mduOutputDir;
            }
        }

        // Add Z-layers whenever kernel suppports these
        public static IEnumerable<DepthLayerType> SupportedDepthLayerTypes
        {
            get
            {
                yield return DepthLayerType.Single;
                yield return DepthLayerType.Sigma;
            }
        }

        // Enable when kernel supports non-equidistant layering
        public static bool CanSpecifyLayerThicknesses => false;

        public IList<ISpatialOperation> GetSpatialOperations(string quantityName)
        {
            IList<ISpatialOperation> result;
            SpatialOperations.TryGetValue(quantityName, out result);
            return result;
        }

        public WaterFlowFMProperty GetModelProperty(string propertyName)
        {
            return
                Properties.FirstOrDefault(
                    p =>
                        p.PropertyDefinition.MduPropertyName.Equals(propertyName,
                                                                    StringComparison.InvariantCultureIgnoreCase));
        }

        public DateTime GetReferenceDateAsDateTime()
        {
            object value = GetModelProperty(KnownProperties.RefDate).Value;
            var refDate = (DateOnly)value;
            return refDate.ToDateTime(TimeOnly.MinValue);
        }

        public void SetReferenceDateAsDateTime(DateTime value)
        {
            DateOnly refDate = DateOnly.FromDateTime(value);
            if (refDate.ToDateTime(TimeOnly.MinValue) != value)
            {
                throw new ArgumentException($"Unexpected non-zero time in ReferenceTime value {value}");
            }
            GetModelProperty(KnownProperties.RefDate).Value = refDate;

        }

        public void SetMapFormatPropertyValue()
        {
            if (!UseMorphologySediment || MapFormat == MapFormatType.Ugrid)
            {
                return;
            }

            MapFormat = MapFormatType.Ugrid;
            Log.InfoFormat(
                Resources
                    .WaterFlowFMModelDefinition_SetMapFormatPropertyValue_MapFormat_property_value_of_FlowFM_model__0__is_changed_to_4_due_to_activation_of_Morphology_,
                ModelName);
        }

        /// <summary>
        /// Sets the mdu time properties from GUI properties for writing his, map, class map, restart and waq files.
        /// </summary>
        public void SetMduTimePropertiesFromGuiProperties()
        {
            SetMduStartStopDeltaTFromGui(KnownProperties.HisInterval, GuiProperties.WriteHisFile,
                                         GuiProperties.HisOutputDeltaT, GuiProperties.SpecifyHisStart,
                                         GuiProperties.HisOutputStartTime, GuiProperties.SpecifyHisStop,
                                         GuiProperties.HisOutputStopTime);

            SetMduStartStopDeltaTFromGui(KnownProperties.MapInterval, GuiProperties.WriteMapFile,
                                         GuiProperties.MapOutputDeltaT, GuiProperties.SpecifyMapStart,
                                         GuiProperties.MapOutputStartTime, GuiProperties.SpecifyMapStop,
                                         GuiProperties.MapOutputStopTime);

            SetMduStartStopDeltaTFromGui(KnownProperties.RstInterval, GuiProperties.WriteRstFile,
                                         GuiProperties.RstOutputDeltaT, GuiProperties.SpecifyRstStart,
                                         GuiProperties.RstOutputStartTime, GuiProperties.SpecifyRstStop,
                                         GuiProperties.RstOutputStopTime);

            SetMduStartStopDeltaTFromGui(KnownProperties.WaqInterval, GuiProperties.SpecifyWaqOutputInterval,
                                         GuiProperties.WaqOutputDeltaT, GuiProperties.SpecifyWaqOutputStartTime,
                                         GuiProperties.WaqOutputStartTime, GuiProperties.SpecifyWaqOutputStopTime,
                                         GuiProperties.WaqOutputStopTime);

            SetMduIntervalFromGuiProperty(KnownProperties.ClassMapInterval, GuiProperties.WriteClassMapFile,
                                          GuiProperties.ClassMapOutputDeltaT);
        }

        /// <summary>
        /// Sets the GUI time properties from mdu properties for writing his, map, class map, restart and waq files.
        /// </summary>
        public void SetGuiTimePropertiesFromMduProperties()
        {
            SetGuiStartStopDeltaTFromMdu(KnownProperties.HisInterval, GuiProperties.WriteHisFile,
                                         GuiProperties.HisOutputDeltaT, GuiProperties.SpecifyHisStart,
                                         GuiProperties.HisOutputStartTime, GuiProperties.SpecifyHisStop,
                                         GuiProperties.HisOutputStopTime);

            SetGuiStartStopDeltaTFromMdu(KnownProperties.MapInterval, GuiProperties.WriteMapFile,
                                         GuiProperties.MapOutputDeltaT, GuiProperties.SpecifyMapStart,
                                         GuiProperties.MapOutputStartTime, GuiProperties.SpecifyMapStop,
                                         GuiProperties.MapOutputStopTime);

            SetGuiStartStopDeltaTFromMdu(KnownProperties.RstInterval, GuiProperties.WriteRstFile,
                                         GuiProperties.RstOutputDeltaT, GuiProperties.SpecifyRstStart,
                                         GuiProperties.RstOutputStartTime, GuiProperties.SpecifyRstStop,
                                         GuiProperties.RstOutputStopTime);

            SetGuiStartStopDeltaTFromMdu(KnownProperties.WaqInterval, GuiProperties.SpecifyWaqOutputInterval,
                                         GuiProperties.WaqOutputDeltaT, GuiProperties.SpecifyWaqOutputStartTime,
                                         GuiProperties.WaqOutputStartTime, GuiProperties.SpecifyWaqOutputStopTime,
                                         GuiProperties.WaqOutputStopTime);

            SetDefaultGuiIntervalFromMdu(KnownProperties.ClassMapInterval, GuiProperties.WriteClassMapFile,
                                         GuiProperties.ClassMapOutputDeltaT);
        }

        public bool ContainsProperty(string propertyKey)
        {
            return GetModelProperty(propertyKey) != null;
        }

        public void AddProperty(WaterFlowFMProperty waterFlowFmProperty)
        {
            Properties.Add(waterFlowFmProperty);
        }

        /// <summary>
        /// If one of the known output snapped features is set to true in the mdu
        /// then we set the GUI property to true (and the rest by waterfall).
        /// </summary>
        public void UpdateWriteOutputSnappedFeatures()
        {
            WriteSnappedFeatures = KnownWriteOutputSnappedFeatures.Any(ws => (bool)GetModelProperty(ws).Value);
        }

        /// <summary>
        /// Update the heat flux model once when loading an mdu file.
        /// Used because the events are off during the load of mdu files.
        /// </summary>
        public void UpdateHeatFluxModel()
        {
            HeatFluxModel.Type = (HeatFluxModelType)(int)GetModelProperty(KnownProperties.Temperature).Value;
        }

        /// <summary>
        /// Collects the spatial operations needed to be written for each spatial data item.
        /// The spatial operations will be added to the <see cref="SpatialOperations"/>.
        /// For each data item the original values will be written to the xyz file as samples.
        /// </summary>
        /// <param name="dataItems">The data items with the initial spatial coverages.</param>
        /// <param name="tracerDefinitions"> The tracers. </param>
        /// <param name="spatiallyVaryingSedimentDefinitions"> The sediment fractions. </param>
        public void SelectSpatialOperations(IList<IDataItem> dataItems, IEnumerable<string> tracerDefinitions,
                                            IEnumerable<string> spatiallyVaryingSedimentDefinitions = null)
        {
            InitialTracerNames.Clear();
            InitialTracerNames.AddRange(tracerDefinitions);
            List<string> sedimentDefinitionList = spatiallyVaryingSedimentDefinitions?.ToList();

            if (sedimentDefinitionList != null && sedimentDefinitionList.Any(sd => sd != null))
            {
                InitialSpatiallyVaryingSedimentPropertyNames.Clear();
                InitialSpatiallyVaryingSedimentPropertyNames.AddRange(sedimentDefinitionList);
            }

            SpatialOperations.Clear();

            IDataItem[] dataItemsWithConverter = dataItems
                                                 .Where(d => d.ValueConverter is SpatialOperationSetValueConverter)
                                                 .ToArray();

            IEnumerable<string> dataItemWithConverterNames = dataItemsWithConverter.Select(di => di.Name);
            IDataItem[] dataItemsWithOutConverter = dataItems.Except(dataItemsWithConverter)
                                                             .Where(di => !dataItemWithConverterNames.Contains(di.Name))
                                                             .ToArray();

            foreach (IDataItem dataItem in dataItemsWithConverter)
            {
                var spatialOperationValueConverter = (SpatialOperationSetValueConverter)dataItem.ValueConverter;
                if (spatialOperationValueConverter.SpatialOperationSet.Operations.All(SupportedByExtForceFile))
                {
                    SpatialOperations[dataItem.Name] = GetSpatialOperations(spatialOperationValueConverter);
                }
                // null check to see if it has a final coverage. It could be that there are only point clouds in the set.
                else if (spatialOperationValueConverter.SpatialOperationSet.Output.Provider != null)
                {
                    // unsupported operations are converted to sample operations that are saved with an xyz file via the model definition.
                    var coverage = spatialOperationValueConverter.SpatialOperationSet.Output.Provider.Features[0] as UnstructuredGridCoverage;
                    if (ShouldSkipCoverage(coverage, spatialOperationValueConverter))
                    {
                        continue;
                    }

                    AddSamplesOperation newOperation = CreateSamplesOperation(coverage.ToPointCloud(0, true),
                                                                              spatialOperationValueConverter.SpatialOperationSet.Name);

                    AddSpatialOperation(dataItem.Name, newOperation);
                }
            }

            List<IGrouping<Type, UnstructuredGridCoverage>> coverageByType = dataItemsWithOutConverter
                                                                             .Select(di => di.Value)
                                                                             .OfType<UnstructuredGridCoverage>()
                                                                             .GroupBy(c => c.GetType()).ToList();

            Dictionary<object, string> dataItemNameLookup = dataItemsWithOutConverter
                                                            .GroupBy(o => o.Name)
                                                            .Select(o => o.FirstOrDefault()) //Removing duplicates.
                                                            .ToDictionary(di => di.Value, di => di.Name);

            foreach (UnstructuredGridCoverage coverage in coverageByType.SelectMany(c => c))
            {
                IPointCloud pointCloud = coverage.ToPointCloud(skipMissingValues: true);
                if (pointCloud.PointValues.Count == 0)
                {
                    continue;
                }

                AddSamplesOperation samplesOperation = CreateSamplesOperation(pointCloud, coverage.Name);
                AddSpatialOperation(dataItemNameLookup[coverage], samplesOperation);
            }
        }

        private static List<ISpatialOperation> GetSpatialOperations(SpatialOperationSetValueConverter spatialOperationValueConverter)
        {
            // put in everything except spatial operation sets,
            // because we only use interpolate commands that will grab the importsamplesoperation via the input parameters.
            List<ISpatialOperation> spatialOperations = spatialOperationValueConverter
                                                        .SpatialOperationSet.Operations
                                                        .Where(s => !(s is ISpatialOperationSet))
                                                        .Select(ConvertSpatialOperation)
                                                        .ToList();

            if (!spatialOperations.Any())
            {
                return spatialOperations;
            }

            var originalCoverage = (UnstructuredGridCoverage)spatialOperationValueConverter.OriginalValue;
            IPointCloud samples = originalCoverage.ToPointCloud(skipMissingValues: true);

            if (!samples.PointValues.Any() || SamplesAreEqual(samples, spatialOperations[0]))
            {
                return spatialOperations;
            }

            AddSamplesOperation samplesOperation = CreateSamplesOperation(samples, originalCoverage.Name);
            spatialOperations.Insert(0, samplesOperation);

            return spatialOperations;
        }

        private static bool SamplesAreEqual(IPointCloud samples, ISpatialOperation operation)
        {
            if (!(operation is ImportSamplesOperation importSamplesOperation))
            {
                return false;
            }

            return samples.PointValues.SequenceEqual(importSamplesOperation.GetPoints(), new PointValueEqualityComparer());
        }

        private static bool ShouldSkipCoverage(UnstructuredGridCoverage coverage, SpatialOperationSetValueConverter spatialOperationValueConverter)
        {
            // In the event that the coverage is comprised entirely of non-data values, ignore it and continue
            // (This can happen when exporting spatial operations that comprise of added points but no interpolation
            // - we're not interested in these for the mdu, they will be saved as dataitems to the dsproj)
            return coverage == null || coverage.Components[0].NoDataValues != null &&
                   coverage.GetValues<double>().All(v => coverage.Components[0].NoDataValues.Contains(v)) &&
                   spatialOperationValueConverter.SpatialOperationSet.Operations.Any(op => !(op is EraseOperation));
        }

        private void AddSpatialOperation(string name, AddSamplesOperation newOperation)
        {
            if (SpatialOperations.ContainsKey(name))
            {
                Log.WarnFormat(
                    Resources
                        .WaterFlowFMModelDefinition_SelectSpatialOperations_Duplication_of_spatial_operations_for__0___Please_verify_the_model_after_saving_,
                    name);
            }
            else
            {
                SpatialOperations.Add(name, new[]
                {
                    newOperation
                });
            }
        }

        private static AddSamplesOperation CreateSamplesOperation(IPointCloud pointCloud, string name)
        {
            var featureProvider = new PointCloudFeatureProvider { PointCloud = pointCloud };

            var operation = new AddSamplesOperation(false) { Name = name };
            operation.SetInputData(AddSamplesOperation.SamplesInputName, featureProvider);

            return operation;
        }

        public static bool SupportedByExtForceFile(ISpatialOperation operation)
        {
            var valueOperation = operation as SetValueOperation;
            if (valueOperation != null)
            {
                return ExtForceQuantNames.OperatorMapping.ContainsKey(valueOperation.OperationType);
            }

            var interpolateOperation = operation as InterpolateOperation;
            if (interpolateOperation != null)
            {
                // only write interpolate operations that contain an importsamplesoperation as input samples
                return interpolateOperation.GetInput(InterpolateOperation.InputSamplesName).Source.Operation is
                           ImportSamplesOperation;
            }

            // subsets are supported when only an importsamplesoperation is contained
            var subSet = operation as ISpatialOperationSet;
            if (subSet != null)
            {
                return subSet.Operations.Count == 1 && subSet.Operations[0] is ImportSamplesOperation;
            }

            return false;
        }

        public static ISpatialOperation ConvertSpatialOperation(ISpatialOperation operation)
        {
            var interpolateOperation = operation as InterpolateOperation;
            if (interpolateOperation != null)
            {
                // only write interpolate operations that contain an importsamplesoperation as input samples
                var importSamplesOperation =
                    (ImportSamplesOperation)
                    interpolateOperation.GetInput(InterpolateOperation.InputSamplesName).Source.Operation;

                operation = new ImportSamplesSpatialOperation
                {
                    Name = importSamplesOperation.Name,
                    FilePath = importSamplesOperation.FilePath,
                    Enabled = importSamplesOperation.Enabled,
                    InterpolationMethod = interpolateOperation.InterpolationMethod,
                    AveragingMethod = interpolateOperation.GridCellAveragingMethod,
                    RelativeSearchCellSize = interpolateOperation.RelativeSearchCellSize
                };
            }

            return operation;
        }

        /// <summary>
        /// Gets the name of the tab.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="messageKey"> The message key. </param>
        /// <param name="fmModel"> The fm model. </param>
        /// <returns> </returns>
        public static string GetTabName(string key, string messageKey = null, WaterFlowFMModel fmModel = null)
        {
            if (key == KnownProperties.SedFile)
            {
                if (fmModel == null)
                {
                    return string.Empty;
                }

                WaterFlowFMProperty useSedFileFlowFmProperty =
                    fmModel.ModelDefinition.GetModelProperty(KnownProperties.SedFile);
                string guiSedimentGroupId =
                    string.IsNullOrEmpty(useSedFileFlowFmProperty.PropertyDefinition.FileCategoryName)
                        ? "sediment"
                        : useSedFileFlowFmProperty.PropertyDefinition.FileCategoryName;

                key = guiSedimentGroupId;
                messageKey = "sediment file";
            }

            if (GuiPropertyGroups.ContainsKey(key))
            {
                return GuiPropertyGroups[key].Name;
            }

            Log.ErrorFormat(
                Resources
                    .WaterFlowFMModelDefinition_GetTabName_Invalid_gui_group_id_for___0___in_the_scheme_of_dflowfmmorpropertiescsv___1_,
                messageKey, key);

            return string.Empty;
        }

        private static string FMCsvFilesPath()
        {
            Assembly assembly = typeof(WaterFlowFMModelDefinition).Assembly;
            DirectoryInfo directoryInfo = new FileInfo(assembly.Location).Directory;
            if (directoryInfo != null)
            {
                return Path.Combine(directoryInfo.FullName, csvPropertyFilesDirectory);
            }

            throw new FileNotFoundException("Invalid path for DFlowFM properties definition file");
        }
        private static StructureSchema<ModelPropertyDefinition> ReadStructureSchema(string propertiesCsvFilename)
        {
            string structurePropertiesDefinitionFile = Path.Combine(FMCsvFilesPath(), propertiesCsvFilename);
            return new StructureFMPropertiesFile().ReadProperties(structurePropertiesDefinitionFile);
        }

        private static ModelPropertySchema<WaterFlowFMPropertyDefinition> ReadWaterFlowPropertySchema(string propertiesCsvFilename)
        {
            string propertiesDefinitionFile = Path.Combine(FMCsvFilesPath(), propertiesCsvFilename);
            return new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(propertiesDefinitionFile, "MduGroup");
        }

        private const string csvPropertyFilesDirectory = "CsvFiles";
        private const string structurePropertiesCsvFileName = "structure-properties.csv";
        private const string dflowfmPropertiesCsvFileName = "dflowfm-properties.csv";
        private const string dflowfmMorPropertiesCsvFileName = "dflowfm-mor-properties.csv";
        private static readonly StructureSchema<ModelPropertyDefinition> structureSchemaInstance  = ReadStructureSchema(structurePropertiesCsvFileName);
        private static readonly ModelPropertySchema<WaterFlowFMPropertyDefinition> morphologyModelPropertySchema = ReadWaterFlowPropertySchema(dflowfmMorPropertiesCsvFileName);
        private static readonly ModelPropertySchema<WaterFlowFMPropertyDefinition> modelPropertySchema = ReadWaterFlowPropertySchema(dflowfmPropertiesCsvFileName);

        /// <summary> Sets the default GUI time properties that are derived from the properties (.csv) file. </summary>
        private void SetDefaultGuiTimeProperties()
        {
            SetDefaultTimeProperties(KnownProperties.HisInterval, GuiProperties.HisOutputDeltaT,
                                     GuiProperties.HisOutputStartTime, GuiProperties.HisOutputStopTime);
            SetDefaultTimeProperties(KnownProperties.MapInterval, GuiProperties.MapOutputDeltaT,
                                     GuiProperties.MapOutputStartTime, GuiProperties.MapOutputStopTime);
            SetDefaultTimeProperties(KnownProperties.ClassMapInterval, GuiProperties.ClassMapOutputDeltaT);
            SetDefaultTimeProperties(KnownProperties.RstInterval, GuiProperties.RstOutputDeltaT,
                                     GuiProperties.RstOutputStartTime, GuiProperties.RstOutputStopTime);
            SetDefaultTimeProperties(KnownProperties.WaqInterval, GuiProperties.WaqOutputDeltaT,
                                     GuiProperties.WaqOutputStartTime, GuiProperties.WaqOutputStopTime);
        }
        
        private void ClearPropertySortIndices()
        {
            Properties.ForEach(p => p.PropertyDefinition.SortIndex = -1);
        }

        private void SetDefaultTimeProperties(string intervalPropertyName,
                                              string deltaTPropertyName,
                                              string startTimePropertyName = null,
                                              string stopTimePropertyName = null)
        {
            double intervalInSeconds = ((IList<double>)GetModelProperty(intervalPropertyName).Value)[0];
            if (intervalInSeconds > 0)
            {
                var seconds = (int)Math.Floor(intervalInSeconds);
                GetModelProperty(deltaTPropertyName).Value = new TimeSpan(0, 0, 0, seconds);
            }

            if (startTimePropertyName != null)
            {
                GetModelProperty(startTimePropertyName).Value = GetModelProperty(KnownProperties.StartDateTime).Value;
            }

            if (stopTimePropertyName != null)
            {
                GetModelProperty(stopTimePropertyName).Value = GetModelProperty(KnownProperties.StopDateTime).Value;
            }
        }

        private void SetModelProperty(string mduPropertyName, WaterFlowFMProperty property)
        {
            WaterFlowFMProperty prop = GetModelProperty(mduPropertyName);
            if (prop != null)
            {
                Properties[Properties.IndexOf(prop)] = property;
            }
            else
            {
                Properties.Add(property);
            }
        }

        private string GetFileNameFromProperty(string propertyName, string defaultName)
        {
            WaterFlowFMProperty property =
                Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName == propertyName);
            string fileName = property != null ? (string)property.Value : defaultName;

            return string.IsNullOrEmpty(fileName) ? defaultName : fileName;
        }

        private void SetMduIntervalFromGuiProperty(string intervalPropName, string doWritePropName,
                                                   string deltaTPropName)
        {
            var timeFrame = new List<double>();
            var writePropName = (bool)GetModelProperty(doWritePropName).Value;
            if (writePropName)
            {
                var timeSpan = (TimeSpan)GetModelProperty(deltaTPropName).Value;
                double secondsInInterval = (double)timeSpan.Ticks / TimeSpan.TicksPerSecond;
                if (secondsInInterval > 0)
                {
                    timeFrame.Add(secondsInInterval);
                }
            }
            else
            {
                timeFrame.Add(0.0);
            }

            GetModelProperty(intervalPropName).Value = timeFrame;
        }

        private void SetMduStartStopDeltaTFromGui(string intervalPropName, string doWritePropName,
                                                  string deltaTPropName,
                                                  string specifyStartPropName, string startTimePropName,
                                                  string specifyStopPropName, string stopTimePropName)
        {
            SetMduIntervalFromGuiProperty(intervalPropName, doWritePropName, deltaTPropName);

            var timeFrame = (List<double>)GetModelProperty(intervalPropName).Value;

            var writePropName = (bool)GetModelProperty(doWritePropName).Value;
            var specifyStartTime = (bool)GetModelProperty(specifyStartPropName).Value;
            if (writePropName && specifyStartTime)
            {
                AddRelativeTimeFromPropertyToList(startTimePropName, timeFrame);

                var specifyStopTime = (bool)GetModelProperty(specifyStopPropName).Value;
                if (specifyStopTime)
                {
                    AddRelativeTimeFromPropertyToList(stopTimePropName, timeFrame);
                }
            }

            GetModelProperty(intervalPropName).Value = timeFrame;
        }

        private void AddRelativeTimeFromPropertyToList(string timePropName, List<double> timeFrame)
        {
            var time = (DateTime)GetModelProperty(timePropName).Value;
            timeFrame.Add(GetRelativeDateTime(time, false));
        }

        private void SetDefaultGuiIntervalFromMdu(string intervalPropName, string doWritePropName,
                                                  string deltaTPropName)
        {
            var timeFrame = (IList<double>)GetModelProperty(intervalPropName).Value;
            if (timeFrame.Count == 0)
            {
                GetModelProperty(deltaTPropName).Value = new TimeSpan(0, 0, 5, 0);
                GetModelProperty(doWritePropName).Value = true;
            }
            else
            {
                // interval is present
                SetGuiIntervalFromMduProperty(doWritePropName, deltaTPropName, timeFrame);
            }
        }

        private void SetGuiStartStopDeltaTFromMdu(string intervalPropName, string doWritePropName,
                                                  string deltaTPropName,
                                                  string specifyStartPropName, string startTimePropName,
                                                  string specifyStopPropName, string stopTimePropName)
        {
            var timeFrame = (IList<double>)GetModelProperty(intervalPropName).Value;
            if (timeFrame.Count == 0)
            {
                if (intervalPropName == KnownProperties.MapInterval)
                {
                    GetModelProperty(deltaTPropName).Value = new TimeSpan(0, 0, 20, 0);
                    GetModelProperty(doWritePropName).Value = true;
                }

                if (intervalPropName == KnownProperties.HisInterval)
                {
                    GetModelProperty(deltaTPropName).Value = new TimeSpan(0, 0, 5, 0);
                    GetModelProperty(doWritePropName).Value = true;
                }

                if (intervalPropName == KnownProperties.RstInterval)
                {
                    GetModelProperty(deltaTPropName).Value = new TimeSpan(0, 24, 0, 0);
                    GetModelProperty(doWritePropName).Value = true;
                }

                if (intervalPropName == KnownProperties.WaqInterval)
                {
                    GetModelProperty(deltaTPropName).Value = new TimeSpan(0, 0, 0, 0);
                    GetModelProperty(doWritePropName).Value = true;
                }
            }

            if (timeFrame.Count > 0)
            {
                // interval is present
                SetGuiIntervalFromMduProperty(doWritePropName, deltaTPropName, timeFrame);
            }

            if (timeFrame.Count > 1)
            {
                // output start time is specified
                GetModelProperty(startTimePropName).Value = GetAbsoluteDateTime(timeFrame[1], false);
                GetModelProperty(specifyStartPropName).Value = true;
            }
            else
            {
                // output start time not specified, set to model start time
                GetModelProperty(startTimePropName).Value = GetModelProperty(KnownProperties.StartDateTime).Value;
            }

            if (timeFrame.Count > 2)
            {
                // output stop time is specified
                GetModelProperty(stopTimePropName).Value = GetAbsoluteDateTime(timeFrame[2], false);
                GetModelProperty(specifyStopPropName).Value = true;
            }
            else
            {
                // output start time not specified, set to model stop time
                GetModelProperty(stopTimePropName).Value = GetModelProperty(KnownProperties.StopDateTime).Value;
            }
        }

        private void SetGuiIntervalFromMduProperty(string doWritePropName,
                                                   string deltaTPropName,
                                                   IList<double> timeFrame)
        {
            var seconds = (int)Math.Floor(timeFrame[0]);
            var millis = (int)((timeFrame[0] * 1000d) - (seconds * 1000d));
            var interval = new TimeSpan(0, 0, 0, seconds, millis);
            GetModelProperty(deltaTPropName).Value = interval;
            GetModelProperty(doWritePropName).Value = interval.Ticks > 0;
            // 0 = off (for backward compatibility only)
        }

        /// <summary>
        /// Get the absolute date and time based on the model reference date, a relative time and a time unit.
        /// </summary>
        /// <param name="relativeTime">The relative time from the model reference date.</param>
        /// <param name="useTUnit">The time unit the relative time is specified in.</param>
        /// <returns>DateTime that represents the model reference date plus the provided relative time.</returns>
        private DateTime GetAbsoluteDateTime(double relativeTime, bool useTUnit)
        {
            string unitString = GetModelProperty(KnownProperties.Tunit).GetValueAsString();

            double timeUnitInSeconds = 1;
            if (useTUnit)
            {
                if (unitString.ToLower().Equals("m"))
                {
                    timeUnitInSeconds = 60d;
                }
                else if (unitString.ToLower().Equals("h"))
                {
                    timeUnitInSeconds = 3600d;
                }
            }

            var ticks = (long)(TimeSpan.TicksPerSecond * relativeTime * timeUnitInSeconds);
            var referenceDateTime = GetReferenceDateAsDateTime();
            return referenceDateTime.AddTicks(ticks);
        }

        private double GetRelativeDateTime(DateTime dateTime, bool useTUnit)
        {
            string unitString = GetModelProperty(KnownProperties.Tunit).GetValueAsString();

            long numSecondsInTimeStep = 1;
            if (useTUnit)
            {
                if (unitString.ToLower().Equals("m"))
                {
                    numSecondsInTimeStep = 60;
                }
                else if (unitString.ToLower().Equals("h"))
                {
                    numSecondsInTimeStep = 3600;
                }
            }

            var referenceDate = GetReferenceDateAsDateTime();
            double ticks = dateTime.Ticks - referenceDate.Ticks;
            return ticks / TimeSpan.TicksPerSecond / numSecondsInTimeStep;
        }
    }
}