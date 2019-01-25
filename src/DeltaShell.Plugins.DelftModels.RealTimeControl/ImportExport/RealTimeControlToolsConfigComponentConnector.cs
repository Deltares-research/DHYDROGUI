using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlToolsConfigComponentConnector
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlToolsConfigComponentConnector));

        public static void ConnectRules(IList<RuleXML> ruleElements, IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            foreach (var ruleElement in ruleElements)
            {
                var item = ruleElement.Item;

                if (item is TimeAbsoluteXML timeRuleElement)
                {
                    ConnectTimeRule(controlGroups, connectionPoints, timeRuleElement);
                }
                else if (item is TimeRelativeXML relativeTimeRuleElement)
                {
                    ConnectRelativeTimeRule(controlGroups, connectionPoints, relativeTimeRuleElement);
                }
                else if (item is PidXML pidRuleElement)
                {
                    // ConnectPidRule()
                }
                else if (item is IntervalXML intervalRuleElement)
                {
                    // ConnectIntervalRule()
                }
                else if (item is LookupTableXML lookupTableElement)
                {
                    // ConnectHydraulicRule()
                }
            }
        }

        public static void ConnectConditions(List<TriggerXML> conditionElements, IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            foreach (var conditionElement in conditionElements)
            {
                var item = conditionElement.Item;

                if (item is StandardTriggerXML standardConditionElement)
                {
                    ConnectStandardCondition(controlGroups, connectionPoints, standardConditionElement);
                }
            }
        }

        private static void ConnectTimeRule(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, TimeAbsoluteXML timeRuleElement)
        {
            var id = timeRuleElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id);
            var rule = controlGroup.GetRuleByElementId<TimeRule>(id);

            var ruleOutputElementName = timeRuleElement.output.y;
            var output = (Output) connectionPoints.GetByName<Output>(ruleOutputElementName);

            AddOutputToRule(rule, output);
            AddConnectionPointToControlGroup(output, controlGroup);
        }

        private static void ConnectRelativeTimeRule(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, TimeRelativeXML relativeTimeRuleElement)
        {
            var id = relativeTimeRuleElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id);
            var rule = (RelativeTimeRule) controlGroup.GetRuleByElementId<RelativeTimeRule>(id);
            if (rule == null)
            {
                Log.WarnFormat(
                    Resources
                        .RealTimeControlToolsConfigComponentConnector_ConnectRelativeTimeRules_Could_not_find_Relative_Time_Rule_with_id___0____See_file____1___,
                    id, RealTimeControlXMLFiles.XmlTools);
                return;
            }

            var outputName = relativeTimeRuleElement.output.y;
            var output = (Output) connectionPoints.GetByName<Output>(outputName);

            AddConnectionPointToControlGroup(output, controlGroup);
            AddOutputToRule(rule, output);
        }

        private static void AddOutputToRule(RuleBase rule, Output output)
        {
            if (!rule.Outputs.Contains(output))
            {
                rule.Outputs.Add(output);
            }
        }

        private static StandardCondition ConnectStandardCondition(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, StandardTriggerXML standardConditionElement)
        {
            var id = standardConditionElement.id;
            var tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

            var controlGroup = controlGroups.GetControlGroupByElementId(id);

            if (controlGroup == null) return null;

            var condition = FindStandardConditionWithCorrectType(tag, controlGroup, id);

            if (condition == null) return null;

            ConnectStandardConditionToInput(connectionPoints, standardConditionElement, condition, controlGroup);
            ConnectStandardConditionToTrueOutput(controlGroups, connectionPoints, standardConditionElement, condition, controlGroup);
            ConnectStandardConditionToFalseOutput(controlGroups, connectionPoints, standardConditionElement, condition, controlGroup);

            return condition;
        }

        private static void ConnectStandardConditionToFalseOutput(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, StandardTriggerXML standardConditionElement, StandardCondition condition, IControlGroup controlGroup)
        {
            var falseOutputItems = standardConditionElement.@false.Select(f => f.Item);
            foreach (var falseOutputItem in falseOutputItems)
            {
                var falseOutputs = condition.FalseOutputs;

                if (falseOutputItem is string ruleID)
                {
                    var ruleAsOutput = controlGroup.GetRuleByElementId(ruleID);

                    if (!falseOutputs.Contains(ruleAsOutput))
                        falseOutputs.Add(ruleAsOutput);
                    continue;
                }

                if (falseOutputItem is StandardTriggerXML standardConditionAsOutputElement)
                {
                    var standardConditionAsOutput =
                        ConnectStandardCondition(controlGroups, connectionPoints, standardConditionAsOutputElement);

                    if (!falseOutputs.Contains(standardConditionAsOutput))
                        falseOutputs.Add(standardConditionAsOutput);
                }
            }
        }

        private static void ConnectStandardConditionToTrueOutput(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, StandardTriggerXML standardConditionElement, StandardCondition condition, IControlGroup controlGroup)
        {
            var trueOutputItems = standardConditionElement.@true.Select(t => t.Item);
            foreach (var trueOutputItem in trueOutputItems)
            {
                var trueOutputs = condition.TrueOutputs;

                if (trueOutputItem is string ruleID)
                {
                    var ruleAsOutput = controlGroup.GetRuleByElementId(ruleID);

                    if (!trueOutputs.Contains(ruleAsOutput))
                    {
                        trueOutputs.Add(ruleAsOutput);
                    }

                    continue;
                }

                if (trueOutputItem is StandardTriggerXML standardConditionAsOutputElement)
                {
                    var standardConditionAsOutput =
                        ConnectStandardCondition(controlGroups, connectionPoints, standardConditionAsOutputElement);

                    if (!trueOutputs.Contains(standardConditionAsOutput))
                        trueOutputs.Add(standardConditionAsOutput);
                }
            }
        }

        private static void ConnectStandardConditionToInput(IList<ConnectionPoint> connectionPoints, StandardTriggerXML standardConditionElement, StandardCondition condition, IControlGroup controlGroup)
        {
            if (condition.Reference == StandardCondition.ReferenceType.Explicit)
            {
                var inputName = (standardConditionElement.condition.Item as RelationalConditionXMLX1Series)?.Value;
                var correspondingInput = (Input) connectionPoints.GetByName<Input>(inputName);
                condition.Input = correspondingInput;
                AddConnectionPointToControlGroup(correspondingInput, controlGroup);
            }
        }

        private static StandardCondition FindStandardConditionWithCorrectType(string tag, IControlGroup controlGroup, string id)
        {
            StandardCondition condition = null;

            switch (tag)
            {
                case RtcXmlTag.StandardCondition:
                    condition = (StandardCondition) controlGroup.GetConditionByElementId<StandardCondition>(id);
                    break;
                case RtcXmlTag.TimeCondition:
                    condition = (TimeCondition) controlGroup.GetConditionByElementId<TimeCondition>(id);
                    break;
                case RtcXmlTag.DirectionalCondition:
                    condition = (DirectionalCondition) controlGroup.GetConditionByElementId<DirectionalCondition>(id);
                    break;
            }

            return condition;
        }

        private static void AddConnectionPointToControlGroup(ConnectionPoint connectionPoint, IControlGroup controlGroup)
        {
            if (connectionPoint == null || controlGroup == null) return;

            var input = connectionPoint as Input;
            if (input != null)
            {
                if (!controlGroup.Inputs.Contains(connectionPoint) &&
                    !controlGroup.Inputs.Select(i => i.Name).Contains(input.Name))
                {
                    controlGroup.Inputs.Add(input);
                }

                return;
            }

            var output = connectionPoint as Output;
            if (output != null)
            {
                if (!controlGroup.Outputs.Contains(output) &&
                    !controlGroup.Outputs.Select(o => o.Name).Contains(output.Name))
                {
                    controlGroup.Outputs.Add(output);
                }
            }
        }
    }
}

