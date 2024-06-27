using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// Responsible for setting the information that comes from the data config xml on the rtc components.
    /// </summary>
    public class RealTimeControlDataConfigXmlSetter
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlDataConfigXmlSetter(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Sets the interpolation and extrapolation RTC components.
        /// </summary>
        /// <param name="elements">The elements.</param>
        /// <param name="controlGroups">The control groups.</param>
        /// <remarks>If parameter elements or controlGroups is NULL, methods returns.</remarks>
        public void SetInterpolationAndExtrapolationRtcComponents(IList<PITimeSeriesComplexType> elements, IList<IControlGroup> controlGroups)
        {
            if (elements == null || controlGroups == null)
            {
                return;
            }

            foreach (PITimeSeriesComplexType element in elements)
            {
                string id = element.locationId;
                string tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

                if (!RtcXmlTag.ComponentTags.Contains(tag))
                {
                    continue;
                }

                IControlGroup controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);

                if (controlGroup == null)
                {
                    continue;
                }

                switch (tag)
                {
                    case RtcXmlTag.TimeRule:
                        SetInterpolationAndExtrapolationOnTimeRule(controlGroup, element);
                        break;
                    case RtcXmlTag.RelativeTimeRule:
                        SetInterpolationAndExtrapolationOnRelativeTimeRule(controlGroup, element);
                        break;
                    case RtcXmlTag.PIDRule:
                        SetInterpolationAndExtrapolationOnPidRule(controlGroup, element);
                        break;
                    case RtcXmlTag.IntervalRule:
                        SetInterpolationAndExtrapolationOnIntervalRule(controlGroup, element);
                        break;
                    case RtcXmlTag.TimeCondition:
                        SetInterpolationAndExtrapolationOnTimeCondition(controlGroup, element);
                        break;
                }
            }
        }

        /// <summary>
        /// Sets the time lag on hydraulic rules.
        /// </summary>
        /// <param name="elements">The elements.</param>
        /// <param name="hydraulicRules">The hydraulic rules.</param>
        /// <param name="modelTimeStep">The model time step.</param>
        /// <remarks>If parameter elements or hydraulicRules is NULL, methods returns.</remarks>
        public void SetTimeLagOnHydraulicRules(IList<RTCTimeSeriesComplexType> elements, IList<HydraulicRule> hydraulicRules, TimeSpan modelTimeStep)
        {
            if (elements == null || hydraulicRules == null)
            {
                return;
            }

            foreach (HydraulicRule hydraulicRule in hydraulicRules)
            {
                IInput input = hydraulicRule.Inputs.FirstOrDefault();

                if (input == null)
                {
                    logHandler.ReportErrorFormat(
                        Resources.RealTimeControlDataConfigXmlSetter_SetTimeLagOnHydraulicRules_Hydraulic_rule___0___must_have_an_input__Please_check_file____1___,
                        hydraulicRule.Name, RealTimeControlXmlFiles.XmlTools);
                    continue;
                }

                string correspondingElementName = RtcXmlTag.Delayed + input.Name;
                RTCTimeSeriesComplexType correspondingInputElement = elements.FirstOrDefault(e => e.id == correspondingElementName);

                if (correspondingInputElement == null)
                {
                    continue;
                }

                int timeLagFactor = correspondingInputElement.vectorLength;
                var timeLagInSeconds = (int) Math.Round((timeLagFactor + 1) * modelTimeStep.TotalSeconds);
                hydraulicRule.TimeLag = timeLagInSeconds;
            }
        }

        private void SetInterpolationAndExtrapolationOnTimeCondition(IControlGroup controlGroup,
                                                                     PITimeSeriesComplexType conditionItem)
        {
            var condition = controlGroup.GetConditionByElementId<TimeCondition>(conditionItem.locationId, logHandler);
            condition.InterpolationOptionsTime = GetInterpolationType(conditionItem.interpolationOption);
            condition.Extrapolation = GetExtrapolationType(conditionItem.extrapolationOption);
        }

        private void SetInterpolationAndExtrapolationOnTimeRule(IControlGroup controlGroup,
                                                                PITimeSeriesComplexType ruleItem)
        {
            var rule = (TimeRule) controlGroup.GetRuleByElementId<TimeRule>(ruleItem.locationId, logHandler);
            rule.InterpolationOptionsTime = GetInterpolationType(ruleItem.interpolationOption);
            rule.Periodicity = GetExtrapolationType(ruleItem.extrapolationOption);
        }

        private void SetInterpolationAndExtrapolationOnRelativeTimeRule(IControlGroup controlGroup,
                                                                        PITimeSeriesComplexType ruleItem)
        {
            var rule = (RelativeTimeRule) controlGroup.GetRuleByElementId<RelativeTimeRule>(ruleItem.locationId, logHandler);
            rule.Interpolation = GetInterpolationType(ruleItem.interpolationOption);
        }

        private void SetInterpolationAndExtrapolationOnPidRule(IControlGroup controlGroup,
                                                               PITimeSeriesComplexType ruleItem)
        {
            var rule = (PIDRule) controlGroup.GetRuleByElementId<PIDRule>(ruleItem.locationId, logHandler);
            rule.InterpolationOptionsTime = GetInterpolationType(ruleItem.interpolationOption);
            rule.ExtrapolationOptionsTime = GetExtrapolationType(ruleItem.extrapolationOption);
        }

        private void SetInterpolationAndExtrapolationOnIntervalRule(IControlGroup controlGroup,
                                                                    PITimeSeriesComplexType ruleItem)
        {
            var rule = (IntervalRule) controlGroup.GetRuleByElementId<IntervalRule>(ruleItem.locationId, logHandler);
            rule.InterpolationOptionsTime = GetInterpolationType(ruleItem.interpolationOption);
            rule.Extrapolation = GetExtrapolationType(ruleItem.extrapolationOption);
        }

        private static InterpolationType GetInterpolationType(PIInterpolationOptionEnumStringType conditionItemExtrapolationOption)
        {
            return conditionItemExtrapolationOption == PIInterpolationOptionEnumStringType.BLOCK
                       ? InterpolationType.Constant
                       : InterpolationType.Linear;
        }

        private static ExtrapolationType GetExtrapolationType(PIExtrapolationOptionEnumStringType conditionItemExtrapolationOption)
        {
            return conditionItemExtrapolationOption == PIExtrapolationOptionEnumStringType.PERIODIC
                       ? ExtrapolationType.Periodic
                       : ExtrapolationType.Constant;
        }
    }
}