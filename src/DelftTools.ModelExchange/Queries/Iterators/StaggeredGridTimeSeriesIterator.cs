using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.ModelExchange.Queries.Iterators
{
    /// <summary>
    /// Transforms the INetworkCoverage to a sequence of (DateTime, double) tuples
    /// </summary>
    internal class StaggeredGridTimeSeriesIterator : NetworkCoverageTimeSeriesIterator
    {
        internal override IEnumerable<Utils.Tuple<DateTime, double>> GetIterator(INetworkCoverage networkCoverage, string locationId)
        {
            ThrowIfLocationIdIsEmpty(locationId);

            const char delimeter = StaggeredGridPointHelper.GridPointDelimeter;
            NetworkLocation location = null;
            if (locationId.Contains(delimeter))
            {
                var tuple = StaggeredGridPointHelper.ParseLocation(locationId);
                var branchName = tuple.First;
                var offset = tuple.Second;

                // get time series at location
                IBranch branch = networkCoverage.Network.Branches.FirstOrDefault(b => b.Name == branchName);
                if (branch != null && !double.IsNaN(offset))
                {
                    location = new NetworkLocation { Branch = branch, Chainage = offset };
                }
            }

            ThrowIfLocationIsNotFound(locationId, location);

            var nearestLocation = GetNearestLocation(networkCoverage, location);

            IFunction function = networkCoverage.GetTimeSeries(nearestLocation);
            if (function != null)
            {
                foreach (var tuple in ToTimesValuesTuples(function))
                    yield return tuple;
            }            
        }
    }
}