using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Threading;
using Deltares.Infrastructure.API.Guards;
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
        private IFunction maxWaterLevelValues;
        private readonly SideViewFunctionCreator sideViewFunctionCreator;

        public NetworkSideViewDataController(Route route, NetworkSideViewCoverageManager coverageManager, ModelNameForCoverageDelegate modelNameForCoverageDelegate = null)
        {
            Ensure.NotNull(route, nameof(route));
            
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

            sideViewFunctionCreator = new SideViewFunctionCreator(route, createdRoutes, ActiveStructures, waterLevelUnit);
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
            get => networkSideViewCoverageManager;
            private set
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
                case Route _:
                    return;
                case INetworkCoverage networkCoverage when !Equals(networkCoverage.Network, Network):
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
                    AllFeatureCoverages.Add(FilterWithTime(featureCoverage, null));
                    break;
            }
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
            if (coverage is INetworkCoverage networkCoverage)
            {
                if (WaterLevelNetworkCoverage != null && WaterLevelNetworkCoverage.IsEqualOrDescendant(coverage))
                {
                    WaterLevelNetworkCoverage = null;
                }

                RemoveCoverageFromLists(AllNetworkCoverages, RenderedNetworkCoverages, networkCoverage);
            }
            else if (coverage is IFeatureCoverage featureCoverage)
            {
                RemoveCoverageFromLists(AllFeatureCoverages, RenderedFeatureCoverages, featureCoverage);
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

        public IFunction CreateWaterLevelSideViewFunction()
        {
            IFunction bedLevel = ProfileSideViewFunctions.FirstOrDefault(function => function.Name == BedLevelNetworkCoverageBuilder.BedLevelCoverageName);
            return sideViewFunctionCreator.CreateWaterLevelSideViewFunction(waterLevelNetworkCoverage, bedLevel);
        }

        public IFunction CreateRouteFunctionFromNetworkCoverage(INetworkCoverage coverage, IUnit yUnit)
        {
            return sideViewFunctionCreator.CreateRouteFunctionFromNetworkCoverage(coverage, yUnit);
        }

        public IFunction CreateRouteFunctionFromFeatureCoverage(IFeatureCoverage coverage, IUnit yUnit)
        {
            return sideViewFunctionCreator.CreateRouteFunctionFromFeatureCoverage(coverage, yUnit);
        }

        /// <summary>
        /// Returns the <see cref="IManhole"/>s in the route with there chainage relative to the route
        /// </summary>
        public IEnumerable<System.Tuple<IManhole, double>> ActiveManholes
        {
            get { return NetworkSideViewHelper.GetNodesInRouteWithChainage<IManhole>(NetworkRoute); }
        }

        public IList<IBranchFeature> ActiveBranchFeatures
        {
            get
            {
                var branchFeatures = new List<IBranchFeature>();
                
                // Get all structures covered by the route
                if (!(Network is IHydroNetwork))
                {
                    return null;
                }

                var hydroNetwork = (IHydroNetwork) Network;
                foreach (IStructure1D structure in hydroNetwork.Structures)
                {
                    if (!(structure is ICompositeBranchStructure))
                        continue;

                    double offset = RouteHelper.GetRouteChainage(NetworkRoute, structure);
                    //check if the structure should be active..
                    bool active = (ActiveCompositeStructure == null) ||
                                  (structure == ActiveCompositeStructure);
                    if ((active) && (offset >= 0))
                    {
                        branchFeatures.Add(structure);
                    }
                }

                branchFeatures.AddRange(GetFeaturesInRoute(hydroNetwork.LateralSources.Cast<IBranchFeature>(), NetworkRoute));
                branchFeatures.AddRange(GetFeaturesInRoute(hydroNetwork.Retentions.Cast<IBranchFeature>(), NetworkRoute));
                branchFeatures.AddRange(GetFeaturesInRoute(hydroNetwork.CrossSections.Cast<IBranchFeature>(), NetworkRoute));
                branchFeatures.AddRange(GetFeaturesInRoute(hydroNetwork.ObservationPoints.Cast<IBranchFeature>(), NetworkRoute));

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
                var hydroNetwork = (IHydroNetwork) Network;
                foreach (IStructure1D structure in hydroNetwork.Structures)
                {
                    if (!(structure is ICompositeBranchStructure))
                        continue;

                    double offset = RouteHelper.GetRouteChainage(NetworkRoute, structure);
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
        public INetwork Network => NetworkRoute.Network;

        /// <summary>
        /// Gets or sets the network route data
        /// </summary>
        public Route NetworkRoute
        {
            get => route;
            private set
            {
                route = value;
                SubscribeToRoute();
                SubscribeToRouteNetwork();
            }
        }
        
        private void SubscribeToRoute()
        {
            //listen to segments, since they are (re-)generated after location changes
            NetworkRoute.RouteSegmentsUpdated += RouteSegmentsUpdated;
            ((INotifyPropertyChange)NetworkRoute).PropertyChanged += RoutePropertyChanged;
        }

        private void UnsubscribeToRoute()
        {
            NetworkRoute.RouteSegmentsUpdated -= RouteSegmentsUpdated;
            ((INotifyPropertyChange)NetworkRoute).PropertyChanged -= RoutePropertyChanged;
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

        /// <summary>
        /// Returns the max water level function.
        /// </summary>
        public IFunction MaxWaterLevelFunction
        {
            get
            {
                IFunction bedLevel = ProfileSideViewFunctions.FirstOrDefault(function => function.Name == BedLevelNetworkCoverageBuilder.BedLevelCoverageName);
                if (maxWaterLevelValues == null)
                {
                    maxWaterLevelValues = sideViewFunctionCreator.CreateMaxWaterLevelFunction((INetworkCoverage)waterLevelNetworkCoverage?.Parent, bedLevel);
                }

                return maxWaterLevelValues;
            }
        }
        
        public IList<INetworkCoverage> ProfileNetworkCoverages { get; private set; }

        public IEnumerable<IFunction> ProfileSideViewFunctions
        {
            get
            {
                return ProfileNetworkCoverages.Select(cov => sideViewFunctionCreator.CreateRouteFunctionFromNetworkCoverage(cov, new Unit(cov.Name, "m AD"))); 
            }
        }

        public IEnumerable<IFunction> PipeSideViewFunctions
        {
            get { return NetworkSideViewHelper.GetPipeSideViewFunctions(NetworkRoute); }
        }

        public IList<INetworkCoverage> RenderedNetworkCoverages
        {
            get { return renderedNetworkCoverages; }
        }

        public IEnumerable<IFunction> RenderedNetworkSideViewFunctions
        {
            get
            {
                return renderedNetworkCoverages.Where(IsValidCoverage).Select(cov => sideViewFunctionCreator.CreateRouteFunctionFromNetworkCoverage(cov, new Unit(cov.Components[0].Name, cov.Components[0].Unit.Symbol)));
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
                return RenderedFeatureCoverages.Where(IsValidCoverage).Select(cov => sideViewFunctionCreator.CreateRouteFunctionFromFeatureCoverage(cov, new Unit(cov.Components[0].Name, cov.Components[0].Unit.Symbol)));
            }
        }

        public IList<INetworkCoverage> AllNetworkCoverages { get; set; }
        public IList<IFeatureCoverage> AllFeatureCoverages { get; set; }

        public void AddRenderedCoverage(INetworkCoverage networkCoverage)
        {
            if (networkCoverage.Network == null || networkCoverage.Network != Network)
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
            if (Network is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged) Network).PropertyChanged -= RouteNetworkPropertyChanged;
                ((INotifyCollectionChange) Network).CollectionChanged -= RouteNetworkCollectionChanged;
            }
        }

        private void SubscribeToRouteNetwork()
        {
            if (Network is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged) Network).PropertyChanged += RouteNetworkPropertyChanged;
                ((INotifyCollectionChange) Network).CollectionChanged += RouteNetworkCollectionChanged;
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
                var network = Network;
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
            yield return BedLevelNetworkCoverageBuilder.BuildLeftEmbankmentCoverage(NetworkRoute);
            yield return BedLevelNetworkCoverageBuilder.BuildRightEmbankmentCoverage(NetworkRoute);
            yield return BedLevelNetworkCoverageBuilder.BuildLowestEmbankmentCoverage(NetworkRoute);
        }

        private INetworkCoverage GetBuildBedLevelCoverage()
        {
            var buildBedLevelCoverage = BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(NetworkRoute);

            NetworkSideViewHelper.AddPipeSurfaceLevelsInRoute(NetworkRoute, buildBedLevelCoverage);

            return buildBedLevelCoverage;
        }

        // Update minValue and maxValue with minimum and maximum values from function values, NaN-safe.
        public static void UpdateMinMaxFromFunctionValues(IFunction function, ref double minValue, ref double maxValue)
        {
            var mda = function.GetValues<double>();
            var noDataValue = function.Components.FirstOrDefault()?.NoDataValue as double?;
            
            var mdaNoNaNs = mda.Where(value => IsValidValue(value, noDataValue)).ToList();
            if (mdaNoNaNs.Any())
            {
                minValue = double.IsNaN(minValue) ? mdaNoNaNs.Min() : Math.Min(mdaNoNaNs.Min(), minValue);
                maxValue = double.IsNaN(maxValue) ? mdaNoNaNs.Max() : Math.Max(mdaNoNaNs.Max(), maxValue);
            }
        }
        
        private static bool IsValidValue(double value, double? noDataValue)
        {
            return !double.IsNaN(value) && !value.Equals(noDataValue);
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
                UnsubscribeToRoute();
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