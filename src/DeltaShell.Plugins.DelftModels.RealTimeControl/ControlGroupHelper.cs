using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public class ControlGroupHelper
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
            foreach (var input in ruleBase.Inputs.OfType<Input>())
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

        /// <summary>
        /// Returns all input items that are connected via conditions or are Input to the condition.
        /// This function is recursive; conditions can be connected to other conditions.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="controlGroup"></param>
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

            foreach (var rootCondition in controlGroup.Conditions)
            {
                if (rootCondition.TrueOutputs.Contains(conditionBase) || rootCondition.FalseOutputs.Contains(conditionBase))
                {
                    foreach (var inputForCondition in InputsForCondition(controlGroup, rootCondition))
                    {
                        yield return inputForCondition;
                    }
                }
            }
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
    }
}
