using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.ModelExchange.Queries.Iterators
{
    internal class NetworkCoverageTimeSeriesIterator : FunctionTimeSeriesIterator
    {
        internal INetworkCoverage Coverage { set; private get; }
        internal string LocationId { set; private get; }
        internal IDiscretization ComputationalGrid { set; private get; }

        internal override IEnumerable<Utils.Tuple<DateTime, double>> GetIterator()
        {
            return GetIterator(Coverage, LocationId);
        }

        /// <summary>
        /// Transforms the INetworkCoverage to a sequence of (DateTime, double) DelftTools.Utils.Tuples
        /// </summary>
        /// <param name="networkCoverage"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        internal virtual IEnumerable<Utils.Tuple<DateTime, double>> GetIterator(INetworkCoverage networkCoverage, string locationId)
        {
            ThrowIfLocationIdIsEmpty(locationId);

            if (ComputationalGrid == null)
                throw new InvalidOperationException("There is no computational grid set");

            NetworkLocation location =
                (NetworkLocation)networkCoverage.Locations.Values.FirstOrDefault(l => l.Name == locationId) ??
                (NetworkLocation)ComputationalGrid.Locations.Values.FirstOrDefault(l => l.Name == locationId);

            ThrowIfLocationIsNotFound(locationId, location);

            var nearestLocation = GetNearestLocation(networkCoverage, location);

            IFunction function = networkCoverage.GetTimeSeries(nearestLocation);
            if (function != null)
            {
                foreach (var tuple in ToTimesValuesTuples(function))
                    yield return tuple;
            }
        }

        protected static INetworkLocation GetNearestLocation(INetworkCoverage networkCoverage, NetworkLocation locationToFind)
        {
            bool exactMatch = networkCoverage.Locations.Values.IndexOf(locationToFind) >= 0;
            if (exactMatch)
            {
                return locationToFind;
            }
            throw new ArgumentException(String.Format("location '{0}' not found in network spatial data '{1}'", locationToFind, networkCoverage.Name));
        }
    }
}