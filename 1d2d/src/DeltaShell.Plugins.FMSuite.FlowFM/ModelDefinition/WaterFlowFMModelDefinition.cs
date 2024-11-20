using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public class WaterFlowFMModelDefinition
    {
        public const string BathymetryDataItemName = "Bed Level";
        public const string InitialWaterLevelDataItemName = "Initial Water Level";
        public const string InitialWaterDepthDataItemName = "Initial Water Depth";
        public const string InitialSalinityDataItemName = "Initial Salinity";
        public const string InitialTemperatureDataItemName = "Initial Temperature";
        public const string RoughnessDataItemName = "Roughness";
        public const string ViscosityDataItemName = "Viscosity";
        public const string DiffusivityDataItemName = "Diffusivity";
        public const string InfiltrationDataItemName = "Infiltration";
        
        public static readonly string[] SpatialDataItemNames =
        {
            BathymetryDataItemName,
            InitialWaterLevelDataItemName,
            InitialWaterDepthDataItemName,
            InitialSalinityDataItemName,
            InitialTemperatureDataItemName,
            RoughnessDataItemName,
            ViscosityDataItemName,
            DiffusivityDataItemName,
            InfiltrationDataItemName
        };

        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMModelDefinition));

        public List<string> InitialTracerNames { get; private set; }
        public List<string> InitialSpatiallyVaryingSedimentPropertyNames { get; private set; }

        private StructureSchema<ModelPropertyDefinition> StructureSchemaInstance { get; set; }
        private ModelSchema<WaterFlowFMPropertyDefinition> MorphologyModelPropertySchema { get; set; }
        private ModelSchema<WaterFlowFMPropertyDefinition> ModelPropertySchema { get; set; }
        public IEventedList<WaterFlowFMProperty> Properties { get; private set; }

        /// <summary>
        /// Gets the GUI property groups from the default properties file and the Morphology properties file.
        /// </summary>
        /// <value>
        /// The GUI property groups.
        /// </value>
        public Dictionary<string, ModelPropertyGroup> GuiPropertyGroups
        {
            get
            {
                var modelPropertyGroups =
                    ModelPropertySchema?.GuiPropertyGroups?
                        .Union(MorphologyModelPropertySchema.GuiPropertyGroups);
                    
                return modelPropertyGroups?
                    .GroupBy(kvp => kvp.Key)
                    .Select(grp => grp.First())
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        }
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
        public IEventedList<IFmMeteoField> FmMeteoFields { get; private set; }
        public IList<IUnsupportedFileBasedExtForceFileItem> UnsupportedFileBasedExtForceFileItems { get; private set; }

        public HeatFluxModel HeatFluxModel { get; private set; }

        public IEventedList<Feature2D> Boundaries { get; private set; }
        
        public IEventedList<BoundaryConditionSet> BoundaryConditionSets { get; private set; }

        public StructureSchema<ModelPropertyDefinition> StructureSchema { get { return StructureSchemaInstance; } }

        public IEnumerable<IBoundaryCondition> BoundaryConditions
        {
            get { return BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions); }
        }

        public IEventedList<Feature2D> Pipes { get; private set; }
        
        public IEventedList<SourceAndSink> SourcesAndSinks { get; private set; }
        public IFeatureCoverage Inflows { get; private set; }

        public IList<Embankment> Embankments { get; set; }

        public WaterFlowFMModelDefinition()
        {
            ReadProperties();

            HeatFluxModel = new HeatFluxModel();
            Properties = new EventedList<WaterFlowFMProperty>();

            ((INotifyPropertyChange)Properties).PropertyChanged += OnWaterFlowFMPropertyChanged;
            Properties.CollectionChanged += OnWaterFlowFMCollectionChanged;

            foreach (var propertyDefinition in ModelPropertySchema.PropertyDefinitions.Values)
            {
                SetModelProperty(propertyDefinition.MduPropertyName,
                                 new WaterFlowFMProperty(propertyDefinition, propertyDefinition.DefaultValueAsString));
            }
            
            foreach (var propertyDefinition in MorphologyModelPropertySchema.PropertyDefinitions.Values)
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

            Dependencies.CompileDefaultValueIndexerDependencies(Properties);
            SetDefaultReferenceDate();
            SetGuiTimePropertiesFromMduProperties();
            ClearPropertySortIndices();

            Boundaries = new EventedList<Feature2D>();
            BoundaryConditionSets = new EventedList<BoundaryConditionSet>();
            WindFields = new EventedList<IWindField>();
            FmMeteoFields = new EventedList<IFmMeteoField>();
            UnsupportedFileBasedExtForceFileItems = new EventedList<IUnsupportedFileBasedExtForceFileItem>();
            SourcesAndSinks = new EventedList<SourceAndSink>();
            Pipes = new EventedList<Feature2D>();
            SpatialOperations = new Dictionary<string, IList<ISpatialOperation>>();
            InitialTracerNames = new List<string>();
            InitialSpatiallyVaryingSedimentPropertyNames = new List<string>();
            Embankments = new EventedList<Embankment>();

            Inflows = new FeatureCoverage("Inflows");
            Inflows.Arguments.Add(new Variable<DateTime>()); //time variable
            Inflows.Arguments.Add(new Variable<IFeature> { IsAutoSorted = false }); //feature variable
            Inflows.Components.Add(new Variable<double>("Inflows", new Unit("Discharge", "m³/s"))); //component


            UpdateWriteOutputSnappedFeatures();
        }

        private void ReadProperties()
        {
            const string dflowfmPropertiesCsvFileName = "dflowfm-properties.csv";
            const string dflowfmStructurePropertiesCsvFileName = "structure-properties.csv";
            const string dflowfmMorPropertiesCsvFileName = "dflowfm-mor-properties.csv";
            var assembly = typeof(WaterFlowFMModelDefinition).Assembly;
            var assemblyLocation = assembly.Location;
            var directoryInfo = new FileInfo(assemblyLocation).Directory;
            if (directoryInfo != null)
            {
                var path = directoryInfo.FullName;
                var propertiesDefinitionFile = Path.Combine(path, dflowfmPropertiesCsvFileName);
                ModelPropertySchema =
                    new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(propertiesDefinitionFile,
                                                                                            "MduGroup");

                var structurePropertiesDefinitionFile = Path.Combine(path, dflowfmStructurePropertiesCsvFileName);
                StructureSchemaInstance =
                    new StructureFMPropertiesFile().ReadProperties(structurePropertiesDefinitionFile);

                var morPropertiesDefinitionFile = Path.Combine(path, dflowfmMorPropertiesCsvFileName);
                MorphologyModelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(
                    morPropertiesDefinitionFile, "MduGroup");
            }
            else
            {
                throw new Exception("Invalid path for DFlowFM properties definition file");
            }
        }

        /// <summary>
        /// Sets the initial model reference date equal to today's date.
        /// </summary>
        private void SetDefaultReferenceDate()
        {
            GetModelProperty(KnownProperties.RefDate).Value = DateOnly.FromDateTime(DateTime.Today);
        }


        private void OnWaterFlowFMCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add ||
                e.GetRemovedOrAddedItem() != GetModelProperty(KnownProperties.Temperature)) return;

            var prop = (WaterFlowFMProperty) e.GetRemovedOrAddedItem();
            HeatFluxModel.Type = (HeatFluxModelType) ((int)prop.Value);
        }

        private bool handlingPropertyChanged;
        private readonly Dictionary<string, Action<WaterFlowFMProperty>> waterFlowFmPropertyChangedHandler;

        private void OnWaterFlowFMPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (handlingPropertyChanged) return; //prevent recursion in syncing useTemperature with heat flux model type

            handlingPropertyChanged = true;

            try
            {
                var prop = (WaterFlowFMProperty) sender;
                var propName = prop.PropertyDefinition.MduPropertyName.ToLower();
                if (waterFlowFmPropertyChangedHandler.ContainsKey(propName))
                    waterFlowFmPropertyChangedHandler[propName](prop);
                
                UpdateLinkedProperties(prop);
            }
            finally
            {
                handlingPropertyChanged = false;
            }
        }

        private static void UpdateLinkedProperties(ModelProperty property)
        {
            if (property.LinkedModelProperty != null)
            {
                property.LinkedModelProperty.SetValueFromString(property.LinkedModelProperty.PropertyDefinition.DefaultValueAsStringArray[(int)property.Value]);
            }
        }

        private void OnIcdTypePropertyChanged(WaterFlowFMProperty icdtypProp)
        {
            var icdtyp = (int) icdtypProp.Value;
            if (icdtyp == 2 || icdtyp == 3)
            {
                var cdbreakpointsProperty = GetModelProperty(KnownProperties.Cdbreakpoints);
                CorrectWindDragCoefficientBreakpointsCollection(cdbreakpointsProperty, icdtyp);

                var windspeedbreakpointsProperty = GetModelProperty(KnownProperties.Windspeedbreakpoints);
                CorrectWindDragCoefficientBreakpointsCollection(windspeedbreakpointsProperty, icdtyp);
            }
        }

        private void OnTimePropertyChanged(WaterFlowFMProperty prop)
        {
            UpdateOutputTimes();
        }

        private void OnTemperaturePropertyChanged(WaterFlowFMProperty temperatureProp)
        {
            HeatFluxModel.Type = (HeatFluxModelType) ((int) temperatureProp.Value);
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
            foreach (var writeProp in KnownWriteOutputSnappedFeatures)
            {
                GetModelProperty(writeProp).Value = WriteSnappedFeatures;
            }
        }

        private void OnMorphologySedimentPropertyChanged(WaterFlowFMProperty prop)
        {
            if(prop.PropertyDefinition.MduPropertyName != GuiProperties.UseMorSed) return;
            SetMapFormatPropertyValue();
        }

        private void SetModelProperty(string mduPropertyName, WaterFlowFMProperty property)
        {
            var prop = GetModelProperty(mduPropertyName);
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

        public void SetModelProperty(string propertyName, string value)
        {
            WaterFlowFMProperty waterFlowFMProperty = GetModelProperty(propertyName);
            waterFlowFMProperty?.SetValueFromString(value);
        }
        
        /// <summary>
        /// Sets the property <paramref name="propertyName"/> with the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="propertyName"> The property name. </param>
        /// <param name="value"> The new property value. </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="propertyName"/> is <c>null</c> or empty.
        /// </exception>
        /// <remarks> The property is expected to exist. </remarks>
        public void SetModelProperty(string propertyName, object value)
        {
            Ensure.NotNullOrEmpty(propertyName, nameof(propertyName));

            WaterFlowFMProperty waterFlowFMProperty = GetModelProperty(propertyName);
            waterFlowFMProperty.Value = value;
        }

        public WaterFlowFMModelDefinition(string modelName) : this()
        {
            ModelName = modelName;
        }

        public int Kmx
        {
            get { return (int) GetModelProperty(KnownProperties.Kmx).Value; }
            set { GetModelProperty(KnownProperties.Kmx).Value = value; }
        }

        public MapFormatType MapFormat
        {
            get
            {
                var mapFormatStringValue = GetModelProperty(KnownProperties.MapFormat).GetValueAsString();
                MapFormatType mapFormatValue;
                return Enum.TryParse(mapFormatStringValue, out mapFormatValue) ? mapFormatValue : MapFormatType.Unknown;
            }
            set { GetModelProperty(KnownProperties.MapFormat).SetValueFromString(((int)value).ToString()); }
        }

        public void SetMapFormatPropertyValue()
        {
            if (UseMorphologySediment && MapFormat != MapFormatType.Ugrid)
            {
                MapFormat = MapFormatType.Ugrid;
                Log.InfoFormat(Resources.WaterFlowFMModelDefinition_SetMapFormatPropertyValue_MapFormat_property_value_of_FlowFM_model__0__is_changed_to_4_due_to_activation_of_Morphology_, ModelName);
            }
        }

        public bool WriteSnappedFeatures
        {
            get { return (bool)GetModelProperty(GuiProperties.WriteSnappedFeatures).Value; }
            set { GetModelProperty(GuiProperties.WriteSnappedFeatures).Value = value; }
        }

        public bool UseMorphologySediment
        {
            get { return (bool)GetModelProperty(GuiProperties.UseMorSed).Value; }
            set { GetModelProperty(GuiProperties.UseMorSed).Value = value; }
        }

        /// <summary>
        /// Retrieves the properties that represent a file location.
        /// </summary>
        public IEnumerable<WaterFlowFMProperty> FileProperties => Properties.Where(x => x.PropertyDefinition.IsFile);
        
        public string MapFileName => GetFileNameFromProperty(KnownProperties.MapFile, ModelName + FileConstants.MapFileExtension);

        public string HisFileName => GetFileNameFromProperty(KnownProperties.HisFile, ModelName + FileConstants.HisFileExtension);

        private string GetFileNameFromProperty(string propertyName, string defaultName)
        {
            WaterFlowFMProperty property = Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.EqualsCaseInsensitive(propertyName));
            string fileName = property != null ? (string)property.Value : defaultName;

            return string.IsNullOrEmpty(fileName) ? defaultName : fileName;
        }

        public string RelativeComFilePath
        {
            get
            {
                var comFileName = ModelName + "_com.nc";
                return Path.Combine(OutputDirectory, comFileName);
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
        public string OutputDirectory
        {
            get
            {
                if (!ContainsProperty(KnownProperties.OutDir))
                {
                    return DirectoryNameConstants.OutputDirectoryName;
                }

                string mduOutputDir = GetModelProperty(KnownProperties.OutDir).GetValueAsString()?.Trim();

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
        public static bool CanSpecifyLayerThicknesses { get { return false; } }
        
        public void SetMduTimePropertiesFromGuiProperties()
        {
            var originalStartTime = GetAbsoluteDateTime((double)GetModelProperty(KnownProperties.TStart).Value, true);
            var originalStopTime = GetAbsoluteDateTime((double)GetModelProperty(KnownProperties.TStop).Value, true);
            var modelStartTime = (DateTime)GetModelProperty(GuiProperties.StartTime).Value;
            var modelStopTime = (DateTime)GetModelProperty(GuiProperties.StopTime).Value;

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

            SetMduIntervalFromGuiProperty(KnownProperties.ClassMapInterval, GuiProperties.WriteClassMapFile, GuiProperties.ClassMapOutputDeltaT);
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
        private void SetMduStartStopDeltaTFromGui(string intervalPropName, string doWritePropName, string deltaTPropName,
                                                  string specifyStartPropName, string startTimePropName, string specifyStopPropName, 
                                                  string stopTimePropName)
        {
            var timeFrame = new List<double>();
            if ((bool)GetModelProperty(doWritePropName).Value)
            {
                double deltaT = ((double)((TimeSpan)GetModelProperty(deltaTPropName).Value).Ticks / TimeSpan.TicksPerSecond);
                if (deltaT > 0)
                {
                    // delta t specified
                    timeFrame.Add(deltaT);
                }
                if ((bool)GetModelProperty(specifyStartPropName).Value)
                {
                    // output start time specified
                    timeFrame.Add(GetRelativeDateTime((DateTime)GetModelProperty(startTimePropName).Value, false));

                    if ((bool)GetModelProperty(specifyStopPropName).Value)
                    {
                        // output stop time specified
                        timeFrame.Add(GetRelativeDateTime((DateTime)GetModelProperty(stopTimePropName).Value, false));
                    }
                }
            }
            else
            {
                timeFrame.Add(0.0);
            }
            GetModelProperty(intervalPropName).Value = timeFrame;
        }

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

            SetGuiStartStopDeltaTFromMdu(KnownProperties.ClassMapInterval, GuiProperties.WriteClassMapFile,
                                         GuiProperties.ClassMapOutputDeltaT);

        }
        
        private void ClearPropertySortIndices()
        {
            Properties.ForEach(p => p.PropertyDefinition.SortIndex = -1);
        }

        private void SetGuiStartStopDeltaTFromMdu(string intervalPropName, string doWritePropName, string deltaTPropName)
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
                    GetModelProperty(deltaTPropName).Value = new TimeSpan(0, 0, 2, 0);
                    GetModelProperty(doWritePropName).Value = true;
                }
                if (intervalPropName == KnownProperties.RstInterval)
                {
                    GetModelProperty(deltaTPropName).Value = new TimeSpan(0, 24, 0, 0);
                    GetModelProperty(doWritePropName).Value = false;
                }
                if (intervalPropName == KnownProperties.WaqInterval)
                {
                    GetModelProperty(deltaTPropName).Value = new TimeSpan(0, 0, 0, 0);
                    GetModelProperty(doWritePropName).Value = true;
                }
                if (intervalPropName == KnownProperties.ClassMapInterval)
                {
                    GetModelProperty(deltaTPropName).Value = new TimeSpan(0, 0, 0, 0);
                    GetModelProperty(doWritePropName).Value = false;
                }
            }

            if (timeFrame.Count > 0)
            {
                // interval is present
                var seconds = (int) Math.Floor(timeFrame[0]);
                var millis = (int) ((timeFrame[0] - seconds)*1000d);
                var interval = new TimeSpan(0, 0, 0, seconds, millis);
                GetModelProperty(deltaTPropName).Value = interval;
                GetModelProperty(doWritePropName).Value = interval.Ticks > 0;
                    // 0 = off (for backward compatibility only)
            }
        }

        private void SetGuiStartStopDeltaTFromMdu(string intervalPropName, string doWritePropName, string deltaTPropName,
            string specifyStartPropName, string startTimePropName, string specifyStopPropName, string stopTimePropName)
        {
            SetGuiStartStopDeltaTFromMdu(intervalPropName, doWritePropName, deltaTPropName);
            var timeFrame = (IList<double>) GetModelProperty(intervalPropName).Value;
            if (timeFrame.Count > 0)
            {

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
        }

        private DateTime GetAbsoluteDateTime(double relativeTime, bool useTUnit)
        {
            var unitString = GetModelProperty(KnownProperties.Tunit).GetValueAsString();

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
                else if (unitString.ToLower().Equals("d"))
                {
                    timeUnitInSeconds = 86400d;
                }
            }
            var ticks = (long)(TimeSpan.TicksPerSecond * relativeTime * timeUnitInSeconds);
            return GetReferenceDateAsDateTime().AddTicks(ticks);
        }

        private double GetRelativeDateTime(DateTime dateTime, bool useTUnit)
        {
            var unitString = GetModelProperty(KnownProperties.Tunit).GetValueAsString();

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
                else if (unitString.ToLower().Equals("d"))
                {
                    numSecondsInTimeStep = 86400;
                }
            }

            double ticks = dateTime.Ticks - GetReferenceDateAsDateTime().Ticks;
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
            UpdateOutputTimesFromSimulationPeriod(GuiProperties.SpecifyWaqOutputStartTime, GuiProperties.WaqOutputStartTime,
                GuiProperties.SpecifyWaqOutputStopTime, GuiProperties.WaqOutputStopTime);/*rstoutput needs to be replaced */
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
            var cdbreakpoints = (IList<double>)breakPointsProperty.Value;
            // Append new values:
            if (cdbreakpoints.Count < icdtyp)
            {
                breakPointsProperty.Value = new List<double>(cdbreakpoints.Concat(Enumerable.Repeat(0.0, icdtyp - cdbreakpoints.Count)));
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
            WriteSnappedFeatures = KnownWriteOutputSnappedFeatures.Any(ws => (bool)GetModelProperty(ws).Value);
        }

        /// <summary>
        /// Update the heat flux model once when loading an mdu file.
        /// Used because the events are off during the load of mdu files.
        /// </summary>
        public void UpdateHeatFluxModel()
        {
            HeatFluxModel.Type = (HeatFluxModelType) ((int) GetModelProperty(KnownProperties.Temperature).Value);
        }

        public void SelectSpatialOperations(IEventedList<IDataItem> dataItems, IEnumerable<string> tracerDefinitions, IEnumerable<string> spatiallyVaryingSedimentDefinitions = null)
        {
            InitialTracerNames.Clear();
            InitialTracerNames.AddRange(tracerDefinitions);
            var sedimentDefinitionList = spatiallyVaryingSedimentDefinitions?.ToList();

            if ((sedimentDefinitionList != null) && sedimentDefinitionList.Any(sd => sd != null))
            {
                InitialSpatiallyVaryingSedimentPropertyNames.Clear();
                InitialSpatiallyVaryingSedimentPropertyNames.AddRange(sedimentDefinitionList);
            }

            SpatialOperations.Clear();

            var combinedSpatialDataItemNames = SpatialDataItemNames.Concat(InitialTracerNames).Concat(InitialSpatiallyVaryingSedimentPropertyNames);
            var dataItemsFound = combinedSpatialDataItemNames.SelectMany(n => dataItems.Where(di => di.Name.StartsWith(n))).ToArray();
            var dataItemsWithConverter = dataItemsFound.Where(d => d.ValueConverter is SpatialOperationSetValueConverter).Distinct().ToList();
            var dataItemsWithOutConverter = dataItemsFound.Except(dataItemsWithConverter).Distinct().ToList();

            foreach (var dataItem in dataItemsWithConverter)
            {
                var spatialOperationValueConverter = (SpatialOperationSetValueConverter) dataItem.ValueConverter;
                if (spatialOperationValueConverter.SpatialOperationSet.Operations.All(SupportedByExtForceFile))
                {
                    // put in everything except spatial operation sets,
                    // because we only use interpolate commands that will grab the importsamplesoperation via the input parameters.
                    var spatialOperations = spatialOperationValueConverter.SpatialOperationSet.Operations
                        .Where(s => !( s is ISpatialOperationSet )).Select(ConvertSpatialOperation)
                        .ToList();

                    SpatialOperations.Add(dataItem.Name, spatialOperations);
                }
                // null check to see if it has a final coverage. It could be that there are only point clouds in the set.
                else if (spatialOperationValueConverter.SpatialOperationSet.Output.Provider != null)
                {
                    // unsupported operations are converted to sample operations that are saved with an xyz file via the model definition.
                    var coverage = spatialOperationValueConverter.SpatialOperationSet.Output.Provider.Features[0] as UnstructuredGridCoverage;

                    // In the event that the coverage is comprised entirely of non-data values, ignore it and continue
                    // (This can happen when exporting spatial operations that comprise of added points but no interpolation
                    // - we're not interested in these for the mdu, they will be saved as dataitems to the dsproj)
                    if (coverage == null || ( coverage.Components[0].NoDataValues != null &&
                    coverage.GetValues<double>().All(v => coverage.Components[0].NoDataValues.Contains(v)) ) &&
                    spatialOperationValueConverter.SpatialOperationSet.Operations.Any(op => !(op is EraseOperation)))
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
                        Log.WarnFormat(Resources.WaterFlowFMModelDefinition_SelectSpatialOperations_Duplication_of_spatial_operations_for__0___Please_verify_the_model_after_saving_, dataItem.Name);
                    }
                    else
                    {
                        SpatialOperations.Add(dataItem.Name, new[] { newOperation });
                    }
                }
            }

            var coverageByType = dataItemsWithOutConverter.Select(di => di.Value).OfType<UnstructuredGridCoverage>().GroupBy(c => c.GetType()).ToList();
            
            var dataItemNameLookup = dataItemsWithOutConverter
                                        .GroupBy(o => o.Name).Select(o => o.FirstOrDefault())   //Removing duplicates.
                                        .ToDictionary(di => di.Value,di => di.Name);

            foreach (var coverageGrouping in coverageByType)
            {
                Coordinate[] coordinates = null;

                foreach (var coverage in coverageGrouping)
                {
                    if (coverage.IsTimeDependent)
                        throw new NotSupportedException("Converting time dependent spatial data to samples is not supported");

                    var component = coverage.Components[0] as IVariable<double>;
                    if (component == null)
                    {
                        throw new NotSupportedException("Converting a non-double valued coverage component to a point cloud is not supported");
                    }

                    var values = component.Values;
                    double? noDataValue = (double?) component.NoDataValue;

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
                                throw new InvalidOperationException("Spatial data is not consistent: number of coordinate does not match number of values");
                        }

                        var coord = coordinates[i];
                        pointCloud.PointValues.Add(new PointValue { X = coord.X, Y = coord.Y, Value = v });
                        i++;
                    }

                    if (pointCloud.PointValues.Count == 0)
                    {
                        continue;
                    }
                                    
                    var pointCloudFeatureProvider = new PointCloudFeatureProvider
                    {
                        PointCloud = pointCloud
                    };

                    var newOperation = new AddSamplesOperation(false) { Name = coverage.Name };
                    newOperation.SetInputData(AddSamplesOperation.SamplesInputName, pointCloudFeatureProvider);

                    if (SpatialOperations.ContainsKey(dataItemNameLookup[coverage]))
                    {
                        Log.WarnFormat(Resources.WaterFlowFMModelDefinition_SelectSpatialOperations_Duplication_of_spatial_operations_for__0___Please_verify_the_model_after_saving_, dataItemNameLookup[coverage]);
                    }
                    else
                    {
                        SpatialOperations.Add(dataItemNameLookup[coverage], new[] {newOperation});
                    }
                }
            }

            foreach (var operations in SpatialOperations.Values)
            {
                NamingHelper.MakeNamesUnique(operations);
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
                return interpolateOperation.GetInput(InterpolateOperation.InputSamplesName).Source.Operation is ImportSamplesOperation;
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
            if (interpolateOperation == null)
            {
                return operation;
            }

            // only write interpolate operations that contain an importsamplesoperation as input samples
            var importSamplesOperation = (ImportSamplesOperation)interpolateOperation.GetInput(InterpolateOperation.InputSamplesName).Source.Operation;

            operation = new ImportSamplesOperationImportData
            {
                Name = importSamplesOperation.Name,
                FilePath = importSamplesOperation.FilePath,
                Enabled = importSamplesOperation.Enabled,
                InterpolationMethod = interpolateOperation.InterpolationMethod,
                AveragingMethod = interpolateOperation.GridCellAveragingMethod,
                RelativeSearchCellSize = interpolateOperation.RelativeSearchCellSize,
                MinSamplePoints = interpolateOperation.MinNumSamples,
                Operand = interpolateOperation.OperationType
            };

            return operation;
        }

        /// <summary>
        /// Gets the name of the tab.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="messageKey">The message key.</param>
        /// <param name="fmModel">The fm model.</param>
        /// <returns></returns>
        public string GetTabName(string key, string messageKey = null, WaterFlowFMModel fmModel = null)
        {
            if (key == KnownProperties.SedFile)
            {
                if (fmModel == null) return String.Empty;

                var useSedFileFlowFmProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.SedFile);
                var guiSedimentGroupId = String.IsNullOrEmpty(useSedFileFlowFmProperty.PropertyDefinition.FileSectionName)
                    ? "sediment"
                    : useSedFileFlowFmProperty.PropertyDefinition.FileSectionName;

                key = guiSedimentGroupId;
                messageKey = "sediment file";
            }

            if (GuiPropertyGroups.ContainsKey(key)) return GuiPropertyGroups[key].Name;

            Log.ErrorFormat(
                Resources.WaterFlowFMModelDefinition_GetTabName_Invalid_gui_group_id_for___0___in_the_scheme_of_dflowfmmorpropertiescsv___1_, messageKey, key);

            return String.Empty;
        }

        public DateTime GetReferenceDateAsDateTime()
        {
            object value = GetModelProperty(KnownProperties.RefDate).Value;
            var refDate = (DateOnly)value;
            return refDate.ToDateTime(TimeOnly.MinValue);
        }

        public void SetReferenceDateFromDatePartOfDateTime(DateTime value)
        {
            GetModelProperty(KnownProperties.RefDate).Value = DateOnly.FromDateTime(value);
        }
    }
}
