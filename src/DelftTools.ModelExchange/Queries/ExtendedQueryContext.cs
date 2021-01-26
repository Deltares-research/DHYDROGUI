using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;

namespace DelftTools.ModelExchange.Queries
{
    public class ExtendedQueryContext : QueryContext, IExtendedQueryContext
    {
        /// <summary>
        /// Create a query context using all known query strategies
        /// </summary>
        /// <param name="itemContainer">The project, model or submodel to query</param>
        /// <returns></returns>
        public ExtendedQueryContext(IItemContainer itemContainer) : base(itemContainer)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemContainer"></param>
        /// <param name="strategy"></param>
        /// <param name="grid"></param>
        public ExtendedQueryContext(IItemContainer itemContainer, DataAggregationStrategy strategy, IDiscretization grid = null)
            : this(itemContainer, new[] { strategy }, grid)
        {
        }

        private ExtendedQueryContext(IItemContainer itemContainer, IList<DataAggregationStrategy> strategies, IDiscretization grid = null)
            : this(itemContainer)
        {
            if (strategies == null) 
                throw new ArgumentNullException("strategies");
            if (strategies.Any(t => t == null))
                throw new ArgumentException("The strategy list contains null elements", "strategies");

            Discretization = grid;
            AggregationStrategies = new List<DataAggregationStrategy>(strategies);

            CacheResults = false;
        }

        /// <summary>
        /// Gets a single input time series query result from the project. This method throws an 
        /// exception if more then one time series is found satisfying the parameterId, locationId 
        /// and Output type. Returns null if no match is found
        /// </summary>
        /// <param name="parameterId"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public AggregationResult GetSingleInputTimeSeries(string parameterId, string locationId)
        {
            ThrowIfLocationIdIsEmpty(locationId);
            ThrowIfParameterIdIsEmpty(parameterId);

            return GetAllByParameterIdAndLocationId(parameterId, locationId)
                .SingleOrDefault(result => result.ExchangeType == ExchangeType.Input);
        }

        /// <summary>
        /// Gets a single output time series query result from the project. This method throws an 
        /// exception if more then one time series is found satisfying the parameterId, locationId 
        /// and Output type
        /// </summary>
        /// <param name="parameterId"></param>
        /// <param name="locationId"></param>
        /// <param name="qualifierId"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public AggregationResult GetSingleOutputTimeSeries(string parameterId, string locationId, string qualifierId = null)
        {
            ThrowIfLocationIdIsEmpty(locationId);
            ThrowIfParameterIdIsEmpty(parameterId);

            IEnumerable<AggregationResult> results = GetAllBy(
                result => result.ExchangeType == ExchangeType.Output && result.ParameterId == parameterId && result.LocationId == locationId);

            return results.SingleOrDefault(result => result.ExchangeType == ExchangeType.Output);
        }

        public IEnumerable<AggregationResult> GetAllByFeatureOwner<T>()
        {
            return GetAllByFeatureOwner(typeof (T));
        }

        public IEnumerable<AggregationResult> GetAllByFeatureOwner(Type type)
        {
            return GetAllBy(r => r.FeatureOwner != null && r.FeatureOwner.GetType() == type);
        }

        public IEnumerable<AggregationResult> GetAllByFeatureType<T>() where T: IFeature
        {
            return GetAllByFeatureType(typeof (T));
        }

        public IEnumerable<AggregationResult> GetAllByFeatureType(Type type)
        {
            if (!typeof(IFeature).IsAssignableFrom(type))
                throw new ArgumentException();

            return GetAllBy(r => r.Feature != null && r.Feature.GetType() == type);
        }

        public IEnumerable<AggregationResult> GetAllByLocationType(string locationType)
        {
            if (IsNullOrEmptyOrWhitespace(locationType))
                throw new ArgumentException("locationType");

            return GetAllBy(result => result.LocationType == locationType);
        }

        public IEnumerable<AggregationResult> GetAllByLocationId(string locationId)
        {
            ThrowIfLocationIdIsEmpty(locationId);

            return GetAllBy(result => result.LocationId == locationId);
        }

        public IEnumerable<AggregationResult> GetAllByParameterId(string parameterId)
        {
            ThrowIfParameterIdIsEmpty(parameterId);

            return GetAllBy(result => result.ParameterId == parameterId);
        }

        public IEnumerable<AggregationResult> GetAllByParameterIdAndLocationId(string parameterId, string locationId)
        {
            ThrowIfLocationIdIsEmpty(locationId);
            ThrowIfParameterIdIsEmpty(parameterId);

            return GetAllBy(result => result.ParameterId == parameterId && result.LocationId == locationId);
        }

        #region Helper methods

        internal static bool IsNullOrEmptyOrWhitespace(string val)
        {
            return (string.IsNullOrEmpty(val) || val.Trim() == String.Empty);
        }

        protected static void ThrowIfLocationIdIsEmpty(string locationId)
        {
            if (IsNullOrEmptyOrWhitespace(locationId))
                throw new ArgumentException("locationId");
        }

        protected static void ThrowIfParameterIdIsEmpty(string parameterId)
        {
            if (IsNullOrEmptyOrWhitespace(parameterId))
                throw new ArgumentException("parameterId");
        }

        #endregion

        public IEnumerable<IGrouping<object, AggregationResult>> GetAllGroupedByFeatureOwner()
        {
            return GetAll().Where(f => f.FeatureOwner != null).GroupBy(f => f.FeatureOwner);
        }
    }
}