using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DelftTools.ModelExchange.Queries
{
    public static class QueryHelper
    {
        public static string GetLocationId(this INetworkLocation location)
        {
            return location.Name;
        }

        public static string GetLocationId(this IBranchFeature feature)
        {
            return feature.Name;
        }

        public static string GetLocationType(this IFeature feature)
        {
            return feature.GetEntityType().Name;
        }

        public static string GetLocationType(this INetworkCoverage coverage)
        {
            return GetLocationType((IFunction) coverage);
        }

        public static string GetLocationType(this IFunction function)
        {
            var locationType = "";
            if (function.Attributes.ContainsKey(FunctionAttributes.LocationType))
            {
                locationType = function.Attributes[FunctionAttributes.LocationType];
            }
            else if (function.Attributes.ContainsKey(FunctionAttributes.StandardFeatureName))
            {
                locationType = function.Attributes[FunctionAttributes.StandardFeatureName];
            }
            return locationType;
        }

        public static string GetParameterId(this IFunction function)
        {
            return GetAttributeOnFirstComponent(function, FunctionAttributes.StandardName);
        }

        public static string GetAggregationType(this ICoverage coverage)
        {
            return GetAttributeOnFirstComponent(coverage, FunctionAttributes.AggregationType);
        }

        public static string GetAttributeOnFirstComponent(this IFunction function, string attributeKey)
        {
            var attributeValue = "";
            if (function.Components.Count > 0)
            {
                var firstComponent = function.Components[0];
                if (firstComponent.Attributes.ContainsKey(attributeKey))
                {
                    attributeValue = firstComponent.Attributes[attributeKey];
                }
            }
            return attributeValue;
        }

        public static string GetAttributeValue(this IFunction function, string attributeName)
        {
            var attributeValue = "";
            if (function.Attributes.ContainsKey(attributeName))
            {
                attributeValue = function.Attributes[attributeName];
            }
            return attributeValue;
        }

        public static string GetLocationId(this IFunction func)
        {
            return GetAttributeValue(func, FunctionAttributes.LocationType);
        }

        /// <summary>
        /// Gets the parameter type (input or output) by looking at DataItemRole
        /// </summary>
        /// <param name="dataItem"></param>
        /// <returns></returns>        
        public static ExchangeType GetParameterType(IDataItem dataItem)
        {
            return (dataItem.Role & DataItemRole.Input) == DataItemRole.Input || (dataItem.Role & DataItemRole.None) == DataItemRole.None ? ExchangeType.Input : ExchangeType.Output;
        }
    }
}