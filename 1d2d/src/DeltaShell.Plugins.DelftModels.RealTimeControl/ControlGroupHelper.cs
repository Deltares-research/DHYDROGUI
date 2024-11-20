using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public static class ControlGroupHelper
    {
        /// <summary>
        /// Returns all input items that are connected via mathematical
        /// expressions, rules and conditions to the output item.
        /// </summary>
        /// <param name="controlGroup">The control group.</param>
        /// <param name="output">The output item.</param>
        /// <returns>All inputs that are indirectly connected to the output.</returns>
        public static IEnumerable<Input> InputItemsForOutput(ControlGroup controlGroup, Output output)
        {
            var inputs = new HashSet<Input>();
            foreach (RuleBase ruleBase in controlGroup.Rules)
            {
                if (ruleBase.Outputs.Contains(output))
                {
                    foreach (Input r in InputsForRule(controlGroup, ruleBase))
                    {
                        inputs.Add(r);
                    }
                }
            }

            return inputs;
        }

        /// <summary>
        /// RetrieveTriggerObjects returns all main triggers from which the serializers should start.
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <returns>IEnumerable with triggers</returns>
        public static IEnumerable<RtcBaseObject> RetrieveTriggerObjects(IControlGroup controlGroup)
        {
            IEventedList<ConditionBase> conditions = controlGroup.Conditions;
            List<RtcBaseObject> conditionOutputs = conditions
                                                   .SelectMany(c => c.TrueOutputs.Concat(c.FalseOutputs))
                                                   .ToList();

            IEnumerable<RtcBaseObject> triggerCandidates = conditions
                .Concat<RtcBaseObject>(controlGroup.MathematicalExpressions);

            return triggerCandidates.Except(conditionOutputs);
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

            foreach (ConditionBase conditionBase in ConditionsOfRule(controlGroup, rule))
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
        /// A rule or condition is a 'start' rule/condition if it is the first element in an Active Condition Path (Inputs are
        /// ignored).
        /// In the diagram this will be visible by a bold border.
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <param name="rtcBaseObject"></param>
        /// <returns></returns>
        public static bool IsStartRtcBaseObject(ControlGroup controlGroup, RtcBaseObject rtcBaseObject)
        {
            foreach (ConditionBase condition in controlGroup.Conditions)
            {
                if (condition.TrueOutputs.Contains(rtcBaseObject) || condition.FalseOutputs.Contains(rtcBaseObject))
                {
                    // if a condition controls the rtcBaseObject it is not a start point.
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<ConditionBase> ConditionsOfRule(ControlGroup controlGroup, RuleBase rule)
        {
            var conditions = new HashSet<ConditionBase>();
            foreach (ConditionBase condition in controlGroup.Conditions)
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

        /// <summary>
        /// Returns true if condition can result in rule.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static bool IsSourceOfRule(ConditionBase condition, RuleBase rule)
        {
            if (condition.TrueOutputs.Contains(rule) || condition.FalseOutputs.Contains(rule))
            {
                return true;
            }

            foreach (RtcBaseObject rtcBaseObject in condition.TrueOutputs)
            {
                if (rtcBaseObject is ConditionBase && IsSourceOfRule((ConditionBase) rtcBaseObject, rule))
                {
                    return true;
                }
            }

            foreach (RtcBaseObject rtcBaseObject in condition.FalseOutputs)
            {
                if (rtcBaseObject is ConditionBase && IsSourceOfRule((ConditionBase) rtcBaseObject, rule))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns all input items that are connected via conditions or are Input to the rule.
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <param name="ruleBase"> </param>
        /// <returns></returns>
        private static IEnumerable<Input> InputsForRule(ControlGroup controlGroup, RuleBase ruleBase)
        {
            foreach (Input input in ruleBase.Inputs.OfType<Input>())
            {
                yield return input;
            }

            foreach (MathematicalExpression expression in ruleBase.Inputs.OfType<MathematicalExpression>())
            {
                foreach (Input input in GetExpressionInputs(expression))
                {
                    yield return input;
                }
            }

            foreach (ConditionBase conditionBase in controlGroup.Conditions)
            {
                if (conditionBase.TrueOutputs.Contains(ruleBase))
                {
                    foreach (Input input in InputsForCondition(controlGroup, conditionBase))
                    {
                        yield return input;
                    }
                }

                if (conditionBase.FalseOutputs.Contains(ruleBase))
                {
                    foreach (Input input in InputsForCondition(controlGroup, conditionBase))
                    {
                        yield return input;
                    }
                }
            }
        }

        private static IEnumerable<Input> GetExpressionInputs(MathematicalExpression rootExpression)
        {
            foreach (IInput input in rootExpression.Inputs)
            {
                switch (input)
                {
                    case Input rootInput:
                        yield return rootInput;

                        break;
                    case MathematicalExpression expression:
                    {
                        foreach (Input childInput in GetExpressionInputs(expression))
                        {
                            yield return childInput;
                        }

                        break;
                    }
                }
            }
        }

        private static void ConditionsOfCondition(ControlGroup controlGroup, ConditionBase currentCondition, HashSet<ConditionBase> conditions)
        {
            foreach (ConditionBase condition in controlGroup.Conditions)
            {
                if (conditions.Contains(condition))
                {
                    continue; //breaks the recursive method
                }

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
        /// Returns all input items that are connected via conditions or are Input to the condition.
        /// This function is recursive; conditions can be connected to other conditions.
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <param name="conditionBase"> </param>
        /// <returns></returns>
        private static IEnumerable<Input> InputsForCondition(ControlGroup controlGroup, ConditionBase conditionBase)
        {
            IInput conditionInput = conditionBase.Input;
            if (conditionInput is Input input)
            {
                yield return input;
            }
            else if (conditionInput is MathematicalExpression expression)
            {
                foreach (Input expressionInput in GetExpressionInputs(expression))
                {
                    yield return expressionInput;
                }
            }

            foreach (ConditionBase rootCondition in controlGroup.Conditions)
            {
                if (rootCondition.TrueOutputs.Contains(conditionBase) || rootCondition.FalseOutputs.Contains(conditionBase))
                {
                    foreach (Input inputForCondition in InputsForCondition(controlGroup, rootCondition))
                    {
                        yield return inputForCondition;
                    }
                }
            }
        }
    }
}