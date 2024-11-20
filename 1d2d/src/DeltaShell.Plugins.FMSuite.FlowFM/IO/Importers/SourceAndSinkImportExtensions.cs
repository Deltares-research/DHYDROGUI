using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public static class SourceAndSinkImportExtensions
    {
        internal const string TimFileColumnAttributePrefix = "TimFileColumn";
        private static readonly ILog Log = LogManager.GetLogger(typeof(SourceAndSinkImportExtensions));
        
        public static void CopyValuesFromFileToSourceAndSinkAttributes(this SourceAndSink sourceAndSink, IFunction functionFromFile)
        {
            if (sourceAndSink == null || sourceAndSink.Feature == null || functionFromFile == null) return;

            var allVariables = functionFromFile.Arguments.Concat(functionFromFile.Components).ToList();

            if(sourceAndSink.Feature.Attributes == null) sourceAndSink.Feature.Attributes = new DictionaryFeatureAttributeCollection();
            var sourceAndSinkFeatureAttributes = sourceAndSink.Feature.Attributes;

            sourceAndSinkFeatureAttributes.RemoveAllWhere(a => a.Key.StartsWith(TimFileColumnAttributePrefix));

            for (var i = 0; i < allVariables.Count(); i++)
            {
                sourceAndSinkFeatureAttributes.Add(new KeyValuePair<string, object>(TimFileColumnAttributePrefix + i, allVariables[i].Values));
            }
        }

        public static void PopulateFunctionValuesFromAttributes(this SourceAndSink sourceAndSink, IDictionary<string, bool> componentSettings)
        {
            if (sourceAndSink == null || sourceAndSink.Feature == null || sourceAndSink.Function == null) return;
            var attributesFromTimFile = sourceAndSink.Feature.Attributes.Where(a => a.Key.StartsWith(TimFileColumnAttributePrefix)).ToList();

            var sourceAndSinkFunction = sourceAndSink.Function;
            sourceAndSinkFunction.Clear();

            var namesForActiveVariablesInFunction = sourceAndSinkFunction.Arguments
                .Concat(sourceAndSinkFunction.Components
                    .Where(c =>
                    {
                        bool componentIsActive;

                        // always default to true unless we explicitly say false
                        return componentSettings == null || (!componentSettings.TryGetValue(c.Name, out componentIsActive) || componentIsActive);
                    })
                )
                .Select(v => v.Name).ToList();

            var numberOfColumnsToCopy = 0;           
            if (attributesFromTimFile.Count > namesForActiveVariablesInFunction.Count)
            {
                Log.WarnFormat(Resources.SourceAndSinkImportExtensions_GenerateFunctionFromAttributes_There_were_more_columns_in_the___tim_file_for__0__than_expected, sourceAndSink.Name);
                numberOfColumnsToCopy = namesForActiveVariablesInFunction.Count;
            }
            else 
            {
                if (attributesFromTimFile.Count < namesForActiveVariablesInFunction.Count)
                {
                    Log.WarnFormat(Resources.SourceAndSinkImportExtensions_GenerateFunctionFromAttributes_There_were_less_columns_in_the___tim_file_for__0__than_expected, sourceAndSink.Name);
                }

                numberOfColumnsToCopy = attributesFromTimFile.Count;
            }
            
            for (var i = 0; i < numberOfColumnsToCopy; i++)
            {
                var matchingVariable = sourceAndSinkFunction.Arguments
                    .Concat(sourceAndSinkFunction.Components)
                    .First(v => v.Name == namesForActiveVariablesInFunction[i]);

                var matchingAttribute = attributesFromTimFile.First(a => a.Key == TimFileColumnAttributePrefix + i);

                matchingVariable.Values = (IMultiDimensionalArray)matchingAttribute.Value;
            }

            // Finally, remove the attributes
            sourceAndSink.Feature.Attributes.RemoveAllWhere(a => a.Key.StartsWith(TimFileColumnAttributePrefix));
        }

    }
}