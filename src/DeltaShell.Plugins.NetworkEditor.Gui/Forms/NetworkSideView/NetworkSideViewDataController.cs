using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Threading;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    public class NetworkSideViewDataController : IDisposable, INotifyPropertyChanged
    {
        public const string WaterLevelCoverageNameInMapFile = "water level (mesh1d_s1)";

        public delegate string ModelNameForCoverageDelegate(ICoverage coverage);
        private readonly ModelNameForCoverageDelegate modelNameForCoverageDelegate;

        public delegate void OnDataChangedDelegate();
        public OnDataChangedDelegate OnDataChanged;

        private readonly DelayedEventHandler<FunctionValuesChangingEventArgs> delayedCoverageValuesChanged;

        private bool disposed;
        private DelftTools.Utils.Tuple<double, double> minMax;
        private Route route;

        private INetworkCoverage waterLevelNetworkCoverage;
        private readonly IList<IFeatureCoverage> renderedFeatureCoverages = new List<IFeatureCoverage>();
        private readonly IList<INetworkCoverage> renderedNetworkCoverages = new List<INetworkCoverage>();
        private readonly IDictionary<string, IFunction> createdRoutes = new Dictionary<string, IFunction>();
        private NetworkSideViewCoverageManager networkSideViewCoverageManager;
        private static readonly IUnit waterLevelUnit = new Unit("Water level", "m AD");
        private List<DelftTools.Utils.Tuple<double, double>> maxWaterLevelValues;

        public NetworkSideViewDataController(Route route, NetworkSideViewCoverageManager coverageManager, ModelNameForCoverageDelegate modelNameForCoverageDelegate = null)
        {
            // not synchronized? maybe we should change implementation of DelayedEventHandler that it will always used current SynchronizationContext
            delayedCoverageValuesChanged = new DelayedEventHandler<FunctionValuesChangingEventArgs>(CoverageValuesChanged) { Delay = 50, Enabled = false };
            DelayedEventHandlerController.FireEventsChanged += DelayedEventHandlerFireEventsChanged;
            this.modelNameForCoverageDelegate = modelNameForCoverageDelegate;
            
            ProfileNetworkCoverages = new List<INetworkCoverage>();
            AllNetworkCoverages = new List<INetworkCoverage>();
            AllFeatureCoverages = new List<IFeatureCoverage>();

            NetworkRoute = route;
            NetworkSideViewCoverageManager = coverageManager;
            
            BuildProfileNetworkCoverages();
        }

        public void OnSideViewHandleCreated()
        {
            delayedCoverageValuesChanged.Enabled = true;
        }

        public void OnSideViewHandleDestroyed()
        {
            delayedCoverageValuesChanged.Enabled = false;
        }

        #region Coverage Values Changed

        private void DelayedEventHandlerFireEventsChanged(object sender, EventArgs e)
        {
            if (!delayedCoverageValuesChanged.Enabled)
            {
                return;
            }

            if (DelayedEventHandlerController.FireEvents)
            {
                OnDataChanged?.Invoke();
            }
        }
        
        private void UnsubscribeFromCoverage(ICoverage coverage)
        {
            coverage.ValuesChanged -= delayedCoverageValuesChanged;
        }

        private void SubscribeToCoverage(ICoverage coverage)
        {
            coverage.ValuesChanged += delayedCoverageValuesChanged;
        }

        void CoverageValuesChanged(object sender, EventArgs e)
        {
            ResetMinMaxZ();

            OnDataChanged?.Invoke();
        }

        #endregion

        public NetworkSideViewCoverageManager NetworkSideViewCoverageManager
        {
            get { return networkSideViewCoverageManager; }
            set
            {
                networkSideViewCoverageManager = value;

                if (networkSideViewCoverageManager != null)
                {
                    networkSideViewCoverageManager.OnCoverageAddedToProject = AddCoverageDelegated;
                    networkSideViewCoverageManager.OnCoverageRemovedFromProject = RemoveCoverageDelegated;
                    networkSideViewCoverageManager.RequestInitialCoverages();
                }
            }
        }

        private void AddCoverageDelegated(ICoverage coverage)
        {
            switch (coverage)
            {
                case INetworkCoverage networkCoverage when !Equals(networkCoverage.Network, route.Network):
                    return;
                //special case:
                case INetworkCoverage networkCoverage when string.Equals(networkCoverage.Name,WaterLevelCoverageNameInMapFile, StringComparison.CurrentCultureIgnoreCase) 
                                                           && networkCoverage.IsTimeDependent:

                    WaterLevelNetworkCoverage = FilterWithTime(networkCoverage, null);
                    break;
                case INetworkCoverage networkCoverage:
                    AllNetworkCoverages.Add(FilterWithTime(networkCoverage, null));
                    break;
                case IFeatureCoverage featureCoverage:
                    //todo: validate!!
                    AllFeatureCoverages.Add(FilterWithTime(featureCoverage, null));
                    break;
            }
        }

        private IFunction CreateMaxLevelFunction(INetworkCoverage networkCoverage)
        {
            if (networkCoverage == null) 
                return null;

            var chainagesValues = new List<double>();
            var values = new List<double>();

            if (maxWaterLevelValues == null)
            {
                var locations = RouteHelper.GetLocationsInRoute(networkCoverage, route);
                chainagesValues = locations.Select(loc => RouteHelper.GetRouteChainage(route, loc)).ToList();

                values = locations
                         .Select(l =>
                         {
                             var multiDimensionalArray = networkCoverage.GetValues<double>(new VariableValueFilter<INetworkLocation>(networkCoverage.Locations, l));
                             return multiDimensionalArray != null && multiDimensionalArray.Count > 0 ? multiDimensionalArray.Max() : double.NaN;
                         })
                         .Where(v => !double.IsNaN(v))
                         .ToList();

                // cache values because they do not change when using time navigator
                maxWaterLevelValues = chainagesValues.Zip(values).ToList();
            }
            else
            {
                chainagesValues = maxWaterLevelValues.Select(kvp => kvp.First).ToList();
                values = maxWaterLevelValues.Select(kvp => kvp.Second).ToList();
            }

            return NetworkSideViewHelper.CreateFunction(waterLevelUnit, chainagesValues, values, $"Max {networkCoverage.Name}");
        }

        private static INetworkCoverage FilterWithTime(INetworkCoverage coverage, DateTime? time)
        {
            return coverage.IsTimeDependent && coverage.Time.Values.Count > 0
                ? (INetworkCoverage) coverage.Filter(new VariableValueFilter<DateTime>(coverage.Time,
                    time ?? coverage.Time.Values[0]))
                : coverage;
        }

        private static IFeatureCoverage FilterWithTime(IFeatureCoverage coverage, DateTime? time)
        {
            return coverage.IsTimeDependent && coverage.Time.Values.Count > 0
                ? coverage.FilterAsFeatureCoverage(new VariableValueFilter<DateTime>(coverage.Time,
                    time ?? coverage.Time.Values[0]))
                : coverage;
        }

        private void RemoveCoverageDelegated(ICoverage coverage)
        {
            if (coverage is INetworkCoverage)
            {
                if (WaterLevelNetworkCoverage != null && WaterLevelNetworkCoverage.IsEqualOrDescendant(coverage))
                {
                    WaterLevelNetworkCoverage = null;
                }

                RemoveCoverageFromLists(AllNetworkCoverages, RenderedNetworkCoverages, coverage as INetworkCoverage);
            }
            else if (coverage is IFeatureCoverage)
            {
                RemoveCoverageFromLists(AllFeatureCoverages, RenderedFeatureCoverages, coverage as IFeatureCoverage);
            }
        }

        private void RemoveCoverageFromLists<T>(IList<T> all, IList<T> rendered, T coverage) where T : ICoverage
        {
            int indexAll = IndexOfDescendant(all, coverage);
            if (indexAll >= 0)
            {
                all.RemoveAt(indexAll);
            }
            int indexRendered = IndexOfDescendant(rendered, coverage);
            if (indexRendered >= 0)
            {
                if (coverage is INetworkCoverage)
                {
                    RemoveRenderedCoverage((INetworkCoverage)coverage);
                }
                else
                {
                    RemoveRenderedCoverage((IFeatureCoverage)coverage);
                }
            }
        }

        private static int IndexOfDescendant<T>(IEnumerable<T> coverages, IFunction coverageToFind) where T : IFunction
        {
            int index = 0;
            foreach(T coverage in coverages)
            {
                if (coverage.IsEqualOrDescendant(coverageToFind))
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        public string Name { get; set; }

        /// <summary>
        /// Only structues on this location are active. Other structure are in the InactiveBranchFeatures list.
        /// If no location is specified all features are active
        /// </summary>
        public ICompositeBranchStructure ActiveCompositeStructure { get; set; }

        public double ZMinValueRenderedCoverages
        {
            get
            {
                double min = double.MaxValue;
                foreach (
                    INetworkCoverage coverage in RenderedNetworkCoverages.Where(c => c.Components[0].Values.Count != 0))
                {
                    min = Math.Min(min, (double) coverage.Components[0].MinValue);
                }
                return min != double.MaxValue ? min : 0;
            }
        }

        public double ZMaxValueRenderedCoverages
        {
            get
            {
                double max = double.MinValue;
                foreach (
                    INetworkCoverage coverage in RenderedNetworkCoverages.Where(c => c.Components[0].Values.Count != 0))
                {
                    max = Math.Max(max, (double) coverage.Components[0].MaxValue);
                }
                return max != double.MinValue ? max : 0;
            }
        }

        /// <summary>
        /// Returns the <see cref="IManhole"/>s in the route with there chainage relative to the route
        /// </summary>
        public IEnumerable<System.Tuple<IManhole, double>> ActiveManholes
        {
            get { return NetworkSideViewHelper.GetNodesInRouteWithChainage<IManhole>(route); }
        }

        public IList<IBranchFeature> ActiveBranchFeatures
        {
            get
            {
                var branchFeatures = new List<IBranchFeature>();

                if (null == route)
                {
                    return null;
                }
                // Get all structures covered by the route
                if (!(route.Network is IHydroNetwork))
                {
                    return null;
                }

                var hydroNetwork = (IHydroNetwork) route.Network;
                foreach (IStructure1D structure in hydroNetwork.Structures)
                {
                    if (!(structure is ICompositeBranchStructure))
                        continue;

                    double offset = RouteHelper.GetRouteChainage(route, structure);
                    //check if the structure should be active..
                    bool active = (ActiveCompositeStructure == null) ||
                                  (structure == ActiveCompositeStructure);
                    if ((active) && (offset >= 0))
                    {
                        branchFeatures.Add(structure);
                    }
                }

                branchFeatures.AddRange(GetFeaturesInRoute(hydroNetwork.LateralSources.Cast<IBranchFeature>(), route));
                branchFeatures.AddRange(GetFeaturesInRoute(hydroNetwork.Retentions.Cast<IBranchFeature>(), route));
                branchFeatures.AddRange(GetFeaturesInRoute(hydroNetwork.CrossSections.Cast<IBranchFeature>(), route));
                branchFeatures.AddRange(GetFeaturesInRoute(hydroNetwork.ObservationPoints.Cast<IBranchFeature>(), route));

                return branchFeatures;
            }
        }

        private IList<IStructure1D> ActiveStructures
        {
            get { return ActiveBranchFeatures.Where(x => x is IStructure1D).Cast<IStructure1D>().ToList(); }
        }

        private static IEnumerable<IBranchFeature> GetFeaturesInRoute(IEnumerable<IBranchFeature> branchFeatures, Route route1)
        {
            var result = new List<IBranchFeature>();
            foreach (var source in branchFeatures)
            {
                double offset = RouteHelper.GetRouteChainage(route1, source);
                if (offset >= 0)
                {
                    result.Add(source);
                }
            }
            return result;
        }

        public IList<IBranchFeature> InactiveBranchFeatures
        {
            get
            {
                var result = new List<IBranchFeature>();
                var hydroNetwork = (IHydroNetwork) route.Network;
                foreach (IStructure1D structure in hydroNetwork.Structures)
                {
                    if (!(structure is ICompositeBranchStructure))
                        continue;

                    double offset = RouteHelper.GetRouteChainage(route, structure);
                    //check if the structure should be active..
                    bool inActive = (ActiveCompositeStructure != null) &&
                                    (structure != ActiveCompositeStructure);

                    if ((inActive) && (offset >= 0))
                    {
                        result.Add(structure);
                    }
                }
                return result;
            }
        }

        #region IDisposable Members
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region INetworkSideViewData Members

        /// <summary>
        /// Gets the route dependend network
        /// </summary>
        public INetwork Network
        {
            get
            {
                if (route == null)
                    return null;

                return route.Network;
            }
        }

        /// <summary>
        /// Gets or sets the network route data
        /// </summary>
        public Route NetworkRoute
        {
            get { return route; }
            private set
            {
                UnsubscribeToRouteNetwork();
                UnsubscribeToRoute();
                maxWaterLevelValues = null;

                route = value;
                SubscribeToRoute();
                SubscribeToRouteNetwork();
            }
        }
        
        private void SubscribeToRoute()
        {
            if (NetworkRoute != null)
            {
                //listen to segments, since they are (re-)generated after location changes
                NetworkRoute.RouteSegmentsUpdated += RouteSegmentsUpdated;
                ((INotifyPropertyChange)route).PropertyChanged += RoutePropertyChanged;
            }
        }

        private void UnsubscribeToRoute()
        {
            if (NetworkRoute != null)
            {
                NetworkRoute.RouteSegmentsUpdated -= RouteSegmentsUpdated;
                ((INotifyPropertyChange)route).PropertyChanged -= RoutePropertyChanged;
            }
        }

        private void RouteSegmentsUpdated(object sender, EventArgs e)
        {
            maxWaterLevelValues = null;
            if (OnDataChanged != null)
            {
                OnDataChanged();
            }
        }

        private void RoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (OnDataChanged != null)
            {
                OnDataChanged();
            }
        }

        /// <summary>
        /// Gets the minimal Y value in the route (structure z or coverage)
        /// </summary>
        public double ZMinValue
        {
            get
            {
                if (minMax == null)
                {
                    minMax = GetMinMax();
                }

                if (minMax == null)
                {
                    return 0;
                }

                return minMax.First;
            }
        }

        /// <summary>
        /// Gets the maximum Y value in the route (structure z or coverage)
        /// </summary>
        public double ZMaxValue
        {
            get
            {
                if (minMax == null)
                {
                    minMax = GetMinMax();
                }

                if (minMax == null)
                {
                    return 0;
                }

                return minMax.Second;
            }
        }
        
        /// <summary>
        /// Gets or sets the water level network coverage
        /// </summary>
        public INetworkCoverage WaterLevelNetworkCoverage
        {
            get { return waterLevelNetworkCoverage; }
            private set
            {
                if (waterLevelNetworkCoverage != null)
                {
                    UnsubscribeFromCoverage(waterLevelNetworkCoverage);
                }

                waterLevelNetworkCoverage = value;

                if (waterLevelNetworkCoverage != null)
                {
                    SubscribeToCoverage(waterLevelNetworkCoverage);
                }
            }
        }

        public IFunction MaxWaterLevelFunction
        {
            get
            {
                return CreateMaxLevelFunction((INetworkCoverage) waterLevelNetworkCoverage?.Parent);
            }
        }

        public IFunction CreateRouteFunctionFromNetworkCoverage(INetworkCoverage coverage, IUnit yUnit)
        {
            if (route == null || coverage == null || (coverage.Time != null && coverage.Time.Values.Count == 0))
                // Last condition: if time-dependent, but no times available, do not create function. 
                return null;

            var locIndex = coverage.Locations.GetValues().ToIndexDictionary();
            var locationsInRoute = RouteHelper.GetLocationsInRoute(coverage, route).Select(l =>
                                              {
                                                  var found = locIndex.TryGetValue(l, out var currentLocIndex);
                                                  return new
                                                  {
                                                      loc = l,
                                                      index = found ? currentLocIndex : -1,
                                                      chainage = RouteHelper.GetRouteChainage(route, l)
                                                  };
                                              })
                                              .OrderBy(l => l.chainage)
                                              .ToList();

            var chainagesValues = locationsInRoute.Select(l => l.chainage).ToArray();

            var values = coverage.GetValues<double>();
            var yValues = locationsInRoute.Select(l => l.index != -1
                                                       ? values[l.index]
                                                       : coverage.Evaluate(l.loc))
                                      .ToArray();

            var chainages = new Variable<double>("Chainage");
            chainages.Unit = new Unit("Chainage", "m");
            FunctionHelper.SetValuesRaw(chainages, (IList)chainagesValues);
            
            var yVar = new Variable<double>(yUnit.Name);
            yVar.Unit = yUnit;
            FunctionHelper.SetValuesRaw<double>(yVar, yValues);

            IFunction function = new Function();
            function.Arguments.Add(chainages);
            function.Components.Add(yVar);
            function.Name = yUnit.Name;

            if (function.Name != null && createdRoutes.ContainsKey(function.Name))
            {
                //prevent memory leaking
                createdRoutes[function.Name].Components = null;
                createdRoutes[function.Name].Arguments = null;
                createdRoutes[function.Name].Store = null;
                createdRoutes[function.Name].Parent = null;
                createdRoutes[function.Name] = function;
            }
            
            return function; 
        }

        public IFunction CreateRouteFunctionFromFeatureCoverage(IFeatureCoverage coverage, IUnit yUnit)
        {
            if (route == null || coverage == null || (coverage.Time != null && coverage.Time.Values.Count == 0))
                // Last condition: if time-dependent, but no times available, do not create function. 
                return null;

            IList<IBranchFeature> filteredFeatures = coverage.Features.OfType<IBranchFeature>().Where(f => RouteHelper.GetRouteChainage(route, f) > -1).ToList();

            var chainagesValues = filteredFeatures.Select(f => RouteHelper.GetRouteChainage(route, (IBranchFeature)f));
            var chainages = new Variable<double>("Chainage");
            chainages.Unit = new Unit("Chainage", "m");
            FunctionHelper.SetValuesRaw<double>(chainages, chainagesValues);

            var yValues = filteredFeatures.Select(coverage.Evaluate<double>);
            var yVar = new Variable<double>(yUnit.Name);
            yVar.Unit = yUnit;
            FunctionHelper.SetValuesRaw<double>(yVar, yValues);

            IFunction function = new Function();
            function.Arguments.Add(chainages);
            function.Components.Add(yVar);
            function.Name = yUnit.Name;
            return function; 
        }

        public IFunction WaterLevelSideViewFunction
        {
            get
            {
                var function = CreateRouteFunctionFromNetworkCoverage(WaterLevelNetworkCoverage, waterLevelUnit);

                if (function == null)
                {
                    return null;
                }
                
                // Adapt the values in the function to show more realistic waterlevels close to structures. 
                // Basically, around each structure, two additional data points are added, with the waterlevel set to the closest waterlevel on that side of the structure. 
                // This has no effect when grid points are added close the structures (which is good practice), but if this is not the case, the sideview will be more realistic. 
                var chainages = function.Arguments[0].GetValues<double>().ToArray();
                var waterlevels = function.Components[0].GetValues<double>().ToArray();

                // Move old values to sorted dictionary
                var dict = new SortedDictionary<double, double>();
                for (int i = 0; i < chainages.Count(); i++)
                {
                    dict[chainages[i]] = waterlevels[i];
                }

                // Add the extra data points in the vicinity of the structures. 
                IList<double> structureRouteChainages = ActiveStructures.Select(struc => RouteHelper.GetRouteChainage(route, struc)).ToList();
                foreach (double structureChainage in structureRouteChainages)
                {
                    double chainage = structureChainage - 0.001;
                    var waterlevel = dict[dict.Select(x => x.Key).Where(x => x <= chainage).Max()];
                    dict[chainage] = waterlevel;

                    chainage = structureChainage + 0.001;
                    waterlevel = dict[dict.Select(x => x.Key).Where(x => x >= chainage).Min()];
                    dict[chainage] = waterlevel;
                }

                function.Arguments[0].Clear();
                function.Components[0].Clear();
                function.Arguments[0].SetValues(dict.Keys);
                function.Components[0].SetValues(dict.Values);
                    
                return function; 
            }
        }

        public IList<INetworkCoverage> ProfileNetworkCoverages { get; private set; }

        public IEnumerable<IFunction> ProfileSideViewFunctions
        {
            get
            {
                return ProfileNetworkCoverages.Select(cov => CreateRouteFunctionFromNetworkCoverage(cov, new Unit(cov.Name, "m AD"))); 
            }
        }

        public IEnumerable<IFunction> PipeSideViewFunctions
        {
            get { return NetworkSideViewHelper.GetPipeSideViewFunctions(route); }
        }

        public IList<INetworkCoverage> RenderedNetworkCoverages
        {
            get { return renderedNetworkCoverages; }
        }

        public IEnumerable<IFunction> RenderedNetworkSideViewFunctions
        {
            get
            {
                return renderedNetworkCoverages.Where(IsValidCoverage).Select(cov => CreateRouteFunctionFromNetworkCoverage(cov, new Unit(cov.Components[0].Name, cov.Components[0].Unit.Symbol)));
            }
        }

        public IList<IFeatureCoverage> RenderedFeatureCoverages
        {
            get { return renderedFeatureCoverages.Where(f => f.IsTimeDependent).ToList(); }
        }

        public IEnumerable<IFunction> RenderedFeatureViewFunctions
        {
            get
            {
                return RenderedFeatureCoverages.Where(IsValidCoverage).Select(cov => CreateRouteFunctionFromFeatureCoverage(cov, new Unit(cov.Components[0].Name, cov.Components[0].Unit.Symbol)));
            }
        }

        public IList<INetworkCoverage> AllNetworkCoverages { get; set; }
        public IList<IFeatureCoverage> AllFeatureCoverages { get; set; }

        public void AddRenderedCoverage(INetworkCoverage networkCoverage)
        {
            if (networkCoverage.Network == null || networkCoverage.Network != NetworkRoute.Network)
            {
                throw new InvalidOperationException(Resources.NetworkSideViewDataController_NetworkOfAddedSpatialDataDoesNotMatchNetworkOfRoute);
            }
            if (RenderedNetworkCoverages.Contains(networkCoverage))
            {
                throw new InvalidOperationException(Resources.NetworkSideViewDataController_NetworkSpatialDataAlreadyInSideviewData);
            }
            if (!AllNetworkCoverages.Contains(networkCoverage))
            {
                throw new InvalidOperationException(Resources.NetworkSideViewDataController_NetworkSpatialDataNotKnownInSideviewData);
            }

            SubscribeToCoverage(networkCoverage);
            renderedNetworkCoverages.Add(networkCoverage);
            ResetMinMaxZ(); //the left axis might have changed..
            FirePropertyChanged(this, new PropertyChangedEventArgs(
                                          nameof(RenderedNetworkCoverages)));
        }

        public void AddRenderedCoverage(IFeatureCoverage featureCoverage)
        {
            if (RenderedFeatureCoverages.Contains(featureCoverage))
            {
                throw new InvalidOperationException(Resources.NetworkSideViewDataController_FeatureSpatialDataAlreadyInSideviewData);
            }
            if (!AllFeatureCoverages.Contains(featureCoverage))
            {
                throw new InvalidOperationException(Resources.NetworkSideViewDataController_FeatureSpatialDataNotKnownInSideviewData);
            }

            SubscribeToCoverage(featureCoverage);
            renderedFeatureCoverages.Add(featureCoverage);
            ResetMinMaxZ(); //the left axis might have changed..
            FirePropertyChanged(this,
                                new PropertyChangedEventArgs(nameof(RenderedFeatureCoverages)));
        }

        public void RemoveRenderedCoverage(INetworkCoverage networkCoverage)
        {
            var networkCoverageToRemove = networkCoverage;

            if (!RenderedNetworkCoverages.Contains(networkCoverage))
            {
                var filteredNetworkCoverage = RenderedNetworkCoverages.FirstOrDefault(nc => nc.Parent == networkCoverage);
                if (filteredNetworkCoverage != null)
                {
                    networkCoverageToRemove = filteredNetworkCoverage;
                }
                else
                {
                    throw new InvalidOperationException(Resources.NetworkSideViewDataController_NetworkSpatialDataNotInSideviewData);
                }
            }

            UnsubscribeFromCoverage(networkCoverageToRemove);
            renderedNetworkCoverages.Remove(networkCoverageToRemove);
            ResetMinMaxZ();
            FirePropertyChanged(this,
                                               new PropertyChangedEventArgs(
                                                   nameof(RenderedNetworkCoverages)));
        }

        public void RemoveRenderedCoverage(IFeatureCoverage featureCoverage)
        {
            var featureCoverageToRemove = featureCoverage;

            if (!RenderedFeatureCoverages.Contains(featureCoverage))
            {
                var filteredFeatureCoverage = RenderedFeatureCoverages.FirstOrDefault(nc => nc.Parent == featureCoverage);
                if (filteredFeatureCoverage != null)
                {
                    featureCoverageToRemove = filteredFeatureCoverage;
                }
                else
                {
                    throw new InvalidOperationException(Resources.NetworkSideViewDataController_FeatureSpatialDataNotInSideviewData);
                }
            }

            UnsubscribeFromCoverage(featureCoverageToRemove);
            renderedFeatureCoverages.Remove(featureCoverageToRemove);
            ResetMinMaxZ();
            FirePropertyChanged(this,
                                               new PropertyChangedEventArgs(
                                                   nameof(RenderedFeatureCoverages)));
        }

        #endregion

        private void UnsubscribeToRouteNetwork()
        {
            if (route != null && route.Network != null && route.Network is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged) route.Network).PropertyChanged -= RouteNetworkPropertyChanged;
                ((INotifyCollectionChange) route.Network).CollectionChanged -= RouteNetworkCollectionChanged;
            }
        }

        private void SubscribeToRouteNetwork()
        {
            if (route != null && route.Network != null && route.Network is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged) route.Network).PropertyChanged += RouteNetworkPropertyChanged;
                ((INotifyCollectionChange) route.Network).CollectionChanged += RouteNetworkCollectionChanged;
            }
        }

        private void RouteNetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((null == Network) || (Network.IsEditing))
            {
                // igore collection when network IsEditing but handle IsEditing property changed event
                return;
            }
            BuildProfileNetworkCoverages();
        }

        private void RouteNetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            minMax = null;
            if (sender is ICrossSectionDefinition) // and in branch part of route
            {
                // geometry changes -> update min max; recreate shape
                if (e.PropertyName != "IsEditing" || !((ICrossSectionDefinition)sender).IsEditing)
                {
                    BuildProfileNetworkCoverages();
                }
            }
            else if (sender is INetwork && e.PropertyName == "IsEditing")
            {
                // possible optimization: only recreate after collection changed
                var network = route.Network;
                if (network != null && !network.IsEditing)
                {
                    BuildProfileNetworkCoverages();
                }
            }
            else if (sender is IBranch && (e.PropertyName == "OrderNumber" || e.PropertyName == "Length"))
            {
                BuildProfileNetworkCoverages();
            }

            FirePropertyChanged(sender, e);
        }

        private void BuildProfileNetworkCoverages()
        {
            ProfileNetworkCoverages.ForEach(nc => nc.Network = null);
            ProfileNetworkCoverages = CreateProfileNetworkCoverages().Where(nc => nc != null).ToList();
        }

        private IEnumerable<INetworkCoverage> CreateProfileNetworkCoverages()
        {
            yield return GetBuildBedLevelCoverage();
            yield return BedLevelNetworkCoverageBuilder.BuildLeftEmbankmentCoverage(route);
            yield return BedLevelNetworkCoverageBuilder.BuildRightEmbankmentCoverage(route);
            yield return BedLevelNetworkCoverageBuilder.BuildLowestEmbankmentCoverage(route);
        }

        private INetworkCoverage GetBuildBedLevelCoverage()
        {
            var buildBedLevelCoverage = BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(route);

            NetworkSideViewHelper.AddPipeSurfaceLevelsInRoute(route, buildBedLevelCoverage);

            return buildBedLevelCoverage;
        }

        // Update minValue and maxValue with minimum and maximum values from function values, NaN-safe.
        public static void UpdateMinMaxFromFunctionValues(IFunction function, ref double minValue, ref double maxValue)
        {
            var mda = function.GetValues<double>();
            var mdaNoNaNs = mda.Where(v => !double.IsNaN(v)).ToList();
            if (mdaNoNaNs.Any())
            {
                minValue = double.IsNaN(minValue) ? mdaNoNaNs.Min() : Math.Min(mdaNoNaNs.Min(), minValue);
                maxValue = double.IsNaN(maxValue) ? mdaNoNaNs.Max() : Math.Max(mdaNoNaNs.Max(), maxValue);
            }
        }

        private static bool IsValidCoverage(IFunction function)
        {
            ICoverage coverage = function as ICoverage;
            if (coverage == null)
                return false; 
            return coverage.Time == null || coverage.Time.Values.Count > 0; 
        }

        private void UpdateMinMax(out double minValue, out double maxValue)
        {
            double min = double.NaN;
            double max = double.NaN;
            if (WaterLevelNetworkCoverage != null && WaterLevelNetworkCoverage.Parent != null)
            {
                UpdateMinMaxFromFunctionValues(WaterLevelNetworkCoverage.Parent, ref min, ref max);
            }
            foreach (var profileNetworkCoverage in ProfileNetworkCoverages)
            {
                UpdateMinMaxFromFunctionValues(profileNetworkCoverage, ref min, ref max); 
            }
            foreach (var renderedNetworkCoverage in RenderedNetworkCoverages.Where(rnc => rnc.Parent != null && IsValidCoverage(rnc.Parent)))
            {
                UpdateMinMaxFromFunctionValues(renderedNetworkCoverage.Parent, ref min, ref max); 
            }
            if (ActiveBranchFeatures != null && ActiveBranchFeatures.Any())
            {
                CompositeStructureViewHelper.UpdateMinMaxForBranchFeatures(ActiveBranchFeatures, ref min, ref max);
            }

            if (ActiveManholes.Any())
            {
                var allCompartments = ActiveManholes
                    .Select(t => t.Item1)
                    .SelectMany(m => m.Compartments)
                    .ToList();

                if (allCompartments.Count > 0)
                {
                    var minManholes = allCompartments.Min(c => c.BottomLevel);
                    var maxManholes = allCompartments.Max(c => c.SurfaceLevel);

                    if (minManholes < min)
                    {
                        min = minManholes;
                    }

                    if (maxManholes > max)
                    {
                        max = maxManholes;
                    }
                }
            }

            if (double.IsNaN(min)) min = -1.0d;
            if (double.IsNaN(max)) max = 1.0d;

            minValue = min;
            maxValue = max;
        }

        private DelftTools.Utils.Tuple<double, double> GetMinMax()
        {
            UpdateMinMax(out double minValue, out double maxValue);
            
            if(Math.Abs(minValue - double.MaxValue) < 1e-10 || Math.Abs(maxValue - double.MinValue) < 1e-10)
            {
                return null;
            }
            
            return new DelftTools.Utils.Tuple<double, double>(minValue, maxValue);
        }

        public void ResetMinMaxZ()
        {
            minMax = null;
        }
        
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed && disposing)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                OnDataChanged = null;
                UnsubscribeToRouteNetwork();
                NetworkSideViewCoverageManager?.Dispose();

                DelayedEventHandlerController.FireEventsChanged -= DelayedEventHandlerFireEventsChanged;

                WaterLevelNetworkCoverage = null;
                foreach (var networkCoverage in ProfileNetworkCoverages)
                {
                    networkCoverage.Network = null;
                }
                ProfileNetworkCoverages.Clear();

                foreach(var cov in RenderedNetworkCoverages)
                {
                    UnsubscribeFromCoverage(cov);
                }

                foreach (var cov in RenderedFeatureCoverages)
                {
                    UnsubscribeFromCoverage(cov);
                }

                delayedCoverageValuesChanged?.Dispose();

                foreach (var createdRoute in createdRoutes)
                {
                    createdRoute.Value.Components = null;
                    createdRoute.Value.Arguments = null;
                    createdRoute.Value.Store = null;
                    createdRoute.Value.Parent = null;
                }
                createdRoutes.Clear();
            }
            disposed = true;
        }
        
        public string GetModelNameForCoverage(ICoverage coverage) => 
            modelNameForCoverageDelegate != null ? modelNameForCoverageDelegate(coverage) : "";

        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(object sender, PropertyChangedEventArgs args) => 
            PropertyChanged?.Invoke(sender, args);
    }
}