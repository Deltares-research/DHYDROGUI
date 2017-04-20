using System.Collections.Generic;
using System.Linq;
using DelftTools.ModelExchange.Queries.Iterators;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DelftTools.ModelExchange.Queries.Aggregators
{
    public class FeatureCoverageTimeSeriesAggregator : DataAggregationStrategy
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FeatureCoverageTimeSeriesAggregator));

        public override IEnumerable<AggregationResult> GetAll()
        {
            if (DataItems == null)
                yield break;

            foreach (var dataItem in DataItems.OfType<IDataItem>())
            {
                var featureCoverage = dataItem.Value as IFeatureCoverage;
                if (featureCoverage == null || !featureCoverage.IsTimeDependent) 
                    continue;

                var parameterId = QueryHelper.GetParameterId(featureCoverage);

                if (IsNullOrEmptyOrWhitespace(parameterId)) 
                    continue;

                if (featureCoverage.Features.Count == 0 && ModelHasBeenInitialized)
                {
                    log.ErrorFormat("Feature spatial data {0} is not initialized (contains no features).",
                                    featureCoverage.Name);
                }

                var aggregationType = QueryHelper.GetAggregationType(featureCoverage);

                foreach (var branchFeature in featureCoverage.Features.OfType<IBranchFeature>())
                {
                    var locationId = branchFeature.GetLocationId();
                    var locationType = branchFeature.GetLocationType();
                        
                    if (IsNullOrEmptyOrWhitespace(locationId))
                        continue;

                    yield return new AggregationResult
                                 {
                                     Name = branchFeature.Description,
                                     ParameterId = parameterId,
                                     LocationId = locationId,
                                     TimeSeries = featureCoverage,                                  
                                     LocationType = locationType,
                                     AggregationType = aggregationType,
                                     Feature = branchFeature,                            
                                     FeatureOwner = featureCoverage,
                                     FeatureOwnerName = featureCoverage.Name,
                                     ExchangeType = QueryHelper.GetParameterType(dataItem),
                                     TimeSeriesIterator = () => (new FeatureCoverageTimeSeriesIterator
                                                                 {
                                                                     FeatureCoverage = featureCoverage, 
                                                                     LocationId = locationId

                                                                 }).GetIterator()
                                 };
                }
            }
        }
    }
}