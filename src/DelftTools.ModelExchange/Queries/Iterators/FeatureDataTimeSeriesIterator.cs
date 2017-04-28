using System;
using System.Collections.Generic;
using DelftTools.Functions;
using GeoAPI.Extensions.Feature;

namespace DelftTools.ModelExchange.Queries.Iterators
{
    /// <summary>
    /// Service for transforming IFeatureData time series to a sequence of (DateTime, double) DelftTools.Utils.Tuples
    /// </summary>
    internal class FeatureDataTimeSeriesIterator : FunctionTimeSeriesIterator
    {
        internal IFeatureData FeatureData { set; private get; }

        internal override IEnumerable<Utils.Tuple<DateTime, double>> GetIterator()
        {
            return GetIterator(FeatureData);
        }

        internal IEnumerable<Utils.Tuple<DateTime, double>> GetIterator(IFeatureData featureData)
        {
            var function = featureData.Data as IFunction;
            if (function == null)
                yield break;

            foreach (var tuple in ToTimesValuesTuples(function))
                yield return tuple;
        }
    }
}