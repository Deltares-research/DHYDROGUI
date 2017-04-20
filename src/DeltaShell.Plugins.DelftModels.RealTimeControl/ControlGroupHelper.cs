using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public class ControlGroupHelper
    {
        /// <summary>
        /// Returns all input items that are connected via rules and conditions to the output item.
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static IEnumerable<Input> InputItemsForOutput(ControlGroup controlGroup, Output output)
        {
            HashSet<Input> inputs = new HashSet<Input>();
            foreach (var ruleBase in controlGroup.Rules)
            {
                if (ruleBase.Outputs.Contains(output))
                {
                    foreach (var r in InputsForRule(controlGroup, ruleBase))
                    {
                        inputs.Add(r);
                    }
                }
            }
            return inputs;
        }

        /// <summary>
        /// Returns all input items that are connected via conditions or are Input to the rule.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="controlGroup"></param>
        /// <returns></returns>
        private static IEnumerable<Input> InputsForRule(ControlGroup controlGroup, RuleBase ruleBase)
        {
            foreach (var input in ruleBase.Inputs)
            {
                yield return input;
            }
            foreach (var conditionBase in controlGroup.Conditions)
            {
                if (conditionBase.TrueOutputs.Contains(ruleBase))
                {
                    foreach (var input in InputsForCondition(controlGroup, conditionBase))
                    {
                        yield return input;
                    }
                }
                if (conditionBase.FalseOutputs.Contains(ruleBase))
                {
                    foreach (var input in InputsForCondition(controlGroup, conditionBase))
                    {
                        yield return input;
                    }
                }
            }
        }

        /// <summary>
        /// Returns all input items that are connected via conditions or are Input to the condition.
        /// This function is recursive; conditions can be connected to other conditions.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="controlGroup"></param>
        /// <returns></returns>
        private static IEnumerable<Input> InputsForCondition(ControlGroup controlGroup, ConditionBase conditionBase)
        {
            if (conditionBase.Input != null)
            {
                yield return conditionBase.Input;
            }
            foreach (var rootCondition in controlGroup.Conditions)
            {
                if (rootCondition.TrueOutputs.Contains(conditionBase) || rootCondition.FalseOutputs.Contains(conditionBase))
                {
                    foreach (var input in InputsForCondition(controlGroup, rootCondition))
                    {
                        yield return input;
                    }
                }
            }
        }

        public static IEnumerable<ConditionBase> ConditionsOfRule(ControlGroup controlGroup, RuleBase rule)
        {
            HashSet<ConditionBase> conditions = new HashSet<ConditionBase>();
            foreach (var condition in controlGroup.Conditions)
            {
                if (condition.TrueOutputs.Contains(rule))
                {
                    conditions.Add(condition);
                    ConditionsOfCondition(controlGroup, condition, conditions);
                }
                if (condition.FalseOutputs.Contains(rule))
                {
                    conditions.Add(condition);
                    ConditionsOfCondition(controlGroup, condition, conditions);
                }
            }
            return conditions;
        }

        private static void ConditionsOfCondition(ControlGroup controlGroup, ConditionBase currentCondition, HashSet<ConditionBase> conditions)
        {
            foreach (var condition in controlGroup.Conditions)
            {
                if (conditions.Contains(condition)) continue; //breaks the recursive method

                if (condition.TrueOutputs.Contains(currentCondition))
                {
                    conditions.Add(condition);
                    ConditionsOfCondition(controlGroup, condition, conditions);
                }
                if (condition.FalseOutputs.Contains(currentCondition))
                {
                    conditions.Add(condition);
                    ConditionsOfCondition(controlGroup, currentCondition, conditions);
                }
            }
        }

        /// <summary>
        /// Returns a list with all startpoints of active paths of Output. A startpoints can be a rule or a condition.
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static IList<RtcBaseObject> StartObjectsForOutput(ControlGroup controlGroup, Output output)
        {
            var results = new List<RtcBaseObject>();
            foreach (var ruleBase in controlGroup.Rules)
            {
                if (!ruleBase.Outputs.Contains(output))
                {
                    continue;
                }

                if (IsStartRtcBaseObject(controlGroup, ruleBase) && ruleBase.Outputs.Contains(output) && (!results.Contains(ruleBase)))
                {
                    results.Add(ruleBase);
                }
                foreach (var conditionBase in controlGroup.Conditions)
                {
                    if (IsActiveConditionForRule(controlGroup, conditionBase, ruleBase) && (!results.Contains(conditionBase)))
                    {
                        results.Add(conditionBase);
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Returns the first startobject of a rule
        /// Can be the rule self or a condition
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static RtcBaseObject StartObjectOfARule(ControlGroup controlGroup, RuleBase rule)
        {
            if (IsStartRtcBaseObject(controlGroup, rule))
            {
                return rule;
            }
            foreach (var conditionBase in ConditionsOfRule(controlGroup, rule))
            {
                if (IsActiveConditionForRule(controlGroup, conditionBase, rule))
                {
                    return conditionBase;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true if condition is the start point of an active path that results in rule
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <param name="condition"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static bool IsActiveConditionForRule(ControlGroup controlGroup, ConditionBase condition, RuleBase rule)
        {
            return IsStartRtcBaseObject(controlGroup, condition) && IsSourceOfRule(condition, rule);
        }

        /// <summary>
        /// Returns true if condition can result in rule.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static bool IsSourceOfRule(ConditionBase condition, RuleBase rule)
        {
            if ((condition.TrueOutputs.Contains(rule)) || (condition.FalseOutputs.Contains(rule)))
            {
                return true;
            }
            foreach (var rtcBaseObject in condition.TrueOutputs)
            {
                if ((rtcBaseObject is ConditionBase) && (IsSourceOfRule((ConditionBase)rtcBaseObject, rule)))
                {
                    return true;
                }
            }
            foreach (var rtcBaseObject in condition.FalseOutputs)
            {
                if ((rtcBaseObject is ConditionBase) && (IsSourceOfRule((ConditionBase)rtcBaseObject, rule)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// A rule or condition is a 'start' rule/condition if it is the first element in an Active Condition Path (Inputs are ignored).
        /// In the diagram this will be visible by a bold border.
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <param name="rtcBaseObject"></param>
        /// <returns></returns>
        public static bool IsStartRtcBaseObject(ControlGroup controlGroup, RtcBaseObject rtcBaseObject)
        {
            foreach (var condition in controlGroup.Conditions)
            {
                if (condition.TrueOutputs.Contains(rtcBaseObject) || condition.FalseOutputs.Contains(rtcBaseObject))
                {
                    // if a condition controls the rtcBaseObject it is not a start point.
                    return false;
                }
            }
            return true;
        }
    }
}
