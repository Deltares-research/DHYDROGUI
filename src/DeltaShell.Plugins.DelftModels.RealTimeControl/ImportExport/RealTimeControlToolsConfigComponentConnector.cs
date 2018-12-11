using DelftTools.Functions;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlToolsConfigComponentConnector
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlToolsConfigComponentConnector));

        public static void ConnectTimeRules(List<TimeAbsoluteXML> timeRuleElements, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            if (timeRuleElements == null || controlGroups == null || connectionPoints == null) return;

            foreach (var timeRuleElement in timeRuleElements)
            {
                var id = timeRuleElement.id;

                var correspondingControlGroup = RealTimeControlXmlReaderHelper.GetControlGroupByElementId(id, controlGroups);

                var timeRule = RealTimeControlXmlReaderHelper.GetRuleByElementIdInControlGroup(id, correspondingControlGroup) as TimeRule;
                if (timeRule == null)
                {
                    Log.WarnFormat(Resources.RealTimeControlToolsConfigComponentConnector_ConnectTimeRules_Could_not_find_Time_Rule_with_id___0____See_file____1___, id, RealTimeControlXMLFiles.XmlTools);
                    continue;
                }

                var ruleOutputElementName = timeRuleElement.output.y;

                if (!ruleOutputElementName.StartsWith(RtcXmlTag.Output))
                {
                    Log.WarnFormat(Resources.RealTimeControlToolsConfigComponentConnector_ConnectTimeRules_The_output_of_a_rule_should_be_an_output__tagged_with___0____See_file____1___, RtcXmlTag.Output, RealTimeControlXMLFiles.XmlTools);
                    continue;
                }

                var correspondingOutput = (Output)RealTimeControlXmlReaderHelper.GetConnectionPointByName(ruleOutputElementName, connectionPoints);
                timeRule.Outputs.Add(correspondingOutput);
                AddConnectionPointToControlGroup(correspondingOutput, correspondingControlGroup);
            }
        }

        public static void ConnectRelativeTimeRules(List<TimeRelativeXML> relativeTimeRuleElements, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            if (relativeTimeRuleElements == null || controlGroups == null || connectionPoints == null) return;

            foreach (var relativeTimeRuleElement in relativeTimeRuleElements)
            {
                var id = relativeTimeRuleElement.id;

                var correspondingControlGroup = RealTimeControlXmlReaderHelper.GetControlGroupByElementId(id, controlGroups);

                var relativeTimeRule = RealTimeControlXmlReaderHelper.GetRuleByElementIdInControlGroup(id, correspondingControlGroup) as RelativeTimeRule;
                if (relativeTimeRule == null)
                {
                    Log.WarnFormat(Resources.RealTimeControlToolsConfigComponentConnector_ConnectRelativeTimeRules_Could_not_find_Relative_Time_Rule_with_id___0____See_file____1___, id, RealTimeControlXMLFiles.XmlTools);
                    continue;
                }

                var fromValue = relativeTimeRuleElement.valueOption == timeRelativeEnumStringType.RELATIVE;
                var minimumPeriod = relativeTimeRuleElement.maximumPeriod;
                var table = relativeTimeRuleElement.controlTable;

                var ruleOutputElementName = relativeTimeRuleElement.output.y;
                if (!ruleOutputElementName.StartsWith(RtcXmlTag.Output))
                {
                    Log.WarnFormat(Resources.RealTimeControlToolsConfigComponentConnector_ConnectRelativeTimeRules_The_output_of_relative_time_rule___0___should_be_an_output__see_tag__Output____See_file____1___, relativeTimeRule.Name, RealTimeControlXMLFiles.XmlTools);
                    continue;
                }

                var correspondingOutput = (Output)RealTimeControlXmlReaderHelper.GetConnectionPointByName(ruleOutputElementName, connectionPoints);

                AddConnectionPointToControlGroup(correspondingOutput, correspondingControlGroup);

                relativeTimeRule.FromValue = fromValue;
                relativeTimeRule.MinimumPeriod = (int)minimumPeriod;
                DefineFunctionFromXmlTable(table, relativeTimeRule.Function);

                if (!relativeTimeRule.Outputs.Contains(correspondingOutput))
                {
                    relativeTimeRule.Outputs.Add(correspondingOutput);
                }              
            }
        }

        public static void ConnectStandardConditions(List<StandardTriggerXML> standardConditionElements, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            if (standardConditionElements == null || controlGroups == null || connectionPoints == null) return;

            standardConditionElements.ForEach(e =>
                {
                    GetAndConnectStandardConditionRecursively(e, controlGroups, connectionPoints);
                });
        }

        private static void AddConnectionPointToControlGroup(ConnectionPoint connectionPoint, ControlGroup controlGroup)
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

        private static void DefineFunctionFromXmlTable(List<TimeRelativeControlTableRecordXML> records, IFunction function)
        {
            if (records == null || function == null) return;

            function.Arguments[0].SetValues(records.Select(e => e.time));
            function.Components[0].SetValues(records.Select(e => e.value));
        }

        private static StandardCondition GetAndConnectStandardConditionRecursively(StandardTriggerXML standardConditionElement, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            if (standardConditionElement == null || controlGroups == null || connectionPoints == null) return null;

            var id = standardConditionElement.id;
            var correspondingControlGroup = RealTimeControlXmlReaderHelper.GetControlGroupByElementId(id, controlGroups);
            var correspondingStandardCondition = (StandardCondition)RealTimeControlXmlReaderHelper.GetConditionByElementIdInControlGroup(id, correspondingControlGroup);

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
                var inputName = inputElement.Value;
                var correspondingInput = (Input)RealTimeControlXmlReaderHelper.GetConnectionPointByName(inputName, connectionPoints);
                correspondingStandardCondition.Input = correspondingInput;
                AddConnectionPointToControlGroup(correspondingInput, correspondingControlGroup);
            }

            correspondingStandardCondition.Operation = GetOperationFromXmlObject(operatorElementValue);

            if (valueElementValue != null)
                correspondingStandardCondition.Value = double.Parse(valueElementValue, CultureInfo.InvariantCulture);

            foreach (var trueOutputItem in trueOutputItems)
            {
                var trueOutputs = correspondingStandardCondition.TrueOutputs;

                var rule = trueOutputItem as string;
                if (rule != null)
                {
                    var ruleAsOutput = RealTimeControlXmlReaderHelper.GetRuleByElementIdInControlGroup(rule, correspondingControlGroup);

                    if (!trueOutputs.Contains(ruleAsOutput))
                        trueOutputs.Add(ruleAsOutput);
                    continue;
                }

                var standardCondition = trueOutputItem as StandardTriggerXML;
                if (standardCondition != null)
                {
                    var standardConditionAsOutput = GetAndConnectStandardConditionRecursively(standardCondition, controlGroups, connectionPoints);

                    if (!trueOutputs.Contains(standardConditionAsOutput))
                        trueOutputs.Add(standardConditionAsOutput);
                }
            }

            foreach (var falseOutputItem in falseOutputItems)
            {
                var falseOutputs = correspondingStandardCondition.FalseOutputs;

                var rule = falseOutputItem as string;
                if (rule != null)
                {
                    var ruleAsOutput = RealTimeControlXmlReaderHelper.GetRuleByElementIdInControlGroup(rule, correspondingControlGroup);

                    if (!falseOutputs.Contains(ruleAsOutput))
                        falseOutputs.Add(ruleAsOutput);
                    continue;
                }

                // Standard Condition
                var standardCondition = falseOutputItem as StandardTriggerXML;
                if (standardCondition != null)
                {
                    var standardConditionAsOutput = GetAndConnectStandardConditionRecursively(standardCondition, controlGroups, connectionPoints);

                    if (!falseOutputs.Contains(standardConditionAsOutput))
                        falseOutputs.Add(standardConditionAsOutput);
                }
            }

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
                    throw new InvalidEnumArgumentException();
            }

            return operation;
        }
    }
}
