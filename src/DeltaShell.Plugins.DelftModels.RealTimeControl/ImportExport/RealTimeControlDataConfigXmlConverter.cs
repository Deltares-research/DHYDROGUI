using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using log4net;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlDataConfigXmlConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlDataConfigXmlConverter));



        public static IEnumerable<ConnectionPoint> CreateConnectionPointsFromXmlElements(
            List<RTCTimeSeriesXML> elements)
        {
            foreach (var element in elements)
            {
                var id = element.id;

                if (id.Contains(RtcXmlTag.OutputAsInput)) continue;

                var tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

                if (tag != RtcXmlTag.Input && tag != RtcXmlTag.Output) continue;

                ConnectionPoint connectionPoint = null;

                switch (tag)
                {
                    case RtcXmlTag.Input:
                        connectionPoint = new Input();
                        break;
                    case RtcXmlTag.Output:
                        connectionPoint = new Output();
                        break;
                    default:
                        yield break;
                }

                // serves as a temporary name, used for coupler
                connectionPoint.Name = id;

                yield return connectionPoint;
            }
        }
    }

    public static class RealTimeControlDataConfigXmlSetter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlDataConfigXmlSetter));

        public static void SetInterpolationAndExtraPolationRtcComponents(IList<RTCTimeSeriesXML> elements, IList<IControlGroup> controlGroups)
        {
            foreach (var element in elements)
            {
                var id = element.id;
                var tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

                if (tag == string.Empty || tag == RtcXmlTag.Input || tag == RtcXmlTag.Output) continue;

                var controlGroup = RealTimeControlXmlReaderHelper.GetControlGroupByElementId(controlGroups, id);
                var item = element.PITimeSeries;

                switch (tag)
                {
                    case RtcXmlTag.TimeRule:
                        SetInterpolationAndExtraPolationOnTimeRule(controlGroup, id, item);
                        break;
                    case RtcXmlTag.RelativeTimeRule:
                        SetInterpolationAndExtraPolationOnRelativeTimeRule(controlGroup, id, item);
                        break;
                    case RtcXmlTag.PIDRule:
                        //
                        break;
                    case RtcXmlTag.IntervalRule:
                        //
                        break;
                    case RtcXmlTag.HydraulicRule:
                        //
                        break;
                    case RtcXmlTag.FactorRule:
                        //
                        break;
                    case RtcXmlTag.StandardCondition:
                        //
                        break;
                    case RtcXmlTag.TimeCondition:
                        SetInterpolationAndExtraPolationOnTimeCondition(controlGroup, id, item);
                        break;
                    case RtcXmlTag.DirectionalCondition:
                        //
                        break;
                }
            }
        }

        private static void SetInterpolationAndExtraPolationOnTimeCondition(IControlGroup controlGroup, string id,
            PITimeSeriesXML item)
        {
            var condition = (TimeCondition) controlGroup.GetConditionByElementId<TimeCondition>(id);
            condition.InterpolationOptionsTime = GetInterpolation(item.interpolationOption);
            condition.Extrapolation = GetExtrapolation(item.extrapolationOption);
        }

        private static void SetInterpolationAndExtraPolationOnTimeRule(IControlGroup controlGroup, string id, PITimeSeriesXML ruleItem)
        {
            var rule = (TimeRule)controlGroup.GetRuleByElementId<TimeRule>(id);
            rule.InterpolationOptionsTime = GetInterpolation(ruleItem.interpolationOption);
            rule.Periodicity = GetExtrapolation(ruleItem.extrapolationOption);
        }

        private static void SetInterpolationAndExtraPolationOnRelativeTimeRule(IControlGroup controlGroup, string id, PITimeSeriesXML item)
        {
            var rule = (RelativeTimeRule) controlGroup.GetRuleByElementId<RelativeTimeRule>(id);
            rule.Interpolation = GetInterpolation(item.interpolationOption);
        }


        private static InterpolationType GetInterpolation(
            PIInterpolationOptionEnumStringType conditionItemExtrapolationOption)
        {
            return conditionItemExtrapolationOption == PIInterpolationOptionEnumStringType.BLOCK
                ? InterpolationType.Constant
                : InterpolationType.Linear;
        }

        private static ExtrapolationType GetExtrapolation(
            PIExtrapolationOptionEnumStringType conditionItemExtrapolationOption)
        {
            return conditionItemExtrapolationOption == PIExtrapolationOptionEnumStringType.PERIODIC
                ? ExtrapolationType.Periodic
                : ExtrapolationType.Constant;
        }
    }
}

