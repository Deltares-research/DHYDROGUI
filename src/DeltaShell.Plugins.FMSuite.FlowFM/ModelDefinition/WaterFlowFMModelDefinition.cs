using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccess;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    // TODO: Make this an [Entity]. Needs refactoring.
    public class WaterFlowFMModelDefinition
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
        public const string MapFileExtension = "_map.nc";
        public const string HisFileExtension = "_his.nc";
        public const string ClassMapFileExtension = "_clm.nc";
        public const string DefaultOutputDirectoryName = "output";

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

        public List<string> InitialTracerNames { get; private set; }
        public List<string> InitialSpatiallyVaryingSedimentPropertyNames { get; private set; }

        private static StructureSchema<ModelPropertyDefinition> StructureSchemaInstance { get; set; }
        private static ModelSchema<WaterFlowFMPropertyDefinition> MorphologyModelPropertySchema { get; set; }
        private static ModelSchema<WaterFlowFMPropertyDefinition> ModelPropertySchema { get; set; }
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
                    ModelPropertySchema?.GuiPropertyGroups?
                        .Union(MorphologyModelPropertySchema.GuiPropertyGroups);

                return modelPropertyGroups?
                       .GroupBy(kvp => kvp.Key)
                       .Select(grp => grp.First())
                       .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        public string ModelDirectory { get; set; }
        public string ModelName { get; set; }
        public ICoordinateSystem CoordinateSystem { get; set; }

        public readonly IDictionary<string, IList<ISpatialOperation>> SpatialOperations;
        public UnstructuredGridCoverage Bathymetry { get; set; }

        public IList<ISpatialOperation> GetSpatialOperations(string quantityName)
        {
            IList<ISpatialOperation> result;
            SpatialOperations.TryGetValue(quantityName, out result);
            return result;
        }

        public IEventedList<IWindField> WindFields { get; private set; }

        public IList<IUnsupportedFileBasedExtForceFileItem> UnsupportedFileBasedExtForceFileItems { get; private set; }

        public HeatFluxModel HeatFluxModel { get; private set; }

        public IEventedList<Feature2D> Boundaries { get; private set; }

        public IEventedList<BoundaryConditionSet> BoundaryConditionSets { get; private set; }

        public StructureSchema<ModelPropertyDefinition> StructureSchema => StructureSchemaInstance;

        public IEnumerable<IBoundaryCondition> BoundaryConditions
        {
            get
            {
                return BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions);
            }
        }

        public IEventedList<Feature2D> Pipes { get; private set; }

        public IEventedList<SourceAndSink> SourcesAndSinks { get; private set; }

        public IList<Embankment> Embankments { get; set; }

        static WaterFlowFMModelDefinition()
        {
            const string dflowfmCsvFileDirectoryName = "CsvFiles";
            const string dflowfmPropertiesCsvFileName = "dflowfm-properties.csv";
            const string dflowfmStructurePropertiesCsvFileName = "structure-properties.csv";
            const string dflowfmMorPropertiesCsvFileName = "dflowfm-mor-properties.csv";

            Assembly assembly = typeof(WaterFlowFMModelDefinition).Assembly;
            string assemblyLocation = assembly.Location;
            DirectoryInfo directoryInfo = new FileInfo(assemblyLocation).Directory;
            if (directoryInfo != null)
            {
                string path = Path.Combine(directoryInfo.FullName, dflowfmCsvFileDirectoryName);
                string propertiesDefinitionFile = Path.Combine(path, dflowfmPropertiesCsvFileName);
                ModelPropertySchema =
                    new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(propertiesDefinitionFile,
                                                                                            "MduGroup");

                string structurePropertiesDefinitionFile = Path.Combine(path, dflowfmStructurePropertiesCsvFileName);
                StructureSchemaInstance =
                    new StructureFMPropertiesFile().ReadProperties(structurePropertiesDefinitionFile);

                string morPropertiesDefinitionFile = Path.Combine(path, dflowfmMorPropertiesCsvFileName);
                MorphologyModelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(
                    morPropertiesDefinitionFile, "MduGroup");
            }
            else
            {
                throw new Exception("Invalid path for DFlowFM properties definition file");
            }
        }

        public WaterFlowFMModelDefinition()
        {
            HeatFluxModel = new HeatFluxModel();
            Properties = new EventedList<WaterFlowFMProperty>();

            ((INotifyPropertyChange) Properties).PropertyChanged += OnWaterFlowFMPropertyChanged;
            Properties.CollectionChanged += OnWaterFlowFMCollectionChanged;

            foreach (WaterFlowFMPropertyDefinition propertyDefinition in ModelPropertySchema.PropertyDefinitions.Values)
            {
                SetModelProperty(propertyDefinition.MduPropertyName,
                                 new WaterFlowFMProperty(propertyDefinition, propertyDefinition.DefaultValueAsString));
            }

            foreach (WaterFlowFMPropertyDefinition propertyDefinition in MorphologyModelPropertySchema
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
                {GuiProperties.StopTime.ToLower(), OnTimePropertyChanged},
                {GuiProperties.StartTime.ToLower(), OnTimePropertyChanged},
                {KnownProperties.RefDate.ToLower(), OnTimePropertyChanged},
                {KnownProperties.Temperature.ToLower(), OnTemperaturePropertyChanged},
                {GuiProperties.UseMorSed.ToLower(), OnMorphologySedimentPropertyChanged},
                {GuiProperties.WriteSnappedFeatures.ToLower(), OnWriteSnappedFeaturesPropertyChanged},
            };

            SetDefaultGuiTimeProperties();

            Boundaries = new EventedList<Feature2D>();
            BoundaryConditionSets = new EventedList<BoundaryConditionSet>();
            WindFields = new EventedList<IWindField>();
            UnsupportedFileBasedExtForceFileItems = new EventedList<IUnsupportedFileBasedExtForceFileItem>();
            SourcesAndSinks = new EventedList<SourceAndSink>();
            Pipes = new EventedList<Feature2D>();
            SpatialOperations = new Dictionary<string, IList<ISpatialOperation>>();
            InitialTracerNames = new List<string>();
            InitialSpatiallyVaryingSedimentPropertyNames = new List<string>();
            Embankments = new EventedList<Embankment>();
            UpdateWriteOutputSnappedFeatures();
        }

        /// <summary> Sets the default GUI time properties that are derived from the properties (.csv) file. </summary>
        private void SetDefaultGuiTimeProperties()
        {
            var modelStartTime = (double) GetModelProperty(KnownProperties.TStart).Value;
            GetModelProperty(GuiProperties.StartTime).Value = GetAbsoluteDateTime(modelStartTime, true);

            var modelStopTime = (double) GetModelProperty(KnownProperties.TStop).Value;
            GetModelProperty(GuiProperties.StopTime).Value = GetAbsoluteDateTime(modelStopTime, true);

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

        private void SetDefaultTimeProperties(string intervalPropertyName,
                                              string deltaTPropertyName,
                                              string startTimePropertyName = null,
                                              string stopTimePropertyName = null)
        {
            double intervalInSeconds = ((IList<double>) GetModelProperty(intervalPropertyName).Value)[0];
            if (intervalInSeconds > 0)
            {
                var seconds = (int) Math.Floor(intervalInSeconds);
                GetModelProperty(deltaTPropertyName).Value = new TimeSpan(0, 0, 0, seconds);
            }

            if (startTimePropertyName != null)
            {
                GetModelProperty(startTimePropertyName).Value = GetModelProperty(GuiProperties.StartTime).Value;
            }

            if (stopTimePropertyName != null)
            {
                GetModelProperty(stopTimePropertyName).Value = GetModelProperty(GuiProperties.StopTime).Value;
            }
        }

        private void OnWaterFlowFMCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                object removedOrAddedItem = e.GetRemovedOrAddedItem();
                if (removedOrAddedItem == GetModelProperty(KnownProperties.Temperature))
                {
                    var prop = (WaterFlowFMProperty) removedOrAddedItem;
                    HeatFluxModel.Type = (HeatFluxModelType) (int) prop.Value;
                }
            }
        }

        private bool handlingPropertyChanged;
        private readonly Dictionary<string, Action<WaterFlowFMProperty>> waterFlowFmPropertyChangedHandler;

        [EditAction]
        private void OnWaterFlowFMPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (handlingPropertyChanged)
            {
                return; //prevent recursion in syncing useTemperature with heat flux model type
            }

            handlingPropertyChanged = true;

            try
            {
                var prop = (WaterFlowFMProperty) sender;
                string propName = prop.PropertyDefinition.MduPropertyName.ToLower();
                if (waterFlowFmPropertyChangedHandler.ContainsKey(propName))
                {
                    waterFlowFmPropertyChangedHandler[propName](prop);
                }
            }
            finally
            {
                handlingPropertyChanged = false;
            }
        }

        private void OnIcdTypePropertyChanged(WaterFlowFMProperty icdtypProp)
        {
            var icdtyp = (int) icdtypProp.Value;
            if (icdtyp == 2 || icdtyp == 3)
            {
                WaterFlowFMProperty cdbreakpointsProperty = GetModelProperty(KnownProperties.Cdbreakpoints);
                CorrectWindDragCoefficientBreakpointsCollection(cdbreakpointsProperty, icdtyp);

                WaterFlowFMProperty windspeedbreakpointsProperty =
                    GetModelProperty(KnownProperties.Windspeedbreakpoints);
                CorrectWindDragCoefficientBreakpointsCollection(windspeedbreakpointsProperty, icdtyp);
            }
        }

        private void OnTimePropertyChanged(WaterFlowFMProperty prop)
        {
            UpdateOutputTimes();
        }

        private void OnTemperaturePropertyChanged(WaterFlowFMProperty temperatureProp)
        {
            HeatFluxModel.Type = (HeatFluxModelType) (int) temperatureProp.Value;
        }

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

        private void OnWriteSnappedFeaturesPropertyChanged(WaterFlowFMProperty prop)
        {
            foreach (string writeProp in KnownWriteOutputSnappedFeatures)
            {
                GetModelProperty(writeProp).Value = WriteSnappedFeatures;
            }
        }

        private void OnMorphologySedimentPropertyChanged(WaterFlowFMProperty prop)
        {
            if (prop.PropertyDefinition.MduPropertyName != GuiProperties.UseMorSed)
            {
                return;
            }

            SetMapFormatPropertyValue();
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

        public WaterFlowFMProperty GetModelProperty(string propertyName)
        {
            return
                Properties.FirstOrDefault(
                    p =>
                        p.PropertyDefinition.MduPropertyName.Equals(propertyName,
                                                                    StringComparison.InvariantCultureIgnoreCase));
        }

        public WaterFlowFMModelDefinition(string modelDir, string modelName) : this()
        {
            ModelDirectory = modelDir;
            ModelName = modelName;
        }

        public int Kmx
        {
            get => (int) GetModelProperty(KnownProperties.Kmx).Value;
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
            set => GetModelProperty(KnownProperties.MapFormat).SetValueAsString(((int) value).ToString());
        }

        public void SetMapFormatPropertyValue()
        {
            var isPartOf1D2DModel = (bool) GetModelProperty(GuiProperties.PartOf1D2DModel).Value;
            if (isPartOf1D2DModel && MapFormat != MapFormatType.NetCdf)
            {
                MapFormat = MapFormatType.NetCdf;
                Log.InfoFormat(
                    Resources
                        .WaterFlowFMModelDefinition_SetMapFormatPropertyValue_MapFormat_property_value_of_FlowFM_model__0__is_changed_to_1__because_it_is_part_of_an_1D2D_integrated_model_,
                    ModelName);
            }
            else if (!isPartOf1D2DModel && UseMorphologySediment && MapFormat != MapFormatType.Ugrid)
            {
                MapFormat = MapFormatType.Ugrid;
                Log.InfoFormat(
                    Resources
                        .WaterFlowFMModelDefinition_SetMapFormatPropertyValue_MapFormat_property_value_of_FlowFM_model__0__is_changed_to_4_due_to_activation_of_Morphology_,
                    ModelName);
            }
        }

        public bool WriteSnappedFeatures
        {
            get => (bool) GetModelProperty(GuiProperties.WriteSnappedFeatures).Value;
            set => GetModelProperty(GuiProperties.WriteSnappedFeatures).Value = value;
        }

        public bool UseMorphologySediment
        {
            get => (bool) GetModelProperty(GuiProperties.UseMorSed).Value;
            set => GetModelProperty(GuiProperties.UseMorSed).Value = value;
        }

        public string MapFileName => GetFileNameFromProperty(MapFilePropertyName, ModelName + MapFileExtension);

        public string HisFileName => GetFileNameFromProperty(HisFilePropertyName, ModelName + HisFileExtension);

        /// <summary> Gets the relative class map file path. </summary>
        /// <value> The relative class map file path. </value>
        public string ClassMapFileName =>
            GetFileNameFromProperty(ClassMapFilePropertyName, ModelName + ClassMapFileExtension);

        private string GetFileNameFromProperty(string propertyName, string defaultName)
        {
            WaterFlowFMProperty property =
                Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName == propertyName);
            string fileName = property != null ? (string) property.Value : defaultName;

            return string.IsNullOrEmpty(fileName) ? defaultName : fileName;
        }

        public string RelativeComFilePath
        {
            get
            {
                string comFileName = ModelName + "_com.nc";
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
                    return DefaultOutputDirectoryName;
                }

                string mduOutputDir = GetModelProperty(KnownProperties.OutputDir).GetValueAsString()?.Trim();

                if (string.IsNullOrEmpty(mduOutputDir))
                {
                    return DefaultOutputDirectoryName;
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

        /// <summary>
        /// Sets the mdu time properties from GUI properties for writing his, map, class map, restart and waq files.
        /// </summary>
        public void SetMduTimePropertiesFromGuiProperties()
        {
            DateTime originalStartTime =
                GetAbsoluteDateTime((double) GetModelProperty(KnownProperties.TStart).Value, true);
            DateTime originalStopTime =
                GetAbsoluteDateTime((double) GetModelProperty(KnownProperties.TStop).Value, true);
            var modelStartTime = (DateTime) GetModelProperty(GuiProperties.StartTime).Value;
            var modelStopTime = (DateTime) GetModelProperty(GuiProperties.StopTime).Value;

            if (modelStartTime != originalStartTime
                || modelStopTime != originalStopTime)
            {
                GetModelProperty(KnownProperties.TStart).Value = GetRelativeDateTime(modelStartTime, true);
                GetModelProperty(KnownProperties.TStop).Value = GetRelativeDateTime(modelStopTime, true);
            }

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

        private void SetMduIntervalFromGuiProperty(string intervalPropName, string doWritePropName,
                                                   string deltaTPropName)
        {
            var timeFrame = new List<double>();
            var writePropName = (bool) GetModelProperty(doWritePropName).Value;
            if (writePropName)
            {
                var timeSpan = (TimeSpan) GetModelProperty(deltaTPropName).Value;
                double secondsInInterval = (double) timeSpan.Ticks / TimeSpan.TicksPerSecond;
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

            var timeFrame = (List<double>) GetModelProperty(intervalPropName).Value;

            var writePropName = (bool) GetModelProperty(doWritePropName).Value;
            var specifyStartTime = (bool) GetModelProperty(specifyStartPropName).Value;
            if (writePropName && specifyStartTime)
            {
                AddRelativeTimeFromPropertyToList(startTimePropName, timeFrame);

                var specifyStopTime = (bool) GetModelProperty(specifyStopPropName).Value;
                if (specifyStopTime)
                {
                    AddRelativeTimeFromPropertyToList(stopTimePropName, timeFrame);
                }
            }

            GetModelProperty(intervalPropName).Value = timeFrame;
        }

        private void AddRelativeTimeFromPropertyToList(string timePropName, List<double> timeFrame)
        {
            var time = (DateTime) GetModelProperty(timePropName).Value;
            timeFrame.Add(GetRelativeDateTime(time, false));
        }

        /// <summary>
        /// Sets the GUI time properties from mdu properties for writing his, map, class map, restart and waq files.
        /// </summary>
        public void SetGuiTimePropertiesFromMduProperties()
        {
            var mduStartTime = (double) GetModelProperty(KnownProperties.TStart).Value;
            var mduStopTime = (double) GetModelProperty(KnownProperties.TStop).Value;

            GetModelProperty(GuiProperties.StartTime).Value = GetAbsoluteDateTime(mduStartTime, true);
            GetModelProperty(GuiProperties.StopTime).Value = GetAbsoluteDateTime(mduStopTime, true);

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

        private void SetDefaultGuiIntervalFromMdu(string intervalPropName, string doWritePropName,
                                                  string deltaTPropName)
        {
            var timeFrame = (IList<double>) GetModelProperty(intervalPropName).Value;
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
            var timeFrame = (IList<double>) GetModelProperty(intervalPropName).Value;
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
                GetModelProperty(startTimePropName).Value = GetModelProperty(GuiProperties.StartTime).Value;
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
                GetModelProperty(stopTimePropName).Value = GetModelProperty(GuiProperties.StopTime).Value;
            }
        }

        private void SetGuiIntervalFromMduProperty(string doWritePropName, string deltaTPropName,
                                                   IList<double> timeFrame)
        {
            var seconds = (int) Math.Floor(timeFrame[0]);
            var millis = (int) ((timeFrame[0] - seconds) * 1000d);
            var interval = new TimeSpan(0, 0, 0, seconds, millis);
            GetModelProperty(deltaTPropName).Value = interval;
            GetModelProperty(doWritePropName).Value = interval.Ticks > 0;
            // 0 = off (for backward compatibility only)
        }

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

            var ticks = (long) (TimeSpan.TicksPerSecond * relativeTime * timeUnitInSeconds);
            var referenceDate = (DateTime) GetModelProperty(KnownProperties.RefDate).Value;
            return referenceDate.AddTicks(ticks);
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

            var referenceDate = (DateTime) GetModelProperty(KnownProperties.RefDate).Value;
            double ticks = dateTime.Ticks - referenceDate.Ticks;
            return ticks / TimeSpan.TicksPerSecond / numSecondsInTimeStep;
        }

        private void UpdateOutputTimes()
        {
            UpdateOutputTimesFromSimulationPeriod(GuiProperties.SpecifyHisStart, GuiProperties.HisOutputStartTime,
                                                  GuiProperties.SpecifyHisStop, GuiProperties.HisOutputStopTime);
            UpdateOutputTimesFromSimulationPeriod(GuiProperties.SpecifyMapStart, GuiProperties.MapOutputStartTime,
                                                  GuiProperties.SpecifyMapStop, GuiProperties.MapOutputStopTime);
            UpdateOutputTimesFromSimulationPeriod(GuiProperties.SpecifyRstStart, GuiProperties.RstOutputStartTime,
                                                  GuiProperties.SpecifyRstStop, GuiProperties.RstOutputStopTime);
            UpdateOutputTimesFromSimulationPeriod(GuiProperties.SpecifyWaqOutputStartTime,
                                                  GuiProperties.WaqOutputStartTime,
                                                  GuiProperties.SpecifyWaqOutputStopTime,
                                                  GuiProperties.WaqOutputStopTime); /*rstoutput needs to be replaced */
        }

        private void UpdateOutputTimesFromSimulationPeriod(string specifyStartPropName, string startTimePropName,
                                                           string specifyStopPropName, string stopTimePropName)
        {
            if (!(bool) GetModelProperty(specifyStartPropName).Value)
            {
                GetModelProperty(startTimePropName).Value = GetModelProperty(GuiProperties.StartTime).Value;
            }

            if (!(bool) GetModelProperty(specifyStopPropName).Value)
            {
                GetModelProperty(stopTimePropName).Value = GetModelProperty(GuiProperties.StopTime).Value;
            }
        }

        public bool ContainsProperty(string propertyKey)
        {
            return GetModelProperty(propertyKey) != null;
        }

        public void AddProperty(WaterFlowFMProperty waterFlowFmProperty)
        {
            Properties.Add(waterFlowFmProperty);
        }

        private void CorrectWindDragCoefficientBreakpointsCollection(WaterFlowFMProperty breakPointsProperty,
                                                                     int icdtyp)
        {
            var cdbreakpoints = (IList<double>) breakPointsProperty.Value;
            // Append new values:
            if (cdbreakpoints.Count < icdtyp)
            {
                breakPointsProperty.Value =
                    new List<double>(cdbreakpoints.Concat(Enumerable.Repeat(0.0, icdtyp - cdbreakpoints.Count)));
            }

            // Remove obsolete values:
            if (cdbreakpoints.Count > icdtyp)
            {
                breakPointsProperty.Value = new List<double>(cdbreakpoints.Take(icdtyp));
            }
        }

        /// <summary>
        /// If one of the known output snapped features is set to true in the mdu
        /// then we set the GUI property to true (and the rest by waterfall).
        /// </summary>
        public void UpdateWriteOutputSnappedFeatures()
        {
            WriteSnappedFeatures = KnownWriteOutputSnappedFeatures.Any(ws => (bool) GetModelProperty(ws).Value);
        }

        /// <summary>
        /// Update the heat flux model once when loading an mdu file.
        /// Used because the events are off during the load of mdu files.
        /// </summary>
        public void UpdateHeatFluxModel()
        {
            HeatFluxModel.Type = (HeatFluxModelType) (int) GetModelProperty(KnownProperties.Temperature).Value;
        }

        public void SelectSpatialOperations(IEventedList<IDataItem> dataItems, IEnumerable<string> tracerDefinitions,
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

            IEnumerable<string> combinedSpatialDataItemNames = SpatialDataItemNames
                                                               .Concat(InitialTracerNames)
                                                               .Concat(InitialSpatiallyVaryingSedimentPropertyNames);
            IDataItem[] dataItemsFound = combinedSpatialDataItemNames
                                         .SelectMany(n => dataItems.Where(di => di.Name.StartsWith(n))).ToArray();
            List<IDataItem> dataItemsWithConverter = dataItemsFound
                                                     .Where(d => d.ValueConverter is SpatialOperationSetValueConverter)
                                                     .Distinct().ToList();
            List<IDataItem> dataItemsWithOutConverter =
                dataItemsFound.Except(dataItemsWithConverter).Distinct().ToList();

            foreach (IDataItem dataItem in dataItemsWithConverter)
            {
                var spatialOperationValueConverter = (SpatialOperationSetValueConverter) dataItem.ValueConverter;
                if (spatialOperationValueConverter.SpatialOperationSet.Operations.All(SupportedByExtForceFile))
                {
                    // put in everything except spatial operation sets,
                    // because we only use interpolate commands that will grab the importsamplesoperation via the input parameters.
                    List<ISpatialOperation> spatialOperations = spatialOperationValueConverter
                                                                .SpatialOperationSet.Operations
                                                                .Where(s => !(s is ISpatialOperationSet))
                                                                .Select(ConvertSpatialOperation)
                                                                .ToList();

                    SpatialOperations.Add(dataItem.Name, spatialOperations);
                }
                // null check to see if it has a final coverage. It could be that there are only point clouds in the set.
                else if (spatialOperationValueConverter.SpatialOperationSet.Output.Provider != null)
                {
                    // unsupported operations are converted to sample operations that are saved with an xyz file via the model definition.
                    var coverage =
                        spatialOperationValueConverter.SpatialOperationSet.Output.Provider.Features[0] as
                            UnstructuredGridCoverage;

                    // In the event that the coverage is comprised entirely of non-data values, ignore it and continue
                    // (This can happen when exporting spatial operations that comprise of added points but no interpolation
                    // - we're not interested in these for the mdu, they will be saved as dataitems to the dsproj)
                    if (coverage == null || coverage.Components[0].NoDataValues != null &&
                        coverage.GetValues<double>().All(v => coverage.Components[0].NoDataValues.Contains(v)) &&
                        spatialOperationValueConverter.SpatialOperationSet.Operations.Any(op => !(op is EraseOperation))
                    )
                    {
                        continue;
                    }

                    var newOperation = new AddSamplesOperation(false)
                    {
                        Name = spatialOperationValueConverter.SpatialOperationSet.Name
                    };
                    newOperation.SetInputData(AddSamplesOperation.SamplesInputName,
                                              new PointCloudFeatureProvider
                                              {
                                                  PointCloud = coverage.ToPointCloud(0, true),
                                              });

                    if (SpatialOperations.ContainsKey(dataItem.Name))
                    {
                        Log.WarnFormat(
                            Resources
                                .WaterFlowFMModelDefinition_SelectSpatialOperations_Duplication_of_spatial_operations_for__0___Please_verify_the_model_after_saving_,
                            dataItem.Name);
                    }
                    else
                    {
                        SpatialOperations.Add(dataItem.Name, new[]
                        {
                            newOperation
                        });
                    }
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

            foreach (IGrouping<Type, UnstructuredGridCoverage> coverageGrouping in coverageByType)
            {
                Coordinate[] coordinates = null;

                foreach (UnstructuredGridCoverage coverage in coverageGrouping)
                {
                    if (coverage.IsTimeDependent)
                    {
                        throw new NotSupportedException(
                            "Converting time dependent spatial data to samples is not supported");
                    }

                    var component = coverage.Components[0] as IVariable<double>;
                    if (component == null)
                    {
                        throw new NotSupportedException(
                            "Converting a non-double valued coverage component to a point cloud is not supported");
                    }

                    IMultiDimensionalArray<double> values = component.Values;
                    var noDataValue = (double?) component.NoDataValue;

                    var pointCloud = new PointCloud();
                    var i = 0;
                    foreach (double v in values) // using enumerable next is faster than using index (for loop)
                    {
                        if (noDataValue.HasValue && v == noDataValue.Value)
                        {
                            i++;
                            continue;
                        }

                        if (coordinates == null)
                        {
                            coordinates = coverage.Coordinates.ToArray();

                            if (coordinates.Length != values.Count)
                            {
                                throw new InvalidOperationException(
                                    "Spatial data is not consistent: number of coordinate does not match number of values");
                            }
                        }

                        Coordinate coord = coordinates[i];
                        pointCloud.PointValues.Add(new PointValue
                        {
                            X = coord.X,
                            Y = coord.Y,
                            Value = v
                        });
                        i++;
                    }

                    if (pointCloud.PointValues.Count == 0)
                    {
                        continue;
                    }

                    var pointCloudFeatureProvider = new PointCloudFeatureProvider {PointCloud = pointCloud};

                    var newOperation = new AddSamplesOperation(false) {Name = coverage.Name};
                    newOperation.SetInputData(AddSamplesOperation.SamplesInputName, pointCloudFeatureProvider);

                    if (SpatialOperations.ContainsKey(dataItemNameLookup[coverage]))
                    {
                        Log.WarnFormat(
                            Resources
                                .WaterFlowFMModelDefinition_SelectSpatialOperations_Duplication_of_spatial_operations_for__0___Please_verify_the_model_after_saving_,
                            dataItemNameLookup[coverage]);
                    }
                    else
                    {
                        SpatialOperations.Add(dataItemNameLookup[coverage], new[]
                        {
                            newOperation
                        });
                    }
                }
            }
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

                operation = new ImportSamplesSpatialOperationExtension
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
        public static string GetTabName(string key, string messageKey = null, WaterFlowFMModel.WaterFlowFMModel fmModel = null)
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
    }
}