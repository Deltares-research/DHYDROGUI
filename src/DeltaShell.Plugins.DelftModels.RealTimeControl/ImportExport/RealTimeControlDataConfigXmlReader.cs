using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlDataConfigXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlDataConfigXmlReader));

        public static IList<ConnectionPoint> GetConnectionPointsFromXmlElements(List<RTCTimeSeriesXML> elements, string tag, IHydroModel model)
        {
            if (tag != RtcDataConfigTag.Input || tag != RtcDataConfigTag.Output) return null;

            var connectionPointElements = elements.Where(e => e.id.StartsWith(tag) && e.OpenMIExchangeItem.elementId != null);

            var connectionPoints = new List<ConnectionPoint>();

            foreach (var connectionPointElement in connectionPointElements)
            {
                var id = connectionPointElement.id;

                var connectionPointItem = connectionPointElement.OpenMIExchangeItem;

                var connectionPointName = id.Substring(tag.Length);
                var featureName = connectionPointItem.elementId;
                var parameterName = connectionPointItem.quantityId;
                var linkedFeature = model.Region.AllHydroObjects.First(o => o.Name == featureName);

                if (string.IsNullOrEmpty(featureName) || string.IsNullOrEmpty(parameterName))
                    Log.Warn($"The element with id '{id}' needs to have an elementId and quantityId. See file: '{RealTimeControlXMLFiles.XmlData}'.");

                if (linkedFeature == null)
                    Log.Warn($"Element with id '{id}' does not have a corresponding feature in the model. See file: '{RealTimeControlXMLFiles.XmlData}'.");

                ConnectionPoint connectionPoint;

                switch (tag)
                {
                    case RtcDataConfigTag.Input:
                        connectionPoint = new Input();
                        break;
                    case RtcDataConfigTag.Output:
                        connectionPoint = new Output();
                        break;
                    default:
                        continue;
                }

                connectionPoint.Name = connectionPointName;
                connectionPoint.ParameterName = parameterName;
                connectionPoint.Feature = linkedFeature;

                connectionPoints.Add(connectionPoint);
            }

            return connectionPoints;
        }

        public static IList<StandardCondition> GetStandardConditionsFromXmlElements(List<RTCTimeSeriesXML> elements)
        {
            var standardConditionElements = elements.Where(e => string.IsNullOrEmpty(e.PITimeSeries.locationId) && string.IsNullOrEmpty(e.OpenMIExchangeItem.elementId) && e.id.Contains(RtcDataConfigTag.StandardCondition));

            var standardConditions = new List<StandardCondition>();

            foreach (var standardConditionElement in standardConditionElements)
            {
                var id = standardConditionElement.id;

                var splitId = id.Split(new[] { RtcDataConfigTag.StandardCondition }, StringSplitOptions.None);

                var controlGroupName = splitId.ElementAt(1);
                var conditionName = splitId.ElementAt(2);

                if (string.IsNullOrEmpty(controlGroupName) || string.IsNullOrEmpty(conditionName))
                    Log.Warn($"We could not defer the control group name and the condition name based on the id '{id}'. See file: '{RealTimeControlXMLFiles.XmlData}'");

                var standardCondition = new StandardCondition()
                {
                    Name = conditionName
                };

                standardConditions.Add(standardCondition);
            }

            return standardConditions;
        }

        public static IList<TimeCondition> GetTimeConditionsFromXmlElements(List<RTCTimeSeriesXML> elements)
        {
            var timeConditionElements = elements.Where(e => e.PITimeSeries.parameterId == "TimeSeries" && e.id.Contains(RtcDataConfigTag.TimeCondition));

            var timeConditions = new List<TimeCondition>();

            foreach (var timeConditionElement in timeConditionElements)
            {
                var id = timeConditionElement.id;

                var conditionItem = timeConditionElement.PITimeSeries;

                var splitId = id.Split(new[] { RtcDataConfigTag.TimeCondition }, StringSplitOptions.None);

                var controlGroupName = splitId.ElementAt(1);
                var conditionName = splitId.ElementAt(2);
                var interpolation = conditionItem.interpolationOption;
                var extrapolation = conditionItem.extrapolationOption;

                if (string.IsNullOrEmpty(controlGroupName) || string.IsNullOrEmpty(conditionName))
                    Log.Warn($"The element with id '{id}' needs to have an elementId and quantityId. See file: '{RealTimeControlXMLFiles.XmlData}'");

                var timeCondition = new TimeCondition();

                timeCondition.Name = conditionName;
                timeCondition.InterpolationOptionsTime = interpolation == PIInterpolationOptionEnumStringType.BLOCK
                    ? InterpolationType.Constant
                    : InterpolationType.Linear;

                timeCondition.Extrapolation = extrapolation == PIExtrapolationOptionEnumStringType.PERIODIC
                    ? ExtrapolationType.Periodic
                    : ExtrapolationType.Constant;

                timeConditions.Add(timeCondition);
            }

            return timeConditions;
        }

        public static IList<TimeRule> GetTimeRulesFromXmlElements(List<RTCTimeSeriesXML> elements)
        {
            var timeRuleElements = elements.Where(e => e.PITimeSeries.parameterId == "TimeSeries" && e.id.EndsWith(RtcDataConfigTag.TimeRule));

            var timeRules = new List<TimeRule>();

            foreach (var timeRuleElement in timeRuleElements)
            {
                var id = timeRuleElement.id;

                // cant do anything with given information 

                var timeRule = new TimeRule();

                // cant do anything with given information

                timeRules.Add(timeRule);
            }

            return timeRules;
        }

        public static IList<RelativeTimeRule> GetRelativeTimeRulesFromXmlElements(List<RTCTimeSeriesXML> elements)
        {
            var relativeTimeRuleElements = elements.Where(e => e.PITimeSeries.parameterId == "TimeSeries" && e.id.EndsWith(RtcDataConfigTag.RelativeTimeRule));

            var relativeTimeRules = new List<RelativeTimeRule>();

            foreach (var relativeTimeRuleElement in relativeTimeRuleElements)
            {
                var id = relativeTimeRuleElement.id;

                // cant do anything with given information

                var relativeTimeRule = new RelativeTimeRule();

                // cant do anything with given information

                relativeTimeRules.Add(relativeTimeRule);
            }

            return relativeTimeRules;
        }

        public static IList<Output> GetOutputsAsInputsFromXmlElements(List<RTCTimeSeriesXML> elements, IHydroModel model)
        {
            var outputAsInputElements = elements.Where(e => e.id.Contains(RtcDataConfigTag.OutputAsInput) && e.OpenMIExchangeItem.elementId != null);

            var outputsAsInput = new List<Output>();

            foreach (var outputAsInputElement in outputAsInputElements)
            {
                var id = outputAsInputElement.id;

                var outputItem = outputAsInputElement.OpenMIExchangeItem;

                var splitId = id.Split(new[] { RtcDataConfigTag.OutputAsInput }, StringSplitOptions.None);

                var outputName = splitId.ElementAt(1);
                var ruleName = splitId.ElementAt(2); // ? 

                var featureName = outputItem.elementId;
                var parameterName = outputItem.quantityId;
                var linkedFeature = model.Region.AllHydroObjects.First(o => o.Name == featureName);

                if (string.IsNullOrEmpty(featureName) || string.IsNullOrEmpty(parameterName))
                    Log.Warn($"The element with id '{id}' needs to have an elementId and quantityId. See file: '{RealTimeControlXMLFiles.XmlData}'.");

                if (linkedFeature == null)
                    Log.Warn($"Element with id '{id}' does not have a corresponding feature in the model. See file: '{RealTimeControlXMLFiles.XmlData}'.");

                var output = new Output();

                output.Name = outputName;
                output.ParameterName = parameterName;
                output.Feature = linkedFeature;

                outputsAsInput.Add(output);
            }

            return outputsAsInput;
        }
    }

    public static class RtcDataConfigTag                   
    {
        public const string Input = "input_";
        public const string TimeRule = "_TimeSeries";
        public const string TimeCondition = "TimeSeries_"; 
        public const string OutputAsInput = "_AsInputFor_";
        public const string Output = "output_";
        public const string StandardCondition = "Status_"; 
        public const string RelativeTimeRule = "_t";
    }
}
