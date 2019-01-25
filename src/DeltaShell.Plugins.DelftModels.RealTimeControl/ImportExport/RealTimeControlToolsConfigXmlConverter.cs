using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using log4net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    static class RealTimeControlToolsConfigXmlConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlToolsConfigXmlConverter));

        public static IEnumerable<IControlGroup> CreateControlGroupsFromXmlElementIDs(IEnumerable<RuleXML> ruleElements)
        {
            var groupNames = GetDistinctControlGroupNamesFromElements(ruleElements);

            var controlGroups = CreateControlGroupsByName(groupNames);

            return controlGroups;
        }

        public static void CreateRulesFromXmlElementsAndAddToControlGroup(List<RuleXML> ruleElements, IList<IControlGroup> controlGroups)
        {
            if (ruleElements == null || controlGroups == null) return;

            foreach (var ruleElement in ruleElements)
            {
                CreateRuleFromXmlElementAndAddToControlGroup(controlGroups, ruleElement);
            }
        }

        public static void CreateConditionsFromXmlElementsAndAddToControlGroup(List<TriggerXML> conditionElements, IList<IControlGroup> controlGroups)
        {
            if (conditionElements == null || controlGroups == null) return;

            foreach (var conditionElement in conditionElements)
            {
                CreateConditionFromXmlElementAndAddToControlGroup(controlGroups, conditionElement);
            }
        }

        private static IEnumerable<IControlGroup> CreateControlGroupsByName(IEnumerable<string> groupNames)
        {
            foreach (var groupName in groupNames)
            {
                yield return new ControlGroup {Name = groupName};
            }
        }

        private static void CreateRuleFromXmlElementAndAddToControlGroup(IList<IControlGroup> controlGroups, RuleXML ruleElement)
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
                // CreatePidRuleAndAddToControlGroup(controlGroups, pidRuleElement);
            }
            else if (item is IntervalXML intervalRuleElement)
            {
                // CreateIntervalRuleAndAddToControlGroup(controlGroups, intervalRuleElement);
            }
            else if (item is LookupTableXML lookupTableElement)
            {
                // CreateHydraulicRuleAndAddToControlGroup(controlGroups, lookupTableElement);
            }
        }

        private static void CreateTimeRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, TimeAbsoluteXML timeRuleElement)
        {
            var controlGroup = controlGroups.GetControlGroupByElementId(timeRuleElement.id);
            if (controlGroup != null)
            {
                var rule = CreateTimeRule(timeRuleElement);
                if (rule != null)
                {
                    controlGroup.Rules.Add(rule);
                }
            }
        }

        private static TimeRule CreateTimeRule(TimeAbsoluteXML ruleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(ruleElement.id);
            var rule = new TimeRule(ruleName);

            return rule;
        }

        private static void CreateRelativeTimeRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, TimeRelativeXML relativeTimeRuleElement)
        {
            var controlGroup = controlGroups.GetControlGroupByElementId(relativeTimeRuleElement.id);
            if (controlGroup != null)
            {
                var rule = CreateRelativeTimeRule(relativeTimeRuleElement);
                if (rule != null)
                {
                    controlGroup.Rules.Add(rule);
                }
            }
        }

        private static RelativeTimeRule CreateRelativeTimeRule(TimeRelativeXML ruleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(ruleElement.id);
            var fromValue = ruleElement.valueOption == timeRelativeEnumStringType.RELATIVE;
            var minimumPeriod = (int) ruleElement.maximumPeriod;
            var table = ruleElement.controlTable;

            var rule = new RelativeTimeRule(ruleName, fromValue);
            rule.MinimumPeriod = minimumPeriod;
            rule.Function.DefineFunction(table);

            return rule;
        }

        private static void DefineFunction(this IFunction function, List<TimeRelativeControlTableRecordXML> records)
        {
            if (records == null || function == null) return;

            function.Arguments[0].SetValues(records.Select(e => e.time));
            function.Components[0].SetValues(records.Select(e => e.value));
        }

        private static void CreatePidRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, PidXML pidRuleElement)
        {
            var controlGroup = controlGroups.GetControlGroupByElementId(pidRuleElement.id);
            if (controlGroup != null)
            {
                var rule = CreatePidRule(pidRuleElement);
                if (rule != null)
                {
                    controlGroup.Rules.Add(rule);
                }
            }
        }

        private static PIDRule CreatePidRule(PidXML ruleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(ruleElement.id);
            var rule = new PIDRule
            {
                Name = ruleName
            };

            return rule;
        }

        private static void CreateIntervalRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, IntervalXML intervalRuleElement)
        {
            var controlGroup = controlGroups.GetControlGroupByElementId(intervalRuleElement.id);
            if (controlGroup != null)
            {
                var rule = CreateIntervalRule(intervalRuleElement);
                if (rule != null)
                {
                    controlGroup.Rules.Add(rule);
                }
            }
        }

        private static IntervalRule CreateIntervalRule(IntervalXML ruleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(ruleElement.id);
            var rule = new IntervalRule
            {
                Name = ruleName
            };

            return rule;
        }

        private static void CreateHydraulicRuleAndAddToControlGroup(IList<IControlGroup> controlGroups, LookupTableXML lookupTableElement)
        {
            var id = lookupTableElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id);
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

                if (rule != null)
                {
                    controlGroup.Rules.Add(rule);
                }
            }
        }

        private static HydraulicRule CreateHydraulicRule(LookupTableXML ruleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(ruleElement.id);
            var rule = new HydraulicRule()
            {
                Name = ruleName
            };

            return rule;
        }

        private static FactorRule CreateFactorRule(LookupTableXML ruleElement)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(ruleElement.id);
            var rule = new FactorRule
            {
                Name = ruleName
            };

            return rule;
        }

        private static void CreateConditionFromXmlElementAndAddToControlGroup(IList<IControlGroup> controlGroups, TriggerXML conditionElement)
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

        private static void CreateStandardConditionAndAddToControlGroup(IList<IControlGroup> controlGroups, StandardTriggerXML standardConditionElement)
        {
            var id = standardConditionElement.id;
            var tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

            var controlGroup = controlGroups.GetControlGroupByElementId(id);
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

        private static StandardCondition CreateStandardCondition(StandardTriggerXML standardConditionElement)
        {
            var conditionName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(standardConditionElement.id);

            var standardCondition = new StandardCondition();
            standardCondition.Name = conditionName;

            return standardCondition;
        }

        private static TimeCondition CreateTimeCondition(StandardTriggerXML standardConditionElement)
        {
            var conditionName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(standardConditionElement.id);

            var timeCondition = new TimeCondition();
            timeCondition.Name = conditionName;

            return timeCondition;
        }

        private static DirectionalCondition CreateDirectionalCondition(StandardTriggerXML standardConditionElement)
        {
            var conditionName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(standardConditionElement.id);

            var directionalCondition = new DirectionalCondition();
            directionalCondition.Name = conditionName;

            return directionalCondition;
        }

        private static Operation GetOperationFromXmlObject(relationalOperatorEnumStringType relationalOperator)
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

        private static IList<string> GetDistinctControlGroupNamesFromElements(IEnumerable<RuleXML> ruleElements)
        {
            var elementsIDs = ruleElements.SelectMany(e => e.GetIdFromRuleElement());

            var groupNames = new List<string>();

            foreach (var elementID in elementsIDs)
            {
                var groupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(elementID);

                if (!groupNames.Contains(groupName) && !string.IsNullOrEmpty(groupName))
                {
                    groupNames.Add(groupName);
                }
            }

            return groupNames;
        }

        private static IEnumerable<string> GetIdFromRuleElement(this RuleXML ruleElement)
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

        private static void AddConditionToControlGroup(ConditionBase condition, IControlGroup controlGroup)
        {
            if (condition == null
                || controlGroup == null
                || controlGroup.Conditions.Contains(condition)
                || controlGroup.Conditions.Select(i => i.Name).Contains(condition.Name)) return;

            controlGroup.Conditions.Add(condition);
        }
    }
}
