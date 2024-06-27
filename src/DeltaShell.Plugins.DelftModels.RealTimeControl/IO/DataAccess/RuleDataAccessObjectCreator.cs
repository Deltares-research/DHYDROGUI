using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Creates a <see cref="RuleDataAccessObject"/> based of a <see cref="RuleComplexType"/>.
    /// </summary>
    public static class RuleDataAccessObjectCreator
    {
        /// <summary>
        /// Creates a <see cref="RuleDataAccessObject"/> from the specified <paramref name="ruleElement"/>.
        /// </summary>
        /// <param name="ruleElement"> The rule. </param>
        /// <param name="logHandler"> The log handler. </param>
        /// <returns>
        /// A <see cref="RuleDataAccessObject"/> created from the specified <paramref name="ruleElement"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="ruleElement"/> is <c>null</c>.
        /// </exception>
        public static RuleDataAccessObject Create(RuleComplexType ruleElement, ILogHandler logHandler = null)
        {
            Ensure.NotNull(ruleElement, nameof(ruleElement));

            object item = ruleElement.Item;
            switch (item)
            {
                case TimeAbsoluteComplexType timeRuleElement:
                    return CreateTimeRuleDataAccessObject(timeRuleElement);
                case TimeRelativeComplexType relativeTimeRuleElement:
                    return CreateRelativeTimeRuleDataAccessObject(relativeTimeRuleElement);
                case PidComplexType pidRuleElement:
                    return CreatePidRuleDataAccessObject(pidRuleElement);
                case IntervalComplexType intervalRuleElement:
                    return CreateIntervalRuleDataAccessObject(intervalRuleElement);
                case LookupTableComplexType lookupTableElement:
                    return CreateLookupTableRuleDataAccessObject(lookupTableElement, logHandler);
                default:
                    return null;
            }
        }

        private static RuleDataAccessObject CreateTimeRuleDataAccessObject(TimeAbsoluteComplexType ruleElement)
        {
            TimeRule rule = CreateTimeRule(ruleElement);

            string id = ruleElement.id;
            var dataAccessObject = new RuleDataAccessObject(id, rule);

            if (ruleElement.output?.y != null)
            {
                dataAccessObject.OutputReferences.Add(ruleElement.output.y);
            }

            return dataAccessObject;
        }

        private static TimeRule CreateTimeRule(TimeAbsoluteComplexType ruleElement)
        {
            string ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(ruleElement.id);
            return new TimeRule(ruleName);
        }

        private static RuleDataAccessObject CreateRelativeTimeRuleDataAccessObject(TimeRelativeComplexType relativeTimeRuleElement)
        {
            RelativeTimeRule rule = CreateRelativeTimeRule(relativeTimeRuleElement);

            string id = relativeTimeRuleElement.id;
            var dataAccessObject = new RuleDataAccessObject(id, rule);

            if (relativeTimeRuleElement.output?.y != null)
            {
                dataAccessObject.OutputReferences.Add(relativeTimeRuleElement.output.y);
            }

            return dataAccessObject;
        }

        private static RelativeTimeRule CreateRelativeTimeRule(TimeRelativeComplexType ruleElement)
        {
            string ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(ruleElement.id);
            bool fromValue = ruleElement.valueOption == timeRelativeEnumStringType.RELATIVE;
            var minimumPeriod = (int) ruleElement.maximumPeriod;
            InterpolationType interpolation = GetInterpolationType(ruleElement.interpolationOption);
            TimeRelativeControlTableRecordComplexType[] records = ruleElement.controlTable;

            var rule = new RelativeTimeRule(ruleName, fromValue)
            {
                MinimumPeriod = minimumPeriod,
                Interpolation = interpolation
            };
            DefineFunction(rule.Function, records);

            return rule;
        }

        private static RuleDataAccessObject CreatePidRuleDataAccessObject(PidComplexType pidRuleElement)
        {
            PIDRule rule = CreatePidRule(pidRuleElement);

            string id = pidRuleElement.id;
            var dataAccessObject = new RuleDataAccessObject(id, rule);

            if (pidRuleElement.input?.x != null)
            {
                dataAccessObject.InputReferences.Add(pidRuleElement.input.x);
            }

            if (pidRuleElement.input?.Item is string signalId && signalId.Contains(RtcXmlTag.Signal))
            {
                dataAccessObject.SignalReferences.Add(GetCorrectSignalReference(signalId));
            }

            if (pidRuleElement.output?.y != null)
            {
                dataAccessObject.OutputReferences.Add(pidRuleElement.output.y);
            }

            return dataAccessObject;
        }

        private static PIDRule CreatePidRule(PidComplexType ruleElement)
        {
            string ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(ruleElement.id);

            var rule = new PIDRule(ruleName)
            {
                Setting =
                {
                    Min = ruleElement.settingMin,
                    Max = ruleElement.settingMax,
                    MaxSpeed = ruleElement.settingMaxSpeed
                },
                Kp = ruleElement.kp,
                Ki = ruleElement.ki,
                Kd = ruleElement.kd
            };

            object setPointItem = ruleElement.input.Item;

            switch (setPointItem)
            {
                case null:
                    return rule;
                case double doubleValue:
                    rule.ConstantValue = doubleValue;
                    rule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
                    break;
                case string setPointTimeSeries when setPointTimeSeries.Contains(RtcXmlTag.SP):
                    rule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
                    break;
                default:
                    rule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Signal;
                    break;
            }

            return rule;
        }

        private static RuleDataAccessObject CreateIntervalRuleDataAccessObject(IntervalComplexType intervalRuleElement)
        {
            IntervalRule rule = CreateIntervalRule(intervalRuleElement);

            string id = intervalRuleElement.id;
            var dataAccessObject = new RuleDataAccessObject(id, rule);

            if (intervalRuleElement.input?.x != null)
            {
                dataAccessObject.InputReferences.Add(intervalRuleElement.input.x.Value);
            }

            string signalId = intervalRuleElement.input?.setpoint;
            if (signalId != null && signalId.Contains(RtcXmlTag.Signal))
            {
                dataAccessObject.SignalReferences.Add(GetCorrectSignalReference(signalId));
            }

            if (intervalRuleElement.output?.y != null)
            {
                dataAccessObject.OutputReferences.Add(intervalRuleElement.output.y);
            }

            return dataAccessObject;
        }

        private static IntervalRule CreateIntervalRule(IntervalComplexType intervalRuleElement)
        {
            string ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(intervalRuleElement.id);
            IntervalRule.IntervalRuleIntervalType intervalType = GetIntervalType(intervalRuleElement.ItemElementName);
            IntervalRule.IntervalRuleSetPointType setPointType = GetSetPointType(intervalRuleElement.input.setpoint);


            var rule = new IntervalRule(ruleName)
            {
                Setting =
                {
                    Below = intervalRuleElement.settingBelow,
                    Above = intervalRuleElement.settingAbove
                },
                SetPointType = setPointType,
                IntervalType = intervalType,
                DeadBandType = GetDeadBandSetPointType(intervalRuleElement.Item1ElementName),
                DeadbandAroundSetpoint = intervalRuleElement.Item1
            };

            double interval = intervalRuleElement.Item;
            if (intervalType == IntervalRule.IntervalRuleIntervalType.Fixed)
            {
                rule.FixedInterval = interval;
            }
            else
            {
                rule.Setting.MaxSpeed = interval;

            }

            return rule;
        }

        private static RuleDataAccessObject CreateLookupTableRuleDataAccessObject(LookupTableComplexType lookupTableElement, ILogHandler logHandler)
        {
            HydraulicRule rule;

            string id = lookupTableElement.id;
            string tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);
            switch (tag)
            {
                case RtcXmlTag.HydraulicRule:
                    rule = CreateHydraulicRule(lookupTableElement);
                    break;
                case RtcXmlTag.FactorRule:
                    rule = CreateFactorRule(lookupTableElement);
                    break;
                default:
                    logHandler?.ReportWarning($"The type of hydraulic rule with id '{id}' could not be inferred from the id.");
                    return null;
            }

            rule.Interpolation = GetInterpolationType(lookupTableElement.interpolationOption);
            rule.Extrapolation = GetExtrapolationType(lookupTableElement.extrapolationOption);

            var dataAccessObject = new RuleDataAccessObject(id, rule);

            if (lookupTableElement.input?.x != null)
            {
                dataAccessObject.InputReferences.Add(lookupTableElement.input.x.Value);
            }

            if (lookupTableElement.output?.y != null)
            {
                dataAccessObject.OutputReferences.Add(lookupTableElement.output.y);
            }

            return dataAccessObject;
        }

        private static HydraulicRule CreateHydraulicRule(LookupTableComplexType lookupTableElement)
        {
            string ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(lookupTableElement.id);
            DateRecord2DataComplexType[] records = (lookupTableElement.Item as TableLookupTableComplexType)?.record;

            var rule = new HydraulicRule {Name = ruleName};

            if (records != null)
            {
                DefineFunction(rule.Function, records);
            }

            return rule;
        }

        private static FactorRule CreateFactorRule(LookupTableComplexType lookupTableElement)
        {
            string ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(lookupTableElement.id);
            DateRecord2DataComplexType firstRecord = (lookupTableElement.Item as TableLookupTableComplexType)?.record.FirstOrDefault();

            var rule = new FactorRule {Name = ruleName};

            if (firstRecord == null)
            {
                return rule;
            }

            double factor = -firstRecord.y;
            rule.Factor = factor;

            return rule;
        }

        private static string GetCorrectSignalReference(string signalId)
        {
            // the reference id is not the same as the id of the corresponding signal element
            return signalId.Replace(RtcXmlTag.Signal, RtcXmlTag.LookupSignal);
        }

        private static void DefineFunction(IFunction function, IEnumerable<TimeRelativeControlTableRecordComplexType> records)
        {
            if (records == null || function == null)
            {
                return;
            }

            function.Arguments[0].SetValues(records.Select(e => e.time));
            function.Components[0].SetValues(records.Select(e => e.value));
        }

        private static void DefineFunction(IFunction function, IEnumerable<DateRecord2DataComplexType> records)
        {
            if (records == null || function == null)
            {
                return;
            }

            function.Arguments[0].SetValues(records.Select(e => e.x));
            function.Components[0].SetValues(records.Select(e => e.y));
        }

        private static IntervalRule.IntervalRuleDeadBandType GetDeadBandSetPointType(Item1ChoiceType3 itemType)
        {
            return itemType == Item1ChoiceType3.deadbandSetpointRelative
                       ? IntervalRule.IntervalRuleDeadBandType.PercentageDischarge
                       : IntervalRule.IntervalRuleDeadBandType.Fixed;
        }

        private static IntervalRule.IntervalRuleIntervalType GetIntervalType(ItemChoiceType6 itemType)
        {
            return itemType == ItemChoiceType6.settingMaxStep
                       ? IntervalRule.IntervalRuleIntervalType.Fixed
                       : IntervalRule.IntervalRuleIntervalType.Variable;
        }

        private static IntervalRule.IntervalRuleSetPointType GetSetPointType(string setPoint)
        {
            if (setPoint?.Contains(RtcXmlTag.Signal) == true)
            {
                return IntervalRule.IntervalRuleSetPointType.Signal;
            }

            return IntervalRule.IntervalRuleSetPointType.Variable;
        }
        private static ExtrapolationType GetExtrapolationType(interpolationOptionEnumStringType extrapolationOption)
        {
            return extrapolationOption == interpolationOptionEnumStringType.BLOCK
                       ? ExtrapolationType.Constant
                       : ExtrapolationType.Linear;
        }

        private static InterpolationType GetInterpolationType(interpolationOptionEnumStringType interpolationOption)
        {
            return interpolationOption == interpolationOptionEnumStringType.BLOCK
                       ? InterpolationType.Constant
                       : InterpolationType.Linear;
        }
    }
}