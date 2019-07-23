using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>
    /// Responsible for taking the objects that come from the tools config xml file and converting them into rtc objects.
    /// </summary>
    public class RealTimeControlToolsConfigXmlConverter
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlToolsConfigXmlConverter(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Creates the control groups from rule XML elements.
        /// </summary>
        /// <param name="ruleElements">The rule elements.</param>
        /// <returns>The control groups.</returns>
        /// <remarks>Parameter ruleElements is expected to not be NULL.</remarks>
        public IEnumerable<IControlGroup> CreateControlGroupsFromXmlElementIDs(IEnumerable<RuleXML> ruleElements)
        {
            var groupNames = GetDistinctControlGroupNamesFromElements(ruleElements);

            var controlGroups = CreateControlGroupsByName(groupNames);

            return controlGroups;
        }

        /// <summary>
        /// Creates the rules from XML elements and adds the rules to the corresponding control group.
        /// </summary>
        /// <param name="ruleElements">The rule elements.</param>
        /// <param name="controlGroups">The control groups.</param>
        /// <remarks>If parameter ruleElements or controlGroups is NULL, methods returns.</remarks>
        public void CreateRulesFromXmlElementsAndAddToControlGroup(List<RuleXML> ruleElements, IList<IControlGroup> controlGroups)
        {
            if (ruleElements == null || controlGroups == null) return;

            foreach (var ruleElement in ruleElements)
            {
                CreateRuleFromXmlElementAndAddToControlGroup(controlGroups, ruleElement);
            }
        }

        /// <summary>
        /// Creates the conditions from XML elements and adds the conditions to the corresponding control group.
        /// </summary>
        /// <param name="conditionElements">The condition elements.</param>
        /// <param name="controlGroups">The control groups.</param>
        /// <remarks>If parameter conditionElements or controlGroups is NULL, methods returns.</remarks>
        public void CreateConditionsFromXmlElementsAndAddToControlGroup(List<TriggerXML> conditionElements, IList<IControlGroup> controlGroups)
        {
            if (conditionElements == null || controlGroups == null) return;

            foreach (var conditionElement in conditionElements)
            {
                CreateConditionFromXmlElementAndAddToControlGroup(controlGroups, conditionElement);
            }
        }
        /// <summary>
        /// Creates the signals from XML elements and adds the signals to the corresponding control group.
        /// </summary>
        /// <param name="signalElements"></param>
        /// <param name="controlGroups"></param>
        /// <remarks>If parameter signalElements or controlGroups is NULL, methods returns.</remarks>
        public void CreateSignalsFromXmlElementsAndAddToControlGroup(List<RuleXML> signalElements, IList<IControlGroup> controlGroups)
        {
            if (signalElements == null || controlGroups == null) return;

            foreach (var signalElement in signalElements)
            {
                CreateSignalFromXmlElementAndAddToControlGroup(controlGroups, signalElement);
            }
        }

        /// <summary>
        /// Separate signals from rules, since in the toolconfig xml they are written as rules (<rule> </rule>)
        /// </summary>
        /// <param name="ruleElements"></param>
        /// <param name="signalElements"></param>
        /// <remarks>If parameter signalElements or ruleElements is NULL, methods returns.</remarks>
        public void SeparateSignalsFromRules(List<RuleXML> ruleElements, List<RuleXML> signalElements)
        {
            if (ruleElements == null || signalElements == null) return;
            foreach (var ruleElement in ruleElements.Reverse<RuleXML>())
            {
                var item = ruleElement.Item;

                if (item is LookupTableXML lookupTableElement)
                {
                    var id = ((LookupTableXML)ruleElement.Item).id;
                    var tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

                    if (tag == RtcXmlTag.LookupSignal)
                    {
                        signalElements.Add(ruleElement);
                        ruleElements.Remove(ruleElement);
                    }
                }
            }
        }

        private void CreateSignalFromXmlElementAndAddToControlGroup(IList<IControlGroup> controlGroups,
            RuleXML ruleElement)
        {
            var item = ruleElement.Item;

            if (item is LookupTableXML lookupTableElement)
            {
                var id = lookupTableElement.id;
                var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);
                if (controlGroup != null)
                {
                    var signal = CreateLookupSignal(lookupTableElement);

                    signal.Interpolation = GetInterpolationType(lookupTableElement.interpolationOption);
                    signal.Extrapolation = GetExtrapolationType(lookupTableElement.extrapolationOption);

                    controlGroup.Signals.Add(signal);
                }
            }
        }

        private IEnumerable<IControlGroup> CreateControlGroupsByName(IEnumerable<string> groupNames)
        {
            foreach (var groupName in groupNames)
            {
                yield return new ControlGroup {Name = groupName};
            }
        }

        private IEnumerable<string> GetDistinctControlGroupNamesFromElements(IEnumerable<RuleXML> ruleElements)
        {
            var elementsIDs = ruleElements.SelectMany(e => GetIdFromRuleElement(e));

            var groupNames = new HashSet<string>();

            foreach (var elementID in elementsIDs)
            {
                var groupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(elementID);

                if (!string.IsNullOrEmpty(groupName))
                {
                    groupNames.Add(groupName);
                }
            }

            return groupNames;
        }

        private IEnumerable<string> GetIdFromRuleElement(RuleXML ruleElement)
        {
            var item = ruleElement.Item;

            if (item is TimeRelativeXML relativeTimeRuleElement)
            {
                yield return relativeTimeRuleElement.id;
            }
            else if (item is PidXML pidRuleElement)
            {
                yield return pidRuleElement.id;
            }
            else if (item is TimeAbsoluteXML timeRuleElement)
            {
                yield return timeRuleElement.id;
            }
            else if (item is LookupTableXML lookupTableElement)
            {
                yield return lookupTableElement.id;
            }
            else if (item is IntervalXML intervalRuleElement)
            {
                yield return intervalRuleElement.id;
            }
        }

        private void CreateRuleFromXmlElementAndAddToControlGroup(IList<IControlGroup> controlGroups, RuleXML ruleElement)
        {
            var item = ruleElement.Item;

            if (item is TimeAbsoluteXML timeRuleElement)
            {
                CreateTimeRuleAndAddToControlGroup(controlGroups, timeRuleElement);
            }
            else if (item is TimeRelativeXML relativeTimeRuleElement)
            {
                CreateRelativeTimeRuleAndAddToControlGroup(controlGroups, relativeTimeRuleElement);
            }
            else if (item is PidXML pidRuleElement)
            {
                CreatePidRuleAndAddToControlGroup(controlGroups, pidRuleElement);
            }
            else if (item is IntervalXML intervalRuleElement)
            {
                CreateIntervalRuleAndAddToControlGroup(controlGroups, intervalRuleElement);
            }
            else if (item is LookupTableXML lookupTableElement)
            {
                CreateHydraulicRuleAndAddToControlGroup(controlGroups, lookupTableElement);
            }
        }

        private void CreateTimeRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, TimeAbsoluteXML timeRuleElement)
        {
            var controlGroup = controlGroups.GetControlGroupByElementId(timeRuleElement.id, logHandler);
            if (controlGroup != null)
            {
                var rule = CreateTimeRule(timeRuleElement);
                if (rule != null)
                {
                    controlGroup.Rules.Add(rule);
                }
            }
        }

        private TimeRule CreateTimeRule(TimeAbsoluteXML ruleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(ruleElement.id);
            var rule = new TimeRule(ruleName);

            return rule;
        }

        private void CreateRelativeTimeRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, TimeRelativeXML relativeTimeRuleElement)
        {
            var controlGroup = controlGroups.GetControlGroupByElementId(relativeTimeRuleElement.id, logHandler);
            if (controlGroup != null)
            {
                var rule = CreateRelativeTimeRule(relativeTimeRuleElement);
                if (rule != null)
                {
                    controlGroup.Rules.Add(rule);
                }
            }
        }

        private RelativeTimeRule CreateRelativeTimeRule(TimeRelativeXML ruleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(ruleElement.id);
            var fromValue = ruleElement.valueOption == timeRelativeEnumStringType.RELATIVE;
            var minimumPeriod = (int) ruleElement.maximumPeriod;
            var interpolation = GetInterpolationType(ruleElement.interpolationOption);
            var records = ruleElement.controlTable;

            var rule = new RelativeTimeRule(ruleName, fromValue);
            rule.MinimumPeriod = minimumPeriod;
            rule.Interpolation = interpolation;
            DefineFunction(rule.Function, records);

            return rule;
        }

        private void CreatePidRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, PidXML pidRuleElement)
        {
            var controlGroup = controlGroups.GetControlGroupByElementId(pidRuleElement.id, logHandler);
            if (controlGroup != null)
            {
                var rule = CreatePidRule(pidRuleElement);
                if (rule != null)
                {
                    controlGroup.Rules.Add(rule);
                }
            }
        }

        private PIDRule CreatePidRule(PidXML ruleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(ruleElement.id);

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

            var setPointItem = ruleElement.input.Item;

            if (setPointItem == null) return rule;

            if (setPointItem is double doubleValue)
            {
                rule.ConstantValue = doubleValue;
                rule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
            }
            else if (setPointItem is string setPointTimeSeries && setPointTimeSeries.Contains(RtcXmlTag.SP))
            {
                rule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
            }
            else
            {
                rule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Signal;
            }

            return rule;
        }

        private void CreateIntervalRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, IntervalXML intervalRuleElement)
        {
            var controlGroup = controlGroups.GetControlGroupByElementId(intervalRuleElement.id, logHandler);
            if (controlGroup != null)
            {
                var rule = CreateIntervalRule(intervalRuleElement);
                if (rule != null)
                {
                    controlGroup.Rules.Add(rule);
                }
            }
        }

        private IntervalRule CreateIntervalRule(IntervalXML intervalRuleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(intervalRuleElement.id);
            var intervalType = GetIntervalType(intervalRuleElement.ItemElementName);

            var rule = new IntervalRule(ruleName)
            {
                Setting =
                {
                    Below = intervalRuleElement.settingBelow,
                    Above = intervalRuleElement.settingAbove
                },
                IntervalType = intervalType,
                DeadBandType = GetDeadBandSetPointType(intervalRuleElement.Item1ElementName),
                DeadbandAroundSetpoint = intervalRuleElement.Item1
            };

            var interval = intervalRuleElement.Item;
            if (intervalType == IntervalRule.IntervalRuleIntervalType.Fixed)
            {
                rule.FixedInterval = interval;
            }
            else
            {
                rule.Setting.MaxSpeed = interval;

                var setPoint = intervalRuleElement.input.setpoint;

                if (setPoint != null && setPoint.Contains(RtcXmlTag.Signal))
                {
                    rule.IntervalType = IntervalRule.IntervalRuleIntervalType.Signal;
                }
            }

            return rule;
        }

        private IntervalRule.IntervalRuleDeadBandType GetDeadBandSetPointType(Item1ChoiceType3 itemType)
        {
            if (itemType == Item1ChoiceType3.deadbandSetpointRelative)
            {
                return IntervalRule.IntervalRuleDeadBandType.PercentageDischarge;
            }
            else
            {
                return IntervalRule.IntervalRuleDeadBandType.Fixed;
            }
        }

        private IntervalRule.IntervalRuleIntervalType GetIntervalType(ItemChoiceType5 itemType)
        {
            if (itemType == ItemChoiceType5.settingMaxStep)
            {
                return IntervalRule.IntervalRuleIntervalType.Fixed;
            }
            else
            {
                return IntervalRule.IntervalRuleIntervalType.Variable;
            }
        }

        private void CreateHydraulicRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, LookupTableXML lookupTableElement)
        {
            var id = lookupTableElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);
            if (controlGroup != null)
            {
                var tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);
                HydraulicRule rule = null;

                switch (tag)
                {
                    case RtcXmlTag.HydraulicRule:
                        rule = CreateHydraulicRule(lookupTableElement);
                        break;
                    case RtcXmlTag.FactorRule:
                        rule = CreateFactorRule(lookupTableElement);
                        break;
                }

                if (rule == null) return;

                rule.Interpolation = GetInterpolationType(lookupTableElement.interpolationOption);
                rule.Extrapolation = GetExtrapolationType(lookupTableElement.extrapolationOption);
                controlGroup.Rules.Add(rule);
            }
        }

        private HydraulicRule CreateHydraulicRule(LookupTableXML lookupTableElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(lookupTableElement.id);
            var records = (lookupTableElement.Item as TableLookupTableXML)?.record;

            var rule = new HydraulicRule
            {
                Name = ruleName,
            };

            if (records != null)
            {
                DefineFunction(rule.Function, records);
            }

            return rule;
        }

        private FactorRule CreateFactorRule(LookupTableXML lookupTableElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(lookupTableElement.id);
            var firstRecord = (lookupTableElement.Item as TableLookupTableXML)?.record.FirstOrDefault();

            var rule = new FactorRule
            {
                Name = ruleName
            };

            if (firstRecord != null)
            {
                var factor = -firstRecord.y;
                rule.Factor = factor;
            }

            return rule;
        }

        private LookupSignal CreateLookupSignal(LookupTableXML lookupTableElement)
        {
            var signalName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(lookupTableElement.id);
            var records = (lookupTableElement.Item as TableLookupTableXML)?.record;

            var signal = new LookupSignal()
            {
                Name = signalName,
            };

            if (records != null)
            {
                DefineFunction(signal.Function, records);
            }

            return signal;
        }


        private void DefineFunction(IFunction function, List<TimeRelativeControlTableRecordXML> records)
        {
            if (records == null || function == null) return;

            function.Arguments[0].SetValues(records.Select(e => e.time));
            function.Components[0].SetValues(records.Select(e => e.value));
        }

        private void DefineFunction(IFunction function, List<DateRecord2DataXML> records)
        {
            if (records == null || function == null) return;

            function.Arguments[0].SetValues(records.Select(e => e.x));
            function.Components[0].SetValues(records.Select(e => e.y));
        }

        private void CreateConditionFromXmlElementAndAddToControlGroup(IList<IControlGroup> controlGroups, TriggerXML conditionElement)
        {
            var item = conditionElement.Item;

            if (item is StandardTriggerXML standardConditionElement)
            {
                CreateStandardConditionAndAddToControlGroup(controlGroups, standardConditionElement);
                var outputItems = standardConditionElement.@true.Concat(standardConditionElement.@false);
                foreach (var outputItem in outputItems)
                {
                    CreateConditionFromXmlElementAndAddToControlGroup(controlGroups, outputItem);
                }
            }
        }

        private void CreateStandardConditionAndAddToControlGroup(IList<IControlGroup> controlGroups, StandardTriggerXML standardConditionElement)
        {
            var id = standardConditionElement.id;
            var tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

            var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);
            if (controlGroup != null)
            {
                StandardCondition condition = null;

                switch (tag)
                {
                    case RtcXmlTag.StandardCondition:
                        condition = CreateStandardCondition(standardConditionElement);
                        break;
                    case RtcXmlTag.TimeCondition:
                        condition = CreateTimeCondition(standardConditionElement);
                        break;
                    case RtcXmlTag.DirectionalCondition:
                        condition = CreateDirectionalCondition(standardConditionElement);
                        break;
                }

                if (condition != null)
                {
                    var conditionElement = standardConditionElement.condition;
                    var referenceElementValue = (conditionElement.Item as RelationalConditionXMLX1Series)?.@ref;
                    var reference = referenceElementValue == inputReferenceEnumStringType.EXPLICIT
                        ? StandardCondition.ReferenceType.Explicit
                        : StandardCondition.ReferenceType.Implicit;

                    var operatorElementValue = conditionElement.relationalOperator;
                    var operation = GetOperationFromXmlObject(operatorElementValue);

                    var valueElementValue = (conditionElement.Item1 as string);
                    var value = valueElementValue != null
                        ? double.Parse(valueElementValue, CultureInfo.InvariantCulture)
                        : 0.0d;

                    condition.Reference = reference;
                    condition.Operation = operation;
                    condition.Value = value;

                    AddConditionToControlGroup(condition, controlGroup);
                }
            }
        }

        private StandardCondition CreateStandardCondition(StandardTriggerXML standardConditionElement)
        {
            var conditionName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(standardConditionElement.id);

            var standardCondition = new StandardCondition();
            standardCondition.Name = conditionName;

            return standardCondition;
        }

        private TimeCondition CreateTimeCondition(StandardTriggerXML standardConditionElement)
        {
            var conditionName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(standardConditionElement.id);

            var timeCondition = new TimeCondition();
            timeCondition.Name = conditionName;

            return timeCondition;
        }

        private DirectionalCondition CreateDirectionalCondition(StandardTriggerXML standardConditionElement)
        {
            var conditionName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(standardConditionElement.id);

            var directionalCondition = new DirectionalCondition();
            directionalCondition.Name = conditionName;

            return directionalCondition;
        }

        private void AddConditionToControlGroup(ConditionBase condition, IControlGroup controlGroup)
        {
            if (condition == null ||
                controlGroup == null ||
                controlGroup.Conditions.Contains(condition) ||
                controlGroup.Conditions.Select(i => i.Name).Contains(condition.Name)) return;

            controlGroup.Conditions.Add(condition);
        }

        private Operation GetOperationFromXmlObject(relationalOperatorEnumStringType relationalOperator)
        {
            Operation operation;

            switch (relationalOperator)
            {
                case relationalOperatorEnumStringType.Equal:
                    operation = Operation.Equal;
                    break;
                case relationalOperatorEnumStringType.Greater:
                    operation = Operation.Greater;
                    break;
                case relationalOperatorEnumStringType.GreaterEqual:
                    operation = Operation.GreaterEqual;
                    break;
                case relationalOperatorEnumStringType.Less:
                    operation = Operation.Less;
                    break;
                case relationalOperatorEnumStringType.LessEqual:
                    operation = Operation.LessEqual;
                    break;
                case relationalOperatorEnumStringType.Unequal:
                    operation = Operation.Unequal;
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            return operation;
        }

        public ExtrapolationType GetExtrapolationType(interpolationOptionEnumStringType extrapolationOption)
        {
            return extrapolationOption == interpolationOptionEnumStringType.BLOCK
                ? ExtrapolationType.Constant
                : ExtrapolationType.Linear;
        }

        public InterpolationType GetInterpolationType(interpolationOptionEnumStringType interpolationOption)
        {
            return interpolationOption == interpolationOptionEnumStringType.BLOCK
                ? InterpolationType.Constant
                : InterpolationType.Linear;
        }
    }
}
