using System;
using System.Collections.Generic;
using GeoAPI.Extensions.Coverages;

namespace DelftTools.ModelExchange.Queries
{
    public interface IQueryContext
    {
        IEnumerable<AggregationResult> GetAll();
        IEnumerable<AggregationResult> GetAllBy(Func<AggregationResult, bool> predicate);
        IEnumerable<AggregationResult> GetAllOuputParameters();
        IEnumerable<AggregationResult> GetAllInputItems();
        IDiscretization Discretization { get; }
        bool CacheResults { get; set; }
    }
}