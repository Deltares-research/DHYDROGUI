using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using BasicModelInterface;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Shell.Core.Workflow.Restart;
using DelftTools.Units;
using DelftTools.Units.Generics;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;
using log4net;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Features.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    //TODO : this class is too big..split it up
    [Entity(FireOnCollectionChange=false)]
    public class WaterFlowModel1D : TimeDependentModelBase, IDisposable, ICloneable, IHydroModel, IDimrStateAwareModel, IHydFileModel, IModelMerge, IDimrModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1D));
        internal static readonly string SalinityFileName = "Salinity.ini";
        public static readonly string SobekLogfileDataItemTag = "SobekLog";

        private readonly DimrRunner runner;
        /// <summary>
        /// Data structure to hold the output settings which can be changed through the property grid in the gui 
        /// </summary>
        private WaterFlowModel1DOutputSettingData outputSettings;
        private IList<ModelApiParameter> parameterSettings;
        public static string TemplateDataZipFile { get; private set; }

        private string workDirectory; // model engine work directory
        private string previousWorkDirectory;

        private List<WaterFlowModel1DBoundaryNodeData> boundaryConditionDataList;

        // ordered lists of features as used in outputcoverages and by modelapi; allows to store arrays from modelapi to featurecoverage
        private List<IStructure1D> structureMappingToModelApi;
        private List<IObservationPoint> observationPointsMappingToModelApi;
        private List<ILateralSource> lateralSourcesMappingToModelApi;
        private List<IRetention> retentionMappingToModelApi;
        private List<IPump> pumpMappingToModelApi;
        
        private bool useMorphology;
        private bool additionalMorphologyOutput;

        private readonly bool created;
        private InitialConditionsType initialConditionsType;

        public WaterFlowModel1D()
            : this("Flow1D")
        {
        }

        public WaterFlowModel1D(string name)
            : base(name)
        {
            // network
            var network = new HydroNetwork { Name = "Network" };
            AddDataItem(network, DataItemRole.Input, WaterFlowModel1DDataSet.NetworkTag);

            // grid
            var networkDiscretization = new Discretization { Network = network, Name = WaterFlowModel1DDataSet.DiscretizationDataObjectName };
            AddDataItem(networkDiscretization, DataItemRole.Input, WaterFlowModel1DDataSet.NetworkDiscretizationTag);

            // q's supplied by externals
            AddInflowsDataItem();

            // init output settings
            outputSettings = new WaterFlowModel1DOutputSettingData();
            
            ((INotifyPropertyChanged)outputSettings).PropertyChanged += OutputSettingsPropertyChanged;

            outputSettings.LocationWaterLevel = AggregationOptions.Current;
            outputSettings.BranchDischarge = AggregationOptions.Current;
            outputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Observations).AggregationOptions =
                AggregationOptions.Current;
            outputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Observations).AggregationOptions =
                AggregationOptions.Current;

            var useSaltParameter = new Parameter<bool>(WaterFlowModel1DDataSet.UseSaltParameterTag) { Value = false };
            AddDataItem(useSaltParameter, DataItemRole.Input, WaterFlowModel1DDataSet.UseSaltParameterTag);

            // 1D Morphology
            useMorphology = false;
            additionalMorphologyOutput = false;
            AddMorphologyFileDataItem();
            AddBcmFileDataItem();
            AddSedimentFileDataItem();
            AddTraFileDataItem();
            
            var useReverseRoughnessParameter = new Parameter<bool>(WaterFlowModel1DDataSet.UseReverseRoughnessParameterTag) { Value = false };
            AddDataItem(useReverseRoughnessParameter, DataItemRole.Input, WaterFlowModel1DDataSet.UseReverseRoughnessParameterTag);

            var useReverseRoughnessInCalculationParameter = new Parameter<bool>(WaterFlowModel1DDataSet.UseReverseRoughnessInCalculationParameterTag) { Value = false };
            AddDataItem(useReverseRoughnessInCalculationParameter, DataItemRole.Input, WaterFlowModel1DDataSet.UseReverseRoughnessInCalculationParameterTag);

            initialConditionsType = InitialConditionsType.Depth;
            var initialConditionsTypeParameter = new Parameter<string>(WaterFlowModel1DDataSet.InitialConditionsTypeTag) { Value = InitialConditionsType.Depth.ToString() };
            AddDataItem(initialConditionsTypeParameter, DataItemRole.Input, WaterFlowModel1DDataSet.InitialConditionsTypeTag);

            AddDispersionFormulationTypeDataItem();

            var defaultInitialWaterLevelParameter = new Parameter<double>(WaterFlowModel1DDataSet.DefaultInitialWaterLevelTag) { Value = 0 };
            AddDataItem(defaultInitialWaterLevelParameter, DataItemRole.Input, WaterFlowModel1DDataSet.DefaultInitialWaterLevelTag);

            var defaultInitialDepthParameter = new Parameter<double>(WaterFlowModel1DDataSet.DefaultInitialDepthTag) { Value = 0 };
            AddDataItem(defaultInitialDepthParameter, DataItemRole.Input, WaterFlowModel1DDataSet.DefaultInitialDepthTag);

            var useSaltInCalculationParameter = new Parameter<bool>(WaterFlowModel1DDataSet.UseSaltInCalculationParameterTag) { Value = false };
            AddDataItem(useSaltInCalculationParameter, DataItemRole.Input, WaterFlowModel1DDataSet.UseSaltInCalculationParameterTag);

            var initialFlow = new NetworkCoverage("Initial Water Flow", false, "Water Flow", "m³/s") { Network = network };
            initialFlow.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterDischarge;
            initialFlow.Locations.ExtrapolationType = ExtrapolationType.Constant;
            AddDataItem(initialFlow, DataItemRole.Input, WaterFlowModel1DDataSet.InputInitialFlowTag);

            var initialDepth = new NetworkCoverage("Initial Water Depth", false, "Water Depth", "m") { Network = network };
            initialDepth.Locations.ExtrapolationType = ExtrapolationType.Constant;
            AddDataItem(initialDepth, DataItemRole.Input, WaterFlowModel1DDataSet.InputInitialConditionsTag);
            SetAttributeInitialDepth();

            var wind = new WindFunction("Wind");
            AddDataItem(wind, DataItemRole.Input, WaterFlowModel1DDataSet.InputWindTag);

            var windShielding = new NetworkCoverage("Wind Shielding", false, "Wind Shielding", "-") { Network = network, DefaultValue = 1.0 };
            windShielding.Locations.ExtrapolationType = ExtrapolationType.Constant;
            AddDataItem(windShielding, DataItemRole.Input, WaterFlowModel1DDataSet.WindShieldingTag);

            AddUseTemperatureParameterDataItem();
            AddTemperatureModelTypeDataItem();
            AddDensityTypeDataItem();
            AddTemperatureMeteoDataDataItem();

            SurfaceArea = surfaceAreaDefault;
            AtmosphericPressure = atmosphericPressureDefault;
            DaltonNumber = daltonNumberDefault;
            StantonNumber = stantonNumberDefault;
            HeatCapacityWater = heatCapacityWaterDefault;
            Latitude = latitudeDefault;
            Longitude = longitudDefault;

            /* */
            var boundaryNodeDataItemSet = new DataItemSet(new EventedList<WaterFlowModel1DBoundaryNodeData>(), WaterFlowModel1DDataSet.BoundaryConditionsTag, DataItemRole.Input, true, WaterFlowModel1DDataSet.BoundaryConditionsTag, typeof(WaterFlowModel1DBoundaryNodeData))
                {
                    ValueType = typeof(FeatureData<IFunction, INode>)
                };
            dataItems.Add(boundaryNodeDataItemSet);

            var lateralSourceDataItemSet = new DataItemSet(new EventedList<WaterFlowModel1DLateralSourceData>(), WaterFlowModel1DDataSet.LateralSourcesDataTag, DataItemRole.Input, true, WaterFlowModel1DDataSet.LateralSourcesDataTag, typeof(WaterFlowModel1DLateralSourceData));
            dataItems.Add(lateralSourceDataItemSet);

            AddDataItemSet(new EventedList<RoughnessSection>(), WaterFlowModel1DDataSet.RoughnessSectionsTag,
                           DataItemRole.Input, WaterFlowModel1DDataSet.RoughnessSectionsTag);

            UpdateRoughnessSections();
            // Initialize model settings to some usefull default settings;
            // calculation period 1 dat, calculation step 10 minutes.
            //var outputTimeStep = new Parameter<TimeSpan>(OutputTimeStepTag) {Value = TimeStep};
            //AddDataItem(outputTimeStep, DataItemRole.Input, OutputTimeStepTag, true);

            SubscribeToNetwork();
            SubscribeToInitialConditionsCoverageDataItem();

            parameterSettings = GetParametersFromModelApi();

            ((INotifyPropertyChanged)this).PropertyChanged += WaterFlowModel1DPropertyChanged;

            enableUglyFewsHack = Environment.GetEnvironmentVariable("UGLY_FEWS_HACK") == "true";
            runner = new DimrRunner(this);
            created = true;
        }

        private void AddDispersionFormulationTypeDataItem()
        {
            var dispersionFormulationsTypeParameter = new Parameter<string>(WaterFlowModel1DDataSet.DispersionFormulationTypeTag)
            {
                Value = DispersionFormulationType.Constant.ToString()
            };
            AddDataItem(dispersionFormulationsTypeParameter, DataItemRole.Input, WaterFlowModel1DDataSet.DispersionFormulationTypeTag);
        }

        private void AddUseTemperatureParameterDataItem()
        {
            var useTemperatureParameter = new Parameter<bool>(WaterFlowModel1DDataSet.UseTemperatureParameterTag) { Value = false };
            AddDataItem(useTemperatureParameter, DataItemRole.Input, WaterFlowModel1DDataSet.UseTemperatureParameterTag);
        }

        private void AddTemperatureModelTypeDataItem()
        {
            var temperatureModelTypeParameter = new Parameter<string>(WaterFlowModel1DDataSet.TemperatureModelTypeTag)
            {
                Value = TemperatureModelType.Transport.ToString()
            };
            AddDataItem(temperatureModelTypeParameter, DataItemRole.Input, WaterFlowModel1DDataSet.TemperatureModelTypeTag);
        }

        private void AddDensityTypeDataItem()
        {
            var densityTypeParameter = new Parameter<string>(WaterFlowModel1DDataSet.DensityTypeTag)
            {
                Value = DensityType.eckart_modified.ToString()
            };
            AddDataItem(densityTypeParameter, DataItemRole.Input, WaterFlowModel1DDataSet.DensityTypeTag);
        }

        private void AddTemperatureMeteoDataDataItem()
        {
            var meteo = new MeteoFunction(WaterFlowModel1DDataSet.InputMeteoDataTag);
            AddDataItem(meteo, DataItemRole.Input, WaterFlowModel1DDataSet.InputMeteoDataTag);
        }
        
        private void AddMorphologyFileDataItem()
        {
            var morphologyFile = new WaterFlowModel1DMorphologyFile() {Path = string.Empty };
            AddDataItem(morphologyFile, DataItemRole.None, WaterFlowModel1DDataSet.MorphologyFileDataObjectTag);
        }

        private void AddBcmFileDataItem()
        {
            var bcmFile = new WaterFlowModel1DMorphologyFile() { Path = string.Empty };
            AddDataItem(bcmFile, DataItemRole.None, WaterFlowModel1DDataSet.BcmFileDataObjectTag);
        }

        private void AddSedimentFileDataItem()
        {
            var sedimentFile = new WaterFlowModel1DMorphologyFile() { Path = string.Empty };
            AddDataItem(sedimentFile, DataItemRole.None, WaterFlowModel1DDataSet.SedimentFileDataObjectTag);
        }

        private void AddTraFileDataItem()
        {
            var traFile = new WaterFlowModel1DMorphologyFile() { Path = string.Empty };
            AddDataItem(traFile, DataItemRole.None, WaterFlowModel1DDataSet.TraFileDataObjectTag);
        }

        void WaterFlowModel1DPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // deal with undo/redo of linking..yes..I know
            if (EditActionSettings.Disabled) //this makes sure this ONLY works DURING undo/redo
            {
                if ((e.PropertyName == "Value" || e.PropertyName == "LinkedTo") &&
                    Equals(sender, GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag)))
                {
                    UnSubscribeFromNetwork();
                    SubscribeToNetwork();
                }
            }
        }

        private void AddInflowsDataItem()
        {
            var inflows = new FeatureCoverage("Inflows");
            inflows.Arguments.Add(new Variable<DateTime>()); //time variable
            inflows.Arguments.Add(new Variable<IFeature> {IsAutoSorted = false}); //feature variable
            inflows.Components.Add(new Variable<double>("Inflows", new Unit("Discharge", "m³/s"))); //component
            AddDataItem(inflows, DataItemRole.Input, WaterFlowModel1DDataSet.InflowsTag);
        }

        public override IEnumerable<IDataItem> AllDataItems
        {
            get
            {
                var boundaryConditionDataItems = BoundaryConditions.Select(bc => bc.SeriesDataItem);
                var lateralDataItems = LateralSourceData.Select(d => d.SeriesDataItem);

                return base.AllDataItems.Concat(boundaryConditionDataItems).Concat(lateralDataItems);
            }
        }

        public override bool IsDataItemActive(IDataItem dataItem)
        {
            if (dataItem.Tag == WaterFlowModel1DDataSet.InputInitialConditionsTag || 
                dataItem.Tag == WaterFlowModel1DDataSet.InputInitialFlowTag ||
                dataItem.Tag == WaterFlowModel1DDataSet.InputInitialSaltConcentrationTag)
            {
                return !UseRestart;
            }
            return base.IsDataItemActive(dataItem);
        }
        
        public virtual INetworkCoverage InitialFlow //Q(cell)
        {
            get { return GetNetworkCoverageByTag(WaterFlowModel1DDataSet.InputInitialFlowTag); }
        }

        public virtual INetworkCoverage WindShielding
        {
            get { return GetNetworkCoverageByTag(WaterFlowModel1DDataSet.WindShieldingTag); }
        }

        public virtual INetworkCoverage InitialSaltConcentration
        {
            get
            {
                var initialSaltDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InputInitialSaltConcentrationTag);
                return initialSaltDataItem != null ? (INetworkCoverage)initialSaltDataItem.Value : null;
            }
        }

        public virtual INetworkCoverage InitialTemperature
        {
            get
            {
                var initialTemperatureDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InputInitialTemperatureTag);
                return initialTemperatureDataItem != null ? (INetworkCoverage)initialTemperatureDataItem.Value : null;
            }
        }

        public virtual INetworkCoverage DispersionCoverage
        {
            get
            {
                var dispersionDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InputDispersionCoverageTag);
                return dispersionDataItem != null ? (INetworkCoverage)dispersionDataItem.Value : null;
            }
        }

        public virtual INetworkCoverage DispersionF3Coverage
        {
            get
            {
                var dispersionF3DataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InputDispersionF3CoverageTag);
                return dispersionF3DataItem != null ? (INetworkCoverage)dispersionF3DataItem.Value : null;
            }
        }

        public virtual INetworkCoverage DispersionF4Coverage
        {
            get
            {
                var dispersionF4DataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InputDispersionF4CoverageTag);
                return dispersionF4DataItem != null ? (INetworkCoverage)dispersionF4DataItem.Value : null;
            }
        }

        /// <summary>
        /// Can be depth or level based on the initial conditions type
        /// </summary>
        public virtual INetworkCoverage InitialConditions //D(cell)
        {
            get { return GetNetworkCoverageByTag(WaterFlowModel1DDataSet.InputInitialConditionsTag); }
        }

        /// <summary>
        /// Gets the roughness sections for this model
        /// </summary>
        public virtual IEventedList<RoughnessSection> RoughnessSections
        {
            get { return GetDataItemSetByTag(WaterFlowModel1DDataSet.RoughnessSectionsTag).AsEventedList<RoughnessSection>(); }
        }

        /// <summary>
        /// Gets the boundary conditions for this model
        /// </summary>
        public virtual IEventedList<WaterFlowModel1DBoundaryNodeData> BoundaryConditions
        {
            get { return GetDataItemSetByTag(WaterFlowModel1DDataSet.BoundaryConditionsTag).AsEventedList<WaterFlowModel1DBoundaryNodeData>(); }
        }
        
        /// <summary>
        /// Gets the boundary conditions data item set for this model
        /// </summary>
        public virtual IDataItemSet BoundaryConditionsDataItemSet
        {
            get { return GetDataItemSetByTag(WaterFlowModel1DDataSet.BoundaryConditionsTag); }
        }

        /// <summary>
        /// Gets the lateral source data for this model
        /// </summary>
        public virtual IEventedList<WaterFlowModel1DLateralSourceData> LateralSourceData
        {
            get { return GetDataItemSetByTag(WaterFlowModel1DDataSet.LateralSourcesDataTag).AsEventedList<WaterFlowModel1DLateralSourceData>(); }
        }

        /// <summary>
        /// Gets the lateral sources data item set for this model
        /// </summary>
        public virtual IDataItemSet LateralSourcesDataItemSet
        {
            get { return GetDataItemSetByTag(WaterFlowModel1DDataSet.LateralSourcesDataTag); }
        }

        public virtual INetworkCoverage OutputFlow
        {
            get { return (INetworkCoverage)RetrieveOutputFunctionByDataItemTag(WaterFlowModelParameterNames.BranchDischarge); }
        } 

        public virtual INetworkCoverage OutputDepth
        {
            get { return (INetworkCoverage)RetrieveOutputFunctionByDataItemTag(WaterFlowModelParameterNames.LocationWaterDepth); }
        }

        public virtual INetworkCoverage OutputWaterLevel
        {
            get { return (INetworkCoverage)RetrieveOutputFunctionByDataItemTag(WaterFlowModelParameterNames.LocationWaterLevel); }
        }

        public virtual INetworkCoverage OutputVelocity
        {
            get { return (INetworkCoverage)RetrieveOutputFunctionByDataItemTag(WaterFlowModelParameterNames.BranchVelocity); }
        }

        [NoNotifyPropertyChange]
        public virtual IFeatureCoverage Inflows
        {
            get { return (IFeatureCoverage)GetDataItemValueByTag(WaterFlowModel1DDataSet.InflowsTag); }
        }

        [NoNotifyPropertyChange]
        public virtual IDiscretization NetworkDiscretization
        {
            get { return (IDiscretization)GetDataItemValueByTag(WaterFlowModel1DDataSet.NetworkDiscretizationTag); }
            set { GetDataItemByTag(WaterFlowModel1DDataSet.NetworkDiscretizationTag).Value = value; }
        }

        private const double Epsilon = 1e-3;

        public static IDiscretization CreateEnergyHeadDiscretization(IDiscretization discretization)
        {
            var locations = new List<INetworkLocation>();
            var network = discretization.Network;
            INetworkLocation previousLocation = null;
            foreach (var location in discretization.Locations.Values)
            {
                var branch = location.Branch;
                var chainage = location.Chainage;
                if (previousLocation != null && Equals(previousLocation.Branch, branch))
                {
                    locations.Add(new NetworkLocation(branch, 0.5*(previousLocation.Chainage + chainage))
                        {
                            Network = network
                        });
                }
                if (branch == null || Math.Abs(chainage) < double.Epsilon || Math.Abs(chainage - branch.Length) < double.Epsilon)
                {
                    locations.Add((INetworkLocation)location.Clone());
                }
                else
                {
                    locations.Add(new NetworkLocation(branch, chainage - Epsilon)
                        {
                            Network = network,
                            Name = location.Name + "-"
                        });
                    locations.Add(new NetworkLocation(branch, chainage + Epsilon)
                        {
                            Network = network,
                            Name = location.Name + "+"
                        });
                }
                previousLocation = location;
            }

            var result = new Discretization();
            result.Locations.AddValues(locations);
            return result;
        }

        [NoNotifyPropertyChange]
        public virtual IHydroNetwork Network
        {
            get { return (IHydroNetwork)GetDataItemValueByTag(WaterFlowModel1DDataSet.NetworkTag); }
            set
            {
                if (value == Network)
                {
                    return;
                }

                UnSubscribeFromNetwork();

                GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag).Value = value;

                SubscribeToNetwork();

                // refresh data
                RefreshNetworkRelatedData();
            }
        }
        public virtual WindFunction Wind
        {
            get { return (WindFunction)GetDataItemValueByTag(WaterFlowModel1DDataSet.InputWindTag); }
        }
        
        public virtual MeteoFunction MeteoData
        {
            get { return (MeteoFunction)GetDataItemValueByTag(WaterFlowModel1DDataSet.InputMeteoDataTag); }
        }

        public virtual IEnumerable<IFunction> OutputFunctions
        {
            get
            {
                return
                    DataItems.Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is IFunction).Select(di => (IFunction)di.Value);
            }
        }

        [NoNotifyPropertyChange]
        public virtual bool UseReverseRoughnessInCalculation
        {
            get { return GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseReverseRoughnessInCalculationParameterTag).Value; }
            set
            {
                var oldValue = GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseReverseRoughnessInCalculationParameterTag).Value;
                if (oldValue != value)
                {
                    BeginEdit(new DefaultEditAction(String.Format("Setting UseReverseRoughnessInCalculation to {0}", value)));
                    GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseReverseRoughnessInCalculationParameterTag).Value = value;
                    AfterUseReverseRoughnessInCalculationSet(value);
                    EndEdit();
                }
            }
        }

        private void AfterUseReverseRoughnessInCalculationSet(bool value)
        {
            RoughnessSections.OfType<ReverseRoughnessSection>().ForEach(rrs => rrs.UseNormalRoughness = !value);
        }

        [NoNotifyPropertyChange]
        public virtual bool UseReverseRoughness
        {
            get { return GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseReverseRoughnessParameterTag).Value; }
            set
            {
                var oldValue = GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseReverseRoughnessParameterTag).Value;
                if (oldValue != value)
                {
                    BeginEdit(new DefaultEditAction(String.Format("Setting UseReverseRoughness to {0}", value)));
                    GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseReverseRoughnessParameterTag).Value = value;
                    AfterUseReverseRoughnessSet(value);
                    EndEdit();
                }
            }
        }

        [EditAction]
        private void AfterUseReverseRoughnessSet(bool value)
        {
            UpdateRoughnessSectionsForReverseRoughness();
            UseReverseRoughnessInCalculation = value;
        }
        
        [NoNotifyPropertyChange]
        public virtual TimeSpan OutputTimeStep
        {
            get
            {
                return OutputSettings.GridOutputTimeStep;
            }
            set
            {
                OutputSettings.GridOutputTimeStep = value;
            }
        }

        [NoNotifyPropertyChange]
        public override TimeSpan TimeStep
        {
            get { return base.TimeStep; }
            set
            {
                base.TimeStep = value;
                if (OutputTimeStep < value) OutputTimeStep = value;
                if (OutputSettings.StructureOutputTimeStep < value) OutputSettings.StructureOutputTimeStep = value;
            }
        }

        public virtual Parameter<string> InitialConditionsTypeParameter
        {
            get { return GetDataItemValueByTag<Parameter<string>>(WaterFlowModel1DDataSet.InitialConditionsTypeTag); }
        }

        public virtual Parameter<string> TemperatureModelTypeParameter
        {
            get { return GetDataItemValueByTag<Parameter<string>>(WaterFlowModel1DDataSet.TemperatureModelTypeTag); }
        }

        public virtual Parameter<string> DensityTypeParameter
        {
            get { return GetDataItemValueByTag<Parameter<string>>(WaterFlowModel1DDataSet.DensityTypeTag); }
        }
        
        public virtual Parameter<string> DispersionFormulationTypeParameter
        {
            get { return GetDataItemValueByTag<Parameter<string>>(WaterFlowModel1DDataSet.DispersionFormulationTypeTag); }
        }

        public virtual IList<ModelApiParameter> ParameterSettings
        {
            get { return parameterSettings ?? (parameterSettings = GetParametersFromModelApi()); }
            protected set { parameterSettings = UpdateMissingOrSurplusParameters(value); }
        }

        private IList<ModelApiParameter> UpdateMissingOrSurplusParameters(IList<ModelApiParameter> sourceParameters)
        {
            var desiredParametersList = GetParametersFromModelApi();

            var different = sourceParameters.Count != desiredParametersList.Count || //unequal count
                            !sourceParameters.Select(p => p.Name) //or intersection not full
                                             .Intersect(desiredParametersList.Select(p => p.Name))
                                             .Count().Equals(sourceParameters.Count);

            if (!different)
                return sourceParameters;

            foreach (var item in desiredParametersList)
            {
                var matchingItem = sourceParameters.FirstOrDefault(p => p.Name == item.Name);
                if (matchingItem != null)
                {
                    item.Value = matchingItem.Value; //copy source values where possible
                }
            }
            sourceParameters.Clear(); //reuse the same instance to keep nhibernate happy
            sourceParameters.AddRange(desiredParametersList);
            return sourceParameters;
        }

        [NoNotifyPropertyChange]
        public virtual InitialConditionsType InitialConditionsType
        {
            get
            {
                var name = InitialConditionsTypeParameter.Value;
                return (InitialConditionsType)Enum.Parse(typeof(InitialConditionsType), name);
            }
            set
            {
                var newName = value.ToString();
                if (InitialConditionsTypeParameter.Value == newName) return;

                //suspend tracking changes in the coverage..since the type does not math up while the 
                //convertor changes the model
                UnSubscribeToInitialConditionsCoverageDataItem();
                
                //hack to trigger undo/redo nesting, do not remove or whine until you have a better solution ;-)
                ((INotifyPropertyChanging)InitialConditionsTypeParameter).PropertyChanging += InitialConditionsTypeParameterValueChanging;
                                
                try
                {
                    //set the parameter (can cause exception)
                    initialConditionsType = value;
                    InitialConditionsTypeParameter.Value = newName;
                }
                catch (InvalidOperationException invalidOperation)
                {
                    Log.Error(invalidOperation.Message);
                }

                //hack to trigger undo/redo nesting, do not remove or whine until you have a better solution ;-)
                ((INotifyPropertyChanging)InitialConditionsTypeParameter).PropertyChanging -= InitialConditionsTypeParameterValueChanging;

                SubscribeToInitialConditionsCoverageDataItem();
            }
        }

        [EditAction]
        private void InitialConditionsTypeParameterValueChanging(object sender, PropertyChangingEventArgs e)
        {
            //let an external mutator handle the change..
            InitialConditionsConverter.ChangeInitialConditionsType(this, initialConditionsType);
            SetAttributeInitialDepth();
        }

        private void SetAttributeInitialDepth()
        {
            var dataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InputInitialConditionsTag);
            var attributes = ((NetworkCoverage)dataItem.Value).Components[0].Attributes;
            attributes[FunctionAttributes.StandardName] = initialConditionsType == InitialConditionsType.Depth
                                                              ? FunctionAttributes.StandardNames.WaterDepth
                                                              : FunctionAttributes.StandardNames.WaterLevel;
        }

        public virtual bool UseTemperature
        {
            get { return GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseTemperatureParameterTag).Value; }
            set
            {
                ToggleTemperature(value);
            }
        }

        private void ToggleTemperature(bool useTemperature)
        {
            var oldUseTempValue = UseTemperature;

            GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseTemperatureParameterTag).Value = useTemperature;

            if (oldUseTempValue == useTemperature)
            {
                return;
            }

            if (useTemperature)
            {
                var initialTemperature = new NetworkCoverage("Initial Temperature", false, "Temperature", "°C") { Network = Network };
                initialTemperature.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardProcessNames.Temperature;
                initialTemperature.Locations.ExtrapolationType = ExtrapolationType.Constant;
                AddDataItem(initialTemperature, DataItemRole.Input, WaterFlowModel1DDataSet.InputInitialTemperatureTag);
            }
            else
            {
                DataItems.RemoveAllWhere(d => d.Tag == WaterFlowModel1DDataSet.InputInitialTemperatureTag );
            }

            BoundaryConditions.ForEach(bc => bc.UseTemperature = useTemperature);
            LateralSourceData.ForEach(lat => lat.UseTemperature = useTemperature);
        }

        public virtual double latitudeDefault
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueLatitudeDefault; }
        }

        public virtual double longitudDefault
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueLongitudDefault; }
        }

        public virtual double backgroundTemperatureDefault
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueBackgroundTemperatureDefault; }
        }

        public virtual double backgroundTemperatureMin
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueBackgroundTemperatureMin; }
        }

        public virtual double backgroundTemperatureMax
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueBackgroundTemperatureMax; }
        }

        public virtual double surfaceAreaDefault
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueSurfaceAreaDefault; }
        }
        public virtual double surfaceAreaMin
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueSurfaceAreaMin; }
        }

        public virtual double atmosphericPressureDefault
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueAtmosphericPressureDefault; }
        }

        public virtual double daltonNumberDefault
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueDaltonNumberDefault; }
        }
        public virtual double daltonNumberMin
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueDaltonNumberMin; }
        }
        public virtual double daltonNumberMax
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueDaltonNumberMax; }
        }

        public virtual double stantonNumberDefault
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueStantonNumberDefault; }
        }
        public virtual double stantonNumberMin
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueStantonNumberMin; }
        }
        public virtual double stantonNumberMax
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueStantonNumberMax; }
        }

        public virtual double heatCapacityWaterDefault
        {
            get { return WaterFlowModel1DDataSet.Meteo.valueHeatCapacityWaterDefault; }
        }

        public virtual TemperatureModelType TemperatureModelType
        {
            get
            {
                var name = TemperatureModelTypeParameter.Value;
                return (TemperatureModelType)Enum.Parse(typeof(TemperatureModelType), name);
            }
            set
            {
                TemperatureModelTypeParameter.Value = value.ToString();
            }
        }

        public virtual double BackgroundTemperature { get; set; }

        public virtual double StantonNumber { get; set; }

        public virtual double DaltonNumber { get; set; }

        public virtual double SurfaceArea { get; set; }

        public virtual double AtmosphericPressure { get; set; }

        public virtual double HeatCapacityWater { get; set; }

        public virtual DensityType DensityType
        {
            get
            {
                var name = DensityTypeParameter.Value;
                return (DensityType)Enum.Parse(typeof(DensityType), name);
            }
            set
            {
                DensityTypeParameter.Value = value.ToString();
            }
        }

        public virtual double Latitude { get; set; }

        public virtual double Longitude { get; set; }

        public virtual bool UseSalt
        {
            get { return GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseSaltParameterTag).Value; }
            set
            {
                if (UseSalt == value) return;
                EnableSalt(value, value);
            }
        }

        [NoNotifyPropertyChange]
        public virtual bool UseSaltInCalculation
        {
            get
            {
                return GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseSaltInCalculationParameterTag).Value;
            }
            set
            {
                if (UseSaltInCalculation == value) return;
                EnableSalt(value || UseSalt, value);
            }
        }

        public virtual bool UseMorphology
        {
            get { return useMorphology; }
            set { useMorphology = value; }
        }

        public virtual bool AdditionalMorphologyOutput
        {
            get { return additionalMorphologyOutput; }
            set { additionalMorphologyOutput = value; }
        }

        public virtual string MorphologyPath
        {
            get
            {
                var morphologyFile = GetDataItemValueByTag<WaterFlowModel1DMorphologyFile>(WaterFlowModel1DDataSet.MorphologyFileDataObjectTag);
                return morphologyFile == null ? string.Empty : morphologyFile.Path;
            }
            set
            {
                var morphologyFileDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.MorphologyFileDataObjectTag);
                if(morphologyFileDataItem != null) morphologyFileDataItem.Value = new WaterFlowModel1DMorphologyFile { Path = value };
            }
        }

        public virtual string BcmPath
        {
            get
            {
                var bcmFile = GetDataItemValueByTag<WaterFlowModel1DMorphologyFile>(WaterFlowModel1DDataSet.BcmFileDataObjectTag);
                return bcmFile == null ? string.Empty : bcmFile.Path;
            }
            set
            {
                var bcmFileDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.BcmFileDataObjectTag);
                if (bcmFileDataItem != null) bcmFileDataItem.Value = new WaterFlowModel1DMorphologyFile { Path = value };
            }
        }

        public virtual string SedimentPath
        {
            get
            {
                var sedimentFile = GetDataItemValueByTag<WaterFlowModel1DMorphologyFile>(WaterFlowModel1DDataSet.SedimentFileDataObjectTag);
                return sedimentFile == null ? string.Empty : sedimentFile.Path;
            }
            set
            {
                var sedimentFileDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.SedimentFileDataObjectTag);
                if (sedimentFileDataItem != null) sedimentFileDataItem.Value = new WaterFlowModel1DMorphologyFile { Path = value };
            }
        }

        public virtual string TraPath
        {
            get
            {
                var traFile = GetDataItemValueByTag<WaterFlowModel1DMorphologyFile>(WaterFlowModel1DDataSet.TraFileDataObjectTag);
                return traFile == null ? string.Empty : traFile.Path;
            }
            set
            {
                var traFileDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.TraFileDataObjectTag);
                if (traFileDataItem != null) traFileDataItem.Value = new WaterFlowModel1DMorphologyFile { Path = value };
            }
        }

        //need to expose parameter for view.. :(
        public virtual Parameter<double> DefaultInitialWaterLevelParameter
        {
            get
            {
                return GetDataItemValueByTag<Parameter<double>>(WaterFlowModel1DDataSet.DefaultInitialWaterLevelTag);
            }
        }

        [NoNotifyPropertyChange]
        public virtual double DefaultInitialWaterLevel
        {
            get
            {
                return DefaultInitialWaterLevelParameter.Value;
            }
            set
            {
                if (InitialConditionsType == InitialConditionsType.WaterLevel)
                {
                    InitialConditions.DefaultValue = value;
                }
                else
                {
                    DefaultInitialWaterLevelParameter.Value = value;
                }
            }
        }

        public virtual Parameter<double> DefaultInitialDepthParameter
        {
            get
            {
                return GetDataItemValueByTag<Parameter<double>>(WaterFlowModel1DDataSet.DefaultInitialDepthTag);
            }
        }

        [NoNotifyPropertyChange]
        public virtual double DefaultInitialDepth
        {
            get
            {
                return DefaultInitialDepthParameter.Value;
            }
            set
            {
                if (InitialConditionsType == InitialConditionsType.Depth)
                {
                    InitialConditions.DefaultValue = value; //already triggers change in DefaultInitialDepthParameter
                }
                else
                {
                    DefaultInitialDepthParameter.Value = value;
                }
            }
        }

        public virtual string SalinityEstuaryMouthNodeId { get; set; }

        public virtual DispersionFormulationType DispersionFormulationType
        {
            get
            {
                var name = DispersionFormulationTypeParameter.Value;
                return (DispersionFormulationType)Enum.Parse(typeof(DispersionFormulationType), name);
            }
            set
            {
                DispersionFormulationTypeParameter.Value = value.ToString();

                if (value == DispersionFormulationType.Constant)
                {
                    CacheDispersionCoverage(ref cachedDispersionF3Coverage, Resources.WaterFlowModel1D_AddDispersionF3CoverageDataItem_Dispersion_F3_coefficient);
                    CacheDispersionCoverage(ref cachedDispersionF4Coverage, Resources.WaterFlowModel1D_AddDispersionF4CoverageDataItem_Dispersion_F4_coefficient);
                    DataItems.RemoveAllWhere(d => d.Tag == WaterFlowModel1DDataSet.InputDispersionF3CoverageTag || d.Tag == WaterFlowModel1DDataSet.InputDispersionF4CoverageTag);
                }
                else
                {
                    EnableThatcherHarlemanCoverages();
                }
            }
        }

        // internal so that LegacyLoader can access this
        protected internal virtual void EnableThatcherHarlemanCoverages()
        {
            CacheDispersionCoverage(ref cachedDispersionF3Coverage, Resources.WaterFlowModel1D_AddDispersionF3CoverageDataItem_Dispersion_F3_coefficient);
            CacheDispersionCoverage(ref cachedDispersionF4Coverage, Resources.WaterFlowModel1D_AddDispersionF4CoverageDataItem_Dispersion_F4_coefficient);

            AddDataItem(cachedDispersionF3Coverage, DataItemRole.Input, WaterFlowModel1DDataSet.InputDispersionF3CoverageTag);
            AddDataItem(cachedDispersionF4Coverage, DataItemRole.Input, WaterFlowModel1DDataSet.InputDispersionF4CoverageTag);
        }

        private void CacheDispersionCoverage(ref INetworkCoverage cachedDispersionCoverage, string coverageName)
        {
            if (cachedDispersionCoverage != null) return;
            cachedDispersionCoverage = createDispersionCoverage(coverageName);

            // In the event that the dataitem already exists (such as after project load)
            var dispersionCoverageDataItem = DataItems.FirstOrDefault(di => di.Name == coverageName);
            if (dispersionCoverageDataItem == null) return;

            // Retrieve existing coverage
            var dispersionCoverage = dispersionCoverageDataItem.Value as INetworkCoverage;
            if (dispersionCoverage == null) return; // should not happen
            
            // add argument values
            foreach (var argument in dispersionCoverage.Arguments)
            {
                var matchingArgument = cachedDispersionCoverage.Arguments
                    .FirstOrDefault(a =>
                        a.ValueType == argument.ValueType &&
                        a.Name == argument.Name);

                if (matchingArgument == null) continue;

                matchingArgument.Values = (IMultiDimensionalArray)argument.Values.Clone();
            }

            // add component values
            var component = dispersionCoverage.Components.FirstOrDefault(c => c.Name == coverageName);
            if (component == null) return; // should not happen

            var matchingComponent = cachedDispersionCoverage.Components
                .FirstOrDefault(c =>
                    c.ValueType == component.ValueType &&
                    c.Name == coverageName);

            if (matchingComponent != null)
            {
                matchingComponent.Values = (IMultiDimensionalArray)component.Values.Clone();
            }
        }

        private void CreateDataItemsNotAvailableInPreviousVersion()
        {
            if (GetDataItemByTag(WaterFlowModel1DDataSet.InflowsTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.InflowsTag);
            }

            if (GetDataItemByTag(RestartInputStateTag) == null || GetDataItemByTag(UseRestartTag) == null || GetDataItemByTag(WriteRestartTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(RestartInputStateTag);
            }

            if (GetDataItemByTag(WaterFlowModel1DDataSet.DispersionFormulationTypeTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.DispersionFormulationTypeTag);
            }

            if (GetDataItemByTag(WaterFlowModel1DDataSet.UseTemperatureParameterTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.UseTemperatureParameterTag);
            }

            if (GetDataItemByTag(WaterFlowModel1DDataSet.TemperatureModelTypeTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.TemperatureModelTypeTag);
            }

            if (GetDataItemByTag(WaterFlowModel1DDataSet.DensityTypeTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.DensityTypeTag);
            }

            if (GetDataItemByTag(WaterFlowModel1DDataSet.InputMeteoDataTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.InputMeteoDataTag);
            }

            if (GetDataItemByTag(WaterFlowModel1DDataSet.MorphologyFileDataObjectTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.MorphologyFileDataObjectTag);
            }

            if (GetDataItemByTag(WaterFlowModel1DDataSet.BcmFileDataObjectTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.BcmFileDataObjectTag);
            }

            if (GetDataItemByTag(WaterFlowModel1DDataSet.SedimentFileDataObjectTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.SedimentFileDataObjectTag);
            }

            if (GetDataItemByTag(WaterFlowModel1DDataSet.TraFileDataObjectTag) == null)
            {
                CreateDataItemNotAvailableInPreviousVersion(WaterFlowModel1DDataSet.TraFileDataObjectTag);
            }

        }

        /// <summary>
        /// Increndibly ugly construct, but this is used for backward3 compatibility reasons
        /// </summary>
        /// <param name="tag"></param>
        private IDataItem CreateDataItemNotAvailableInPreviousVersion(string tag)
        {
            if (tag == WaterFlowModel1DDataSet.InflowsTag)
            {
                AddInflowsDataItem();
                return GetDataItemByTag(tag);
            }
            if (tag == RestartInputStateTag || tag == UseRestartTag || tag == WriteRestartTag)
            {
                AddRestartDataItems();
                return GetDataItemByTag(tag);
            }
            if (tag == WaterFlowModel1DDataSet.DispersionFormulationTypeTag)
            {
                AddDispersionFormulationTypeDataItem();
                return GetDataItemByTag(tag);
            }
            if (tag == WaterFlowModel1DDataSet.UseTemperatureParameterTag)
            {
                AddUseTemperatureParameterDataItem();
                return GetDataItemByTag(tag);
            }
            if (tag == WaterFlowModel1DDataSet.TemperatureModelTypeTag)
            {
                AddTemperatureModelTypeDataItem();
                return GetDataItemByTag(tag);
            }
            if (tag == WaterFlowModel1DDataSet.DensityTypeTag)
            {
                AddDensityTypeDataItem();
                return GetDataItemByTag(tag);
            }
            if (tag == WaterFlowModel1DDataSet.InputMeteoDataTag)
            {
                AddTemperatureMeteoDataDataItem();
                return GetDataItemByTag(tag);
            }
            if (tag == WaterFlowModel1DDataSet.MorphologyFileDataObjectTag)
            {
                AddMorphologyFileDataItem();
            }
            if (tag == WaterFlowModel1DDataSet.BcmFileDataObjectTag)
            {
                AddBcmFileDataItem();
            }
            if (tag == WaterFlowModel1DDataSet.SedimentFileDataObjectTag)
            {
                AddSedimentFileDataItem();
            }
            if (tag == WaterFlowModel1DDataSet.TraFileDataObjectTag)
            {
                AddTraFileDataItem();
            }
            return null;
        }

        public virtual string WorkingDirectory { get { return ExplicitWorkingDirectory ?? workDirectory; } }

        public virtual WaterFlowModel1DOutputSettingData OutputSettings
        {
            get { return outputSettings; }
            protected set
            {
                if (outputSettings != null)
                {
                    ((INotifyPropertyChanged)outputSettings).PropertyChanged -= OutputSettingsPropertyChanged;
                }

                outputSettings = value;

                if (outputSettings != null)
                {
                    ((INotifyPropertyChanged)outputSettings).PropertyChanged += OutputSettingsPropertyChanged;
                }
            }
        }

        private void UpdateRoughnessSectionsForReverseRoughness()
        {
            if (UseReverseRoughness)
            {
                //add reverse sections
                var normalRoughnesses = RoughnessSections.ToList();

                foreach (var roughness in normalRoughnesses)
                {
                    AddRoughnessSection(new ReverseRoughnessSection(roughness));
                }
            }
            else
            {
                //remove reverse sections
                var reverseRoughnesses = RoughnessSections.OfType<ReverseRoughnessSection>().ToList();

                foreach (var roughness in reverseRoughnesses)
                {
                    RemoveRoughnessSection(roughness);
                }
            }
        }

        public override bool CanCopy(IDataItem item)
        {
            if (item.Value is INetwork)
            {
                return true;
            }

            if (item.Value is FileBasedRestartState)
            {
                return true;
            }

            return false;
        }
        
        protected virtual void AddRoughnessSection(RoughnessSection roughnessSection)
        {
            var bcSet = GetDataItemSetByTag(WaterFlowModel1DDataSet.RoughnessSectionsTag);
            //make the set isremovable false, all children will inherit this
            bcSet.ReadOnly = true;
            bcSet.DataItems.Add(new DataItem(roughnessSection));
        }

        protected virtual void ClearRoughnessSections()
        {
            UnlinkNetworkFromRoughnessSections();
            GetDataItemSetByTag(WaterFlowModel1DDataSet.RoughnessSectionsTag).DataItems.Clear();
        }

        [EditAction]
        private void UnlinkNetworkFromRoughnessSections()
        {
            RoughnessSections.ForEach(rs => rs.Network = null);
        }

        protected virtual void RemoveRoughnessSection(RoughnessSection roughnessSection)
        {
            var bcSet = GetDataItemSetByTag(WaterFlowModel1DDataSet.RoughnessSectionsTag);

            //find the data item to remove
            var dataItem = bcSet.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, roughnessSection));
            bcSet.DataItems.Remove(dataItem);
        }

        //TODO: write code against list adapter e.g. DataItemSet.AsEventedList<T>?

        // TODO: This replace method should not be necessary (=> existing boundary node data objects should be manipulated)
        /// <summary>
        /// Replaces an existing boundary condition by <paramref name="boundaryNodeData"/>
        /// </summary>
        public virtual void ReplaceBoundaryCondition(WaterFlowModel1DBoundaryNodeData boundaryNodeData)
        {
            if (boundaryNodeData == null) return;

            var dataItemSet = GetDataItemSetByTag(WaterFlowModel1DDataSet.BoundaryConditionsTag);
            var currentDataItem = dataItemSet.DataItems.FirstOrDefault(di => ((WaterFlowModel1DBoundaryNodeData)di.Value).Feature == boundaryNodeData.Feature);
            if (currentDataItem == null) return;

            var insertIndex = dataItemSet.DataItems.IndexOf(currentDataItem);

            dataItemSet.DataItems.RemoveAt(insertIndex);
            dataItemSet.DataItems.Insert(insertIndex, new DataItem(boundaryNodeData));
        }

        [EditAction]
        private void OutputSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var engineParameter = sender as EngineParameter;
            if (engineParameter != null && e.PropertyName == "AggregationOptions")
            {
                //note
                //We make an exception for the finite volume grid. This is not strictly Output, as it's not calculated by a model run
                //and will typically contains values even when no model run has been performed. As a result it does not clear when other
                //output is cleared. This gives problems if the coverage updates itself (for example branch split, reverse, etc): being
                //output means being stored in NetCdf and thus it crashes on modification of the function structure. Both conceptually 
                //and practically it is better if it's treated as non-output, e.g. 'none'.
                var isFiniteVolumeGrid = engineParameter.Name == WaterFlowModelParameterNames.FiniteVolumeGridType;
                var expectedRole = isFiniteVolumeGrid ? DataItemRole.None : DataItemRole.Output;

                AddOrRemoveOutputCoverages(engineParameter, expectedRole); 
            }

            MarkOutputOutOfSync();
        }

        private void AddOrRemoveOutputCoverages(EngineParameter engineParameter, DataItemRole expectedRole)
        {
            if (engineParameter.AggregationOptions == AggregationOptions.None)
            {
                RemoveOutputCoverage(engineParameter, expectedRole);
            }
            else
            {
                var dataItem = DataItems.FirstOrDefault(di => (di.Role & expectedRole) == expectedRole && di.Tag == engineParameter.Name);
                if (dataItem == null)
                {
                    AddOutputCoverage(engineParameter, expectedRole);
                }
                else if (((IFunction)dataItem.Value).Name != GetOutputCoverageName(engineParameter))
                {
                    RemoveOutputCoverage(engineParameter, expectedRole);
                    AddOutputCoverage(engineParameter, expectedRole);
                }
            }
        }

        private void RemoveOutputCoverage(EngineParameter engineParameter, DataItemRole expectedRole)
        {
            // Remove all data items corresponding with this engine parameter
            var dataItemsToRemove =
                DataItems.Where(d => (d.Role & expectedRole) == expectedRole && d.Tag == engineParameter.Name).ToList();

            foreach (var dataItemToRemove in dataItemsToRemove)
            {
                // Remove all links to the finite volume output data item
                while (dataItemToRemove.LinkedBy.Count > 0)
                {
                    dataItemToRemove.LinkedBy[0].Unlink();
                }

                // Remove the data item
                DataItems.Remove(dataItemToRemove);
            }
        }

        public virtual void SubscribeToNetwork()
        {
            var networkDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag);
            var networkValue = networkDataItem.Value;
            if (networkValue != null)
            {
                ((INotifyCollectionChange) networkValue).CollectionChanged += NetworkCollectionChanged;
                ((INotifyPropertyChanged) networkValue).PropertyChanged += NetworkPropertyChanged;
                ((INotifyPropertyChanging)networkValue).PropertyChanging += NetworkPropertyChanging;
            }
            observedNetwork = (IHydroNetwork)networkValue;
        }

        protected override void OnDataItemLinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            base.OnDataItemLinking(sender, e);

            // if network data item is being linked
            if(Equals(e.Target, GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag)))
            {
                UnSubscribeFromNetwork();
            }
        }

        protected override void OnDataItemLinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            base.OnDataItemLinked(sender, e);

            // if network data item is linked
            if (Equals(e.Target, GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag)))
            {
                SubscribeToNetwork();

                if (!e.Relinking)
                {
                    RefreshNetworkRelatedData();
                }
                else
                {
                    // updates all related data item (in our case - all objects depending on network)
                    SuspendClearOutputOnInputChange = true;
                    if (networkBeforeLinking != null)
                    {
                        UpdateRelatedModelData(this, networkBeforeLinking);
                    }
                    networkBeforeLinking = null;
                    SuspendClearOutputOnInputChange = false;
                }
            }
        }

        protected override void OnDataItemUnlinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            base.OnDataItemUnlinking(sender, e);

            if (Equals(e.Target, GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag)))
            {
                UnSubscribeFromNetwork();
            }
        }

        private IHydroNetwork networkBeforeLinking;

        protected override void OnDataItemUnlinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            base.OnDataItemUnlinked(sender, e);

            if (Equals(e.Target, GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag)))
            {
                SubscribeToNetwork();

                if (!e.Relinking)
                {
                    RefreshNetworkRelatedData();
                }
                else
                {
                    networkBeforeLinking = (IHydroNetwork) e.PreviousValue;
                }
            }
        }

        private IHydroNetwork observedNetwork;

        public virtual void UnSubscribeFromNetwork()
        {
            if (observedNetwork != null)
            {
                ((INotifyCollectionChange)observedNetwork).CollectionChanged -= NetworkCollectionChanged;
                ((INotifyPropertyChanged)observedNetwork).PropertyChanged -= NetworkPropertyChanged;
                ((INotifyPropertyChanging)observedNetwork).PropertyChanging -= NetworkPropertyChanging;
            }
        }

        private void SubscribeToInitialConditionsCoverageDataItem()
        {
            var initialConditionsDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InputInitialConditionsTag);
            ((INotifyPropertyChanged)initialConditionsDataItem).PropertyChanged += InitialConditionsDataItemPropertyChanged;
        }

        private void UnSubscribeToInitialConditionsCoverageDataItem()
        {
            var initialConditionsDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InputInitialConditionsTag);
            ((INotifyPropertyChanged)initialConditionsDataItem).PropertyChanged -= InitialConditionsDataItemPropertyChanged;
        }

        [EditAction]
        void InitialConditionsDataItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((Equals(sender, InitialConditions.Components[0])) &&
                (e.PropertyName == TypeUtils.GetMemberName(() => InitialConditions.DefaultValue)))
            {
                if (InitialConditionsType == InitialConditionsType.WaterLevel)
                {
                    DefaultInitialWaterLevelParameter.Value = InitialConditions.DefaultValue;
                }
                else //check depth
                {
                    DefaultInitialDepthParameter.Value = InitialConditions.DefaultValue;
                }
            }
        }

        private INetworkCoverage GetNetworkCoverageByTag(string tag)
        {
            return (INetworkCoverage)GetDataItemValueByTag(tag);
        }

        private void ClearBoundaryConditions()
        {
            GetDataItemSetByTag(WaterFlowModel1DDataSet.BoundaryConditionsTag).DataItems.Clear();
        }

        private void AddBoundaryCondition(WaterFlowModel1DBoundaryNodeData boundaryNodeData)
        {
            var dataItem = new DataItem(boundaryNodeData) { Hidden = (boundaryNodeData.DataType == WaterFlowModel1DBoundaryNodeDataType.None) };
            GetDataItemSetByTag(WaterFlowModel1DDataSet.BoundaryConditionsTag).DataItems.Add(dataItem);
        }

        private void RemoveBoundaryCondition(WaterFlowModel1DBoundaryNodeData boundaryNodeData)
        {
            var dataItemSet = GetDataItemSetByTag(WaterFlowModel1DDataSet.BoundaryConditionsTag);
            var dataItem = dataItemSet.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, boundaryNodeData));

            if (dataItem == null) return;

            dataItemSet.DataItems.Remove(dataItem);
        }

        private void RemoveBoundaryCondition(IHydroNode hydroNode)
        {
            var boundaryCondition = BoundaryConditions.FirstOrDefault(bc => bc.Feature == hydroNode);
            if (boundaryCondition == null) return;

            RemoveBoundaryCondition(boundaryCondition);
        }

        private void ClearLateralSourceData()
        {
            GetDataItemSetByTag(WaterFlowModel1DDataSet.LateralSourcesDataTag).DataItems.Clear();
        }

        private void AddLateralSourceData(WaterFlowModel1DLateralSourceData lateralSourceData)
        {
            lateralSourceData.UseSalt = UseSalt;
            lateralSourceData.UseTemperature = UseTemperature;

            GetDataItemSetByTag(WaterFlowModel1DDataSet.LateralSourcesDataTag).DataItems.Add(new DataItem(lateralSourceData));
        }

        private void RemoveLateralSourceData(WaterFlowModel1DLateralSourceData lateralSourceData)
        {
            var dataItemSet = GetDataItemSetByTag(WaterFlowModel1DDataSet.LateralSourcesDataTag);
            var dataItem = dataItemSet.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, lateralSourceData));

            if (dataItem == null) return;

            dataItemSet.DataItems.Remove(dataItem);
        }

        private void RemoveLateralSourceData(LateralSource lateralSource)
        {
            var lateralSourceData = LateralSourceData.FirstOrDefault(ls => ls.Feature == lateralSource);
            if (lateralSourceData == null) return;

            RemoveLateralSourceData(lateralSourceData);
        }

        /// <summary>
        /// Called when a network is inserted into or linked to the model
        /// </summary>
        [EditAction]
        private void RefreshNetworkRelatedData()
        {
            InitialFlow.Clear();
            InitialConditions.Clear();
            WindShielding.Clear();

            //clear salt coverages
            if (UseSalt)
            {
                InitialSaltConcentration.Clear();
                DispersionCoverage.Clear();
                if (cachedDispersionF3Coverage != null)
                {
                    cachedDispersionF3Coverage.Clear();
                }
                if (cachedDispersionF4Coverage != null)
                {
                    cachedDispersionF4Coverage.Clear();
                }
            }

            if (UseTemperature)
            {
                InitialTemperature.Clear();    
            }

            ClearOutput();

            // update network in output coverages
            DataItems
                .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is INetworkCoverage)
                .Select(di => di.Value)
                .Cast<INetworkCoverage>()
                .ForEach(c => c.Network = Network);

            if (NetworkDiscretization.Network != Network)
            {
                NetworkDiscretization.Network = Network;
                NetworkDiscretization.Clear();
                NetworkDiscretization.Name = WaterFlowModel1DDataSet.DiscretizationDataObjectName;
            }

            InitialFlow.Network = Network;
            InitialConditions.Network = Network;
            WindShielding.Network = Network;

            // update salt coverages
            if (UseSalt)
            {
                InitialSaltConcentration.Network = Network;
                DispersionCoverage.Network = Network;
                if (DispersionF3Coverage != null) DispersionF3Coverage.Network = Network;
                if (DispersionF4Coverage != null) DispersionF4Coverage.Network = Network;
                if (cachedDispersionF3Coverage != null) cachedDispersionF3Coverage.Network = Network;
                if (cachedDispersionF4Coverage != null) cachedDispersionF4Coverage.Network = Network;
            }

            SynchronizeSalinityEstuaryMouthNodeId();

            if (UseTemperature)
            {
                InitialTemperature.Network = Network;
            }

            // update boundary conditions
            ClearBoundaryConditions();
            if (Network != null)
            {
                foreach (var node in Network.Nodes)
                {
                    AddBoundaryCondition(WaterFlowModel1DHelper.CreateDefaultBoundaryCondition(node, UseSalt, UseTemperature));
                }
            }

            // update laterals
            ClearLateralSourceData();
            if (Network != null)
            {
                foreach (var lateralSource in Network.LateralSources)
                {
                    AddLateralSourceData(new WaterFlowModel1DLateralSourceData { Feature = (LateralSource)lateralSource });
                }
            }

            // update cross section sections 
            UpdateRoughnessSections();
        }

        private void SynchronizeSalinityEstuaryMouthNodeId()
        {
            if (string.IsNullOrEmpty(SalinityEstuaryMouthNodeId)) return;

            var node = Network.HydroNodes.FirstOrDefault(n => n.Name == SalinityEstuaryMouthNodeId);
            if (node == null)
            {
                Log.WarnFormat("Removed {0} as estuary mouth from model {1} because it is not present in current network", SalinityEstuaryMouthNodeId, Name);
                SalinityEstuaryMouthNodeId = null;
            }
            else
            {
                if (!node.IsValidSalinityEstuaryMouthNodeId())
                {
                    Log.WarnFormat("Removed {0} as estuary mouth from model {1} because it is no longer a valid boundary node", SalinityEstuaryMouthNodeId, Name);
                    SalinityEstuaryMouthNodeId = null;
                }
            }
        }

        public virtual void UpdateRoughnessSections()
        {
            ClearRoughnessSections();
            if (Network != null)
            {
                foreach (var crossSectionSectionType in Network.CrossSectionSectionTypes)
                {
                    AddRoughnessSections(crossSectionSectionType);
                }
            }
        }

        private void AddRoughnessSections(CrossSectionSectionType crossSectionSectionType)
        {
            var roughnessSection = new RoughnessSection(crossSectionSectionType, Network);
            AddRoughnessSection(roughnessSection);

            if (UseReverseRoughness)
            {
                AddRoughnessSection(new ReverseRoughnessSection(roughnessSection)
                    {
                        UseNormalRoughness = !UseReverseRoughnessInCalculation
                    });
            }
        }

        private IList<ModelApiParameter> GetParametersFromModelApi()
        {
            return ModelApiParameters.ReadParametersFromXml();
        }

        private INetworkCoverage cachedDispersionF3Coverage;
        private INetworkCoverage cachedDispersionF4Coverage;

        [EditAction]
        private void EnableSalt(bool useSalt, bool useSaltInCalculation)
        {
            var oldUseSaltValue = UseSalt;

            GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseSaltParameterTag).Value = useSalt;
            GetDataItemValueByTag<Parameter<bool>>(WaterFlowModel1DDataSet.UseSaltInCalculationParameterTag).Value = useSaltInCalculation;

            if (oldUseSaltValue == useSalt)
            {
                return;
            }

            if (useSalt)
            {
                var initialSaltConcentration = new NetworkCoverage("Salinity Concentration", false, "Salinity Concentration", "ppt")
                {
                    Network = Network
                };
                initialSaltConcentration.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterSalinity;
                initialSaltConcentration.Locations.ExtrapolationType = ExtrapolationType.Constant;
                AddDataItem(initialSaltConcentration, DataItemRole.Input, WaterFlowModel1DDataSet.InputInitialSaltConcentrationTag);

                // F1
                var dispersionF1Coverage = createDispersionCoverage(Resources.WaterFlowModel1D_EnableSalt_Dispersion_F1_coefficient);
                AddDataItem(dispersionF1Coverage, DataItemRole.Input, WaterFlowModel1DDataSet.InputDispersionCoverageTag);

                if (DispersionFormulationType != DispersionFormulationType.Constant)
                {
                    EnableThatcherHarlemanCoverages();
                }
            }
            else
            {
                DataItems.RemoveAllWhere(d => d.Tag == WaterFlowModel1DDataSet.InputInitialSaltConcentrationTag || d.Tag == WaterFlowModel1DDataSet.InputDispersionCoverageTag || 
                    d.Tag == WaterFlowModel1DDataSet.InputDispersionF3CoverageTag || d.Tag == WaterFlowModel1DDataSet.InputDispersionF4CoverageTag);
                cachedDispersionF3Coverage = null;
                cachedDispersionF4Coverage = null; 
            }

            BoundaryConditions.ForEach(bc => bc.UseSalt = useSalt);
            LateralSourceData.ForEach(lat => lat.UseSalt = useSalt);
        }

        private INetworkCoverage createDispersionCoverage(string name)
        {
            var dispersionCoverage = new NetworkCoverage(name, false, name, "")
            {
                Network = Network,
                DefaultValue = WaterFlowModel1DDataSet.DefaultSaltDispersion
            };
            dispersionCoverage.Locations.ExtrapolationType = ExtrapolationType.Constant;
            
            return dispersionCoverage;
        }

        private bool changingSalinityEstuaryMouthNodeId;

        private void NetworkPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            var node = sender as IHydroNode;
            if (node != null && e.PropertyName == "Name" && SalinityEstuaryMouthNodeId == node.Name)
            {
                changingSalinityEstuaryMouthNodeId = true;
            }
        }

        /// <summary>
        /// - Synchronize the boundary condition in the model with the IsBoundary property of the Nodes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EditAction]
        void NetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Manual call of OnInputCollectionChanged if the model is not owner of the network, i.e. the network is wrapped in a linked data item:
            if (GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag).LinkedTo != null)
            {
                OnInputPropertyChanged(sender, e);
            }

            if (sender is IDataItem && ((IDataItem)sender).Value is IHydroNetwork)
            {
                if (e.PropertyName == "Value")
                {
                    RefreshNetworkRelatedData();
                }
            }

            if (changingSalinityEstuaryMouthNodeId)
            {
                var node = (IHydroNode)sender;

                SalinityEstuaryMouthNodeId = node.Name;
                changingSalinityEstuaryMouthNodeId = false;
            }

            var endedEditingNetwork = sender == Network && e.PropertyName == "IsEditing" && !Network.IsEditing;

            if (endedEditingNetwork)
            {
                SynchronizeSalinityEstuaryMouthNodeId();

                if (Network.CurrentEditAction is BranchSplitAction && NetworkDiscretization != null && NetworkDiscretization.Locations.Values.Any())
                {
                    OnEndingBranchSplit((BranchSplitAction)Network.CurrentEditAction);
                }
            }

            if (sender == Network && e.PropertyName == "CoordinateSystem")
            {
                UpdateCoordinateSystemInOutputFeatureCoverages();
                Network.UpdateGeodeticDistancesOfChannels();
            }
        }

        

        /// <summary>
        /// - Synchronize the boundary condition in the model with the IsBoundary property of the Nodes. Since this property
        ///   can be set/reset while the node was not part of the network it is necessary to monitor additions and removals.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EditAction]
        private void NetworkCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            // Manual call of OnInputCollectionChanged if the model is not owner of the network, i.e. the network is wrapped in a linked data item:
            if (GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag).LinkedTo != null)
            {
                OnInputCollectionChanged(sender, e);
            }

            // when node is added or removed - check if boundary conditions are updated
            if (e.Item is IHydroNode)
            {
                UpdateBoundaryCondition(e);
            }
            else if (e.Item is LateralSource && !(Network.CurrentEditAction is BranchMergeAction))
            {
                UpdateLateralSource(e);
            }
            else if (e.Item is CrossSectionSectionType)
            {
                UpdateCrossSectionSectionType(e);
            }
            else if (e.Item is IChannel)
            {
                if (Equals(sender, Network.Branches))
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangeAction.Remove:
                            {
                        var channel = (IChannel)e.Item;
                        foreach (var lateralSource in channel.BranchSources)
                        {
                            RemoveLateralSourceData(lateralSource);
                        }

                        // remove all child data items
                        var dataItemsToRemove = new List<IDataItem>();
                        var networkDataItem = GetDataItemByValue(Network);
                        foreach (var dataItem in networkDataItem.Children)
                        {
                            // check if child data item uses WaterFlowModelBranchFeatureValueConverter
                            var valueConverter = dataItem.ValueConverter as WaterFlowModelBranchFeatureValueConverter;
                            if (valueConverter == null || !(valueConverter.Location is IBranchFeature))
                            {
                                continue;
                            }

                            // check if data item is related to the removed branch
                            var branchFeature = (IBranchFeature)valueConverter.Location;
                            if (!channel.BranchFeatures.Contains(branchFeature))
                            {
                                continue;
                            }

                            dataItemsToRemove.Add(dataItem);
                        }

                        foreach (var dataItem in dataItemsToRemove)
                        {
                            dataItem.Unlink();
                            dataItem.LinkedBy.ToArray().ForEach(di => di.Unlink());
                            networkDataItem.Children.Remove(dataItem);
                        }
                    }
                            break;
                        case NotifyCollectionChangeAction.Add:
                    {
                        var channel = (IChannel)e.Item;
                        foreach (var lateralSource in channel.BranchSources)
                        {
                            AddLateralSourceData(new WaterFlowModel1DLateralSourceData { Feature = lateralSource });
                        }
                    }
                            break;
                    }
                    
                    ClearOutput();
                }
            }
            
            // check if removed item is used in the child data items
            if (e.Item is IFeature && e.Action == NotifyCollectionChangeAction.Remove)
            {
                var asNetworkFeature = e.Item as INetworkFeature;
                if (asNetworkFeature != null && asNetworkFeature.IsBeingMoved())
                {
                    return;
                }

                var childDataItems = AllDataItems.Where(di => di.Parent != null && di.ValueConverter != null && di.ValueConverter.OriginalValue == e.Item).ToList();

                foreach (var childDataItem in childDataItems)
                {
                    // unlink all consumers
                    foreach (var targetDataItem in childDataItem.LinkedBy.ToArray())
                    {
                        targetDataItem.Unlink();
                    }

                    // remove item from parent
                    childDataItem.Parent.Children.Remove(childDataItem);
                }
            }
        }
        
        private void OnEndingBranchSplit(BranchSplitAction splitAction)
        {
            var locations = (splitAction.NewBranch.Source == splitAction.SplittedBranch.Target
                    ? new[]
                        {
                            new NetworkLocation(splitAction.SplittedBranch, splitAction.SplittedBranch.Length)
                                {
                                    // reset chainage to realy put the chainage to the end of the branch (this is not done via the contructor)
                                    Chainage = splitAction.SplittedBranch.Length 
                                },
                            new NetworkLocation(splitAction.NewBranch, 0)
                        }
                    : null);

            if (locations != null)
            {
                NetworkDiscretization.BeginEdit(new DefaultEditAction("Adding point at begin and end of branch"));
                NetworkDiscretization.Locations.AddValues(locations.Except(NetworkDiscretization.Locations.GetValues()));
                NetworkDiscretization.EndEdit();
            }
        }

        private void UpdateLateralSource(NotifyCollectionChangingEventArgs e)
        {
            var lateralSource = (LateralSource)e.Item;
            if (lateralSource.IsBeingMoved()) return;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangeAction.Add:
                    AddLateralSourceData(new WaterFlowModel1DLateralSourceData { Feature = lateralSource });
                    break;
                case NotifyCollectionChangeAction.Remove:
                    RemoveLateralSourceData(lateralSource);
                    break;
            }
        }

        private void UpdateBoundaryCondition(NotifyCollectionChangingEventArgs e)
        {
            var node = (IHydroNode)e.Item;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangeAction.Add:
                    AddBoundaryCondition(WaterFlowModel1DHelper.CreateDefaultBoundaryCondition(node, UseSalt, UseTemperature));
                    break;

                case NotifyCollectionChangeAction.Remove:
                    RemoveBoundaryCondition(node);
                    break;
            }
        }

        private void UpdateCrossSectionSectionType(NotifyCollectionChangingEventArgs e)
        {
            var sectionType = (CrossSectionSectionType)e.Item;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangeAction.Add:
                    if (RoughnessSections.Any(rs => rs.Name == sectionType.Name)) break;
                    AddRoughnessSections(sectionType);
                    break;
                case NotifyCollectionChangeAction.Remove:
                    var roughnessSection = RoughnessSections.FirstOrDefault(rs => rs.CrossSectionSectionType.Name == sectionType.Name);
                    if (roughnessSection != null)
                    {
                        RemoveRoughnessSections(roughnessSection.CrossSectionSectionType);
                    }
                    break;
            }
        }

        private void RemoveRoughnessSections(CrossSectionSectionType crossSectionSectionType)
        {
            var roughnessSections = RoughnessSections.Where(rs => rs.CrossSectionSectionType == crossSectionSectionType).ToList();
            foreach (var section in roughnessSections) //can be multiple: normal and reverse
            {
                RemoveRoughnessSection(section);
            }
        }

        public override string KernelVersions
        {
            get
            {
                var file = Path.Combine(DimrApiDataSet.CfDllPath, Flow1DApiDll.CF_DLL_NAME);
                if (!File.Exists(file))
                    return "";

                return "Kernel: " + Flow1DApiDll.CF_DLL_NAME + "  " + FileVersionInfo.GetVersionInfo(file).FileVersion;
            }
        }
       
        private IDictionary<IFeature, int> indexInInflowsCache = new Dictionary<IFeature, int>();

        protected virtual void BuildInflowsCoverage()
        {
            Inflows.Clear();
            indexInInflowsCache.Clear();

            var inflowsDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InflowsTag);
            var inflowsIsLinked = inflowsDataItem.Children.Count > 0;
            
            if (inflowsIsLinked)
            {
                // do this here because it's a heavier check
                // prepare inflows feature coverage: gather all linked laterals & boundaries
                var linkedFeatures = Network.HydroNodes.Concat(Network.LateralSources.Cast<IHydroObject>())
                                          .Where(ho => ho.Links != null && ho.Links.Count > 0)
                                          .Cast<IFeature>()
                                          .ToList();

                if (linkedFeatures.Count > 0)
                {
                    indexInInflowsCache = linkedFeatures.Select((f, i) => new {Feat = f, Index = i})
                                                        .ToDictionary(a => a.Feat, a => a.Index);

                    // set the features:
                    Inflows.Features = new EventedList<IFeature>(linkedFeatures);
                    Inflows.BeginEdit(new DefaultEditAction("Adding features"));

                    var wasSkipping = Inflows.FeatureVariable.SkipUniqueValuesCheck;
                    try
                    {
                        Inflows.FeatureVariable.SkipUniqueValuesCheck = true;
                        Inflows.FeatureVariable.AddValues(linkedFeatures);
                    }
                    finally
                    {
                        Inflows.FeatureVariable.SkipUniqueValuesCheck = wasSkipping;
                    }
                    Inflows.EndEdit();
                }
            }
        }

        private IFunction RetrieveOutputFunctionByDataItemTag(string tag)
        {
            var dataItem = DataItems.Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is IFunction).FirstOrDefault(di => di.Tag == tag);
            if (dataItem == null)
            {
                return null;
            }
            return (IFunction)dataItem.Value;
        }

        protected virtual void SetOrAddModelApiParameter(ParameterCategory parameterCategory, string parameterName, string value)
        {
            var modelApiParameter = GetModelApiParameter(parameterName, parameterCategory);
            if (modelApiParameter == null)
            {
                modelApiParameter = new ModelApiParameter { Name = parameterName, Category = parameterCategory };
                ParameterSettings.Add(modelApiParameter);
            }
            modelApiParameter.Value = value;
        }
        protected virtual void InitializeOutputCoverageArguments(EngineParameter modelApiParameter, ICollection<DateTime> times)
        {
            if (modelApiParameter.AggregationOptions != AggregationOptions.None)
            {
                if (modelApiParameter.ElementSet == ElementSet.GridpointsOnBranches)
                {
                    var coverage = GetNetworkCoverageByTag(modelApiParameter.Name);
                    AddNetworkLocationsToNetworkCoverage(NetworkDiscretization, times, coverage);
                }
                else if (modelApiParameter.ElementSet == ElementSet.ReachSegElmSet)
                {
                    var coverage = GetNetworkCoverageByTag(modelApiParameter.Name);
                    if (modelApiParameter.QuantityType == QuantityType.EnergyLevels)
                    {
                        AddNetworkLocationsToNetworkCoverage(CreateEnergyHeadDiscretization(NetworkDiscretization), times, coverage);
                    }
                    else
                    {
                        AddNetworkStaggeredLocationsToNetworkCoverage(NetworkDiscretization, times, coverage);
                    }
                }
                else if (modelApiParameter.ElementSet == ElementSet.Structures)
                {
                    var coverage = (IFeatureCoverage)GetDataItemValueByTag(modelApiParameter.Name);
                    AddFeaturesToFeatureCoverage(coverage, structureMappingToModelApi, times);
                }
                else if (modelApiParameter.ElementSet == ElementSet.Observations)
                {
                    var coverage = (IFeatureCoverage)GetDataItemValueByTag(modelApiParameter.Name);
                    AddFeaturesToFeatureCoverage(coverage, observationPointsMappingToModelApi, times);
                }
                else if (modelApiParameter.ElementSet == ElementSet.Laterals)
                {
                    var coverage = (IFeatureCoverage)GetDataItemValueByTag(modelApiParameter.Name);
                    AddFeaturesToFeatureCoverage(coverage, lateralSourcesMappingToModelApi, times);
                }
                else if (modelApiParameter.ElementSet == ElementSet.Retentions)
                {
                    var coverage = (IFeatureCoverage)GetDataItemValueByTag(modelApiParameter.Name);
                    AddFeaturesToFeatureCoverage(coverage, retentionMappingToModelApi, times);
                }
                else if (modelApiParameter.ElementSet == ElementSet.ModelWide)
                {
                    var timeseries = (ITimeSeries)GetDataItemValueByTag(modelApiParameter.Name);
                    timeseries.Clear();
                    timeseries.Time.FixedSize = times.Count;
                    if (times.Count != 0) timeseries.Time.SetValues(times);
                }
                else if (modelApiParameter.ElementSet == ElementSet.Pumps)
                {
                    var coverage = (IFeatureCoverage)GetDataItemValueByTag(modelApiParameter.Name);
                    AddFeaturesToFeatureCoverage(coverage, pumpMappingToModelApi, times);                    
                }
            }
        }

        private string GetOutputCoverageName(EngineParameter modelApiParameter)
        {
            if (modelApiParameter.AggregationOptions == AggregationOptions.None || modelApiParameter.AggregationOptions == AggregationOptions.Current)
            {
                return modelApiParameter.Name;
            }
            
            return String.Format("{0} ({1})", modelApiParameter.Name, modelApiParameter.AggregationOptions);
        }

        private void AddOutputCoverage(EngineParameter modelApiParameter, DataItemRole dataItemRole = DataItemRole.Output)
        {
            IFunction createdCoverage = null;

            if (modelApiParameter.AggregationOptions != AggregationOptions.None)
            {
                var coverageName = GetOutputCoverageName(modelApiParameter);
                if (modelApiParameter.ElementSet == ElementSet.GridpointsOnBranches)
                {
                    var coverage = new NetworkCoverage(coverageName, true, coverageName, modelApiParameter.Unit.Symbol)
                        {
                            Network = Network,
                        };
                    coverage.Components[0].NoDataValue = double.NaN;
                    coverage.Locations.FixedSize = 0;
                    AddDataItem(coverage, dataItemRole, modelApiParameter.Name);
                    createdCoverage = coverage;
                    createdCoverage.Attributes[FunctionAttributes.StandardFeatureName] = FunctionAttributes.StandardFeatureNames.GridPoint;
                }
                else if (modelApiParameter.ElementSet == ElementSet.ReachSegElmSet)
                {
                    var coverage = new NetworkCoverage(coverageName, true, coverageName, modelApiParameter.Unit.Symbol)
                        {
                            Network = Network,
                        };
                    coverage.Components[0].NoDataValue = double.NaN;
                    coverage.Locations.FixedSize = 0;
                    AddDataItem(coverage, dataItemRole, modelApiParameter.Name);
                    createdCoverage = coverage;
                    createdCoverage.Attributes[FunctionAttributes.StandardFeatureName] =
                        FunctionAttributes.StandardFeatureNames.ReachSegment; // standard_feature_name
                }
                else if (modelApiParameter.ElementSet == ElementSet.Structures)
                {
                    createdCoverage = AddFeatureCoverage(modelApiParameter);
                    createdCoverage.Attributes[FunctionAttributes.StandardFeatureName] = FunctionAttributes.StandardFeatureNames.Structure;
                }
                else if (modelApiParameter.ElementSet == ElementSet.Pumps)
                {
                    createdCoverage = AddFeatureCoverage(modelApiParameter);
                    createdCoverage.Attributes[FunctionAttributes.StandardFeatureName] = FunctionAttributes.StandardFeatureNames.Structure;
                }
                else if (modelApiParameter.ElementSet == ElementSet.Observations)
                {
                    createdCoverage = AddFeatureCoverage(modelApiParameter);
                    createdCoverage.Attributes[FunctionAttributes.StandardFeatureName] =
                        FunctionAttributes.StandardFeatureNames.ObservationPoint;
                }
                else if (modelApiParameter.ElementSet == ElementSet.Laterals)
                {
                    createdCoverage = AddFeatureCoverage(modelApiParameter);
                    createdCoverage.Attributes[FunctionAttributes.StandardFeatureName] = FunctionAttributes.StandardFeatureNames.LateralSource;
                }
                else if (modelApiParameter.ElementSet == ElementSet.Retentions)
                {
                    createdCoverage = AddFeatureCoverage(modelApiParameter);
                    createdCoverage.Attributes[FunctionAttributes.StandardFeatureName] = FunctionAttributes.StandardFeatureNames.Retention;
                }
                else if (modelApiParameter.ElementSet == ElementSet.ModelWide)
                {
                    createdCoverage = AddTimeSeries(modelApiParameter);
                }

                if (createdCoverage != null)
                {
                    createdCoverage.IsEditable = false;
                    createdCoverage.Components[0].Attributes[FunctionAttributes.StandardName] =
                        EngineParameters.GetStandardName(modelApiParameter.QuantityType, modelApiParameter.ElementSet);
                    createdCoverage.Components[0].Attributes[FunctionAttributes.AggregationType] =
                        GetStandardAggregationType(modelApiParameter.AggregationOptions);
                }
            }
        }

        private static string GetStandardAggregationType(AggregationOptions aggregationOptions)
        {
            switch (aggregationOptions)
            {
                case AggregationOptions.None:
                    return ""; //shouldn't happen
                case AggregationOptions.Maximum:
                    return FunctionAttributes.AggregationTypes.Maximum;
                case AggregationOptions.Minimum:
                    return FunctionAttributes.AggregationTypes.Minimum;
                case AggregationOptions.Average:
                    return FunctionAttributes.AggregationTypes.Average;
                case AggregationOptions.Current:
                    return FunctionAttributes.AggregationTypes.None; //current = no aggregation
                default:
                    throw new ArgumentOutOfRangeException("aggregationOptions");
            }
        }

        private IFeatureCoverage AddFeatureCoverage(EngineParameter modelApiParameter)
        {
            var featureCoverage = new FeatureCoverage(GetOutputCoverageName(modelApiParameter));
            featureCoverage.Components.Add(new Variable<double>("value")
            {
                NoDataValue = double.NaN, 
                Unit = (IUnit)modelApiParameter.Unit.Clone()
            });
            IVariable timeVariable = new Variable<DateTime>("time");
            featureCoverage.Arguments.Add(timeVariable);
            featureCoverage.Arguments.Add(new Variable<IBranchFeature>("feature")); // Pump BranchStructure Weir IStructure
            featureCoverage.IsEditable = false;
            featureCoverage.FeatureVariable.FixedSize = 0;
            AddDataItem(featureCoverage, DataItemRole.Output, modelApiParameter.Name);
            return featureCoverage;
        }

        private ITimeSeries AddTimeSeries(EngineParameter engineParameter)
        {
            var timeseries = new TimeSeries()
            {
                Name = GetOutputCoverageName(engineParameter),
            };

            timeseries.Components.Add(new Variable<double>("value")
            {
                NoDataValue = double.NaN,
                Unit = (IUnit) engineParameter.Unit.Clone()
            });
            timeseries.IsEditable = false;
            AddDataItem(timeseries, DataItemRole.Output, engineParameter.Name);
            return timeseries;
        }
        
        private static void AddFeaturesToFeatureCoverage(IFeatureCoverage featureCoverage, ICollection features, ICollection<DateTime> times)
        {
            if (featureCoverage.Store is WaterFlowModel1DNetCdfFunctionStore) return; // temporary until modelApi is removed

            featureCoverage.Clear();
            featureCoverage.FeatureVariable.IsAutoSorted = false;
            featureCoverage.Features = new EventedList<IFeature>(features.Cast<IFeature>());

            featureCoverage.Time.FixedSize = times.Count;
            featureCoverage.Arguments[1].FixedSize = features.Count;

            if (times.Count != 0) featureCoverage.Time.SetValues(times);
            if (features.Count != 0) featureCoverage.Arguments[1].SetValues(features);
        }


        /// <summary>
        /// Adds networklocations to the networkcoverage. Since each coverage uses 1  
        /// attribute accessable via the NetworkLocationAttributeAccessor. The networklocations
        /// need to be cloned for each networkcoverage.
        /// </summary>
        public static void AddNetworkLocationsToNetworkCoverage(IDiscretization discretization, ICollection<DateTime> times, INetworkCoverage networkCoverage)
        {
            if (networkCoverage.Store is WaterFlowModel1DNetCdfFunctionStore) return; // temporary until modelApi is removed

            var networkLocations = discretization.Locations.Values.OrderBy(l => l).ToArray();

            networkCoverage.Clear();

            networkCoverage.Time.FixedSize = times.Count;
            networkCoverage.Locations.FixedSize = networkLocations.Length;

            if (times.Count != 0) networkCoverage.Time.SetValues(times);
            if (networkLocations.Count() != 0) networkCoverage.SetLocations(networkLocations);
        }

        /// <summary>
        /// Adds staggered networklocations to the networkcoverage. Staggered locations are 
        /// located at the boundaries of the cells in the calculation grid "networkCoverage"
        /// </summary>
        private void AddNetworkStaggeredLocationsToNetworkCoverage(IDiscretization discretization, ICollection<DateTime> times, INetworkCoverage networkCoverage)
        {
            if (networkCoverage.Store is WaterFlowModel1DNetCdfFunctionStore) return; // temporary until modelApi is removed

            var networkLocations = discretization.Locations.Values.OrderBy(l => l).ToArray();
            var staggeredLocations = new List<INetworkLocation>();
            
            IBranch branch = null;
            INetworkLocation previous = null;

            foreach (var networkLocation in networkLocations)
            {
                if (networkLocation.Branch != branch)
                {
                    branch = networkLocation.Branch;
                }
                else if (previous != null)
                {
                    staggeredLocations.Add(new NetworkLocation(branch,previous.Chainage +  (networkLocation.Chainage - previous.Chainage) / 2));
                }

                previous = networkLocation;
            }

            networkCoverage.Clear();

            networkCoverage.Time.FixedSize = times.Count;
            networkCoverage.Locations.FixedSize = networkLocations.Length - discretization.Network.Branches.Count;

            if (times.Count != 0) networkCoverage.Time.SetValues(times);
            networkCoverage.SetLocations(staggeredLocations);
        }
        
        public override void UpdateLink(object data)
        {
            if (data is IHydroNetwork)
            {
                Network = (IHydroNetwork)data;
            }
            else
            {
                throw new ArgumentException("Only network objects can be linked to 1d model");
            }
        }
        
        public override bool IsLinkAllowed(IDataItem source, IDataItem target)
        {
            if (target == null)
            {
                return false;
            }

            if (!base.IsLinkAllowed(source, target)) return false;

            if (target.Value is HydroNetwork)
            {
                return source.Value is HydroNetwork;
            }

            if (target.Value is IDiscretization)
            {
                return source.Value is IDiscretization;
            }

            var boundaryCondition = BoundaryConditions.FirstOrDefault(bc => Equals(bc.SeriesDataItem, target));

            if (boundaryCondition != null)
            {  
                // Q(H) || Q(t)
                if (source.Value is FlowWaterLevelTable)
                {
                    return !boundaryCondition.Node.IsConnectedToMultipleBranches; //exclude internal nodes
                }

                if (source.Value is TimeSeries)
                {
                    return true;
                }
            }

            if (LateralSourceData.Any(lateralSourceData => Equals(lateralSourceData.SeriesDataItem, target)))
            {
                // Q(H) || Q(t)
                if (source.Value is TimeSeries || source.Value is FlowWaterLevelTable)
                {
                    return true;
                }
            }

            return false; // dont allow by default
        }

        protected override void OnBeforeDataItemsSet()
        {
            base.OnBeforeDataItemsSet();
            
            UnSubscribeFromNetwork();
            UnSubscribeToInitialConditionsCoverageDataItem();
            // weird what is use of this? outputCrestLevel = null;
        }

        protected override void OnAfterDataItemsSet()
        {
            base.OnAfterDataItemsSet();

            SubscribeToNetwork();
            SubscribeToInitialConditionsCoverageDataItem();

            //backward compatibility
            CreateDataItemsNotAvailableInPreviousVersion();
            CreateBoundaryConditionsNotAvailableInPreviousVersion();
            UpdateInputCoverageAttributeFromPreviousVersion();
            UpdateDispersionFormulationTypeParameterFromPreviousVersion();
        }

        /// <summary>
        /// backwards compatibility
        /// </summary>
        private void UpdateDispersionFormulationTypeParameterFromPreviousVersion()
        {
            var name = DispersionFormulationTypeParameter.Value;
            if (Enum.GetNames(typeof(DispersionFormulationType)).All(n => n != name))
            {
                DispersionFormulationTypeParameter.Value = DispersionFormulationType.Constant.ToString();
                // A bit odd, but we set the value first (above) so that eventing can call get successfully when setting (below)
                DispersionFormulationType = DispersionFormulationType.Constant;
            }
        }

        /// <summary>
        /// backward compatibility 3.0 -> 3.1
        /// </summary>
        private void UpdateInputCoverageAttributeFromPreviousVersion()
        {
            var inputInitialFlowDataItem = GetDataItemByTag(WaterFlowModel1DDataSet.InputInitialFlowTag);
            if (inputInitialFlowDataItem != null)
            {
                var inputInitialFlow = inputInitialFlowDataItem.Value as INetworkCoverage;

                if (inputInitialFlow != null &&
                    !inputInitialFlow.Components[0].Attributes.ContainsKey(FunctionAttributes.StandardName))
                {
                    inputInitialFlow.Components[0].Attributes[
                        FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterDischarge;
                }
            }
            SetAttributeInitialDepth();
        }

        /// <summary>
        /// backward compatibility 3.0 -> 3.1
        /// </summary>
        [EditAction] //do not remove
        private void CreateBoundaryConditionsNotAvailableInPreviousVersion()
        {
            var numBoundaries = Network.HydroNodes.Count();
            var numBoundaryConditions = BoundaryConditions.Count;
            if (numBoundaries != numBoundaryConditions)
            {
                foreach (var node in Network.HydroNodes)
                {
                    if (BoundaryConditions.All(bc => !Equals(bc.Feature, node)))
                    {
                        //add boundary condition
                        AddBoundaryCondition(WaterFlowModel1DHelper.CreateDefaultBoundaryCondition(node, UseSalt, UseTemperature));
                    }
                }
            }

            // Make sure that output is not always out of sync when updating this ValueType. 
            if (!BoundaryConditionsDataItemSet.ValueType.Equals(typeof (FeatureData<IFunction, INode>)))
            {
                BoundaryConditionsDataItemSet.ValueType = typeof(FeatureData<IFunction, INode>);
            }
        }

        private readonly bool enableUglyFewsHack;

        public override IEnumerable<object> GetDirectChildren()
        {
            return enableUglyFewsHack //HACK: ugly hack for FewsAdapter, this is EXTREMELY SLOW!!
                       ? base.GetDirectChildren().Concat(GetExtraDirectChilderenForFews())
                       : base.GetDirectChildren();
        }

        private IEnumerable<object> GetExtraDirectChilderenForFews()
        {
            if (UseSalt) 
            {
                foreach (var bc in BoundaryConditions.Where(bc => bc.UseSalt))
                {
                    yield return CreateFeatureTimeseriesInputDataItem(bc.SaltConcentrationTimeSeries, bc.Node);
                }

                foreach (var lateralData in LateralSourceData.Where(ld => ld.UseSalt))
                {
                    yield return CreateFeatureTimeseriesInputDataItem(lateralData.SaltConcentrationTimeSeries, lateralData.Feature);
                    yield return CreateFeatureTimeseriesInputDataItem(lateralData.SaltMassTimeSeries, lateralData.Feature);
                }
            }

            yield return new FakeParameterTimeSpan("TimeStep", () => TimeStep, x => TimeStep = x);
            yield return new FakeParameterTimeSpan("GridOutputTimeStep", () => OutputSettings.GridOutputTimeStep,x => OutputSettings.GridOutputTimeStep = x);
            yield return new FakeParameterTimeSpan("StructureOutputTimeStep", () => OutputSettings.StructureOutputTimeStep,x => OutputSettings.StructureOutputTimeStep = x);
        }

        private static IDataItem CreateFeatureTimeseriesInputDataItem(ITimeSeries timeSeries, IFeature feature)
        {
            return new DataItem(new FeatureData<ITimeSeries, IFeature> { Data = timeSeries, Feature = feature }, DataItemRole.Input);
        }

        //for fews adapter
        private class FakeParameterTimeSpan : Parameter<TimeSpan>
        {
            private readonly Func<TimeSpan> getter;
            private readonly Action<TimeSpan> setter;

            public FakeParameterTimeSpan(string name, Func<TimeSpan> getter, Action<TimeSpan> setter)
                : base(name)
            {
                this.getter = getter;
                this.setter = setter;
            }

            public override TimeSpan Value
            {
                get
                {
                    return getter != null ? getter() : default(TimeSpan);
                }
                set
                {
                    if (setter != null)
                        setter(value);
                }
            }
        }

        public void Dispose()
        {
            // Ensure all stores are closed
            var fileStores = AllDataItems.Where(di => di.LinkedTo == null && di.ValueType.Implements(typeof(IFunction)))
                    .Select(di => di.Value).OfType<IFunction>()
                    .Select(nc => nc.Store).OfType<IFileBased>();

            foreach (var fileStore in fileStores)
            {
                fileStore.Close();
            }
        }

        ///<exception cref="NotSupportedException">When unlinked and <see cref="DataItem.Value"/> does not inherit from <see cref="ICloneable"/>, is not null, or is not a value type.</exception>
        ///<exception cref="InvalidOperationException">
        /// When attempting to perform a deep clone on an <see cref="IDataItem"/> where <see cref="IDataItem.Owner"/> is not null and not this model.</exception>
        public override IProjectItem DeepClone()
        {
            return (IProjectItem)Clone();
        }

        // HACK, TODO: call base.CopyFrom() instead and then do what you need in WFM1D!
        ///<exception cref="NotSupportedException">When unlinked and <see cref="DataItem.Value"/> does not inherit from <see cref="ICloneable"/>, is not null, or is not a value type.</exception>
        ///<exception cref="InvalidOperationException">
        /// When attempting to perform a deep clone on an <see cref="IDataItem"/> where <see cref="IDataItem.Owner"/> is not null and not this model.</exception>
        public virtual object Clone()
        {
            //Don't use base-class yet..it is FUBAR (deepclone)
            var clonedModel = new WaterFlowModel1D();

            clonedModel.OnBeforeDataItemsSet(); // unsubscribe all custom event handlers

            var dataItemsClone = new EventedList<IDataItem>();

            var stopwatch = new Stopwatch();
            var message = "";
            foreach (var dataItem in DataItems)
            {
                if (Log.IsDebugEnabled)
                {
                    stopwatch.Start();
                    message = string.Format("Cloned: {0}", dataItem);
                }

                var dataItemClone = (IDataItem)dataItem.DeepClone();
                dataItemClone.Owner = clonedModel;
                dataItemsClone.Add(dataItemClone);

                if (Log.IsDebugEnabled)
                {
                    stopwatch.Stop();
                    message += string.Format(" ({0:0.###} sec)", stopwatch.Elapsed.TotalSeconds);
                    Log.Debug(message);
                    stopwatch.Reset();
                }
            }
            clonedModel.DataItems = dataItemsClone;

            clonedModel.OnAfterDataItemsSet();

            clonedModel.SuspendClearOutputOnInputChange = true;

            //make sure the salty dataitem are created first
            clonedModel.UseSalt = UseSalt;
            clonedModel.SalinityEstuaryMouthNodeId = SalinityEstuaryMouthNodeId;

            CloneTemperatureRelatedModelData(clonedModel);
            clonedModel.Name = Name;

            clonedModel.OutputSettings.CopyFrom(outputSettings);

            var clonedParameterSetttings = new List<ModelApiParameter>();
            ParameterSettings.ForEach(ps => clonedParameterSetttings.Add((ModelApiParameter)ps.Clone()));
            clonedModel.ParameterSettings = clonedParameterSetttings;

            UpdateRelatedModelData(clonedModel, Network);

            clonedModel.SuspendClearOutputOnInputChange = false;

            clonedModel.OutputIsEmpty = OutputIsEmpty;
            clonedModel.OutputOutOfSync = OutputOutOfSync;


            return clonedModel;
        }

        private void CloneTemperatureRelatedModelData(WaterFlowModel1D clonedModel)
        {
            // Temperature related model properties
            clonedModel.UseTemperature = UseTemperature;
            clonedModel.TemperatureModelType = TemperatureModelType;
            clonedModel.BackgroundTemperature = BackgroundTemperature;
            clonedModel.SurfaceArea = SurfaceArea;
            clonedModel.AtmosphericPressure = AtmosphericPressure;
            clonedModel.DaltonNumber = DaltonNumber;
            clonedModel.StantonNumber = StantonNumber;
            clonedModel.HeatCapacityWater = HeatCapacityWater;

            // Advanced options
            clonedModel.DensityType = DensityType;
            clonedModel.Latitude = Latitude;
            clonedModel.Longitude = Longitude;
        }

        private void UpdateRelatedModelData(WaterFlowModel1D clonedModel, IHydroNetwork originalNetwork)
        {
            clonedModel.OnBeforeDataItemsSet();

            UpdateClonedBoundaryConditions(clonedModel, originalNetwork);
            UpdateClonedLateralSources(clonedModel, originalNetwork);
            UpdateClonedNetworkDiscretization(clonedModel, clonedModel.NetworkDiscretization);
            UpdateNetworkInCoverages(clonedModel, originalNetwork);
            UpdateClonedRoughnessSections(clonedModel);

            var allFeatures = originalNetwork.GetAllItemsRecursive().ToList();
            var allFeaturesCloned = clonedModel.Network.GetAllItemsRecursive().ToList();

            // update cloned child data items containing WaterFlowModelBranchFeatureValueConverter
            IEnumerable<WaterFlowModelBranchFeatureValueConverter> valueConverters =
                clonedModel.AllDataItems.Where(di => di.ValueConverter is WaterFlowModelBranchFeatureValueConverter).Select(
                    di => di.ValueConverter as WaterFlowModelBranchFeatureValueConverter);
            foreach (var valueConverter in valueConverters)
            {
                valueConverter.Model = clonedModel;

                var branchFeature = valueConverter.Location as INetworkFeature;

                if(branchFeature != null && !Equals(branchFeature.Network, clonedModel.Network))
                {
                    var index = allFeatures.IndexOf(valueConverter.Location);
                    valueConverter.Location = (IFeature)allFeaturesCloned[index];
                }
            }

            clonedModel.OnAfterDataItemsSet(); // subscribe all custom event handlers
        }

        private static void UpdateClonedNetworkDiscretization(WaterFlowModel1D clonedModel, IDiscretization clonedNetworkDiscretization)
        {
            SegmentGenerationMethod segmentGenerationMethod = clonedNetworkDiscretization.SegmentGenerationMethod;
            clonedNetworkDiscretization.SegmentGenerationMethod = SegmentGenerationMethod.None;
            NetworkCoverage.ReplaceNetworkForClone(clonedModel.Network, clonedNetworkDiscretization);
            clonedNetworkDiscretization.SegmentGenerationMethod = segmentGenerationMethod;
            NetworkCoverageHelper.UpdateSegments(clonedNetworkDiscretization);
        }

        private static void UpdateNetworkInCoverages(WaterFlowModel1D clonedModel, IHydroNetwork originalNetwork)
        {
            var clonedNetwork = clonedModel.Network;
            
            NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, clonedModel.InitialFlow);
            NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, clonedModel.InitialConditions);
            NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, clonedModel.WindShielding);
            
            if (clonedModel.InitialSaltConcentration != null)
            {
                NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, clonedModel.InitialSaltConcentration);
            }
            if (clonedModel.InitialTemperature != null)
            {
                NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, clonedModel.InitialTemperature);
            }
            if (clonedModel.DispersionCoverage != null)
            {
                NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, clonedModel.DispersionCoverage);
            }
            if (clonedModel.DispersionF3Coverage != null)
            {
                NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, clonedModel.DispersionF3Coverage);
            }
            if (clonedModel.DispersionF4Coverage != null)
            {
                NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, clonedModel.DispersionF4Coverage);
            }

            var featuresSuperSetBefore = originalNetwork.GetAllItemsRecursive().OfType<IFeature>().ToList();
            var featuresSuperSetAfter = clonedNetwork.GetAllItemsRecursive().OfType<IFeature>().ToList();

            FeatureCoverage.RefreshAfterClone(clonedModel.Inflows, featuresSuperSetBefore, featuresSuperSetAfter);

            foreach (var function in clonedModel.OutputFunctions)
            {
                // fix network coverages
                var networkCoverage = function as INetworkCoverage;
                if (networkCoverage != null)
                {
                    NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, networkCoverage);
                }
                // fix feature coverages
                var featureCoverage = function as IFeatureCoverage;
                if (featureCoverage != null)
                {
                    if (!featureCoverage.Features.Any() ||
                        !featuresSuperSetBefore.Contains(featureCoverage.Features.First()))
                    {
                        continue;
                    }
                    FeatureCoverage.RefreshAfterClone(featureCoverage,
                                                      featuresSuperSetBefore,
                                                      featuresSuperSetAfter);
                }
            }
        }

        private void UpdateClonedRoughnessSections(WaterFlowModel1D clonedModel)
        {
            foreach (var clonedSection in clonedModel.RoughnessSections)
            {
                clonedSection.CrossSectionSectionType = clonedModel.Network.CrossSectionSectionTypes.First(rs => rs.Name == clonedSection.CrossSectionSectionType.Name);
                var sourceSection = RoughnessSections.First(rs => rs.Name == clonedSection.Name);
                clonedSection.CopyFunctionOfFromRoughnessSection(sourceSection, clonedModel.Network);
                if (clonedSection.RoughnessNetworkCoverage != null)
                {
                    NetworkCoverage.ReplaceNetworkForClone(clonedModel.Network, clonedSection.RoughnessNetworkCoverage);
                }
                clonedSection.Network = clonedModel.Network;

                if (clonedSection is ReverseRoughnessSection)
                {
                    var reverseSection = (clonedSection as ReverseRoughnessSection);
                    reverseSection.NormalSection = clonedModel.RoughnessSections.First(rs => rs.Name.Equals(reverseSection.NormalSection.Name));
                }
            }
        }

        private void UpdateClonedBoundaryConditions(WaterFlowModel1D clonedModel, IHydroNetwork originalNetwork)
        {
            for (int i = 0; i < BoundaryConditions.Count; i++)
            {
                var boundaryCondition = BoundaryConditions[i];
                var clonedBc = clonedModel.BoundaryConditions[i];
                clonedBc.Feature = clonedModel.Network.Nodes[originalNetwork.Nodes.IndexOf(boundaryCondition.Feature)];
            }
        }

        private void UpdateClonedLateralSources(WaterFlowModel1D clonedModel, IHydroNetwork originalNetwork)
        {
            var lateralSourceFeatures = originalNetwork.LateralSources.ToList();
            var clonedLateralSourceFeatures = clonedModel.Network.LateralSources.ToList();
            for (int i = 0; i < LateralSourceData.Count; i++)
            {
                WaterFlowModel1DLateralSourceData lateralSourceData = LateralSourceData[i];
                var clonedLateralSourceData = clonedModel.LateralSourceData[i];
                var index = lateralSourceFeatures.IndexOf(lateralSourceData.Feature);
                clonedLateralSourceData.Feature = (LateralSource)clonedLateralSourceFeatures[index];
            }
        }
        
        public override IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            if ((role & DataItemRole.Input) == DataItemRole.Input || (role & DataItemRole.Output) == DataItemRole.Output)
            {
                foreach (var weir in Network.Weirs)
                {
                    yield return weir;
                }
                foreach (var gate in Network.Gates)
                {
                    yield return gate;
                }
                foreach (var culvert in Network.Culverts)
                {
                    yield return culvert;
                }
                foreach (var pump in Network.Pumps)
                {
                    yield return pump;
                }
                foreach (var lateralSource in Network.LateralSources)
                {
                    yield return lateralSource;
                }
                foreach (var hydroNode in Network.HydroNodes.Where(hn => !hn.IsConnectedToMultipleBranches))
                {                    
                    yield return hydroNode;
                }
            }
            if ((role & DataItemRole.Output) == DataItemRole.Output)
            {
                foreach (var location in Network.ObservationPoints)
                {
                    yield return location;
                }
                foreach (var location in Network.Retentions)
                {
                    yield return location;
                }

                INetworkLocation[] segmentsCentroidLocations = NetworkDiscretization.Segments.Values
                                        .Where(s => s.Geometry.Centroid != null)
                                        .Select(s => new NetworkLocation(s.Branch, (s.EndChainage + s.Chainage)/ 2))
                                        .OfType<INetworkLocation>()
                                        .ToArray();

                yield return new Feature // all locations
                    {
                        Geometry = NetworkDiscretization.Geometry,
                        Attributes = new DictionaryFeatureAttributeCollection
                            {
                                { "locations", NetworkDiscretization.Locations.Values },
                                { "StandardFeatureName", EngineParameters.GetStandardFeatureName(ElementSet.GridpointsOnBranches)},
                                { "ElementType", "GridpointsOnBranches" }
                            }
                    };

                yield return new Feature // all staggered locations
                    {
                        Geometry = new GeometryCollection(segmentsCentroidLocations.Select(nl => nl.Geometry).ToArray()),
                        Attributes = new DictionaryFeatureAttributeCollection
                            {
                                { "locations", segmentsCentroidLocations },
                                { "StandardFeatureName", EngineParameters.GetStandardFeatureName(ElementSet.ReachSegElmSet)},
                                { "ElementType", "ReachSegElmSet" }
                            }
                    };
            }
        }

        public override IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            var networkDataItem = GetDataItemByValue(Network);

            if (location == null)
            {
                // Location independent parameters (i.e. model engine parameters)
                // TODO
                yield break;
            }

            if (location.Geometry is GeometryCollection && location.Attributes.ContainsKey("ElementType"))
            {
                var elementSet = (ElementSet) Enum.Parse(typeof(ElementSet), (string)location.Attributes["ElementType"], true);
                foreach (EngineParameter engineParameter in EngineParameters.EngineMapping().Where(p => p.ElementSet == elementSet))
                {
                    if (engineParameter.QuantityType == QuantityType.Salinity && !UseSaltInCalculation) continue;
                    if (engineParameter.QuantityType == QuantityType.Dispersion && !UseSaltInCalculation) continue;
                    if (engineParameter.QuantityType == QuantityType.Density && !UseSaltInCalculation) continue;

                    yield return new DataItem
                    {
                        Name = EngineParameters.GetStandardFeatureName(elementSet) + " - " + engineParameter.Name,
                        Role = engineParameter.Role,
                        ValueType = typeof(double[]),
                        Parent = networkDataItem,
                        ShouldBeRemovedAfterUnlink = true,
                        ValueConverter =
                            new WaterFlowModel1DGridDataValueConverter(
                                this,
                                engineParameter.QuantityType,
                                engineParameter.ElementSet,
                                engineParameter.Role)
                    };
                }
                yield break;
            }
            if (location.Geometry is Point)
            {
                // Engine parameters that can be set by RTC
                foreach (var engineParameter in GetEngineParametersForLocation(location))
                {
                    // search it first in existing data items
                    var existingDataItem =
                        networkDataItem.Children.FirstOrDefault(
                            delegate(IDataItem di)
                            {
                                var valueConverter = di.ValueConverter as WaterFlowModelBranchFeatureValueConverter;
                                return di.ValueType == typeof(double)
                                       && (
                                           valueConverter != null &&
                                           valueConverter.ParameterName == engineParameter.Name &&
                                           valueConverter.Role == engineParameter.Role
                                           && valueConverter.ElementSet == engineParameter.ElementSet &&
                                           valueConverter.QuantityType == engineParameter.QuantityType
                                           && Equals(valueConverter.Location, location
                                           ));
                            });

                    if (existingDataItem != null)
                    {
                        yield return existingDataItem;
                    }
                    else
                    {
                        yield return new DataItem
                        {
                            Name = location + " - " + engineParameter.Name, //todo: clean this up
                            Role = engineParameter.Role,
                            ValueType = typeof(double),
                            Parent = networkDataItem,
                            ShouldBeRemovedAfterUnlink = true,
                            ValueConverter =
                                new WaterFlowModelBranchFeatureValueConverter(
                                    this,
                                    location,
                                    engineParameter.Name,
                                    engineParameter.QuantityType,
                                    engineParameter.ElementSet,
                                    engineParameter.Role,
                                    engineParameter.Unit.Symbol)
                        };
                    }
                }
            }
        }

        private IEnumerable<EngineParameter> GetEngineParametersForLocation(IFeature location)
        {
            if (location is IHydroNode)
            {
                WaterFlowModel1DBoundaryNodeData boundary =
                    BoundaryConditions.First(boundaryNodeData => boundaryNodeData.Node.Equals(location));
                if (boundary.DataType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant ||
                    boundary.DataType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries)
                {
                    yield return new EngineParameter(QuantityType.WaterLevel, ElementSet.HBoundaries,
                                                     DataItemRole.Input, FunctionAttributes.StandardNames.WaterLevel,
                                                     new Unit("Meter above reference level", "m AD"));
                    yield return new EngineParameter(QuantityType.Discharge, ElementSet.HBoundaries,
                                                     DataItemRole.Output, FunctionAttributes.StandardNames.WaterDischarge,
                                                     new Unit("Cubic meter", "m³"));
                }
                else if (boundary.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowConstant ||
                    boundary.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries)
                {
                    yield return new EngineParameter(QuantityType.Discharge, ElementSet.QBoundaries,
                                                     DataItemRole.Input, FunctionAttributes.StandardNames.WaterDischarge,
                                                     new Unit("Cubic meter", "m³"));
                    yield return new EngineParameter(QuantityType.Discharge, ElementSet.QBoundaries,
                                                     DataItemRole.Output, FunctionAttributes.StandardNames.WaterLevel,
                                                     new Unit("Meter above reference level", "m AD"));
                }
            }
            else
            {
                foreach (EngineParameter exchangableParameter in EngineParameters.GetExchangableParameters(OutputSettings.EngineParameters, location))
                {
                    yield return exchangableParameter;
                }
            }
        }

        /// <summary>
        /// Returns the intial value of a struct before the run has been started.
        /// These are typically the user supplied values or the values from a previous run
        /// now hardcoded; should be mapped to EngienParameter?
        /// </summary>
        protected virtual double GetInitialValue(IFeature feature, string parameterName)
        {
            return EngineParameters.GetInitialValue(feature, parameterName);
        }
        
        
        //'hack' there must another way to do this...why it output not cleared when currenttime changes???
        private readonly string[] ignoreProperties = new[] { "IsEditing"};

        [EditAction]
        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var dataItem = sender as IDataItem;

            if (SuspendClearOutputOnInputChange)
            {
                return;
            }

            if (e.PropertyName == "DataType" && sender is WaterFlowModel1DBoundaryNodeData)
            {
                var bc = sender as WaterFlowModel1DBoundaryNodeData;
                var dataItemSet = GetDataItemSetByTag(WaterFlowModel1DDataSet.BoundaryConditionsTag);
                var bcDataItem = dataItemSet.DataItems.First(di => ReferenceEquals(di.Value, bc));
                bcDataItem.Hidden = bc.DataType == WaterFlowModel1DBoundaryNodeDataType.None;
            }

            // events from child data items do not trigger clear output
            if (sender is IValueConverter || (dataItem != null && dataItem.Parent != null))
            {
                return;
            }

            if (e.PropertyName == "Owner")
            {
                return;
            }
            
            if (ignoreProperties.Contains(e.PropertyName))
            {
                return;
            }

            if (IsBreakingPropertyChangeForOutput(sender, e))
            {
                base.OnInputPropertyChanged(sender, e);
            }
            else
            {
                MarkOutputOutOfSync();
            }
        }

        private bool IsBreakingPropertyChangeForOutput(object sender, PropertyChangedEventArgs e)
        {
            if (Network == null)
                return false;

            //only clear output if branch geometry was modified (for network coverages)
            if (sender is IBranch && e.PropertyName == "Geometry")
                return true;

            //only clear output if feature chainage was modified (for feature coverages)
            if (sender is IBranchFeature && e.PropertyName == "Chainage")
                return true;

            if (sender is INetworkFeature && e.PropertyName == "Geometry")
                return true;

            return false;
        }

        private bool IsBreakingCollectionChangeForOutput(object sender)
        {
            if (Network == null)
                return false;

            if (Equals(sender, Network.Branches)) //network coverage
                return true;

            if (Network.Branches.Any(br => Equals(br.BranchFeatures, sender))) //feature coverage
                return true;

            return false;
        }

        [EditAction]
        protected override void OnInputCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (!created)
            {
                return;
            }

            if (SuspendClearOutputOnInputChange)
            {
                return;
            }

            if (IsBreakingCollectionChangeForOutput(sender)) //only clear output if branch collection was modified
            {
                base.OnInputCollectionChanged(sender, e);
            }
            else
            {
                MarkOutputOutOfSync();
            }
        }

        public virtual T GetOutputFunction<T>(string parameterName, string featureName, bool mustExist = false) where T : IFunction
        {
            T outputCoverage =
                OutputFunctions.OfType<T>().FirstOrDefault(
                    cov =>
                    cov.Components[0].Attributes.ContainsKey(FunctionAttributes.StandardName) &&
                    cov.Attributes.ContainsKey(FunctionAttributes.StandardFeatureName) &&
                    cov.Components[0].Attributes[FunctionAttributes.StandardName].Equals(parameterName) &&
                    cov.Attributes[FunctionAttributes.StandardFeatureName].Equals(featureName));
            if (outputCoverage == null && mustExist)
            {
                throw new Exception(String.Format("Output spatial data {0}.{1} not found", featureName, parameterName));
            }
            return outputCoverage;
        }

        public virtual ModelApiParameter GetModelApiParameter(string parameterName, ParameterCategory? parameterCategory)
        {
            if (parameterCategory != null)
            {
                return
                    ParameterSettings.FirstOrDefault(p => p.Name == parameterName && p.Category == parameterCategory);
            }
            return ParameterSettings.FirstOrDefault(p => p.Name == parameterName);
        }
        
        public virtual IHydroRegion Region { get { return Network; } }

        public virtual Type SupportedRegionType { get { return typeof(HydroNetwork); } }

        public virtual bool SetInitialConditionsFromPreviousOutput(DateTime outputTime)
        {
            var sourceInitialConditions = InitialConditionsType == InitialConditionsType.Depth ? OutputDepth : OutputWaterLevel;

            if (sourceInitialConditions == null || OutputFlow == null)
            {
                Log.Error("Not all of the required spatial data are available: water level / depth / flow");
                return false;
            }

            if (!sourceInitialConditions.Time.Values.Contains(outputTime))
            {
                Log.ErrorFormat("Selected time {0} not present in output spatial data", outputTime);
                return false;
            }

            NetworkCoverageHelper.ExtractTimeSlice(sourceInitialConditions, InitialConditions, outputTime, true);
            NetworkCoverageHelper.ExtractTimeSlice(OutputFlow, InitialFlow, outputTime, true);
            return true;
        }
       
      
        public virtual string HydFilePath
        {
            get { return WorkingDirectory == null ? "" : Path.Combine(WorkingDirectory, "sobek.hyd"); }
        }

        // TODO : change to better ModelApi option
        public virtual bool HydFileOutput
        {
            get
            {
                var engineParameter = OutputSettings.GetEngineParameter(QuantityType.FiniteGridType, ElementSet.FiniteVolumeGridOnGridPoints);
                return (FiniteVolumeDiscretizationType)(int)engineParameter.AggregationOptions != FiniteVolumeDiscretizationType.None;
            }
            set
            {
                OutputSettings.GetEngineParameter(QuantityType.FiniteGridType, ElementSet.FiniteVolumeGridOnGridPoints)
                    .AggregationOptions = (AggregationOptions)(int)(value ? FiniteVolumeDiscretizationType.OnGridPoints : FiniteVolumeDiscretizationType.None);
            }
        }

        #region State Aware Model

        private ModelFileBasedStateHandler modelStateHandler;
        private static readonly int[] SupportedMetaDataVersions = new[] { 1 };

        IModelState IStateAwareModelEngine.GetCopyOfCurrentState()
        {
            return ModelStateHandler.GetState();
        }

        void IStateAwareModelEngine.SetState(IModelState modelState)
        {
            ModelStateHandler.FeedStateToModel(modelState);
        }

        void IStateAwareModelEngine.ReleaseState(IModelState modelState)
        {
            ModelStateHandler.ReleaseState(modelState);
        }

        IModelState IStateAwareModelEngine.CreateStateFromFile(string persistentStateFilePath)
        {
            return ModelStateHandler.CreateStateFromFile(Name, persistentStateFilePath);
        }

        #region Save State: Time Range

        public virtual bool UseSaveStateTimeRange { get; set; }

        public virtual DateTime SaveStateStartTime { get; set; }

        public virtual DateTime SaveStateStopTime { get; set; }

        public virtual TimeSpan SaveStateTimeStep { get; set; }

        #endregion

        public virtual IEnumerable<DateTime> GetRestartWriteTimes()
        {
            if (UseSaveStateTimeRange)
            {
                var time = SaveStateStartTime;
                while (time <= SaveStateStopTime)
                {
                    yield return time;

                    time += SaveStateTimeStep;
                }
            }
        }

        public virtual void ValidateInputState(out IEnumerable<string> errors, out IEnumerable<string> warnings)
        {
            try
            {
                var modelState = (ModelStateFilesImpl)ModelStateHandler.CreateStateFromFile("validate", RestartInput.Path);
                errors = ModelStateValidator.ValidateInputState(modelState, SupportedMetaDataVersions, GetMetaDataRequirements, "WaterFlowModel1D");
                warnings = Enumerable.Empty<string>();
            }
            catch (ArgumentException e)
            {
                errors = new[] { e.Message };
                warnings = Enumerable.Empty<string>();
            }
        }

        private Dictionary<string, string> GetMetaDataRequirements(int version)
        {
            if (version == 1)
            {
                return new Dictionary<string, string>
                    {
                        {"NrOfGridPoints", NetworkDiscretization.Arguments.First().Values.Count.ToString(CultureInfo.InvariantCulture)},
                        {"NrOfPumps", Network.Pumps.Count().ToString(CultureInfo.InvariantCulture)},
                        {"NrOfWeirs", Network.Weirs.Count().ToString(CultureInfo.InvariantCulture)},
                        {"NrOfBridges", Network.Bridges.Count().ToString(CultureInfo.InvariantCulture)},
                        {"NrOfChannels", Network.Channels.Count().ToString(CultureInfo.InvariantCulture)},
                        {"NrOfCulverts", Network.Culverts.Count().ToString(CultureInfo.InvariantCulture)},
                        {"NrOfHydroNodes", Network.HydroNodes.Count().ToString(CultureInfo.InvariantCulture)},
                    };
            }

            throw new NotImplementedException(String.Format("Meta data version {0} for model type {1} is not supported",
                                                            version, "WaterFlowModel1D"));
        }

        protected virtual ModelFileBasedStateHandler ModelStateHandler
        {
            get
            {
                if (modelStateHandler == null)
                {
                    IList<DelftTools.Utils.Tuple<string, string>> outAndInFileNames = new List<DelftTools.Utils.Tuple<string, string>>();
                    outAndInFileNames.Add(new DelftTools.Utils.Tuple<string, string>("sobek.nda", "sobek.rda"));
                    outAndInFileNames.Add(new DelftTools.Utils.Tuple<string, string>("sobek.ndf", "sobek.rdf"));
                    outAndInFileNames.Add(new DelftTools.Utils.Tuple<string, string>("1Dlevels-out.xyz", "1Dlevels-in.xyz"));
                    modelStateHandler = new ModelFileBasedStateHandler(Name, outAndInFileNames);
                }
                return modelStateHandler;
            }
        }
        void IStateAwareModelEngine.SaveStateToFile(IModelState modelState, string persistentStateFilePath)
        {
            modelState.MetaData = new ModelStateMetaData
            {
                ModelTypeId = "WaterFlowModel1D",
                Version = SupportedMetaDataVersions.Last(),
                Attributes = GetMetaDataRequirements(SupportedMetaDataVersions.Last())
            };

            RunInWorkingDirectory(() => ModelStateHandler.SaveStateToFile(modelState, persistentStateFilePath));
        }
        #endregion        
        
        #region IModelMerge
        public virtual ValidationReport ValidateMerge(IModelMerge sourceModel)
        {
            
            if (!CanMerge(sourceModel))
            {
                return new ValidationReport(Name + " (Water Flow 1D Model)", new[]
                                                                   {
                                                                       new ValidationReport("Model", new [] { new ValidationIssue(sourceModel, ValidationSeverity.Error, string.Format("sourceModel {0} (of type {1}) can't be merged with this model {2} (of type {3})",sourceModel.Name, sourceModel.GetType(),Name,GetType())) })
                                                                   });    
            }
            
            return new WaterFlowModel1DModelMergeValidator().Validate(this, (WaterFlowModel1D)sourceModel);
        }

        public virtual bool Merge(IModelMerge sourceModel, IDictionary<IModelMerge, IModelMerge> mergedDependendModelsLookup)
        {
            if (!CanMerge(sourceModel)) return false;
            var sourceWF1DModel = sourceModel as WaterFlowModel1D;
            if (sourceWF1DModel == null) return false;

            var sourceWF1DModel_clone = sourceWF1DModel.Clone() as WaterFlowModel1D;
            if (sourceWF1DModel_clone == null) return false;

            // SOBEK3-595: Ensure file stores on the cloned model are closed
            var clonedModelFileStores = sourceWF1DModel_clone.AllDataItems.Where(di => di.LinkedTo == null && di.ValueType.Implements(typeof(IFunction)))
                    .Select(di => di.Value).OfType<IFunction>()
                    .Select(nc => nc.Store).OfType<IFileBased>();

            foreach (var fileStore in clonedModelFileStores)
            {
                fileStore.Close();
            }

            WaterFlowModel1DModelMergeHelper.RenameAllNetworkElements(this, sourceWF1DModel_clone);
            WaterFlowModel1DModelMergeHelper.ClearBoundaryConditionsOnDestinationModel(this, sourceWF1DModel_clone);
            WaterFlowModel1DModelMergeHelper.Merge(this, sourceWF1DModel_clone);

            return true;
        }

        public virtual bool CanMerge(object sourceModel)
        {
            return sourceModel is WaterFlowModel1D;
        }

        public virtual IEnumerable<IModelMerge> DependendModels { get { yield break; } }

        #endregion

        #region IDimrModel

        public virtual string LibraryName
        {
            get { return "cf_dll"; }
        }

        public virtual string InputFile
        {
            get { return Path.GetFileName(Name + ModelFileNames.ModelFilenameExtension); }
        }

        public virtual string DirectoryName
        {
            get { return "dflow1d"; }
        }

        
        public virtual string ShortName
        {
            get { return "flow1d"; }
        }

        public virtual string GetItemString(IDataItem dataItem)
        {
            var feature = dataItem.GetFeature();

            var category = feature.GetFeatureCategory();
            if (category == null)
                return string.Empty;

            var dataItemName = ((INetworkFeature)((dataItem.ValueConverter).OriginalValue)).Name;

            var parameterName = GetConvertedParameterName(dataItem.GetParameterName(), category);

            string nameWithoutHashTags = dataItemName.Replace("##", "~~");
            var concatNames = new List<string>(new[] { category, nameWithoutHashTags, parameterName });

            concatNames.RemoveAll(s => s == null);

            return string.Join("/", concatNames);
        }

        public virtual IDataItem GetDataItemByItemString(string itemString)
        {
            var idParts = itemString.Split('/');
            var category = idParts[0];
            var featureName = idParts[1].Replace("~~","##");
            var parameterName = GetConvertedParameterName(idParts[2], category, true);

            var feature = (INetworkFeature) GetFeatureListForCategory(category).FirstOrDefault(n => n.Name == featureName);
            var childDataItems = GetChildDataItems(feature);

            var dataItem = childDataItems.FirstOrDefault(di => (di.ValueConverter?.OriginalValue as INetworkFeature)?.Name == featureName &&
                                                               (di.ValueConverter as ParameterValueConverter)?.ParameterName == parameterName);

            return dataItem;
        }

        private IEnumerable<INameable> GetFeatureListForCategory(string category)
        {
            switch (category)
            {
                case WaterFlowParametersCategories.Laterals: return Network?.LateralSources;
                case WaterFlowParametersCategories.ObservationPoints: return Network?.ObservationPoints;
                case WaterFlowParametersCategories.Culverts: return Network?.Culverts;
                case WaterFlowParametersCategories.Pumps: return Network?.Pumps;
                case WaterFlowParametersCategories.Weirs: return Network?.Weirs;
                case WaterFlowParametersCategories.Retentions: return Network?.Retentions;
                default:
                    return null;
            }
        }

        private static string GetConvertedParameterName(string parameterName, string category, bool lookForValue = false)
        {
            var namesLookup = WaterFlowModel1DDataSet.GetDictionaryForCategory(category);
            if (namesLookup == null)
            {
                return parameterName;
            }

            if (!lookForValue)
            {
                string dhydroParameterName;
                return namesLookup.TryGetValue(parameterName, out dhydroParameterName)
                    ? dhydroParameterName
                    : parameterName;
            }

            return namesLookup.ContainsValue(parameterName) 
                ? namesLookup.First(kvp => kvp.Value == parameterName).Key 
                : parameterName;
        }

        public virtual Type ExporterType
        {
            get { return typeof(WaterFlowModel1DExporter); }
        }

        public virtual string GetExporterPath(string directoryName)
        {
            var fileName = Path.GetFileName(InputFile);
            if (fileName == null) throw new ArgumentNullException("InputFile");
            return Path.Combine(directoryName, fileName);
        }

        public virtual bool IsMasterTimeStep { get { return true; } }
        public virtual bool CanRunParallel { get { return false; } }
        public virtual string MpiCommunicatorString { get { return null; } }

        public virtual string KernelDirectoryLocation
        {
            get { return DimrApiDataSet.CfDllPath; }
        }

        public new virtual ActivityStatus Status
        {
            get { return base.Status; }
            set { base.Status = value; }
        }

        [EditAction]
        public virtual bool RunsInIntegratedModel { get; set; }

        public virtual ValidationReport Validate()
        {
            return new WaterFlowModel1DModelValidator().Validate(this);
        }

        public virtual void DisconnectOutput()
        {
            ClearOutput();
        }

        public virtual void ConnectOutput(string outputPath)
        {
            ConnectOutputNetFiles(outputPath);
            UpdateCoordinateSystemInOutputFeatureCoverages();
            ReadSobekLogFile(outputPath);
        }

        private void ConnectOutputNetFiles(string outputDirectory)
        {
            DirectoryInfo netCdfOutputDirectory = new DirectoryInfo(Path.Combine(outputDirectory, "output"));
            if (!netCdfOutputDirectory.Exists) return;

            var dataItemsForOutputFunctionsWithFileBasedStores = DataItems.Where(
                    di => (di.Role & DataItemRole.Output) == DataItemRole.Output
                          && di.Value is IFunction
                          && ((IFunction)di.Value).Store is IFileBased)
                .ToList();

            var existingWaterFlowModel1DNetCdfFunctionStores = dataItemsForOutputFunctionsWithFileBasedStores
                .Select(di => ((IFunction)di.Value).Store)
                .Where(s => s is WaterFlowModel1DNetCdfFunctionStore)
                .Distinct()
                .ToList();

            foreach (var netFile in netCdfOutputDirectory.EnumerateFiles("*.nc"))
            {
                try
                {
                    var metaData = WaterFlowModel1DOutputFileReader.ReadMetaData(netFile.FullName, true);
                    foreach (var timeDependentVariable in metaData.TimeDependentVariables)
                    {
                        var coverageName = WaterFlowModel1DOutputCoverageMappings.GetMappingForVariable(netFile.Name,
                            timeDependentVariable.Name, timeDependentVariable.AggregationOption);
                        if (coverageName == null)
                        {
                            continue; // ignore all unsupported variables
                        }

                        var matchingDataItem =
                            dataItemsForOutputFunctionsWithFileBasedStores.FirstOrDefault(di => di.Name == coverageName);
                        if (matchingDataItem == null) continue;

                        var matchingCoverage = matchingDataItem.Value as IFunction;
                        if (matchingCoverage == null) continue;

                        var featureCoverage = matchingCoverage as FeatureCoverage;
                        if (featureCoverage != null)
                        {
                            SetFeaturesOnCoverage(netFile.Name, featureCoverage);
                        }

                        var existingStore = matchingCoverage.Store as IFileBased;
                        if (existingStore == null) continue;

                        if (existingStore is WaterFlowModel1DNetCdfFunctionStore)
                        {
                            existingStore.Path = netFile.FullName;
                        }
                        else
                        {
                            // replace existing store with new one
                            existingStore.Delete();

                            // check if a store already exists for this netFile
                            var newStore = existingWaterFlowModel1DNetCdfFunctionStores.FirstOrDefault(s =>
                                    ((WaterFlowModel1DNetCdfFunctionStore)s).Path == netFile.FullName);

                            // if not, create a new store and add it to the existing stores
                            if (newStore == null)
                            {
                                newStore = new WaterFlowModel1DNetCdfFunctionStore() { Path = netFile.FullName };
                                existingWaterFlowModel1DNetCdfFunctionStores.Add(newStore);
                            }

                            var functionsToAddToStore = matchingCoverage.Arguments
                                .Concat(matchingCoverage.Components)
                                .Concat(new[] { matchingCoverage });

                            newStore.Functions.AddRange(functionsToAddToStore);

                            matchingCoverage.RemoveValues();
                            matchingCoverage.Store = newStore;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WarnFormat("Unable to read output file '{0}': {1}", netFile.Name, ex.Message);
                }

            }

            OutputIsEmpty = false;
        }

        private void ReadSobekLogFile(string outputDirectory)
        {
            const string SobekLogFileName = "sobek.log";

            var sobekLogFilePath = Path.Combine(outputDirectory, SobekLogFileName);
            if (File.Exists(sobekLogFilePath))
            {
                try
                {
                    var logDataItem = DataItems.FirstOrDefault(di => di.Tag == SobekLogfileDataItemTag);
                    if (logDataItem == null)
                    {
                        // add logfile dataitem if not exists
                        var textDocument = new TextDocument(true) { Name = SobekLogFileName };
                        logDataItem = new DataItem(textDocument, DataItemRole.Output, SobekLogfileDataItemTag);
                        DataItems.Add(logDataItem);
                    }

                    var log = File.ReadAllText(sobekLogFilePath);
                    ((TextDocument)logDataItem.Value).Content = log;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(Resources.WaterFlowModel1D_ReadSobekLogFile_Error_reading_log_file___0____1_, SobekLogFileName, ex.Message);
                }
            }
            else
            {
                Log.WarnFormat(Resources.WaterFlowModel1D_ReadSobekLogFile_Could_not_find_log_file___0__at_expected_path___1_, SobekLogFileName, sobekLogFilePath);
            }
        }

        private void UpdateCoordinateSystemInOutputFeatureCoverages()
        {
            // update coordinatesystem in output feature coverages
            DataItems
                .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is FeatureCoverage)
                .Select(di => di.Value)
                .Cast<FeatureCoverage>()
                .ForEach(c => c.CoordinateSystem = Network.CoordinateSystem);
        }

        private void SetFeaturesOnCoverage(string netFile, FeatureCoverage coverage)
        {
            IEnumerable<IFeature> features = null;
            switch (netFile)
            {
                case WaterFlowModel1DOutputFileConstants.FileNames.ObservationsFile:
                    features = Network.ObservationPoints;
                    break;
                case WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile:
                    features = Network.LateralSources;
                    break;
                case WaterFlowModel1DOutputFileConstants.FileNames.StructuresFile:
                    features = Network.Structures.Except(Network.CompositeBranchStructures.Where(cbs => cbs.Structures.Count < 2));
                    break;
                case WaterFlowModel1DOutputFileConstants.FileNames.PumpsFile:
                    features = Network.Pumps;
                    break;
                case WaterFlowModel1DOutputFileConstants.FileNames.RetentionsFile:
                    features = Network.Retentions;
                    break;
            }

            coverage.Clear();
            coverage.Features = new EventedList<IFeature>(features);
        }

        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get { return base.CurrentTime; }
            set { base.CurrentTime = value; }
        }

        #region Overrides of TimeDependentModelBase

        public override IBasicModelInterface BMIEngine
        {
            get { return runner.Api; }
        }

        #endregion

        public virtual Array GetVar(string category, string itemName = null, string parameter = null)
        {
            if (runner.CanCommunicateWithDimrApi)
            {
                return runner.GetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter));
            }
            IFeature feature = null;
            switch (category)
            {
                case WaterFlowParametersCategories.Weirs:
                    feature = Network.Weirs.FirstOrDefault(w => w.Name == itemName);
                    break;
                case WaterFlowParametersCategories.Culverts:
                    feature = Network.Culverts.FirstOrDefault(c => c.Name == itemName);
                    break;
                case WaterFlowParametersCategories.Pumps:
                    feature = Network.Pumps.FirstOrDefault(p => p.Name == itemName);
                    break;
                case WaterFlowParametersCategories.Laterals:
                    feature = Network.LateralSources.FirstOrDefault(l => l.Name == itemName);
                    break;
            }

            return new[] {EngineParameters.GetInitialValue(feature, parameter)};
        }
        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            runner.SetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter), values);
        }

        #endregion

        #region TimeDependentModelBase
        public override void Initialize()
        {
            if (!RunsInIntegratedModel)
            {
                PrepareModelWorkDirectory();
            }
            else
            {
                workDirectory = ExplicitWorkingDirectory ?? FileUtils.CreateTempDirectory();
                ModelStateHandler.ModelWorkingDirectory = workDirectory;
            }

            base.Initialize();
        }
        protected override void OnCleanup()
        {
            foreach (var bc in boundaryConditionDataList)
            {
                var data = bc.Data;
                if (data != null)
                {
                    data.SkipArgumentValidationInEvaluate = false;
                }
            }

            Wind.Velocity.SkipArgumentValidationInEvaluate = false;
            Wind.Direction.SkipArgumentValidationInEvaluate = false;

            // Clear lookup dictionaries
            boundaryConditionDataList = null;
            base.OnCleanup();
            runner.OnCleanup();
        }
        protected override void OnInitialize()
        {
            DataItems.RemoveAllWhere(di => di.Tag == SobekLogfileDataItemTag);

            SetOrAddModelApiParameter(ParameterCategory.SimulationOptions, "UseRestart", UseRestart ? "true" : "false");
            SetOrAddModelApiParameter(ParameterCategory.SimulationOptions, "WriteSamples",WriteRestart ? "true" : "false");

            boundaryConditionDataList = BoundaryConditions.ToList();

            // performance optimization: skip validation in boundary condition
            foreach (var bc in boundaryConditionDataList)
            {
                var data = bc.Data;
                if (data != null)
                {
                    data.SkipArgumentValidationInEvaluate = true;
                }
            }

            Wind.Velocity.SkipArgumentValidationInEvaluate = true;
            Wind.Direction.SkipArgumentValidationInEvaluate = true;
            RunInWorkingDirectory(() =>
            {
                if (UseRestart)
                {
                    if (RestartInput.IsEmpty)
                    {
                        throw new InvalidOperationException("Cannot use restart; restart empty!");
                    }
                    ModelStateHandler.FeedStateToModel(ModelStateHandler.CreateStateFromFile(Name, RestartInput.Path));
                }

                structureMappingToModelApi = new List<IStructure1D>();

                foreach (var compositeStructure in Network.Structures.OfType<ICompositeBranchStructure>())
                {
                    foreach (var structure in compositeStructure.Structures)
                    {
                        structureMappingToModelApi.Add(structure);
                    }
                }

                observationPointsMappingToModelApi = Network.ObservationPoints.ToList();
                lateralSourcesMappingToModelApi = Network.LateralSources.ToList();
                retentionMappingToModelApi = Network.Retentions.ToList();
                pumpMappingToModelApi = Network.Pumps.ToList();

                // Determine output times:
                var gridPointOutputTimes1 = new List<DateTime>();
                for (var i = StartTime; i <= StopTime; i += OutputSettings.GridOutputTimeStep)
                {
                    gridPointOutputTimes1.Add(i);
                }

                var structuresOutputTimes1 = new List<DateTime>();
                for (var i = StartTime; i <= StopTime; i += OutputSettings.StructureOutputTimeStep)
                {
                    structuresOutputTimes1.Add(i);
                }

                // Initialize output coverage arguments
                foreach (var modelApiParameter in OutputSettings.EngineParameters)
                {
                    var times = IsStructureElementSet(modelApiParameter.ElementSet)
                        ? structuresOutputTimes1
                        : gridPointOutputTimes1;
                    InitializeOutputCoverageArguments(modelApiParameter, times);
                }

                BuildInflowsCoverage();
                runner.OnInitialize();
            });
        }
        protected override void OnProgressChanged()
        {
            runner.OnProgressChanged();
            base.OnProgressChanged();
        }
        protected override void OnExecute()
        {
            runner.OnExecute();
        }
        protected override void OnFinish()
        {
            runner.OnFinish();
        }
        #endregion

        #region TimeDependentModelBase Helper    
        protected override void OnClearOutput()
        {
            if (!SuspendClearOutputOnInputChange)
            {
                OutputOutOfSync = false;

                var stores =
                    OutputFunctions.Select(f => f.Store).OfType<WaterFlowModel1DNetCdfFunctionStore>().Distinct();
                stores.ForEach(s => s.Path = null);

                foreach (var function in OutputFunctions.Where(f => !(f.Store is WaterFlowModel1DNetCdfFunctionStore)))
                {
                    function.Filters.Clear();
                    function.Clear();
                }

                if (Inflows != null)
                {
                    Inflows.Clear();
                }
            }
        }
        private void PrepareModelWorkDirectory()
        {
            if (TemplateDataZipFile == null)
            {
                var assemblyLocation = typeof(WaterFlowModel1D).Assembly.Location;
                if (assemblyLocation == null) throw new ArgumentNullException("assemblyLocation");
                Log.DebugFormat("WaterFlowModel1D plugin path: '{0}'", assemblyLocation);
                var directoryInfo = new FileInfo(assemblyLocation).Directory;

                if (directoryInfo != null)
                {
                    var path = directoryInfo.FullName;
                    TemplateDataZipFile = Path.Combine(path, "template.zip");
                }
            }

            // copy template data files into temp directory and use it as model work directory
            if (ExplicitWorkingDirectory == null)
            {
                ExplicitWorkingDirectory = Path.GetTempFileName();
            }

            workDirectory = ExplicitWorkingDirectory;
            FileUtils.DeleteIfExists(workDirectory);


            Directory.CreateDirectory(workDirectory);
            workDirectory = Path.Combine(workDirectory, DirectoryName);
            Directory.CreateDirectory(workDirectory);
            Log.DebugFormat("Looking for template files in : '{0}'", TemplateDataZipFile);

            // copy template model files
            if (!File.Exists(TemplateDataZipFile))
            {
                throw new IOException(String.Format("Can't find model template file {0}", TemplateDataZipFile));
            }
            ZipFileUtils.Extract(TemplateDataZipFile, workDirectory);
            ModelStateHandler.ModelWorkingDirectory = workDirectory;
        }
        private void RestoreDirectory()
        {
            Directory.SetCurrentDirectory(previousWorkDirectory);
            previousWorkDirectory = null;
        }
        private void ChangeToWorkDirectory()
        {
            previousWorkDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(workDirectory);
        }
        private bool inWorkingDirectory;
        protected void RunInWorkingDirectory(Action action)
        {
            if (inWorkingDirectory)
            {
                action();
                return;
            }

            ChangeToWorkDirectory();
            inWorkingDirectory = true;

            try
            {
                action();
            }
            finally
            {
                RestoreDirectory();
                inWorkingDirectory = false;
            }
        }
        private static bool IsStructureElementSet(ElementSet elementSet)
        {
            return elementSet == ElementSet.Observations
                   || elementSet == ElementSet.Structures
                   || elementSet == ElementSet.Laterals
                   || elementSet == ElementSet.Retentions
                   || elementSet == ElementSet.Pumps;
        }
        #endregion

        #region Implementation of IDimrStateAwareModel

        public virtual void PrepareRestart()
        {
            workDirectory = ExplicitWorkingDirectory ?? FileUtils.CreateTempDirectory();
            ModelStateHandler.ModelWorkingDirectory = workDirectory;
            if (UseRestart)
            {
                if (RestartInput.IsEmpty)
                {
                    throw new InvalidOperationException("Cannot use restart; restart empty!");
                }
                ModelStateHandler.FeedStateToModel(ModelStateHandler.CreateStateFromFile(Name, RestartInput.Path));
            }
            ClearStatesIfRequired();
        }

        public virtual void WriteRestartFiles()
        {
            WriteRestartIfRequired(false);
        }

        public virtual void FinalizeRestart()
        {
            WriteRestartIfRequired(true);
        }

        #endregion

        /// <summary>
        /// Return true when a salinity.ini file needs to be added to the computation. 
        /// </summary> 
        public virtual bool ValidSalinityFileWithNonConstantFormulationAndF4Values
        {
            get
            {
                return UseSaltInCalculation &&
                       DispersionFormulationType != DispersionFormulationType.Constant &&
                       DispersionF4Coverage.GetValues<double>().Any(v => v > 0 || v < 0);
            }
        }

        private static Unit CreateUnit(WaterFlowModel1DDataSet.UnitIds knownUnitId)
        {
            switch (knownUnitId)
            {
                case WaterFlowModel1DDataSet.UnitIds.Meter:
                    return new Unit("meter", "m");
                case WaterFlowModel1DDataSet.UnitIds.CubicMeterPerSecond:
                    return new Unit("cubic meter per second", "m³/s");
                //case UnitIds.None:
                default:
                    return new Unit("", "");
            }
        }
    }
}
