using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>
    /// Responsible for connecting all inputs and outputs of the rtc components to the rtc components.
    /// </summary>
    public class RealTimeControlToolsConfigComponentConnector
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlToolsConfigComponentConnector(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Connects inputs and outputs of rules to rules.
        /// </summary>
        /// <param name="ruleElements">The rule elements.</param>
        /// <param name="controlGroups">The control groups.</param>
        /// <param name="connectionPoints">The connection points.</param>
        /// <remarks>If parameter ruleElements or controlGroups or connectionPoints is NULL, methods returns.</remarks>
        public void ConnectRules(IEnumerable<RuleXML> ruleElements, IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            if (ruleElements == null || controlGroups == null || connectionPoints == null) return;

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
                    ConnectPidRule(controlGroups, connectionPoints, pidRuleElement);
                }
                else if (item is IntervalXML intervalRuleElement)
                {
                    ConnectIntervalRule(controlGroups, connectionPoints, intervalRuleElement);
                }
                else if (item is LookupTableXML lookupTableElement)
                {
                    ConnectHydraulicRule(controlGroups, connectionPoints, lookupTableElement);
                }
            }
        }

        /// <summary>
        /// Connects inputs and outputs of conditions to conditions.
        /// </summary>
        /// <param name="conditionElements">The condition elements.</param>
        /// <param name="controlGroups">The control groups.</param>
        /// <param name="connectionPoints">The connection points.</param>
        /// <remarks>If parameter conditionElements or controlGroups or connectionPoints is NULL, methods returns.</remarks>
        public void ConnectConditions(IEnumerable<TriggerXML> conditionElements, IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            if (conditionElements == null || controlGroups == null || connectionPoints == null) return;

            foreach (var conditionElement in conditionElements)
            {
                var item = conditionElement.Item;

                if (item is StandardTriggerXML standardConditionElement)
                {
                    ConnectStandardCondition(controlGroups, connectionPoints, standardConditionElement);
                }
            }
        }
        /// <summary>
        /// Connects input to signals. Rules will be connected to signals during ConnectRules.
        /// </summary>
        /// <param name="signalElements"></param>
        /// <param name="controlGroups"></param>
        /// <param name="connectionPoints"></param>
        public void ConnectSignals(IList<RuleXML> signalElements, IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            if (signalElements == null || controlGroups == null || connectionPoints == null) return;

            foreach (var signalElement in signalElements)
            {
                var item = signalElement.Item;

                if (item is LookupTableXML lookupTableXml)
                {
                    ConnectSignal(controlGroups, connectionPoints, lookupTableXml);
                }
            }
        }

        private void ConnectTimeRule(IEnumerable<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, TimeAbsoluteXML timeRuleElement)
        {
            var id = timeRuleElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);

            if (controlGroup == null) return;

            var rule = controlGroup.GetRuleByElementId<TimeRule>(id, logHandler);

            if (rule == null) return;

            var ruleOutputElementName = timeRuleElement.output.y;
            ConnectOutputToRule(connectionPoints, ruleOutputElementName, rule, controlGroup);
        }

        private void ConnectRelativeTimeRule(IEnumerable<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, TimeRelativeXML relativeTimeRuleElement)
        {
            var id = relativeTimeRuleElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);
            if (controlGroup == null) return;

            var rule = (RelativeTimeRule) controlGroup.GetRuleByElementId<RelativeTimeRule>(id, logHandler);
            if (rule == null)
            {
                logHandler.ReportWarningFormat(
                    Resources
                        .RealTimeControlToolsConfigComponentConnector_ConnectRelativeTimeRules_Could_not_find_Relative_Time_Rule_with_id___0____See_file____1___,
                    id, RealTimeControlXMLFiles.XmlTools);
                return;
            }

            var ruleOutputElementName = relativeTimeRuleElement.output.y;
            ConnectOutputToRule(connectionPoints, ruleOutputElementName, rule, controlGroup);
        }

        private void ConnectPidRule(IEnumerable<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, PidXML pidRuleElement)
        {
            var id = pidRuleElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);
            if (controlGroup == null) return;

            var rule = controlGroup.GetRuleByElementId<PIDRule>(id, logHandler);
            if (rule == null) return;

            var ruleInputElementName = pidRuleElement.input.x;
            ConnectInputToRule(connectionPoints, ruleInputElementName, rule, controlGroup);

            var setPointItem = pidRuleElement.input.Item;

            if (setPointItem == null)
            {
                logHandler.ReportWarningFormat(
                    Resources
                        .RealTimeControlDataConfigXmlSetter_SetSetPointOnPIDRules_PID_rule___0___must_have_a_setpoint__Please_check_file____1___,
                    id, RealTimeControlXMLFiles.XmlTools);
            }
            else if (setPointItem is string signalId && signalId.Contains(RtcXmlTag.Signal))
            {
                var signal = controlGroup.GetSignalByElementId<LookupSignal>(signalId, logHandler);
                signal.RuleBases.Add(rule);
            }

            var ruleOutputElementName = pidRuleElement.output.y;
            ConnectOutputToRule(connectionPoints, ruleOutputElementName, rule, controlGroup);
        }

        private void ConnectIntervalRule(IEnumerable<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, IntervalXML intervalRuleElement)
        {
            var id = intervalRuleElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);
            if (controlGroup == null) return;

            var rule = controlGroup.GetRuleByElementId<IntervalRule>(id, logHandler);
            if (rule == null) return;

            var ruleInputElementName = intervalRuleElement.input.x.Value;
            ConnectInputToRule(connectionPoints, ruleInputElementName, rule, controlGroup);

            var setPoint = intervalRuleElement.input.setpoint;

            if (setPoint == null)
            {
                logHandler.ReportWarningFormat(
                    Resources
                        .RealTimeControlDataConfigXmlSetter_SetSetPointOnIntervalRules_Interval_rule___0___must_have_a_setpoint__Please_check_file____1___,
                    id, RealTimeControlXMLFiles.XmlTools);
            }
            else if (setPoint.Contains(RtcXmlTag.Signal))
            {
                id = setPoint;
                var signal = controlGroup.GetSignalByElementId<LookupSignal>(id, logHandler);
                signal.RuleBases.Add(rule);
            };

            var ruleOutputElementName = intervalRuleElement.output.y;
            ConnectOutputToRule(connectionPoints, ruleOutputElementName, rule, controlGroup);
        }

        private void ConnectHydraulicRule(IEnumerable<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, LookupTableXML lookupTableElement)
        {
            var id = lookupTableElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);
            if (controlGroup == null) return;

            var rule = controlGroup.GetRuleByElementId<HydraulicRule>(id, logHandler);
            if (rule == null) return;

            var ruleInputElementName = lookupTableElement.input.x.Value;
            ConnectInputToRule(connectionPoints, ruleInputElementName, rule, controlGroup);

            var ruleOutputElementName = lookupTableElement.output.y;
            ConnectOutputToRule(connectionPoints, ruleOutputElementName, rule, controlGroup);
        }

        private void ConnectInputToRule(IEnumerable<ConnectionPoint> connectionPoints, string ruleInputElementName, RuleBase rule, IControlGroup controlGroup)
        {
            if (ruleInputElementName == null)
            {
                logHandler.ReportWarningFormat(
                    Resources
                        .RealTimeControlXmlReaderHelper_ConnectInputToRule_Could_not_find_the_input_for_rule___0____in_control_group__1___The_input_needs_to_be_referenced_in_file___2___,
                    rule.Name, controlGroup.Name, RealTimeControlXMLFiles.XmlTools);
                return;
            }
            // in case there is a time delay between brackets in the name
            var regex = new Regex(@"\[(\d+)\]");
            var inputName = regex
                .Replace(ruleInputElementName, string.Empty)
                .Replace(RtcXmlTag.Delayed, string.Empty);

            var input = (Input)connectionPoints.GetByName<Input>(inputName, logHandler);
            AddInputToRule(rule, input);
            AddConnectionPointToControlGroup(input, controlGroup);
        }

        private void ConnectSignal(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, LookupTableXML lookupTableElement)
        {
            var id = lookupTableElement.id;
            var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);
            if (controlGroup == null) return;

            var signal = controlGroup.GetSignalByElementId<LookupSignal>(id, logHandler);
            if (signal == null) return;

            var signalInputElementName = lookupTableElement.input.x.Value;
            ConnectInputToSignal(connectionPoints, signalInputElementName, signal, controlGroup);
        }

        private void ConnectInputToSignal(IList<ConnectionPoint> connectionPoints, string signalInputElementName,SignalBase signal, IControlGroup controlGroup)
        {
            if (signalInputElementName == null)
            {
                logHandler.ReportWarningFormat(
                    Resources
                        .RealTimeControlXmlReaderHelper_ConnectInputToRule_Could_not_find_the_input_for_signal___0____in_control_group__1___The_input_needs_to_be_referenced_in_file___2___,
                    signal.Name, controlGroup.Name, RealTimeControlXMLFiles.XmlTools);
                return;
            }
            var correspondingInput = (Input)connectionPoints.GetByName<Input>(signalInputElementName, logHandler);
            if (!signal.Inputs.Contains(correspondingInput))
            {
                signal.Inputs.Add(correspondingInput);
            }

            AddConnectionPointToControlGroup(correspondingInput, controlGroup);
        }

        private void AddInputToRule(RuleBase rule, Input input)
        {
            if (!rule.Inputs.Contains(input))
            {
                rule.Inputs.Add(input);
            }
        }

        private void ConnectOutputToRule(IEnumerable<ConnectionPoint> connectionPoints, string ruleOutputElementName, RuleBase rule, IControlGroup controlGroup)
        {
            if (ruleOutputElementName == null)
            {
                logHandler.ReportWarningFormat(
                    Resources
                        .RealTimeControlXmlReaderHelper_ConnectOutputToRule_Could_not_find_the_output_for_rule___0____in_control_group__1___The_output_needs_to_be_referenced_in_file___2___,
                    rule.Name, controlGroup.Name, RealTimeControlXMLFiles.XmlTools);
                return;
            }
            
            var output = (Output)connectionPoints.GetByName<Output>(ruleOutputElementName, logHandler);
            AddOutputToRule(rule, output);
            AddConnectionPointToControlGroup(output, controlGroup);
        }

        private void AddOutputToRule(RuleBase rule, Output output)
        {
            if (!rule.Outputs.Contains(output))
            {
                rule.Outputs.Add(output);
            }
        }
       
        private StandardCondition ConnectStandardCondition(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, StandardTriggerXML standardConditionElement)
        {
            var id = standardConditionElement.id;
            var tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

            var controlGroup = controlGroups.GetControlGroupByElementId(id, logHandler);
            if (controlGroup == null) return null;

            var condition = FindStandardConditionWithCorrectType(tag, controlGroup, id);
            if (condition == null) return null;

            if (condition.Reference == StandardCondition.ReferenceType.Explicit)
            {
                ConnectStandardConditionToInput(connectionPoints, standardConditionElement, condition, controlGroup);
            }

            ConnectStandardConditionToTrueOutputs(controlGroups, connectionPoints, standardConditionElement, condition, controlGroup);
            ConnectStandardConditionToFalseOutputs(controlGroups, connectionPoints, standardConditionElement, condition, controlGroup);

            return condition;
        }

        private void ConnectStandardConditionToInput(IEnumerable<ConnectionPoint> connectionPoints, StandardTriggerXML standardConditionElement, StandardCondition condition, IControlGroup controlGroup)
        {
            var inputName = (standardConditionElement.condition.Item as RelationalConditionXMLX1Series)?.Value;
            var correspondingInput = (Input) connectionPoints.GetByName<Input>(inputName, logHandler);
            condition.Input = correspondingInput;
            AddConnectionPointToControlGroup(correspondingInput, controlGroup);
        }

        private void ConnectStandardConditionToTrueOutputs(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, StandardTriggerXML standardConditionElement, StandardCondition condition, IControlGroup controlGroup)
        {
            var trueOutputItems = standardConditionElement.@true.Select(t => t.Item);
            foreach (var trueOutputItem in trueOutputItems)
            {
                var trueOutputs = condition.TrueOutputs;

                ConnectStandardConditionToOutput(controlGroups, connectionPoints, controlGroup, trueOutputItem, trueOutputs);
            }
        }

        private void ConnectStandardConditionToFalseOutputs(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints, StandardTriggerXML standardConditionElement, StandardCondition condition, IControlGroup controlGroup)
        {
            var falseOutputItems = standardConditionElement.@false.Select(f => f.Item);
            foreach (var falseOutputItem in falseOutputItems)
            {
                var falseOutputs = condition.FalseOutputs;

                ConnectStandardConditionToOutput(controlGroups, connectionPoints, controlGroup, falseOutputItem, falseOutputs);
            }
        }

        private void ConnectStandardConditionToOutput(IList<IControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints,
            IControlGroup controlGroup, object falseOutputItem, IEventedList<RtcBaseObject> falseOutputs)
        {
            if (falseOutputItem is string ruleID)
            {
                var ruleAsOutput = controlGroup.GetRuleByElementId(ruleID, logHandler);

                if (!falseOutputs.Contains(ruleAsOutput))
                    falseOutputs.Add(ruleAsOutput);
                return;
            }

            if (falseOutputItem is StandardTriggerXML standardConditionAsOutputElement)
            {
                var standardConditionAsOutput =
                    ConnectStandardCondition(controlGroups, connectionPoints, standardConditionAsOutputElement);

                if (!falseOutputs.Contains(standardConditionAsOutput))
                    falseOutputs.Add(standardConditionAsOutput);
            }
        }

        private StandardCondition FindStandardConditionWithCorrectType(string tag, IControlGroup controlGroup, string id)
        {
            StandardCondition condition = null;

            switch (tag)
            {
                case RtcXmlTag.StandardCondition:
                    condition = (StandardCondition) controlGroup.GetConditionByElementId<StandardCondition>(id, logHandler);
                    break;
                case RtcXmlTag.TimeCondition:
                    condition = (TimeCondition) controlGroup.GetConditionByElementId<TimeCondition>(id, logHandler);
                    break;
                case RtcXmlTag.DirectionalCondition:
                    condition = (DirectionalCondition) controlGroup.GetConditionByElementId<DirectionalCondition>(id, logHandler);
                    break;
            }

            return condition;
        }

        private void AddConnectionPointToControlGroup(ConnectionPoint connectionPoint, IControlGroup controlGroup)
        {
            if (connectionPoint == null || controlGroup == null) return;

            if (connectionPoint is Input input)
            {
                if (!controlGroup.Inputs.Contains(connectionPoint) &&
                    !controlGroup.Inputs.Select(i => i.Name).Contains(input.Name))
                {
                    controlGroup.Inputs.Add(input);
                }

                return;
            }

            if (connectionPoint is Output output)
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

