using System.Collections.Generic;

namespace DelftTools.ModelExchange.Queries
{
    public interface IExtendedQueryContext : IQueryContext
    {
        AggregationResult GetSingleInputTimeSeries(string parameterId, string locationId);
        AggregationResult GetSingleOutputTimeSeries(string parameterId, string locationId, string qualifierId = null);
        IEnumerable<AggregationResult> GetAllByParameterId(string parameterId);
        IEnumerable<AggregationResult> GetAllByLocationId(string locationId);
        IEnumerable<AggregationResult> GetAllByLocationType(string locationType);
    }
}