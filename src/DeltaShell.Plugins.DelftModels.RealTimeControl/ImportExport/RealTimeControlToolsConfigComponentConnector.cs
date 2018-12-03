using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using log4net;
using ValidationAspects;

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

                var groupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id);
                var correspondingControlGroup = GetControlGroupByName(groupName, controlGroups);

                var ruleName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id); 
                var correspondingRule = (TimeRule)GetRuleByNameInControlGroup(ruleName, correspondingControlGroup);

                var ruleOutputElementName = timeRuleElement.output.y;

                if (!ruleOutputElementName.StartsWith(RtcXmlTag.Output))
                {
                    Log.Warn("WARNING");
                    continue;
                }

                var correspondingOutput = (Output)GetConnectionPointByXmlName(ruleOutputElementName, connectionPoints);
                correspondingRule.Outputs.Add(correspondingOutput);
                correspondingControlGroup.Outputs.Add(correspondingOutput);
            }
        }

        public static void ConnectRelativeTimeRules(List<TimeRelativeXML> relativeTimeRuleElements, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            foreach (var relativeTimeRuleElement in relativeTimeRuleElements)
            {
                var id = relativeTimeRuleElement.id;

                var groupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id);
                var correspondingControlGroup = GetControlGroupByName(groupName, controlGroups);

                var ruleName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id);
                var relativeTimeRule = (RelativeTimeRule)GetRuleByNameInControlGroup(ruleName, correspondingControlGroup);

                var fromValue = relativeTimeRuleElement.valueOption == timeRelativeEnumStringType.RELATIVE;
                var minimumPeriod = relativeTimeRuleElement.maximumPeriod;
                var table = relativeTimeRuleElement.controlTable;

                var ruleOutputElementName = relativeTimeRuleElement.output.y;
                if (!ruleOutputElementName.StartsWith(RtcXmlTag.Output))
                {
                    Log.Warn($"The output of relative time rule '{ruleName}' should be an output (see tag [Output]).");
                    continue;
                }

                var correspondingOutput = (Output)GetConnectionPointByXmlName(ruleOutputElementName, connectionPoints);
                relativeTimeRule.Outputs.Add(correspondingOutput);
                correspondingControlGroup.Outputs.Add(correspondingOutput);

                relativeTimeRule.FromValue = fromValue;
                relativeTimeRule.MinimumPeriod = (int)minimumPeriod;
                DefineFunctionFromXmlTable(table, relativeTimeRule.Function);
            }
        }

        public static void ConnectStandardConditions(List<StandardTriggerXML> standardConditionElements, IList<ControlGroup> controlGroups, IList<ConnectionPoint> connectionPoints)
        {
            foreach (var standardConditionElement in standardConditionElements)
            {
                var id = standardConditionElement.id;
                var conditionElement = standardConditionElement.condition;
                    var relationalOperator = conditionElement.relationalOperator;
                    var value = conditionElement.Item as RelationalConditionXMLX1Series;
                    var series = conditionElement.Item1 as string;
                var trueCondition = standardConditionElement.@true;
                var falseCondition = standardConditionElement.@false;
                var outputElement = standardConditionElement.output;

                var groupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id);
                var correspondingControlGroup = GetControlGroupByName(groupName, controlGroups);

                var conditionName = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id);
                var standardCondition = (StandardCondition)GetConditionByNameInControlGroup(conditionName, correspondingControlGroup);

                // TODO: finish
            }
        }

        private static void DefineFunctionFromXmlTable(List<TimeRelativeControlTableRecordXML> records, IFunction function)
        {
            function.Arguments[0].SetValues(records.Select(e => e.time));
            function.Components[0].SetValues(records.Select(e => e.value));
        }

        private static ControlGroup GetControlGroupByName(string groupName, IList<ControlGroup> controlGroups)
        {
            var controlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);
            if (controlGroup == null)
            {
                Log.Warn($"Could not find the controlgroup '{groupName}' that is referenced in file '{RealTimeControlXMLFiles.XmlTools}'. The group needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}' too.");
                return null;
            }

            return controlGroup;
        }

        private static RuleBase GetRuleByNameInControlGroup(string ruleName, ControlGroup controlGroup)
        {
            var correspondingRule = controlGroup.Rules.FirstOrDefault(r => r.Name == ruleName);
            if (correspondingRule == null)
            {
                Log.Warn($"Could not find the rule '{ruleName}' that is referenced in file '{RealTimeControlXMLFiles.XmlTools}'. The rule needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}' too.");
                return null;
            }

            return correspondingRule;
        }

        private static ConditionBase GetConditionByNameInControlGroup(string conditionName, ControlGroup controlGroup)
        {
            var correspondingCondition = controlGroup.Conditions.FirstOrDefault(r => r.Name == conditionName);
            if (correspondingCondition == null)
            {
                Log.Warn($"Could not find the rule '{conditionName}' that is referenced in file '{RealTimeControlXMLFiles.XmlTools}'. The condition needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}' too.");
                return null;
            }

            return correspondingCondition;
        }

        // TODO: Problem when model has inputs/outputs with the same name (in different control groups) 
        private static ConnectionPoint GetConnectionPointByXmlName(string xmlName, IList<ConnectionPoint> connectionPoints)
        {
            var correspondingConnectionPoint = connectionPoints.FirstOrDefault(o => o.XmlName == xmlName);
            if (correspondingConnectionPoint == null)
            {
                Log.Warn($"Could not find the input/output '{xmlName}' that is referenced in file '{RealTimeControlXMLFiles.XmlTools}'. The input/output needs to be referenced in file '{RealTimeControlXMLFiles.XmlData}' too.");
            }

            return correspondingConnectionPoint;
        }
    }
}
