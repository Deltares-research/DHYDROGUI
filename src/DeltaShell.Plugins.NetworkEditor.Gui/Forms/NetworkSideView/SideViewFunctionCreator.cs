using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Units;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    /// <summary>
    /// Class for creating <see cref="IFunction"/> for the side view.
    /// </summary>
    public class SideViewFunctionCreator
    {
        private const string chainageName = "Chainage";
        private const string chainageUnitName = "m";

        private readonly IDictionary<string, IFunction> createdRoutes;
        private readonly IList<IStructure1D> activeStructures;
        private readonly Route route;
        private readonly IUnit waterLevelUnit;

        /// <summary>
        /// Initializes a new instance of <see cref="SideViewFunctionCreator"/>.
        /// </summary>
        /// <param name="route">The route to create functions for.</param>
        /// <param name="createdRoutes">The routes that have already been created.</param>
        /// <param name="activeStructures">The active structures for the route.</param>
        /// <param name="waterLevelUnit">The water level unit.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public SideViewFunctionCreator(Route route,
                                       IDictionary<string, IFunction> createdRoutes,
                                       IList<IStructure1D> activeStructures,
                                       IUnit waterLevelUnit)
        {
            Ensure.NotNull(route, nameof(route));
            Ensure.NotNull(createdRoutes, nameof(createdRoutes));
            Ensure.NotNull(activeStructures, nameof(activeStructures));
            Ensure.NotNull(waterLevelUnit, nameof(waterLevelUnit));

            this.route = route;
            this.createdRoutes = createdRoutes;
            this.activeStructures = activeStructures;
            this.waterLevelUnit = waterLevelUnit;
        }
        
        /// <summary>
        /// Creates the water level function for the side view.
        /// </summary>
        /// <returns>The created water level function.</returns>
        public IFunction CreateWaterLevelSideViewFunction(INetworkCoverage waterLevelCoverage)
        {
            if (DataRequiredToCreateFunctionIsMissing(waterLevelCoverage))
            {
                return null;
            }
            
            if (WaterLevelNetworkCoverageHasNoValues(waterLevelCoverage))
            {
                return null;
            }

            IEnumerable<LocationInRoute> locationsInRoute = GetLocationsInRoute(waterLevelCoverage);
            IEnumerable<double> locationChainages = GetChainageForLocations(locationsInRoute);
            IEnumerable<double> waterLevels = GetWaterLevelForLocations(locationsInRoute, waterLevelCoverage);

            IFunction function = CreateFunctionFromChainagesAndYValues(locationChainages, waterLevels, waterLevelUnit);

            if (function != null)
            {
                UpdateWaterLevelFunctionToBetterRepresentStructures(function);
            }

            return function;
        }
        
        private static bool WaterLevelNetworkCoverageHasNoValues(INetworkCoverage networkCoverage)
        {
            if (networkCoverage?.Locations?.Values == null
                || !networkCoverage.Locations.Values.Any())
            {
                return true;
            }

            var firstNetworkLocationOfNetworkCoverage = new VariableValueFilter<INetworkLocation>(networkCoverage.Locations, networkCoverage.Locations.Values[0]);

            // check on first timestep of first network location if there is a value
            return networkCoverage.GetValues(firstNetworkLocationOfNetworkCoverage).Count == 0;
        }

        /// <summary>
        /// Creates a function for a route from a network coverage.
        /// </summary>
        /// <param name="coverage">The network coverage to create a function from.</param>
        /// <param name="yUnit">The y unit.</param>
        /// <returns>The created function.</returns>
        public IFunction CreateRouteFunctionFromNetworkCoverage(INetworkCoverage coverage, IUnit yUnit)
        {
            if (DataRequiredToCreateFunctionIsMissing(coverage))
            {
                return null;
            }

            IEnumerable<LocationInRoute> locationsInRoute = GetLocationsInRoute(coverage);
            IEnumerable<double> locationChainages = GetChainageForLocations(locationsInRoute);
            IEnumerable<double> yValues = GetYValueForLocations(locationsInRoute, coverage);

            return CreateFunctionFromChainagesAndYValues(locationChainages, yValues, yUnit);
        }

        /// <summary>
        /// Creates a function for a route from a feature coverage.
        /// </summary>
        /// <param name="coverage">The feature coverage to create a function from.</param>
        /// <param name="yUnit">The y unit.</param>
        /// <returns>The created function.</returns>
        public IFunction CreateRouteFunctionFromFeatureCoverage(IFeatureCoverage coverage, IUnit yUnit)
        {
            if (DataRequiredToCreateFunctionIsMissing(coverage))
            {
                return null;
            }

            IList<IBranchFeature> filteredFeatures = coverage.Features
                                                             .OfType<IBranchFeature>()
                                                             .Where(branchFeature => RouteHelper.GetRouteChainage(route, branchFeature) > -1)
                                                             .ToList();

            IEnumerable<double> chainagesValues = filteredFeatures.Select(
                branchFeature => RouteHelper.GetRouteChainage(route, branchFeature));
            IVariable chainageVariable = CreateChainageVariable(chainagesValues);

            IEnumerable<double> yValues = filteredFeatures.Select(coverage.Evaluate<double>);
            IVariable yVariable = CreateYVariable(yValues, yUnit);

            IFunction function = CreateNewFunction(chainageVariable, yVariable);

            return function; 
        }

        private static IFunction CreateNewFunction(IVariable chainageVariable, IVariable yVariable)
        {
            IFunction function = new Function();

            function.Arguments.Add(chainageVariable);
            function.Components.Add(yVariable);
            function.Name = yVariable.Name;

            return function;
        }

        private bool DataRequiredToCreateFunctionIsMissing(ICoverage coverage)
        {
            return route == null
                   || coverage == null 
                   || (coverage.Time != null && coverage.Time.Values.Count == 0);
        }
        
        private IEnumerable<LocationInRoute> GetLocationsInRoute(INetworkCoverage coverage)
        {
            IDictionary<INetworkLocation, int> locationIndexLookup = coverage.Locations.GetValues().ToIndexDictionary();
            
            List<LocationInRoute> locationsInRoute =  RouteHelper.GetLocationsInRoute(coverage, route)
                                                                 .Select(networkLocation => CreateLocationInRoute(networkLocation, locationIndexLookup))
                                                                 .OrderBy(locationInRoute => locationInRoute.Chainage)
                                                                 .ToList();
            return locationsInRoute;
        }
        
        private LocationInRoute CreateLocationInRoute(INetworkLocation networkLocation, 
                                                      IDictionary<INetworkLocation, int> locationIndexLookup)
        {
            bool found = locationIndexLookup.TryGetValue(networkLocation, out int currentLocIndex);
            int index = found ? currentLocIndex : -1;
            double chainage = RouteHelper.GetRouteChainage(route, networkLocation);
                                                  
            return new LocationInRoute(networkLocation, index, chainage);
        }
        
        private static IEnumerable<double> GetChainageForLocations(IEnumerable<LocationInRoute> locationsInRoute)
        {
            return locationsInRoute.Select(locationInRoute => locationInRoute.Chainage).ToArray();
        }
        
        private static IEnumerable<double> GetWaterLevelForLocations(IEnumerable<LocationInRoute> locationsInRoute, 
                                                                     INetworkCoverage waterLevelCoverage)
        {
            IMultiDimensionalArray<double> waterLevels = waterLevelCoverage.GetValues<double>();
            return locationsInRoute.Select(locationInRoute => GetWaterLevelForLocation(locationInRoute, 
                                                                                       waterLevels,
                                                                                       waterLevelCoverage)).
                                    ToArray();
        }
        
        private static double GetWaterLevelForLocation(LocationInRoute locationInRoute, 
                                                       IMultiDimensionalArray<double> waterLevels,
                                                       INetworkCoverage waterLevelCoverage)
        {
            if (locationInRoute.Location.Branch is IPipe pipe)
            {
                return locationInRoute.Index != -1
                           ? Math.Max(waterLevels[locationInRoute.Index], pipe.LevelTarget)
                           : Math.Max(waterLevelCoverage.Evaluate(locationInRoute.Location), pipe.LevelTarget);
            }
            
            return locationInRoute.Index != -1
                       ? waterLevels[locationInRoute.Index]
                       : waterLevelCoverage.Evaluate(locationInRoute.Location);
        }
        
        private IFunction CreateFunctionFromChainagesAndYValues(IEnumerable<double> locationChainages, 
                                                                IEnumerable<double> yValues, 
                                                                IUnit yUnit)
        {
            IVariable chainageVariable = CreateChainageVariable(locationChainages);
            IVariable yVariable = CreateYVariable(yValues, yUnit);
            
            IFunction function = CreateNewFunction(chainageVariable, yVariable);

            if (function.Name != null && createdRoutes.ContainsKey(function.Name))
            {
                CleanUpCreatedRoutes(function);
            }

            return function;
        }
        
        private static IVariable CreateChainageVariable(IEnumerable<double> locationChainages)
        {
            var chainageVariable = new Variable<double>(chainageName)
            {
                Unit = new Unit(chainageName, chainageUnitName)
            };

            FunctionHelper.SetValuesRaw(chainageVariable, locationChainages);

            return chainageVariable;
        }
        
        private static IVariable CreateYVariable(IEnumerable<double> yValues, IUnit yUnit)
        {
            var yVariable = new Variable<double>(yUnit.Name)
            {
                Unit = yUnit
            };

            FunctionHelper.SetValuesRaw<double>(yVariable, yValues);

            return yVariable;
        }
        
        private void CleanUpCreatedRoutes(IFunction function)
        {
            createdRoutes[function.Name].Components = null;
            createdRoutes[function.Name].Arguments = null;
            createdRoutes[function.Name].Store = null;
            createdRoutes[function.Name].Parent = null;
            createdRoutes[function.Name] = function;
        }
        
        private void UpdateWaterLevelFunctionToBetterRepresentStructures(IFunction function)
        {
            /*
             * Adapt the values in the function to show more realistic water levels close to structures.
             * Basically, around each structure, two additional data points are added, with the water level
             * set to the closest water level on that side of the structure.
             * This has no effect when grid points are added close the structures (which is good practice),
             * but if this is not the case, the side view will be more realistic. 
             */

            double[] structureChainages = GetOrderedActiveStructureChainagesFromRoute();

            SideViewWaterLevelFunctionUpdater.UpdateFunctionWithExtraDataPointsForStructures(function, 
                                                                                             structureChainages);
        }

        private double[] GetOrderedActiveStructureChainagesFromRoute()
        {
            return activeStructures.Select(structure => RouteHelper.GetRouteChainage(route, structure)).OrderBy(c => c).ToArray();
        }
        
        private static IEnumerable<double> GetYValueForLocations(IEnumerable<LocationInRoute> locationsInRoute, 
                                                                 INetworkCoverage networkCoverage)
        {
            IMultiDimensionalArray<double> yValues = networkCoverage.GetValues<double>();

            return locationsInRoute.Select(locationInRoute => GetYValueForLocation(locationInRoute, networkCoverage, yValues)).ToArray();
        }
        
        private static double GetYValueForLocation(LocationInRoute locationInRoute, 
                                                   INetworkCoverage networkCoverage, 
                                                   IMultiDimensionalArray<double> yValues)
        {
            return locationInRoute.Index != -1
                       ? yValues[locationInRoute.Index]
                       : networkCoverage.Evaluate(locationInRoute.Location);
        }

        private sealed class LocationInRoute
        {
            public LocationInRoute(INetworkLocation location, int index, double chainage)
            {
                Location = location;
                Index = index;
                Chainage = chainage;
            }

            public INetworkLocation Location { get; }
            public int Index { get; }
            public double Chainage { get; }
        }
    }
}