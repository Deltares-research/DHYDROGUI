
using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.ModelExchange.Queries.Aggregators;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.ModelExchange.Queries
{
    public class QueryContext : IQueryContext
    {
        public IDiscretization Discretization { get; protected set; }

        private IEnumerable<AggregationResult> cachedResults;
        protected IList<DataAggregationStrategy> AggregationStrategies;
        private readonly IItemContainer itemContainer;

        /// <summary>
        /// Create a query context using all known query strategies
        /// </summary>
        /// <param name="itemContainer">The project, model or submodel to query</param>
        /// <returns></returns>
        public QueryContext(IItemContainer itemContainer)
        {
            if (itemContainer == null)
                throw new ArgumentNullException("itemContainer");

            this.itemContainer = itemContainer;

            AggregationStrategies = new List<DataAggregationStrategy>
                                  {
                                      new DataItemTimeSeriesAggregator(),
                                      new FeatureCoverageTimeSeriesAggregator(),
                                      new FeatureDataTimeSeriesAggregator()
                                  };
            Discretization = itemContainer.GetAllItemsRecursive().OfType<IDataItem>()
                .Where(item => item.ValueType == typeof(Discretization)).Select(item => item.Value)
                .OfType<IDiscretization>().FirstOrDefault();
            if (Discretization != null)
            {
                AggregationStrategies.Add(new NetworkCoverageTimeSeriesAggregator(Discretization));
            }

            CacheResults = false;
        }

        public IEnumerable<AggregationResult> GetAll()
        {
            if (CacheResults)
                return cachedResults ?? (cachedResults = GetAllResults());
            return GetAllResults();
        }

        public IEnumerable<AggregationResult> GetAllBy(Func<AggregationResult, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException("predicate");

            return GetAll().Where(predicate);
        }

        public IEnumerable<AggregationResult> GetAllOuputParameters()
        {
            return GetAllBy(result => result.ExchangeType == ExchangeType.Output);
        }

        public IEnumerable<AggregationResult> GetAllInputItems()
        {
            return GetAllBy(result => result.ExchangeType == ExchangeType.Input);
        }

        /// <summary>
        /// Gets or sets a boolen value indicating that the results of the 
        /// first aggregation collection should be cached
        /// </summary>
        /// <remarks>
        /// This value can be useful in situations where one would like to query the model more then once and 
        /// the aggregation context is stable. This speeds up the performance because the aggregation process 
        /// is expensive...
        /// </remarks>
        public bool CacheResults { get; set; }

        private IEnumerable<AggregationResult> GetAllResults()
        {
            IEnumerable<object> dataItems = itemContainer.GetAllItemsRecursive()
                                                                   .OfType<IDataItem>()
                                                                   .Cast<object>().ToList();

            foreach (DataAggregationStrategy strategy in AggregationStrategies)
            {
                strategy.DataItems = dataItems;
                foreach (AggregationResult series in strategy.GetAll())
                {
                    yield return series;
                }
            }
        }
    }
}
