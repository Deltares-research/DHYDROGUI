using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Units.Generics;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.IO.LogFileReading;
using DeltaShell.NGHS.Common.Validation;
using DeltaShell.NGHS.IO.FunctionStores;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FixedFiles;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.rr_kernel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    [Entity(FireOnCollectionChange=false)]
    public partial class RainfallRunoffModel : TimeDependentModelBase, IRainfallRunoffAreaUnitManager, IRainfallRunoffModel, IDisposable
    {
        public static readonly short MinGreenhouseYear = 1951;
        public static readonly short MaxGreenhouseYear = 1994;

        private static readonly ILog log = LogManager.GetLogger(typeof(RainfallRunoffModel));
        private readonly DimrRunner runner;

        private readonly RainfallRunoffBasinSynchronizer basinSynchronizer;
        private RainfallRunoffChildDataItemProvider childDataItemProvider;
        private RainfallRunoffModelController modelController;
        private RainfallRunoffOutputSettingData outputSettings;
        private IEventedList<CatchmentModelData> modelData;
        private IEventedList<NwrwDryWeatherFlowDefinition> nwrwDryWeatherFlowDefinitions;
        private IEventedList<NwrwDefinition> nwrwDefinitions;
        private IDimrCoupling dimrCoupling;
        
        public RainfallRunoffModel() : base("Rainfall Runoff")
        {
            AddDataItem(new DrainageBasin {Name = "Basin"}, DataItemRole.Input, RainfallRunoffModelDataSet.BasinTag);

            CapSim = false;
            CapSimInitOption = RainfallRunoffEnums.CapsimInitOptions.AtEquilibriumMoisture;
            CapSimCropAreaOption = RainfallRunoffEnums.CapsimCropAreaOptions.PerCropArea;

            basinSynchronizer = new RainfallRunoffBasinSynchronizer(this);

            // evaporation
            var globalEvaporation = new MeteoData(MeteoDataAggregationType.Cumulative)
                {
                    Name = RainfallRunoffModelDataSet.EvaporationName
                };
            GenerateDefaultEvaporationTimeSeries(globalEvaporation.Data);
            AddDataItem(globalEvaporation, RainfallRunoffModelDataSet.EvaporationName, DataItemRole.Input, RainfallRunoffModelDataSet.EvaporationTag);

            // precipitation
            var globalPrecipitation = new MeteoData(MeteoDataAggregationType.Cumulative)
                {
                    Name = RainfallRunoffModelDataSet.PrecipitationName
                };
            AddDataItem(globalPrecipitation, RainfallRunoffModelDataSet.PrecipitationName, DataItemRole.Input, RainfallRunoffModelDataSet.PrecipitationTag);

            // temperature
            var globalTemperature = new MeteoData(MeteoDataAggregationType.NonCumulative)
                {
                    Name = RainfallRunoffModelDataSet.TemperatureName
                };
            AddDataItem(globalTemperature, RainfallRunoffModelDataSet.TemperatureName, DataItemRole.Input, RainfallRunoffModelDataSet.TemperatureTag);

            // input water level (used by unpaved only)
            var inputWaterLevel = CreateCatchmentCoverage(RainfallRunoffModelDataSet.InputWaterLevelUnpaved, "Input water level", null, true, Basin?.CoordinateSystem);
            inputWaterLevel.Components[0].NoDataValue = RainfallRunoffModelDataSet.UndefinedWaterLevel;
            inputWaterLevel.Components[0].DefaultValue = RainfallRunoffModelDataSet.UndefinedWaterLevel;
            AddDataItem(inputWaterLevel, RainfallRunoffModelDataSet.InputWaterLevelUnpaved, DataItemRole.Input, RainfallRunoffModelDataSet.InputWaterLevelTag);

            // init output settings
            OutputSettings = new RainfallRunoffOutputSettingData();

            // model unit
            var unit = new Parameter<int>
                {
                    Name = "Area Unit",
                    ValueType = typeof (RainfallRunoffEnums.AreaUnit),
                    Description = "Area Unit",
                    Value = (int) RainfallRunoffEnums.AreaUnit.m2,
                    DefaultValue = (int) RainfallRunoffEnums.AreaUnit.m2
                };

            // Minimum filling/storage percentage (Greenhouse)
            var minFillingStoragePercentage = new Parameter<double>
                {
                    Name = "Minimum filling/storage percentage",
                    ValueType = typeof (double),
                    Description = "Minimum filling/storage percentage",
                    Value = 10.0,
                    DefaultValue = 10.0
                };

            // Start active period (evaporation)
            var evaporationStartActivePeriod = new Parameter<int>
                {
                    Name = "Start active period (evaporation)",
                    ValueType = typeof (int),
                    Description = "Start active period (evaporation)",
                    Value = 7,
                    DefaultValue = 7,
                    MinValidValue = 1,
                    MaxValidValue = 24
                };

            // End active period (evaporation)
            var evaporationEndActivePeriod = new Parameter<int>
                {
                    Name = "End active period (evaporation)",
                    ValueType = typeof (int),
                    Description = "End active period (evaporation)",
                    Value = 19,
                    DefaultValue = 19,
                    MinValidValue = 1,
                    MaxValidValue = 24
                };

            ModelData = new EventedList<CatchmentModelData>();
            NwrwDryWeatherFlowDefinitions = new EventedList<NwrwDryWeatherFlowDefinition>()
            {
                NwrwDryWeatherFlowDefinition.CreateDefaultDwaDefinition()
            };
            NwrwDefinitions = NwrwDefinition.CreateDefaultNwrwDefinitions();
            BoundaryData = new EventedList<RunoffBoundaryData>();
            MeteoStations = new EventedList<string>();
            TemperatureStations = new EventedList<string>();

            AddDataItem(unit, RainfallRunoffModelDataSet.AreaUnitName, DataItemRole.Input, RainfallRunoffModelDataSet.AreaUnitTag);
            AddDataItem(minFillingStoragePercentage, DataItemRole.Input, RainfallRunoffModelDataSet.MinimumFillingStoragePercentageTag);
            AddDataItem(evaporationStartActivePeriod, DataItemRole.Input, RainfallRunoffModelDataSet.EvaporationStartActivePeriodTag);
            AddDataItem(evaporationEndActivePeriod, DataItemRole.Input, RainfallRunoffModelDataSet.EvaporationEndActivePeriodTag);

            OutputSettings.BoundaryDischarge.IsEnabled = true;

            FixedFiles = new RainfallRunoffModelFixedFiles(this, AddDataItem);

            ((ICatchmentCoverageMaintainer) new MeteoDataController(this)).Initialize(null);

            if (!WorkFlowTypeValidatorFactory.WorkFlowTypeValidators.OfType<RainfallRunoffInWorkFlowTypeValidatorProvider>().Any())
            {
                WorkFlowTypeValidatorFactory.WorkFlowTypeValidators.Add(new RainfallRunoffInWorkFlowTypeValidatorProvider());
            }
            runner = new DimrRunner(this, new DimrApiFactory());
            OutputFiles = new RainfallRunoffOutputFiles();
            RunLogFiles = new RainfallRunoffRunLogFiles(new ReadFileInTwoMegaBytesChunks(), this);
        }

        /// <summary>
        /// Generate a default global evaporation timeseries
        /// from 01-01-1980 until 01-01-2030.
        /// </summary>
        /// <param name="globalEvaporationData"></param>
        private void GenerateDefaultEvaporationTimeSeries(IFunction globalEvaporationData)
        {
            var timeArgument = globalEvaporationData.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
            if (timeArgument != null)
            {
                var startDate = new DateTime(1980, 01, 01);
                var endDate = new DateTime(2030, 01, 01);
                var dates = new List<DateTime>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    dates.Add(currentDate);
                    currentDate = currentDate.AddYears(1);
                }

                timeArgument.SetValues(dates);
            }
        }

        public Func<string> WorkingDirectoryPathFunc { get; set; } = () => System.IO.Path.Combine(DefaultModelSettings.DefaultDeltaShellWorkingDirectory);

        public override bool IsLinkAllowed(IDataItem source, IDataItem target)
        {
            if (target.Value is IDrainageBasin)
            {
                return source.Value is IDrainageBasin;
            }

            return base.IsLinkAllowed(source, target);
        }

       public RainfallRunoffModelFixedFiles FixedFiles { get; set; }

        public virtual IFeatureCoverage BoundaryDischarge
        {
            get
            {
                return(IFeatureCoverage) OutputCoverages.FirstOrDefault(
                        cov => cov.Components[0].Name ==
                        outputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.BoundaryElmSet).Name);
            }
        }

        public RainfallRunoffModelController ModelController
        {
            get
            {
                return modelController ??
                       (modelController = new RainfallRunoffModelController(this));
            }
            set { modelController = value; }
        }

        [NoNotifyPropertyChange]
        public virtual TimeSpan OutputTimeStep
        {
            get { return OutputSettings.OutputTimeStep; }
            set { OutputSettings.OutputTimeStep = value; }
        }

        public virtual IEnumerable<IFunction> OutputFunctions
        {
            get
            {
                return OutputDataItems
                    .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is IFunction)
                    .Select(di => (IFunction)di.Value);
            }
        }

        public virtual IEnumerable<ICoverage> OutputCoverages
        {
            get { return OutputFunctions.OfType<ICoverage>(); }
        }

        [NoNotifyPropertyChange]
        public IDrainageBasin Basin
        {
            get { return (IDrainageBasin) GetDataItemValueByTag(RainfallRunoffModelDataSet.BasinTag); }
            set
            {
                if (!basinSynchronizer.IsDifferentBasin(value))
                {
                    return;
                }

                GetDataItemByTag(RainfallRunoffModelDataSet.BasinTag).Value = value; //will trigger refresh in syncer
            }
        }
        
        public Dictionary<string, SobekRRLink[]> LateralToCatchmentLookup { get; set; } = new Dictionary<string, SobekRRLink[]>();

        public bool InputWaterLevelIsLinked
        {
            get
            {
                var dataItem = GetDataItemByTag(RainfallRunoffModelDataSet.InputWaterLevelTag);
                return dataItem.LinkedBy.Any() || dataItem.Children.Any(c => c.LinkedTo != null);
            }
        }

        public virtual IFeatureCoverage InputWaterLevel
        {
            get { return (IFeatureCoverage) GetDataItemValueByTag(RainfallRunoffModelDataSet.InputWaterLevelTag); }
        }

        public MeteoData Precipitation
        {
            get { return (MeteoData) GetDataItemValueByTag(RainfallRunoffModelDataSet.PrecipitationTag); }
            private set
            {
                if (value == Precipitation)
                {
                    return;
                }
                GetDataItemByTag(RainfallRunoffModelDataSet.PrecipitationTag).Value = value;
            }
        }

        public MeteoData Evaporation
        {
            get { return (MeteoData)GetDataItemValueByTag(RainfallRunoffModelDataSet.EvaporationTag); }
            private set
            {
                if (value == Evaporation)
                {
                    return;
                }
                GetDataItemByTag(RainfallRunoffModelDataSet.EvaporationTag).Value = value;
            }
        }

        public MeteoData Temperature
        {
            get { return (MeteoData)GetDataItemValueByTag(RainfallRunoffModelDataSet.TemperatureTag); }
            private set
            {
                if (value == Temperature)
                {
                    return;
                }
                GetDataItemByTag(RainfallRunoffModelDataSet.TemperatureTag).Value = value;
            }
        }

        public IEventedList<string> MeteoStations
        {
            get { return meteoStations; }
            set
            {
                if (meteoStations != null)
                {
                    meteoStations.CollectionChanging -= MeteoStationsCollectionChanging; 
                    meteoStations.CollectionChanged -= MeteoStationsCollectionChanged;
                }
                meteoStations = value;
                if (meteoStations != null)
                {
                    meteoStations.CollectionChanging += MeteoStationsCollectionChanging;
                    meteoStations.CollectionChanged += MeteoStationsCollectionChanged;
                }
            }
        }

        void MeteoStationsCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            BubbleCollectionChangingEvent(sender, e);
        }

        void MeteoStationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BubbleCollectionChangedEvent(sender, e);
        }

        public IEventedList<string> TemperatureStations
        {
            get { return temperatureStations; }
            set
            {
                if (temperatureStations != null)
                {
                    temperatureStations.CollectionChanging -= TemperatureStationsCollectionChanging;
                    temperatureStations.CollectionChanged -= TemperatureStationsCollectionChanged;
                }
                temperatureStations = value;
                if (temperatureStations != null)
                {
                    temperatureStations.CollectionChanging += TemperatureStationsCollectionChanging;
                    temperatureStations.CollectionChanged += TemperatureStationsCollectionChanged;
                }
            }
        }

        private void TemperatureStationsCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            BubbleCollectionChangingEvent(sender, e);
        }

        private void TemperatureStationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BubbleCollectionChangedEvent(sender, e);
        }

        public bool ModelNeedsTemperatureData
        {
            get { return Basin.Catchments.Any(c => c.CatchmentType == CatchmentType.Hbv); }
        }

        public double MinimumFillingStoragePercentage
        {
            get { return GetDataItemValueByTag<Parameter<double>>(RainfallRunoffModelDataSet.MinimumFillingStoragePercentageTag).Value; }
            set
            {
                if (Comparer.AlmostEqual2sComplement(value, MinimumFillingStoragePercentage))
                {
                    return;
                }
                GetDataItemValueByTag<Parameter<double>>(RainfallRunoffModelDataSet.MinimumFillingStoragePercentageTag).Value = value;
            }
        }

        [NoNotifyPropertyChange]
        public int EvaporationStartActivePeriod
        {
            get { return GetDataItemValueByTag<Parameter<int>>(RainfallRunoffModelDataSet.EvaporationStartActivePeriodTag).Value; }
            set
            {
                if (value == EvaporationStartActivePeriod)
                {
                    return;
                }
                GetDataItemValueByTag<Parameter<int>>(RainfallRunoffModelDataSet.EvaporationStartActivePeriodTag).Value = value;
            }
        }

        [NoNotifyPropertyChange]
        public int EvaporationEndActivePeriod
        {
            get { return GetDataItemValueByTag<Parameter<int>>(RainfallRunoffModelDataSet.EvaporationEndActivePeriodTag).Value; }
            set
            {
                if (value == EvaporationEndActivePeriod)
                {
                    return;
                }
                GetDataItemValueByTag<Parameter<int>>(RainfallRunoffModelDataSet.EvaporationEndActivePeriodTag).Value = value;
            }
        }

        private RainfallRunoffChildDataItemProvider ChildDataItemProvider
        {
            get { return childDataItemProvider ?? (childDataItemProvider = new RainfallRunoffChildDataItemProvider(this)); }
        }

        public RainfallRunoffOutputSettingData OutputSettings
        {
            get { return outputSettings; }
            set
            {
                if (outputSettings != null)
                {
                    ((INotifyPropertyChanged) OutputSettings).PropertyChanged -= OutputSettingsPropertyChanged;
                }

                outputSettings = value;

                ((INotifyPropertyChanged) OutputSettings).PropertyChanged += OutputSettingsPropertyChanged;
            }
        }

        /// <inheritdoc/>
        public bool CapSim { get; set; }

        /// <summary>
        /// CapSim init option  (At equilibrium moisture, At moisture content pF2, At moisture content pF3)
        /// </summary>
        public RainfallRunoffEnums.CapsimInitOptions CapSimInitOption { get; set; }

        /// <summary>
        /// CapSim for each crop area seperately otherwise CapSim with crop area averaged data once per unpaved area
        /// </summary>
        public RainfallRunoffEnums.CapsimCropAreaOptions CapSimCropAreaOption { get; set; }

        /// <summary>
        /// Historic year for data KasInit and Kasgebr
        /// </summary>
        public short GreenhouseYear { get; set; } = 1994;

        protected virtual void BuildInputWaterLevelCoverage()
        {
            InputWaterLevel.Clear();

            if (IsRunningParallelWithFlow())
            {
                var catchments = GetAllModelData().OfType<UnpavedData>().Select(ud => (IFeature) ud.Catchment).ToList();
                InputWaterLevel.Features = new EventedList<IFeature>(catchments);
                InputWaterLevel.FeatureVariable.AddValues(catchments);
            }
        }

        private readonly string[] ignoreProperties = new[] { "IsEditing", "Dummy" };

        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ClearingOutput)
                return;

            if (ignoreProperties.Contains(e.PropertyName))
                return;

            if (IsBreakingPropertyChangeForOutput(sender, e))
            {
                base.OnInputPropertyChanged(sender, e);
            }
        }

        private bool IsBreakingPropertyChangeForOutput(object sender, PropertyChangedEventArgs e)
        {
            if (Basin == null)
                return false;

            //only clear output if feature geometry was modified
            if (sender is IFeature && e.PropertyName == "Geometry")
                return true;

            return false;
        }

        protected override void OnInputCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ClearingOutput)
                return;

            base.OnInputCollectionChanged(sender, e);
        }

        public IEventedList<IDataItem> OutputDataItems { get; set; } = new EventedList<IDataItem>();

        
        private void AddOutputCoverage(EngineParameter modelParameter)
        {
            if (!modelParameter.IsEnabled)
            {
                return;
            }

            string functionName = modelParameter.Name; 

            if (modelParameter.ElementSet == ElementSet.BoundaryElmSet ||
                modelParameter.ElementSet == ElementSet.LinkElmSet ||
                modelParameter.ElementSet == ElementSet.BalanceNodeElmSet ||
                modelParameter.ElementSet == ElementSet.WWTPElmSet)
            {
                IFeatureCoverage coverage = CreateFeatureCoverage(functionName, modelParameter.Name,
                                                                  modelParameter.Unit, true, Basin?.CoordinateSystem);
                coverage.IsEditable = false;
                coverage.Components[0].NoDataValue = double.NaN;
                if(!OutputDataItems.Any(odi => odi.Tag.Equals(coverage.Name, StringComparison.InvariantCultureIgnoreCase)))
                    OutputDataItems.Add(new DataItem(coverage, DataItemRole.Output, coverage.Name));
            }
            else if (modelParameter.ElementSet == ElementSet.UnpavedElmSet ||
                     modelParameter.ElementSet == ElementSet.PavedElmSet ||
                     modelParameter.ElementSet == ElementSet.GreenhouseElmSet ||
                     modelParameter.ElementSet == ElementSet.OpenWaterElmSet ||
                     modelParameter.ElementSet == ElementSet.SacramentoElmSet ||
                     modelParameter.ElementSet == ElementSet.HbvElmSet ||
                     modelParameter.ElementSet == ElementSet.NWRWElmSet)
            {
                var coverage = CreateCatchmentCoverage(functionName, modelParameter.Name,
                                                                    modelParameter.Unit, true, Basin?.CoordinateSystem);
                coverage.IsEditable = false;
                coverage.Components[0].NoDataValue = double.NaN;

                if (Basin != null)
                {
                    coverage.Features.AddRange(Basin.Catchments);
                }

                if (!OutputDataItems.Any(odi => odi.Tag.Equals(coverage.Name, StringComparison.InvariantCultureIgnoreCase)))
                    OutputDataItems.Add(new DataItem(coverage, DataItemRole.Output, coverage.Name));
            }
            else if (modelParameter.ElementSet == ElementSet.BalanceModelElmSet)
            {
                var timeSeries = new TimeSeries();
                timeSeries.Name = functionName;
                timeSeries.IsEditable = false;
                timeSeries.Components.Add(new Variable<double>(modelParameter.Name, modelParameter.Unit));
                timeSeries.Components[0].NoDataValue = double.NaN;
                
                if (!OutputDataItems.Any(odi => odi.Tag.Equals(timeSeries.Name, StringComparison.InvariantCultureIgnoreCase)))
                    OutputDataItems.Add(new DataItem(timeSeries, DataItemRole.Output, timeSeries.Name));
            }
            else
            {
                throw new NotImplementedException(String.Format("Spatial data for elementset {0} not implemented yet.",
                                                                Enum.GetName(typeof (ElementSet),
                                                                             modelParameter.ElementSet)));
            }
        }
        
        public override string KernelVersions
        {
            get
            {
                var entryAssembly = GetType().Assembly;
                if (entryAssembly == null) return "";
                var file = System.IO.Path.Combine(RRModelEngineDll.RR_DLL_NAME, DimrApiDataSet.RrDllPath);
                if (!File.Exists(DimrApiDataSet.RrDllPath))
                    return "";

                return "Kernel: " + System.IO.Path.GetFileName(RRModelEngineDll.RR_DLL_NAME) + "  " + FileVersionInfo.GetVersionInfo(file).FileVersion;
            }
        }
        
        #region IRainfallRunoffAreaUnitManager Members
        
        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get { return (RainfallRunoffEnums.AreaUnit) GetDataItemValueByTag<Parameter<int>>(RainfallRunoffModelDataSet.AreaUnitTag).Value; }
            set
            {
                if (value == AreaUnit)
                {
                    return;
                }
                AfterAreaUnitSet(value);
            }
        }
        
        [EditAction]
        private void AfterAreaUnitSet(RainfallRunoffEnums.AreaUnit value)
        {
            GetDataItemValueByTag<Parameter<int>>(RainfallRunoffModelDataSet.AreaUnitTag).Value = (int) value;
            if (AreaUnitChanged != null)
            {
                AreaUnitChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler AreaUnitChanged;

        #endregion
        
        public event EventHandler<EventArgs<CatchmentModelData>> ModelDataAdded;
        
        public event EventHandler<EventArgs<CatchmentModelData>> ModelDataRemoved;

        public virtual IHydroRegion Region
        {
            get { return Basin; }
        }

        public bool IsActivityOfEnumType(ModelType type)
        {
            return type == ModelType.DRR;
        }

        public void OnFinishIntegratedModelRun(string hydroModelWorkingDirectoryPath)
        {
            // Nothing to clean up.
        }

        public ISet<string> IgnoredFilePathsWhenCleaningWorkingDirectory => new HashSet<string>();

        public virtual Type SupportedRegionType { get { return typeof (IDrainageBasin); } }

        public IEnumerable<CatchmentModelData> GetAllModelData()
        {
            return ModelData;
        }

        /// <inheritdoc cref="IRainfallRunoffModel.ModelData"/>
        IEnumerable<CatchmentModelData> IRainfallRunoffModel.ModelData
        {
            get { return ModelData; }
        }
        
        public IEventedList<CatchmentModelData> ModelData
        {
            get { return modelData; }
            private set
            {
                if (modelData != null)
                {
                    modelData.CollectionChanging -= ModelDataCollectionChanging;
                    modelData.CollectionChanged -= ModelDataCollectionChanged;
                }
                modelData = value;
                if (modelData != null)
                {
                    modelData.CollectionChanging += ModelDataCollectionChanging;
                    modelData.CollectionChanged += ModelDataCollectionChanged;
                }
            }
        }

        void ModelDataCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            BubbleCollectionChangingEvent(sender, e);
        }

        void ModelDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BubbleCollectionChangedEvent(sender ,e);
        }

        public IEventedList<NwrwDryWeatherFlowDefinition> NwrwDryWeatherFlowDefinitions
        {
            get { return nwrwDryWeatherFlowDefinitions; }
            set
            {
                if (nwrwDryWeatherFlowDefinitions != null)
                {
                    nwrwDryWeatherFlowDefinitions.CollectionChanging -= NwrwDryWeatherFlowDefinitionCollectionChanging;
                    nwrwDryWeatherFlowDefinitions.CollectionChanged -= NwrwDryWeatherFlowDefinitionCollectionChanged;
                }
                nwrwDryWeatherFlowDefinitions = value;
                if (nwrwDryWeatherFlowDefinitions != null)
                {
                    nwrwDryWeatherFlowDefinitions.CollectionChanging += NwrwDryWeatherFlowDefinitionCollectionChanging;
                    nwrwDryWeatherFlowDefinitions.CollectionChanged += NwrwDryWeatherFlowDefinitionCollectionChanged;
                }
            }
        }

        private void NwrwDryWeatherFlowDefinitionCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            BubbleCollectionChangingEvent(sender, e);
        }

        private void NwrwDryWeatherFlowDefinitionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BubbleCollectionChangedEvent(sender, e);
        }

        public IEventedList<NwrwDefinition> NwrwDefinitions
        {
            get { return nwrwDefinitions; }
            set
            {
                if (nwrwDefinitions != null)
                {
                    nwrwDefinitions.CollectionChanging -= NwrwDefinitionCollectionChanging;
                    nwrwDefinitions.CollectionChanged -= NwrwDefinitionCollectionChanged;
                }
                nwrwDefinitions = value;
                if (nwrwDefinitions != null)
                {
                    nwrwDefinitions.CollectionChanging += NwrwDefinitionCollectionChanging;
                    nwrwDefinitions.CollectionChanged += NwrwDefinitionCollectionChanged;
                }
            }
        }

        private void NwrwDefinitionCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            BubbleCollectionChangingEvent(sender, e);
        }

        private void NwrwDefinitionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BubbleCollectionChangedEvent(sender, e);
        }

        public IEventedList<RunoffBoundaryData> BoundaryData
        {
            get { return boundaryData; }
            set
            {
                if (boundaryData != null)
                {
                    boundaryData.CollectionChanging -= BoundaryDataCollectionChanging;
                    boundaryData.CollectionChanged -= BoundaryDataCollectionChanged;
                }
                boundaryData = value;
                if (boundaryData != null)
                {
                    boundaryData.CollectionChanging += BoundaryDataCollectionChanging;
                    boundaryData.CollectionChanged += BoundaryDataCollectionChanged;
                }
            }
        }

        private void BoundaryDataCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            BubbleCollectionChangingEvent(sender, e);
        }

        private void BoundaryDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BubbleCollectionChangedEvent(sender, e);
        }

        public CatchmentModelData GetCatchmentModelData(Catchment catchment)
        {
            return GetAllModelData().FirstOrDefault(cmd => Equals(cmd.Catchment, catchment));
        }

        [EditAction]
        private void OutputSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is EngineParameter && e.PropertyName.Equals(nameof(EngineParameter.IsEnabled)))
            {
                var engineParameter = (EngineParameter) sender;

                SynchronizeOutputSettings(engineParameter);
            }
        }

        private void SynchronizeOutputSettings(EngineParameter engineParameter)
        {
            if (!engineParameter.IsEnabled)
            {
                DataItems.RemoveAllWhere(
                    di =>
                        (di.Role & DataItemRole.Output) == DataItemRole.Output &&
                        IsDataItemCoverageForEngineParameter(di, engineParameter));
            }
            else
            {
                if (
                    !DataItems.Any(
                        di =>
                            (di.Role & DataItemRole.Output) == DataItemRole.Output &&
                            IsDataItemCoverageForEngineParameter(di, engineParameter)))
                {
                    AddOutputCoverage(engineParameter);
                }
            }
        }

        private static bool IsDataItemCoverageForEngineParameter(IDataItem dataItem, EngineParameter engineParameter)
        {
            if (dataItem == null)
                return false;

            var coverage = dataItem.Value as ICoverage;

            if (coverage == null)
                return false;

            return coverage.Components[0].Name == engineParameter.Name;
        }
        
        protected override void OnBeforeDataItemsSet()
        {
            base.OnBeforeDataItemsSet();
            basinSynchronizer.BeforeDataItemsSet();
        }

        protected override void OnAfterDataItemsSet()
        {
            base.OnAfterDataItemsSet();
            basinSynchronizer.AfterDataItemsSet();

            OutputFunctions.ForEach(SetReadOnlyMapHisFileFunctionStoreLookups);
        }

        private static IFeatureCoverage CreateCatchmentCoverage(string name, string valueName = "Value", Unit valueUnit = null, bool timeDependent = false, ICoordinateSystem coordinateSystem = null)
        {
            IFeatureCoverage catchmentCoverage = CreateFeatureCoverage(name, valueName, valueUnit, timeDependent, coordinateSystem);
            catchmentCoverage.Arguments.Last().Name = "Catchment";
            
            return catchmentCoverage;
        }

        private static IFeatureCoverage CreateFeatureCoverage(string name, string valueName = "Value", Unit valueUnit = null, bool timeDependent = false, ICoordinateSystem coordinateSystem = null)
        {
            var featureCoverage = new FeatureCoverage(name)
            {
                CoordinateSystem = coordinateSystem,
                IsTimeDependent = timeDependent
            };

            var argument = new Variable<IFeature>("Feature") {FixedSize = 0};
            featureCoverage.Arguments.Add(argument);
            featureCoverage.Components.Add(new Variable<double>(valueName) {Unit = valueUnit});

            return featureCoverage;
        }
        
        public void FireModelDataAdded(CatchmentModelData catchmentModelData)
        {
            ModelDataAdded?.Invoke(this, new EventArgs<CatchmentModelData>(catchmentModelData));
        }

        public void FireModelDataRemoved(CatchmentModelData removedModelData)
        {
            ModelDataRemoved?.Invoke(this, new EventArgs<CatchmentModelData>(removedModelData));
        }
        
        public override IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            return ChildDataItemProvider.GetChildDataItemLocations(role);
        }

        public override IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            return ChildDataItemProvider.GetChildDataItems(location);
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            return base.GetDirectChildren()
                .Concat(GetRainfallRunoffMDEData())
                .Concat(OutputCoverages
                            .Where(oc => oc.Store is ReadOnlyMapHisFileFunctionStore osFileStore && osFileStore.Functions != null)
                            .SelectMany(oc => ((ReadOnlyMapHisFileFunctionStore)oc.Store).Functions.OfType<IFeatureCoverage>()));
        }

        public Func<IEnumerable<object>> GetRainfallRunoffMDEData { get; set; } = Enumerable.Empty<object>;

        public override DelftTools.Shell.Core.IProjectItem DeepClone()
        {
            var clone = (RainfallRunoffModel) base.DeepClone();

            // refresh basin syncer
            clone.basinSynchronizer.BeforeDataItemsSet();
            clone.basinSynchronizer.AfterDataItemsSet();

            // copy output settings
            clone.OutputSettings = (RainfallRunoffOutputSettingData)OutputSettings.Clone();

            clone.OutputDataItems = new EventedList<IDataItem>(OutputDataItems.Select(odi => (IDataItem)odi.DeepClone()));

            // clone model data
            clone.ModelData = new EventedList<CatchmentModelData>(ModelData.Select(md => (CatchmentModelData) md.Clone()));

            // clone boundary data
            clone.BoundaryData = new EventedList<RunoffBoundaryData>(BoundaryData.Select(bd => (RunoffBoundaryData) bd.Clone()));

            clone.MeteoStations = new EventedList<string>(meteoStations);
            clone.TemperatureStations = new EventedList<string>(temperatureStations);

            RefreshBasinRelatedData(clone, Basin);

            return clone;
        }

        public static void RefreshBasinRelatedData(RainfallRunoffModel clone, IDrainageBasin originalBasin)
        {
            if (Equals(clone.Basin, originalBasin))
                return;

            // replace catchments in model data
            var allCatchments = originalBasin.AllCatchments.ToList();
            var allClonedCatchments = clone.Basin.AllCatchments.ToList();

            if (allCatchments.Count != allClonedCatchments.Count)
                throw new InvalidOperationException("Error during clone: non matching catchments count");

            foreach (var data in clone.GetAllModelData())
            {
                var indexInOriginal = allCatchments.IndexOf(data.Catchment);
                ((ICatchmentSettable) data).CatchmentSetter = allClonedCatchments[indexInOriginal];
            }

            var allBoundaries = originalBasin.Boundaries.ToList();
            var allClonedBoundaries = clone.Basin.Boundaries.ToList();

            if (allBoundaries.Count != allClonedBoundaries.Count)
                throw new InvalidOperationException("Error during clone: non matching catchments count");

            foreach (var data in clone.BoundaryData)
            {
                var indexInOriginal = allBoundaries.IndexOf(data.Boundary);
                data.Boundary = allClonedBoundaries[indexInOriginal];
            }

            // refresh InputWaterLevel
            FeatureCoverage.RefreshAfterClone(clone.InputWaterLevel, originalBasin.AllHydroObjects.OfType<IFeature>(),
                                              clone.Basin.AllHydroObjects.OfType<IFeature>());
            
            // refresh output coverages, hard cast: we want to crash if this is ever not a FeatureCoverage anymore
            foreach (var clonedFeatureCoverage in clone.OutputCoverages.Cast<FeatureCoverage>())
            {
                FeatureCoverage.RefreshAfterClone(clonedFeatureCoverage,
                                                  originalBasin.GetAllItemsRecursive().OfType<IFeature>(),
                                                  clone.Basin.GetAllItemsRecursive().OfType<IFeature>());
            }
        }

        #region IStateAwareModelEngine

        protected virtual bool ClearingOutput { get; set; }
        private IEventedList<RunoffBoundaryData> boundaryData;
        private IEventedList<string> meteoStations;
        private IEventedList<string> temperatureStations; 
        
        #region Save State: Time Range

        public virtual bool UseSaveStateTimeRange { get; set; }

        public virtual DateTime SaveStateStartTime { get; set; }

        public virtual DateTime SaveStateStopTime { get; set; }

        public virtual TimeSpan SaveStateTimeStep { get; set; }

        #endregion

        
        #endregion

        public bool IsRunningParallelWithFlow()
        {
            var owner = Owner as ICompositeActivity;
            if (owner == null)
                return false;

            var simtaneousActivities = owner.GetActivitiesRunningSimultaneous(this);
            return simtaneousActivities.Any();
        }
        #region Implementation of IDimrModel

        public virtual string LibraryName { get { return "rr_dll"; } }
        public virtual string InputFile { get { return "Sobek_3b.fnm"; } }
        public virtual string DirectoryName { get { return "rr"; } }
        public virtual bool IsMasterTimeStep
        {
            get
            {
                if (Owner == null) return true;

                var parent = Owner as ICompositeActivity;
                if (parent == null) return true;
                var otherDimrModels = parent.Activities.OfType<IDimrModel>()
                    .Where(dm => !(dm is RainfallRunoffModel));

                return otherDimrModels.Count(dm => dm.IsMasterTimeStep) == 0;
            }
        }

        public virtual string ShortName { get { return "rr"; } }

        public string DimrModelRelativeOutputDirectory => DirectoryName;

        public virtual string GetItemString(IDataItem value)
        {
            return null;
        }

        public virtual Type ExporterType { get { return typeof(RainfallRunoffModelExporter); } }
        public virtual string GetExporterPath(string directoryName)
        {
            return directoryName;
        }

        public virtual string KernelDirectoryLocation
        {
            get { return DimrApiDataSet.RrDllPath; }
        }
        public virtual void DisconnectOutput()
        {
            ClearOutput();
        }

        public virtual void ConnectOutput(string outputPath)
        {
            OutputFiles.SetDirectory(outputPath);
            OutputFunctions.ForEach(ChangeToReadOnlyMapHisFileFunctionStore);
            OutputFunctions.ForEach(SetReadOnlyMapHisFileFunctionStoreLookups);
            SetPathsOfFunctionStores(outputPath);
            RunLogFiles.ConnectLoggingFiles(outputPath);
            OutputIsEmpty = false;
        }

        /// <summary>
        /// The output files of the model.
        /// </summary>
        public RainfallRunoffOutputFiles OutputFiles { get; }
        
        /// <summary>
        /// Visualizing the log files produced by running the model is handled in this object.
        /// </summary>
        private RainfallRunoffRunLogFiles RunLogFiles { get; }

        public virtual void RestoreOutputSettings()
        {
            foreach (EngineParameter engineParameter in OutputSettings.EngineParameters)
            {
                SynchronizeOutputSettings(engineParameter);
            }
        }

        public virtual string WorkingDirectory
        {
            get { return System.IO.Path.Combine(WorkingDirectoryPathFunc(), Name); }
        }

        public virtual ValidationReport Validate()
        {
            return RainfallRunoffModelValidator.Validate(this);
        }

        public new virtual ActivityStatus Status
        {
            get { return base.Status; }
            set { base.Status = value; }
        }

        [EditAction]
        public virtual bool RunsInIntegratedModel { get; set; }

        public string DimrExportDirectoryPath => WorkingDirectory;

        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get { return base.CurrentTime; }
            set
            {
                base.CurrentTime = value;
                base.OnProgressChanged();
            }
        }
        public virtual Array GetVar(string category, string itemName = null, string parameter = null)
        {
            return runner.GetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter));
        }
        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            runner.SetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter), values);
        }
        public virtual bool CanRunParallel { get { return false; } }
        public virtual string MpiCommunicatorString { get { return null; } }

        #endregion



        protected virtual void SetReadOnlyMapHisFileFunctionStoreLookups(IFunction function)
        {
            var store = function.Store as ReadOnlyMapHisFileFunctionStore;
            if (store == null) return;

            store.GetParameterName = n => RainfallRunoffModelParameterHisFileMapping.HisFileParameterLookup[n].ParameterName;
            var featureCoverage = function as FeatureCoverage;
            if (featureCoverage == null) return;

            store.LocationsFromStringToObject = s =>
            {
                if (GetElementSetForCoverage(featureCoverage) == ElementSet.BoundaryElmSet)
                {
                    var boundarySufix = "_boundary";
                    if (s.EndsWith(boundarySufix))
                    {
                        s = s.Replace(boundarySufix, "");
                    }
                }
                return featureCoverage.Features.OfType<INameable>().FirstOrDefault(n => n.Name == s);
            };
            store.LocationFromObjectToString = f =>
            {
                var sufix = GetElementSetForCoverage(featureCoverage) == ElementSet.BoundaryElmSet
                    ? "_boundary"
                    : "";

                var fullName = ( (INameable) f ).Name + sufix;
                return fullName.Truncate(20); // only 20 characters are allowed in his file
            };
        }

        private ElementSet? GetElementSetForCoverage(IFunction function)
        {
            var engineParameters = OutputSettings.EngineParameters.Where(ep => ep.IsEnabled).ToList();
            var parameter = engineParameters.FirstOrDefault(p => p.Name == function.Components[0].Name);

            return parameter == null ? (ElementSet?) null : parameter.ElementSet;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            // Ensure all stores are closed
            var fileStores = AllDataItems.Where(di => di.LinkedTo == null && di.ValueType.Implements(typeof(IFunction)))
                    .Select(di => di.Value).OfType<IFunction>()
                    .Select(nc => nc.Store).OfType<IFileBased>();

            foreach (var fileStore in fileStores)
            {
                fileStore.Close();
            }

            if (modelController == null) return;

            try
            {
                modelController.Cleanup();
                modelController = null;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Could not dispose model engine : {0}", ex.Message);
            }
        }

        public virtual void ChangeToReadOnlyMapHisFileFunctionStore(IFunction outputFunction)
        {
            var ncStore = outputFunction.Store as NetCdfFunctionStore;

            outputFunction.Store = new ReadOnlyMapHisFileFunctionStore
            {
                // set functions needed for storage (nhibernate)
                Functions = new EventedList<IFunction>(outputFunction.Arguments.Concat(outputFunction.Components).Plus(outputFunction))
            };
            SetReadOnlyMapHisFileFunctionStoreLookups(outputFunction);

            if (ncStore != null)
            {
                // remove old temporary nc file
                ncStore.Dispose();
                FileUtils.DeleteIfExists(ncStore.Path);
            }
        }

        public virtual void SetPathsOfFunctionStores(string workingDir)
        {
            var functionLookup = OutputFunctions.ToDictionary(f => f.Name);

            OutputSettings.EngineParameters.Where(ep => ep.IsEnabled).ForEach(parameter =>
            {
                if (functionLookup.ContainsKey(parameter.Name))
                {
                    IFunction function = functionLookup[parameter.Name];
                    string fileName = RainfallRunoffModelParameterHisFileMapping.HisFileParameterLookup[parameter.Name].HisFileName;

                    var readOnlyMapHisFileFunctionStore = function.Store as ReadOnlyMapHisFileFunctionStore;
                    if (readOnlyMapHisFileFunctionStore != null)
                    {
                        var path = System.IO.Path.Combine(workingDir, fileName);
                        readOnlyMapHisFileFunctionStore.Path = File.Exists(path) ? path : null;
                    }
                }
            });
        }
        #region TimeDependentModelBase
        protected override void OnInitialize()
        {
            if (RunsInIntegratedModel) return;

            BuildInputWaterLevelCoverage();
            runner.OnInitialize();
            RunLogFiles.Clear();
        }
        protected override void OnProgressChanged()
        {
            runner.OnProgressChanged();
        }
        protected override void OnExecute()
        {
            runner.OnExecute();
        }
        protected override void OnFinish()
        {
            runner.OnFinish();
        }
        protected override void OnCleanup()
        {
            runner.OnCleanup();
        }
        protected override void OnClearOutput()
        {
            if (SuspendClearOutputOnInputChange)
                return;

            ClearingOutput = true;

            try
            {
                OutputOutOfSync = false;

                foreach (var coverage in OutputCoverages)
                {
                    coverage.Filters.Clear();
                    var hisStore = coverage.Store as ReadOnlyMapHisFileFunctionStore;
                    if (hisStore != null)
                    {
                        hisStore.Close();
                        hisStore.Path = null;
                    }
                    else
                    {
                        coverage.Clear();
                    }
                }

                OutputFiles.Clear();
            }
            finally
            {
                ClearingOutput = false;
            }
            
            RunLogFiles.Clear();
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RainfallRunoffModel()
        {
            Dispose(false);
        }
        
        public virtual IEnumerable<IDataItem> GetDataItemsByItemString(string itemString, string itemString2)
        {
            yield break;
        }

        /// <summary>
        /// The dimr coupling for this <see cref="RainfallRunoffModel"/>.
        /// </summary>
        /// <remarks>
        /// Always returns an up-to-date <see cref="IDimrCoupling"/>.
        /// Does not return <c>null</c>.
        /// </remarks>
        public IDimrCoupling DimrCoupling
        {
            get
            {
                if (dimrCoupling == null || dimrCoupling.HasEnded)
                {
                    dimrCoupling = new RainfallRunoffDimrCoupling(Basin, LateralToCatchmentLookup);
                    return dimrCoupling;
                }

                return dimrCoupling;
            }
        }
    }
}