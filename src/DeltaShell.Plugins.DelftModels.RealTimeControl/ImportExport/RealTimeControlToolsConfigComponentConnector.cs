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

                var correspondingControlGroup = RealTimeControlXmlReaderHelper.GetControlGroupByElementId(id, controlGroups);

                var timeRule = RealTimeControlXmlReaderHelper.GetRuleByElementIdInControlGroup(id, correspondingControlGroup) as TimeRule;
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

                var correspondingOutput = (Output)RealTimeControlXmlReaderHelper.GetConnectionPointByName(ruleOutputElementName, connectionPoints);
                timeRule.Outputs.Add(correspondingOutput);
                AddConnectionPointToControlGroup(correspondingOutput, correspondingControlGroup);
            }
        }

        public static void ConnectRelativeTimeRules(List<TimeRelativeXML> relativeTimeRuleElements, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            foreach (var relativeTimeRuleElement in relativeTimeRuleElements)
            {
                var id = relativeTimeRuleElement.id;

                var correspondingControlGroup = RealTimeControlXmlReaderHelper.GetControlGroupByElementId(id, controlGroups);

                var relativeTimeRule = RealTimeControlXmlReaderHelper.GetRuleByElementIdInControlGroup(id, correspondingControlGroup) as RelativeTimeRule;
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
            correspondingStandardCondition.Value = Double.Parse(valueElementValue, CultureInfo.InvariantCulture);

            trueOutputItems.ForEach(item =>
            {
                // Rule
                if (item is string)
                {
                    var ruleAsOutput = RealTimeControlXmlReaderHelper.GetRuleByElementIdInControlGroup((string)item, correspondingControlGroup);
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
                    var ruleAsOutput = RealTimeControlXmlReaderHelper.GetRuleByElementIdInControlGroup((string)item, correspondingControlGroup);
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
    }
}
