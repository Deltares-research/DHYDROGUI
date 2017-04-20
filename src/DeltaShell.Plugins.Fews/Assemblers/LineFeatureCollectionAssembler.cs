using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange;
using DelftTools.ModelExchange.Queries;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.Fews.Assemblers
{
    public static class LineFeatureCollectionAssembler
    {
        public static IList<DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>> Assemble(IEnumerable<AggregationResult> context, INetworkCoverage discretization, INetworkCoverage staggeredGridResults=null)
        {
            if (context == null && (staggeredGridResults == null || discretization == null))
                throw new InvalidOperationException("There is no context set to build a lookup from. Please set the context or set the water level and water discharge spatial data");

            if (staggeredGridResults == null)
            {
                // try to find Q in the context
                AggregationResult result = context.FirstOrDefault(c => c.LocationType == FunctionAttributes.StandardFeatureNames.ReachSegment && c.ExchangeType == ExchangeType.Output);
                if (result == null)
                    throw new InvalidOperationException("There are no results available on the staggered grid points");
                staggeredGridResults = result.FeatureOwner as INetworkCoverage;
                if (staggeredGridResults == null)
                    throw new InvalidOperationException("There is no network spatial data for the staggered grid points");
            }

            if (staggeredGridResults.Network == null)
            {
                throw new InvalidOperationException("There is no network to assemble the collection with");
            }

            if (discretization == null)
                throw new InvalidOperationException("There is no network spatial data set to assemble the collection with");

            if (discretization.Network == null)
            {
                throw new InvalidOperationException("There is no network to assemble the collection with");
            }

            var segmentsAdded = new HashSet<string>();
            var linesFeatureCollection = new List<DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>>();

            foreach (var segment in discretization.Segments.Values)
            {
                // parse the segment information to write in the dbf file
                var segmentName = segment.ToString().Trim();

                var location = GetLocationsForSegment(segment, staggeredGridResults).Single();
                string locationId = StaggeredGridPointHelper.GetLocationId(location);
                const string locationType = FunctionAttributes.StandardFeatureNames.ReachSegment; //waterDischarge.GetLocationType();

                var fromLocation = GetMatchingLocation(discretization, segment.Branch, segment.Chainage);
                var toLocation = GetMatchingLocation(discretization, segment.Branch, segment.EndChainage);

                if (segment.Geometry.IsEmpty)
                {
                    Console.WriteLine("NO GEOMETRY DEFINED FOR SEGMENT,id=" + segment.Id + ",name=" + segment.Name +
                        ",nr=" + segment.SegmentNumber + ",locID=" + locationId + ",from=" + fromLocation + ",to=" + toLocation);
                    continue;
                }

                if (segmentsAdded.Contains(segmentName))
                    continue; // filter out segments already added

                segmentsAdded.Add(segmentName);

                // collect the attribute data for this feature
                // (will be written to the dbf file)
                var attributes = new Dictionary<string, object>
                                     {
                                         {"ID", locationId},
                                         {"NAME", segmentName },
                                         {"TYPE", locationType },
                                         {"ID_FROM", fromLocation.GetLocationId() ?? "-"},
                                         {"ID_TO", toLocation.GetLocationId() ?? "-"},
                                         {"X_CENTRE", location.Geometry.Coordinate.X},
                                         {"Y_CENTRE", location.Geometry.Coordinate.Y},
                                         {"LENGTH_MAP", segment.Length},
                                     };

                // create and add the feature to the collection
                var feature = new DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>(segment.Geometry, attributes);
                linesFeatureCollection.Add(feature);
            }

            return linesFeatureCollection;
        }

        private static INetworkLocation GetMatchingLocation(INetworkCoverage discretization, GeoAPI.Extensions.Networks.IBranch branch, double offset)
        {
            return discretization.Locations.Values.First(nl => nl.Branch == branch && (Math.Abs(nl.Chainage - offset) < BranchFeature.Epsilon));
        }

        private static IEnumerable<INetworkLocation> GetLocationsForSegment(INetworkSegment segment, INetworkCoverage coverage)
        {
            double min = Math.Min(segment.EndChainage, segment.Chainage);
            double max = Math.Max(segment.EndChainage, segment.Chainage);

            return coverage.Locations.Values.Where(
                nl =>
                nl.Branch == segment.Branch && nl.Chainage >= min && nl.Chainage <= max);
        }
    }
}