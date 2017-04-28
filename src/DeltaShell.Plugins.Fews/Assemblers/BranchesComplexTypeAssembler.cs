using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.Fews.Assemblers
{
    /// y = y(t, nl=(b, l))
    /// r = r(nl=(b, l))
    /// 
    /// y_r_locations = GetLocationsInRoute(y, r)
    /// 
    /// for each time in y.t
    ///     y.get_values(t_current, y_r_locations)
    public class BranchesComplexTypeAssembler : LongitudinalAssemblerBase
    {
        /// <summary>
        /// Assembles a data transfer object for branches. 
        /// Data is extracted from the given NetworkCoverage and Route. 
        /// The data is added to the given complex type
        /// </summary>
        /// <param name="locTypeExtension">type of the route (reach_segments or grid_points)</param>
        /// <param name="branchesComplexType">XML object containing the profile definition</param>
        internal void AssembleDto(string locTypeExtension, BranchesComplexType branchesComplexType)
        {
            ThrowIfNetworkCoverageIsNotValid();

            IEnumerable<INetworkLocation> locationsInRoute = GetLocationsInRoute().ToList();

            if (Route.Locations.Values.Count < 2 || locationsInRoute.Count() <2)
                throw new InvalidOperationException("There should be a route defined, with at least two locations (start and end chainage)");


            var firstNetworkLocation = locationsInRoute.FirstOrDefault();
            var lastNetworkLocation = locationsInRoute.LastOrDefault();
            var branchComplexType = new BranchComplexType
            {
                branchName = Route.Name + locTypeExtension,
                id = Route.Name + locTypeExtension,
                startChainage = GetRouteOffset(),
                endChainage = GetRouteLength(),
                upNode = firstNetworkLocation != null ? firstNetworkLocation.Name : string.Empty,
                downNode = lastNetworkLocation != null ? lastNetworkLocation.Name : string.Empty,
            };

            
            foreach (NodePointComplexType nodePointComplexType in locationsInRoute.Select(GetNodePoint))
            {
                branchComplexType.pt.Add(nodePointComplexType);
            }

            branchesComplexType.branch.Add(branchComplexType);
        }

        private double GetRouteOffset()
        {
            var firstLocation = Route.Locations.Values.First();
            return RouteHelper.GetRouteChainage(Route, firstLocation);
        }

        private double GetRouteLength()
        {
            return RouteHelper.GetRouteLength(Route);
        }

        
        private NodePointComplexType GetNodePoint(INetworkLocation networkLocation)
        {
            var nodePointComplexType = new NodePointComplexType()
            {
                chainage = RouteHelper.GetRouteChainage(Route, networkLocation),
                label = networkLocation.Name, //longname, description
                description = networkLocation.LongName,
                x = networkLocation.Geometry.Coordinate.X,
                xSpecified = !double.IsNaN(networkLocation.Geometry.Coordinate.X),
                y = networkLocation.Geometry.Coordinate.Y,
                ySpecified = !double.IsNaN(networkLocation.Geometry.Coordinate.Y),
                z = networkLocation.Geometry.Coordinate.Z,
                zSpecified = !double.IsNaN(networkLocation.Geometry.Coordinate.Z),
            };
            return nodePointComplexType;
        }
    }
}