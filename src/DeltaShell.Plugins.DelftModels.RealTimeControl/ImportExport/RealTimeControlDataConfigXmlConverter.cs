using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IList<ControlGroup> CreateControlGroupsFromXmlElementIDs(IEnumerable<RTCTimeSeriesXML> elements)
        {
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
            RuleTags.ForEach(tag =>
            {
                CreateRulesByTagFromXmlElementsAndAddToControlGroup(elements, tag, controlGroups);
            });
        }

        public static void CreateConditionsFromXmlElementsAndAddToControlGroup(List<RTCTimeSeriesXML> elements, IList<ControlGroup> controlGroups)
        {
            ConditionTags.ForEach(tag =>
            {
                CreateConditionsByTagFromXmlElementsAndAddToControlGroup(elements, tag, controlGroups);
            });
        }

        public static IList<ConnectionPoint> GetConnectionPointsFromXmlElements(List<RTCTimeSeriesXML> elements)
        {
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

        public static void AddOutputAsInputForRelativeTimeRule(List<RTCTimeSeriesXML> elements, IList<ControlGroup> controlGroups, IEnumerable<Output> outputs)
        {
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
                    Log.Warn($"Output '{outputName}' is input for rule '{ruleName}', but the rule could not be found. See file: '{RealTimeControlXMLFiles.XmlData}'.");
                    continue;
                }

                if (correspondingRelativeTimeRule.FromValue)
                {
                    Log.Warn($"Relative Time Rules can only have one output as input. It seems that rule '{ruleName}' has multiple outputs as input. See file: '{RealTimeControlXMLFiles.XmlData}'.");
                    continue;
                }

                var correspondingOutput = outputs.FirstOrDefault(o => o.Name == outputName);
                if (correspondingOutput == null)
                {
                    Log.Warn($"When getting an output as input for rule '{ruleName}', the output '{outputName}' could not be found in the file. See file: '{RealTimeControlXMLFiles.XmlData}'.");
                    continue;
                }

                correspondingRelativeTimeRule.Outputs.Insert(0, correspondingOutput);
                correspondingRelativeTimeRule.FromValue = true;
            }
        }

        private static IList<string> GetAllControlGroupNamesFromElements(IEnumerable<RTCTimeSeriesXML> elements)
        {
            var groupNames = new List<string>();

            var selectedElementsIDs =
                elements.Select(e => e.id).Where(id => !id.StartsWith(RtcXmlTag.Input) && !id.StartsWith(RtcXmlTag.Output));

            selectedElementsIDs.ForEach(id =>
            {
                var groupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id);

                if (!groupNames.Contains(groupName))
                {
                    groupNames.Add(groupName);
                }
            });

            return groupNames;
        }

        private static void CreateRulesByTagFromXmlElementsAndAddToControlGroup(List<RTCTimeSeriesXML> elements, string tag, IList<ControlGroup> controlGroups)
        {
            if (!RuleTags.Contains(tag)) return;

            var ruleElements = elements.Where(e => e.id.StartsWith(tag));

            foreach (var ruleElement in ruleElements)
            {
                var id = ruleElement.id;

                var name = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id);

                var rule = CreateRuleByTag(tag, name, ruleElement.PITimeSeries);

                if (rule == null) continue;

                var correspondingGroupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id, tag);

                if (!FindCorrespondingControlGroupAndAddRule(controlGroups, correspondingGroupName, rule))
                {
                    Log.Warn($"For Rule '{name}', corresponding control group '{correspondingGroupName}' could not be found. See file: {RealTimeControlXMLFiles.XmlData}");
                }
            }
        }

        private static RuleBase CreateRuleByTag(string tag, string name, PITimeSeriesXML ruleItem)
        {
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

        private static bool FindCorrespondingControlGroupAndAddRule(IList<ControlGroup> controlGroups, string groupName, RuleBase rule)
        {
            var correspondingControlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);

            if (correspondingControlGroup == null) return false;

            correspondingControlGroup.Rules.Add(rule);

            return true;
        }

        private static void CreateConditionsByTagFromXmlElementsAndAddToControlGroup(List<RTCTimeSeriesXML> elements, string tag, IList<ControlGroup> controlGroups)
        {
            if (!ConditionTags.Contains(tag)) return;

            var conditionElements = elements.Where(e => e.id.StartsWith(tag));

            foreach (var conditionElement in conditionElements)
            {
                var id = conditionElement.id;

                var name = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(id);

                var condition = CreateConditionByTag(tag, name, conditionElement.PITimeSeries);

                if (condition == null) continue;

                var correspondingGroupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id, tag);

                if (!FindCorrespondingControlGroupAndAddCondition(controlGroups, correspondingGroupName, condition))
                {
                    Log.Warn($"For Condition '{name}', corresponding control group '{correspondingGroupName}' could not be found. See file: {RealTimeControlXMLFiles.XmlData}");
                }
            }
        }

        private static bool FindCorrespondingControlGroupAndAddCondition(IList<ControlGroup> controlGroups, string groupName, ConditionBase condition)
        {
            var correspondingControlGroup = controlGroups.FirstOrDefault(g => g.Name == groupName);
            var conditionName = condition.Name;

            if (correspondingControlGroup == null) return false;

            if (correspondingControlGroup.Conditions.Select(c => c.Name).Contains(conditionName))
            {
                Log.Warn($"Control Group '{groupName}' already contains a condition with name '{conditionName}'. Names must be unique. See file: {RealTimeControlXMLFiles.XmlData}.");
                return false;
            }
            correspondingControlGroup.Conditions.Add(condition);

            return true;
        }

        private static ConditionBase CreateConditionByTag(string tag, string name, PITimeSeriesXML conditionItem)
        {
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

        private static RelativeTimeRule CreateRelativeTimeRule(string name, PITimeSeriesXML conditionItem)
        {
            var interpolation = GetInterpolation(conditionItem.interpolationOption);

            var relativeTimeRule = new RelativeTimeRule()
            {
                Name = name,
                Interpolation = interpolation,
            };

            return relativeTimeRule;
        }

        private static TimeCondition CreateTimeCondition(string name, PITimeSeriesXML conditionItem)
        {
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
