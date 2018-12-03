using DelftTools.Functions;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlToolsConfigComponentConnector
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlDataConfigXmlConverter));

        public static void ConnectTimeRules(List<TimeAbsoluteXML> timeRuleElements, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            foreach (var timeRuleElement in timeRuleElements)
            {
                var id = timeRuleElement.id;

                var correspondingControlGroup = GetControlGroupByElementId(id, controlGroups);

                var timeRule = GetRuleByElementIdInControlGroup(id, correspondingControlGroup) as TimeRule;
                if (timeRule == null)
                {
                    Log.Warn("WARNING");
                    continue;
                }

                var ruleOutputElementName = timeRuleElement.output.y;

                if (!ruleOutputElementName.StartsWith(RtcXmlTag.Output))
                {
                    Log.Warn("WARNING");
                    continue;
                }

                var correspondingOutput = (Output)GetConnectionPointByXmlName(ruleOutputElementName, connectionPoints);
                timeRule.Outputs.Add(correspondingOutput);
                AddConnectionPointToControlGroup(correspondingOutput, correspondingControlGroup);
            }
        }

        public static void ConnectRelativeTimeRules(List<TimeRelativeXML> relativeTimeRuleElements, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            foreach (var relativeTimeRuleElement in relativeTimeRuleElements)
            {
                var id = relativeTimeRuleElement.id;

                var correspondingControlGroup = GetControlGroupByElementId(id, controlGroups);

                var relativeTimeRule = GetRuleByElementIdInControlGroup(id, correspondingControlGroup) as RelativeTimeRule;
                if (relativeTimeRule == null)
                {
                    Log.Warn("WARNING");
                    continue;
                }

                var fromValue = relativeTimeRuleElement.valueOption == timeRelativeEnumStringType.RELATIVE;
                var minimumPeriod = relativeTimeRuleElement.maximumPeriod;
                var table = relativeTimeRuleElement.controlTable;

                var ruleOutputElementName = relativeTimeRuleElement.output.y;
                if (!ruleOutputElementName.StartsWith(RtcXmlTag.Output))
                {
                    Log.Warn($"The output of relative time rule '{relativeTimeRule.Name}' should be an output (see tag [Output]).");
                    continue;
                }

                var correspondingOutput = (Output)GetConnectionPointByXmlName(ruleOutputElementName, connectionPoints);

                AddConnectionPointToControlGroup(correspondingOutput, correspondingControlGroup);

                relativeTimeRule.FromValue = fromValue;
                relativeTimeRule.MinimumPeriod = (int)minimumPeriod;
                DefineFunctionFromXmlTable(table, relativeTimeRule.Function);
                relativeTimeRule.Outputs.Add(correspondingOutput);
            }
        }

        public static void ConnectStandardConditions(List<StandardTriggerXML> standardConditionElements, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            standardConditionElements.ForEach(e =>
                {
                    GetAndConnectStandardConditionRecursively(e, controlGroups, connectionPoints);
                });
        }

        private static void AddConnectionPointToControlGroup(ConnectionPoint connectionPoint, ControlGroup controlGroup)
        {
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

        private static void DefineFunctionFromXmlTable(List<TimeRelativeControlTableRecordXML> records, IFunction function)
        {
            function.Arguments[0].SetValues(records.Select(e => e.time));
            function.Components[0].SetValues(records.Select(e => e.value));
        }

        private static StandardCondition GetAndConnectStandardConditionRecursively(StandardTriggerXML standardConditionElement, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            var id = standardConditionElement.id;
            var correspondingControlGroup = GetControlGroupByElementId(id, controlGroups);
            var correspondingStandardCondition = (StandardCondition)GetConditionByElementIdInControlGroup(id, correspondingControlGroup);


            var conditionElement = standardConditionElement.condition;
            var inputElement = conditionElement.Item as RelationalConditionXMLX1Series;
            var referenceElementValue = inputElement?.@ref;
            var operatorElementValue = conditionElement.relationalOperator;
            var valueElementValue = conditionElement.Item1 as string;

            var trueOutputItems = standardConditionElement.@true.Select(t=> t.Item);
            var falseOutputItems = standardConditionElement.@false.Select(f => f.Item);

            var hasExplicitInput = referenceElementValue == inputReferenceEnumStringType.EXPLICIT; 

            correspondingStandardCondition.Reference = hasExplicitInput
                ? StandardCondition.ReferenceType.Explicit
                : StandardCondition.ReferenceType.Implicit;

            if (hasExplicitInput)
            {
                var inputXmlName = inputElement.Value;
                var correspondingInput = (Input)GetConnectionPointByXmlName(inputXmlName, connectionPoints);
                correspondingStandardCondition.Input = correspondingInput;
                AddConnectionPointToControlGroup(correspondingInput, correspondingControlGroup);
            }

            correspondingStandardCondition.Operation = GetOperationFromXmlObject(operatorElementValue);
            correspondingStandardCondition.Value = Double.Parse(valueElementValue, CultureInfo.InvariantCulture);

            trueOutputItems.ForEach(item =>
            {
                // Rule
                if (item is string)
                {
                    var ruleAsOutput = GetRuleByElementIdInControlGroup((string)item, correspondingControlGroup);
                    correspondingStandardCondition.TrueOutputs.Add(ruleAsOutput);
                }

                // Standard Condition
                if (item is StandardTriggerXML)
                {
                    var standardConditionAsOutput = GetAndConnectStandardConditionRecursively((StandardTriggerXML)item, controlGroups, connectionPoints);
                    correspondingStandardCondition.TrueOutputs.Add(standardConditionAsOutput);
                }
            });

            falseOutputItems.ForEach(item =>
            {
                // Rule
                if (item is string)
                {
                    var ruleAsOutput = GetRuleByElementIdInControlGroup((string)item, correspondingControlGroup);
                    correspondingStandardCondition.FalseOutputs.Add(ruleAsOutput);
                }

                // Standard Condition
                if (item is StandardTriggerXML)
                {
                    var standardConditionAsOutput = GetAndConnectStandardConditionRecursively((StandardTriggerXML)item, controlGroups, connectionPoints);
                    correspondingStandardCondition.FalseOutputs.Add(standardConditionAsOutput);
                }
            });

            return correspondingStandardCondition;
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
                    throw new Exception();
            }

            return operation;
        }

        private static ControlGroup GetControlGroupByElementId(string id, IList<ControlGroup> controlGroups)
        {
            var groupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id);
            var controlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);
            if (controlGroup == null)
            {
                Log.Warn($"Could not find the controlgroup '{groupName}' that is referenced in file '{RealTimeControlXMLFiles.XmlTools}'. The group needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}' too.");
                return null;
            }
            return controlGroup;
        }

        private static ConnectionPoint GetConnectionPointByXmlName(string xmlName, IList<ConnectionPoint> connectionPoints)
        {
            var correspondingConnectionPoint = connectionPoints.FirstOrDefault(o => o.XmlName == xmlName);
            if (correspondingConnectionPoint == null)
            {
                Log.Warn($"Could not find the input/output '{xmlName}' that is referenced in file '{RealTimeControlXMLFiles.XmlTools}'. The input/output needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}' too.");
            }

            return correspondingConnectionPoint;
        }

        private static RuleBase GetRuleByElementIdInControlGroup(string id, ControlGroup controlGroup)
        {
            var ruleName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id);

            var correspondingRule = controlGroup.Rules.FirstOrDefault(r => r.Name == ruleName);
            if (correspondingRule == null)
            {
                Log.Warn($"Could not find the rule '{ruleName}' that is referenced in file '{RealTimeControlXMLFiles.XmlTools}'. The rule needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}' too.");
                return null;
            }

            return correspondingRule;
        }

        private static ConditionBase GetConditionByElementIdInControlGroup(string id, ControlGroup controlGroup)
        {
            var conditionName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id);

            var correspondingCondition = controlGroup.Conditions.FirstOrDefault(r => r.Name == conditionName);
            if (correspondingCondition == null)
            {
                Log.Warn($"Could not find the condition '{conditionName}' that is referenced in file '{RealTimeControlXMLFiles.XmlTools}'. The condition needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}' too.");
                return null;
            }

            return correspondingCondition;
        }
    }
}
