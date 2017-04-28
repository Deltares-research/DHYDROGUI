using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DelftTools.ModelExchange.Queries.Iterators
{
    /// <summary>
    /// Class that transforms a feature coverage time series to a sequence of (DateTime, double) DelftTools.Utils.Tuples
    /// </summary>
    internal class FeatureCoverageTimeSeriesIterator : FunctionTimeSeriesIterator
    {
        internal IFeatureCoverage FeatureCoverage { set; private get; }
        internal string LocationId { set; private get; }

        internal override IEnumerable<Utils.Tuple<DateTime, double>> GetIterator()
        {
            return GetIterator(FeatureCoverage, LocationId);
        }

        /// <summary>
        /// Gets an iterator for iterating over feature coverage time series
        /// </summary>
        /// <param name="featureCoverage"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        internal IEnumerable<Utils.Tuple<DateTime, double>> GetIterator(IFeatureCoverage featureCoverage, string locationId)
        {
            ThrowIfLocationIdIsEmpty(locationId);

            IBranchFeature branchFeature =
                featureCoverage.Features.OfType<IBranchFeature>().FirstOrDefault(f => f.Name == locationId);
            if (branchFeature != null)
            {
                IMultiDimensionalArray<DateTime> times = featureCoverage.Time.Values;

                var branchFeatureFilter = new VariableValueFilter<IFeature>(featureCoverage.FeatureVariable, branchFeature);

                IMultiDimensionalArray<double> values = featureCoverage.GetValues<double>(branchFeatureFilter);

                if (values == null)
                    throw new InvalidOperationException(
                        "Can't cast values from feature spatial data to IMultiDimensionalArray<double>");

                for (int i = 0; i < values.Count; i++)
                    yield return new Utils.Tuple<DateTime, double>(times[i], values[i]);
            }
        }
    }
}