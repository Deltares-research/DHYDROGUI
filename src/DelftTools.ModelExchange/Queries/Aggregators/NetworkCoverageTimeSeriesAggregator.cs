using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange.Queries.Iterators;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.ModelExchange.Queries.Aggregators
{
    /// <summary>
    /// This class is used for quering network coverage related parameters 
    /// in the project tree
    /// </summary>
    public class NetworkCoverageTimeSeriesAggregator : DataAggregationStrategy
    {        
        private IDiscretization computationalGrid;

        public NetworkCoverageTimeSeriesAggregator()
        {
        }

        public NetworkCoverageTimeSeriesAggregator(IDiscretization computationalGrid)
        {
            this.computationalGrid = computationalGrid;
        }

        public override IEnumerable<AggregationResult> GetAll()
        {
            if (DataItems == null)
                yield break;

            IEnumerable<IDataItem> dataItems = DataItems.OfType<IDataItem>().ToList();
            if (computationalGrid == null)
            {
                computationalGrid = dataItems
                    .Where(item => item.ValueType == typeof (Discretization))
                    .Select(item => item.Value)
                    .OfType<IDiscretization>().FirstOrDefault();
            }

            if (computationalGrid == null)
                throw new InvalidOperationException(
                    "There is no computational grid available to extract network spatial data");

            foreach (IDataItem dataItem in dataItems)
            {
                var coverage = dataItem.Value as INetworkCoverage;
                if (coverage == null) 
                    continue;

                if (coverage == computationalGrid)
                    continue;

                string parameterId = coverage.GetParameterId();

                if (IsNullOrEmptyOrWhitespace(parameterId))
                    continue;

                string locationType = coverage.GetLocationType();
                string aggregationType = QueryHelper.GetAggregationType(coverage);

                if (!coverage.IsTimeDependent) 
                    continue;

                if (coverage.Locations != null)
                {
                    string gridType = ((IFunction) coverage).Attributes[FunctionAttributes.StandardFeatureName];

                    if (!gridType.Equals(FunctionAttributes.StandardFeatureNames.ReachSegment))
                    {
                        // Coverage is not based on staggered grid
                        // The grid contains duplicate node
                        IList<string> locationsAdded = new List<string>();
                        foreach (INetworkLocation location in computationalGrid.Locations.Values)
                        {
                            string locationId = location.GetLocationId();
                            if (locationsAdded.Contains(locationId))
                            {
                                continue;
                            }

                            if (IsNullOrEmptyOrWhitespace(locationId))
                                continue;

                            var nl = location as NetworkLocation;
                            string name = nl != null ? nl.LongName : location.Description;

                            locationsAdded.Add(locationId);

                            yield return new AggregationResult
                                         {
                                             Name = name,
                                             LocationId = locationId,
                                             ParameterId = parameterId,
                                             TimeSeries = coverage,                                                     
                                             LocationType = locationType,
                                             AggregationType = aggregationType,
                                             FeatureOwner = coverage,
                                             FeatureOwnerName = coverage.Name,
                                             Feature = location,
                                             ExchangeType = QueryHelper.GetParameterType(dataItem),
                                             TimeSeriesIterator = () => (new NetworkCoverageTimeSeriesIterator
                                                                         {
                                                                             ComputationalGrid = computationalGrid,
                                                                             Coverage = coverage, 
                                                                             LocationId = locationId
                                                                         }).GetIterator()
                                         };
                        }
                    }
                    else
                    {
                        // coverage is based on staggered grid, enable output on staggered locations
                        foreach (INetworkLocation location in coverage.Locations.Values)
                        {
                            string locationId = StaggeredGridPointHelper.GetLocationId(location);

                            if (IsNullOrEmptyOrWhitespace(locationId))
                                continue;

                            var nl = location as NetworkLocation;
                            string name = nl != null ? nl.LongName : location.Description;

                            yield return new AggregationResult
                                         {
                                             LocationId = locationId,
                                             ParameterId = parameterId,
                                             TimeSeries = coverage,
                                             Name = name,
                                             FeatureOwner = coverage,
                                             FeatureOwnerName = coverage.Name,
                                             LocationType = locationType,
                                             AggregationType = aggregationType,                                                     
                                             Feature = location,
                                             ExchangeType = QueryHelper.GetParameterType(dataItem),
                                             TimeSeriesIterator = () => (new StaggeredGridTimeSeriesIterator
                                                                         {
                                                                             Coverage = coverage, 
                                                                             LocationId = locationId
                                                                         }).GetIterator()
                                         };
                        }
                    }
                }
            }
        }
    }
}