using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.ModelExchange.Queries.Iterators;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using GeoAPI.Extensions.Feature;

namespace DelftTools.ModelExchange.Queries.Aggregators
{
    public class FeatureDataTimeSeriesAggregator : DataAggregationStrategy
    {
        public override IEnumerable<AggregationResult> GetAll()
        {
            if (DataItems == null)
                yield break;

            foreach (var dataItem in DataItems.OfType<IDataItem>())
            {
                var featureData = dataItem.Value as IFeatureData;
                if (featureData == null) 
                    continue;

                var function = featureData.Data as IFunction;
                if (function == null) 
                    continue;

                var locationType = function.GetLocationType();
                // now for most parts empty, in future to be solved in IFeatureData
                if (locationType == string.Empty)
                {
                    locationType = featureData.Feature.GetLocationType();
                }
                var parameterId = QueryHelper.GetParameterId(function);
                if (parameterId == string.Empty)
                {
                    parameterId = function.Name;    
                }
                        
                if (IsNullOrEmptyOrWhitespace(parameterId))
                    continue;

                var featureName = featureData.Feature as INameable;
                if (featureName == null) 
                    continue;

                var locationId = featureName.Name;
                if (IsNullOrEmptyOrWhitespace(locationId))
                    continue;

                if (!IsTimeDependent(function))
                    continue;

                // ------------------------------------------------------------
                // TODO: This is too specific. To make it more generic apply this information in client/caller methods
                var node = featureData.Feature as HydroNode;
                if (node != null && !node.IsConnectedToMultipleBranches)
                {
                    switch (parameterId)
                    {
                        case FunctionAttributes.StandardNames.WaterDischarge:
                            locationType = FunctionAttributes.QBoundary;
                            break;
                        case FunctionAttributes.StandardNames.WaterLevel:
                            locationType = FunctionAttributes.HBoundary;
                            break;
                        // The two cases below are for backwards compatibility
                        // (old time series names are stored in dsproj file)
                        case "flow time series":
                            locationType = FunctionAttributes.QBoundary;
                            break;
                        case "water level time series":
                            locationType = FunctionAttributes.HBoundary;
                            break;
                    }
                }
                // ------------------------------------------------------------

                yield return new AggregationResult
                             {
                                 LocationId = locationId,
                                 ParameterId = parameterId,
                                 Feature = featureData.Feature,
                                 FeatureOwnerName = featureData.Name, 
                                 FeatureOwner = featureData, 
                                 LocationType = locationType,
                                 TimeSeries = function,
                                 TimeSeriesIterator = () => (new FeatureDataTimeSeriesIterator{ FeatureData = featureData }).GetIterator(),
                                 ExchangeType = QueryHelper.GetParameterType(dataItem)
                             };
            }
        }
        
        internal static bool IsTimeDependent(IFunction function)
        {
            return function.Arguments.Count > 0 && function.Arguments[0].ValueType == typeof (DateTime);
        }
    }
}
