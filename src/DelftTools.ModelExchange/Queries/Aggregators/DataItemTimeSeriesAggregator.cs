using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange.Queries.Iterators;
using DelftTools.Shell.Core.Workflow.DataItems;

namespace DelftTools.ModelExchange.Queries.Aggregators
{
    public class DataItemTimeSeriesAggregator : DataAggregationStrategy
    {
        public override IEnumerable<AggregationResult> GetAll()
        {
            if (DataItems == null)
                yield break;

            const char delimeter = '.';

            foreach (IDataItem dataItem in DataItems.OfType<IDataItem>())
            {
                var function = dataItem.Value as IFunction;
                if (function != null)
                {
                    if (!IsMultipleComponentFunction(function))
                    {
                        bool isCandidate = !string.IsNullOrEmpty(dataItem.Name) &&
                                           (dataItem.Name.Contains(delimeter) ||
                                            (dataItem.IsLinked && dataItem.LinkedTo.Name.Contains(delimeter)));
                        if (isCandidate)
                        {
                            // get/parse the parameter and location id
                            var names = dataItem.IsLinked
                                            ? dataItem.LinkedTo.Name.Split(delimeter)
                                            : dataItem.Name.Split(delimeter);

                            yield return new AggregationResult
                                             {
                                                 TimeSeries = function,
                                                 LocationId = names[0],
                                                 ParameterId = names[1],
                                                 FeatureOwnerName = dataItem.Name,
                                                 ExchangeType = QueryHelper.GetParameterType(dataItem),
                                                 TimeSeriesIterator = () => FunctionTimeSeriesIterator.ToTimesValuesTuples(function)
                                             };
                        }
                    }
                    else
                    {
                        for (int i = 0; i < function.Components.Count; i++)
                        {
                            var func = function.Components[i];
                            var locationType = func.GetLocationType();
                            // now for most parst empty, in future to be solved in IFeatureData
                            if (locationType == string.Empty)
                            {
                                locationType = dataItem.Name;
                            }

                            var locationId = function.GetLocationId();
                            if (IsNullOrEmptyOrWhitespace(locationId))
                            {
                                locationId = func.Name;
                            }

                            var parameterId = QueryHelper.GetParameterId(func);
                            if (parameterId == string.Empty)
                            {
                                parameterId = func.Name;
                            }

                            yield return new AggregationResult
                                             {
                                                 TimeSeries = func,
                                                 LocationId = locationId,
                                                 LocationType = locationType,
                                                 ParameterId = parameterId,
                                                 FeatureOwnerName = dataItem.Name,
                                                 ExchangeType = QueryHelper.GetParameterType(dataItem),
                                                 TimeSeriesIterator = () => FunctionTimeSeriesIterator.ToTimesValuesTuples(func)
                                             };
                        }
                    }
                }
            }
        }

        private bool IsMultipleComponentFunction(IFunction function)
        {
            return function.Components.Count > 1;
        }
    }
}