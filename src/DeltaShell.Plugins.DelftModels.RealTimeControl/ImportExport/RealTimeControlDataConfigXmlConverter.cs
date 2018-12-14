using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlDataConfigXmlConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlDataConfigXmlConverter));

        private static readonly IList<string> RuleTags = new List<string>
        {
            RtcXmlTag.RelativeTimeRule,
            RtcXmlTag.TimeRule,
            RtcXmlTag.FactorRule,
            RtcXmlTag.HydraulicRule,
            RtcXmlTag.IntervalRule,
            RtcXmlTag.PIDRule
        };

        private static readonly IList<string> ConditionTags = new List<string>
        {
            RtcXmlTag.TimeCondition,
            RtcXmlTag.DirectionalCondition,
            RtcXmlTag.StandardCondition,
        };

        private static readonly IList<string> ConnectionPointTags = new List<string>
        {
            RtcXmlTag.Input,
            RtcXmlTag.Output,
        };

        public static IList<ControlGroup> CreateControlGroupsFromXmlElementIDs(IList<RTCTimeSeriesXML> elements)
        {
            if (elements == null || !elements.Any()) return null;

            var controlGroups = new List<ControlGroup>();

            var groupNames = GetAllControlGroupNamesFromElements(elements).Distinct().ToList();

            groupNames.ForEach(name =>
                {
                    controlGroups.Add(new ControlGroup { Name = name });
                }
             );

            return controlGroups;
        }

        public static void CreateRulesFromXmlElementsAndAddToControlGroup(List<RTCTimeSeriesXML> elements, IList<ControlGroup> controlGroups)
        {
            if (elements == null || controlGroups == null) return; 

            RuleTags.ForEach(tag =>
            {
                CreateRulesByTagFromXmlElementsAndAddToControlGroup(elements, tag, controlGroups);
            });
        }

        public static void CreateConditionsFromXmlElementsAndAddToControlGroup(List<RTCTimeSeriesXML> elements, IList<ControlGroup> controlGroups)
        {
            if (elements == null || controlGroups == null) return;

            ConditionTags.ForEach(tag =>
            {
                CreateConditionsByTagFromXmlElementsAndAddToControlGroup(elements, tag, controlGroups);
            });
        }

        public static IList<ConnectionPoint> GetConnectionPointsFromXmlElements(List<RTCTimeSeriesXML> elements)
        {
            if (elements == null) return null;

            var connectionPoints = new List<ConnectionPoint>();

            foreach (var tag in ConnectionPointTags)
            {
                var connectionPointsByTag = GetConnectionPointsByTagFromXmlElements(elements, tag);
                connectionPoints.AddRange(connectionPointsByTag);
            }

            return connectionPoints;
        }

        private static IList<ConnectionPoint> GetConnectionPointsByTagFromXmlElements(List<RTCTimeSeriesXML> elements, string tag)
        {
            if (elements == null || !ConnectionPointTags.Contains(tag)) return null;

            var connectionPointElements = elements.Where(e => e.id.StartsWith(tag) && !e.id.Contains(RtcXmlTag.OutputAsInput));

            var connectionPoints = new List<ConnectionPoint>();

            foreach (var connectionPointElement in connectionPointElements)
            {
                var temporaryConnectionPointName = connectionPointElement.id;

                ConnectionPoint connectionPoint;

                switch (tag)
                {
                    case RtcXmlTag.Input:
                        connectionPoint = new Input();
                        break;
                    case RtcXmlTag.Output:
                        connectionPoint = new Output();
                        break;
                    default:
                        continue;
                }

                connectionPoint.Name = temporaryConnectionPointName;
                connectionPoints.Add(connectionPoint);
            }

            return connectionPoints;
        }

        public static void AddOutputAsInputForRelativeTimeRule(List<RTCTimeSeriesXML> elements, IList<ControlGroup> controlGroups, IList<Output> outputs)
        {
            if (elements == null || controlGroups == null || outputs == null) return;

            var outputAsInputElements = elements.Where(e => e.id.Contains(RtcXmlTag.OutputAsInput));

            foreach (var outputAsInputElement in outputAsInputElements)
            {
                var id = outputAsInputElement.id;

                var splitId = id.Split(new[] { RtcXmlTag.OutputAsInput }, StringSplitOptions.None);

                var outputName = splitId.First();
                var ruleName = splitId.Last();

                 var relativeTimeRules = controlGroups.SelectMany(c => c.Rules).OfType<RelativeTimeRule>();

                var correspondingRelativeTimeRule = relativeTimeRules.FirstOrDefault(r => r.Name == ruleName);
                if (correspondingRelativeTimeRule == null)
                {
                    Log.WarnFormat(Resources.RealTimeControlDataConfigXmlConverter_AddOutputAsInputForRelativeTimeRule_Output___0___is_input_for_rule___1____but_the_rule_could_not_be_found__See_file____2___, outputName, ruleName, RealTimeControlXMLFiles.XmlData);
                    continue;
                }

                if (correspondingRelativeTimeRule.FromValue)
                {
                    Log.WarnFormat(Resources.RealTimeControlDataConfigXmlConverter_AddOutputAsInputForRelativeTimeRule_Relative_Time_Rules_can_only_have_one_output_as_input__It_seems_that_rule___0___has_multiple_outputs_as_input__See_file____1___, ruleName, RealTimeControlXMLFiles.XmlData);
                    continue;
                }

                var correspondingOutput = outputs.FirstOrDefault(o => o.Name == outputName);
                if (correspondingOutput == null)
                {
                    Log.WarnFormat(Resources.RealTimeControlDataConfigXmlConverter_AddOutputAsInputForRelativeTimeRule_When_getting_an_output_as_input_for_rule___0____the_output___1___could_not_be_found_in_the_file__See_file____2___, ruleName, outputName, RealTimeControlXMLFiles.XmlData);
                    continue;
                }

                correspondingRelativeTimeRule.Outputs.Insert(0, correspondingOutput);
                correspondingRelativeTimeRule.FromValue = true;
            }
        }

        private static IList<string> GetAllControlGroupNamesFromElements(IEnumerable<RTCTimeSeriesXML> elements)
        {
            if (elements == null) return null;

            var groupNames = new List<string>();

            var selectedElementsIDs =
                elements.Select(e => e.id).Where(id => !id.StartsWith(RtcXmlTag.Input) && !id.StartsWith(RtcXmlTag.Output));

            selectedElementsIDs.ForEach(id =>
            {
                var groupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id);

                if (!groupNames.Contains(groupName) && !string.IsNullOrEmpty(groupName))
                {
                    groupNames.Add(groupName);
                }
            });

            return groupNames;
        }

        private static void CreateRulesByTagFromXmlElementsAndAddToControlGroup(List<RTCTimeSeriesXML> elements, string tag, IList<ControlGroup> controlGroups)
        {
            if (elements == null || controlGroups== null || !RuleTags.Contains(tag)) return;

            var ruleElements = elements.Where(e => e.id.StartsWith(tag));

            foreach (var ruleElement in ruleElements)
            {
                var id = ruleElement.id;

                var name = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id);

                var rule = CreateRuleByTag(tag, name, ruleElement.PITimeSeries);

                if (rule == null) continue;

                var correspondingGroupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id, tag);

                FindCorrespondingControlGroupAndAddRule(controlGroups, correspondingGroupName, rule);
            }
        }

        private static RuleBase CreateRuleByTag(string tag, string name, PITimeSeriesXML ruleItem)
        {
            if (ruleItem == null || !RuleTags.Contains(tag)) return null;

            RuleBase rule;

            switch (tag)
            {
                case RtcXmlTag.RelativeTimeRule:
                    rule = CreateRelativeTimeRule(name, ruleItem);
                    break;
                case RtcXmlTag.TimeRule:
                    rule = new TimeRule { Name = name };
                    break;
                case RtcXmlTag.FactorRule:
                    rule = new FactorRule { Name = name };
                    break;
                case RtcXmlTag.HydraulicRule:
                    rule = new HydraulicRule { Name = name };
                    break;
                case RtcXmlTag.IntervalRule:
                    rule = new IntervalRule { Name = name };
                    break;
                case RtcXmlTag.PIDRule:
                    rule = new PIDRule { Name = name };
                    break;
                default:
                    throw new NotImplementedException();
            }

            return rule;
        }

        private static void FindCorrespondingControlGroupAndAddRule(IList<ControlGroup> controlGroups, string groupName, RuleBase rule)
        {
            if (controlGroups == null || rule == null) return;

            var ruleName = rule.Name;

            var correspondingControlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);

            if (correspondingControlGroup == null)
            {
                Log.WarnFormat(Resources.RealTimeControlDataConfigXmlConverter_FindCorrespondingControlGroupAndAddRule_For_Rule___0____corresponding_control_group___1___could_not_be_found__See_file___2_, ruleName, groupName, RealTimeControlXMLFiles.XmlData);
                return;
            }
       
            if (!correspondingControlGroup.Rules.Select(c => c.Name).Contains(ruleName))
            {
                correspondingControlGroup.Rules.Add(rule);
            }    
        }

        private static void CreateConditionsByTagFromXmlElementsAndAddToControlGroup(List<RTCTimeSeriesXML> elements, string tag, IList<ControlGroup> controlGroups)
        {
            if (elements == null || controlGroups == null || !ConditionTags.Contains(tag)) return;

            var conditionElements = elements.Where(e => e.id.StartsWith(tag));

            foreach (var conditionElement in conditionElements)
            {
                var id = conditionElement.id;

                var name = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id);

                var condition = CreateConditionByTag(tag, name, conditionElement.PITimeSeries);

                if (condition == null) continue;

                var correspondingGroupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id, tag);

                FindCorrespondingControlGroupAndAddCondition(controlGroups, correspondingGroupName, condition);
            }
        }

        private static void FindCorrespondingControlGroupAndAddCondition(IList<ControlGroup> controlGroups, string groupName, ConditionBase condition)
        {
            if (controlGroups == null || condition == null) return;

            var conditionName = condition.Name;

            var correspondingControlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);

            if (correspondingControlGroup == null)
            {
                Log.WarnFormat(Resources.RealTimeControlDataConfigXmlConverter_FindCorrespondingControlGroupAndAddCondition_For_Condition___0____corresponding_control_group___1___could_not_be_found__See_file___2_, conditionName, groupName, RealTimeControlXMLFiles.XmlData);
                return;
            }

            if (!correspondingControlGroup.Conditions.Select(c => c.Name).Contains(conditionName))
            {
                correspondingControlGroup.Conditions.Add(condition);
            }      
        }

        private static ConditionBase CreateConditionByTag(string tag, string name, PITimeSeriesXML conditionItem)
        {
            if (conditionItem == null || !ConditionTags.Contains(tag)) return null;

            ConditionBase condition;

            switch (tag)
            {
                case RtcXmlTag.StandardCondition:
                    condition = new StandardCondition { Name = name };
                    break;
                case RtcXmlTag.TimeCondition:
                    condition = CreateTimeCondition(name, conditionItem);
                    break;
                case RtcXmlTag.DirectionalCondition:
                    condition = new DirectionalCondition { Name = name };
                    break;
                default:
                    throw new NotImplementedException();
            }

            return condition;
        }

        private static RelativeTimeRule CreateRelativeTimeRule(string name, PITimeSeriesXML ruleItem)
        {
            if (ruleItem == null) return null;

            var interpolation = GetInterpolation(ruleItem.interpolationOption);

            var relativeTimeRule = new RelativeTimeRule
            {
                Name = name,
                Interpolation = interpolation,
            };

            return relativeTimeRule;
        }

        private static TimeCondition CreateTimeCondition(string name, PITimeSeriesXML conditionItem)
        {
            if (conditionItem == null) return null;
            var interpolation = GetInterpolation(conditionItem.interpolationOption);
            var extrapolation = GetExtrapolation(conditionItem.extrapolationOption);

            var timeCondition = new TimeCondition
            {
                Name = name,
                InterpolationOptionsTime = interpolation, 
                Extrapolation = extrapolation
            };

            return timeCondition;
        }

        private static InterpolationType GetInterpolation(PIInterpolationOptionEnumStringType conditionItemExtrapolationOption)
        {
            return conditionItemExtrapolationOption == PIInterpolationOptionEnumStringType.BLOCK
                ? InterpolationType.Constant
                : InterpolationType.Linear;
        }

        private static ExtrapolationType GetExtrapolation(PIExtrapolationOptionEnumStringType conditionItemExtrapolationOption)
        {
            return conditionItemExtrapolationOption == PIExtrapolationOptionEnumStringType.PERIODIC
                ? ExtrapolationType.Periodic
                : ExtrapolationType.Constant;
        }
    }
}
