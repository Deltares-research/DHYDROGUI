using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.Fews.Assemblers
{
    public class LongitudinalAssemblerBase
    {
        public NetworkCoverage NetworkCoverage { get; set; }
        public Route Route { get; set; }

        protected IEnumerable<INetworkLocation> GetLocationsInRoute()
        {
            var locations = RouteHelper.GetLocationsInRoute(NetworkCoverage, Route);
            return locations.Intersect(NetworkCoverage.Locations.Values);
        }

        protected void ThrowIfNetworkCoverageIsNotValid()
        {
            if (NetworkCoverage == null)
                throw new InvalidOperationException("There is no network spatial data available");

            if (NetworkCoverage.Network == null)
                throw new InvalidOperationException("The network in the network spatial data is empty or null");

            if (NetworkCoverage.Network.Nodes.Count < 2)
                throw new InvalidOperationException("The network has not enough nodes");
        }

    }
}