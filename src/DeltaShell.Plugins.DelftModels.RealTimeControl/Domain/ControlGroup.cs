using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class ControlGroup : EditableObjectUnique<long>, ICloneable, IControlGroup, IItemContainer
    {
        public ControlGroup()
        {
            Name = string.Empty;
            Conditions = new EventedList<ConditionBase>();
            Rules = new EventedList<RuleBase>();
            Inputs = new EventedList<Input>();
            Outputs = new EventedList<Output>();
            Signals = new EventedList<SignalBase>();
            MathematicalExpressions = new EventedList<MathematicalExpression>();
        }
        
        public string Name { get; set; }
        
        public IEventedList<RuleBase> Rules { get; protected set; }
        public IEventedList<ConditionBase> Conditions { get; protected set; }
        public IEventedList<SignalBase> Signals { get; protected set; }
        public IEventedList<MathematicalExpression> MathematicalExpressions { get; protected set; }
        public IEventedList<Input> Inputs { get; protected set; }
        public IEventedList<Output> Outputs { get; protected set; }

        [ValidationMethod]
        public static void Validate(ControlGroup controlGroup)
        {
            var exceptions = new List<ValidationException>();

            foreach (var input in controlGroup.Inputs)
            {
                if(string.IsNullOrEmpty(input.LocationName))
                {
                    exceptions.Add(new ValidationException(string.Format("Input at index {0} in control group '{1}' has empty location name.",
                        controlGroup.Inputs.IndexOf(input), controlGroup.Name)));
                }

                if (!input.IsConnected)
                {
                    exceptions.Add(new ValidationException(string.Format("Input item at index {0} in control group '{1}' is not connected.",
                        controlGroup.Inputs.IndexOf(input), controlGroup.Name)));
                }
                if (!InputInConditionRuleSignalOrMathematicalExpression(controlGroup, input))
                {
                    exceptions.Add(new ValidationException(string.Format("Input item at index {0} in control group '{1}' is not connected to a rule or condition.", 
                        controlGroup.Inputs.IndexOf(input), controlGroup.Name)));
                }
            }
            foreach (var output in controlGroup.Outputs)
            {
                if (string.IsNullOrEmpty(output.LocationName))
                {
                    exceptions.Add(new ValidationException(string.Format("Output at index {0} in control group '{1}' has empty location name.",
                        controlGroup.Outputs.IndexOf(output), controlGroup.Name)));
                }

                if (!output.IsConnected)
                {
                    exceptions.Add(new ValidationException(string.Format("Output item at index {0} in control group '{1}' is not connected.",
                        controlGroup.Outputs.IndexOf(output), controlGroup.Name)));
                }
            }

            foreach (var condition in controlGroup.Conditions)
            {
                ValidationResult validationResult = condition.Validate();
                if (!validationResult.IsValid)
                {
                    AddAndPrefixExceptions(exceptions, String.Format("Control group '{0}' : ", controlGroup.Name), validationResult.ValidationException);
                }
            }

            foreach (var signal in controlGroup.Signals)
            {
                ValidationResult validationResult = signal.Validate();
                if (!validationResult.IsValid)
                {
                    AddAndPrefixExceptions(exceptions, String.Format("Control group '{0}' : ", controlGroup.Name), validationResult.ValidationException);
                }
            }

            foreach (var rule in controlGroup.Rules)
            {
                ValidationResult validationResult = rule.Validate();
                if (!validationResult.IsValid)
                {
                    AddAndPrefixExceptions(exceptions, String.Format("Control group '{0}' : ", controlGroup.Name), validationResult.ValidationException);
                }
            }

            if (exceptions.Any())
            {
                throw new ValidationContextException(exceptions);
            }
        }

        private static void AddAndPrefixExceptions(IList<ValidationException> exceptions, string prefix, ValidationException innerException)
        {
            foreach (var message in innerException.Messages)
            {
                exceptions.Add(new ValidationException(String.Format("{0}{1}", prefix, message)));
            }
        }

        private static bool InputInConditionRuleSignalOrMathematicalExpression(ControlGroup controlGroup, Input input)
        {
            foreach (var ruleBase in controlGroup.Rules)
            {
                if (ruleBase.Inputs.Contains(input))
                {
                    return true;
                }
            }
            foreach (var signalBase in controlGroup.Signals)
            {
                if (signalBase.Inputs.Contains(input))
                {
                    return true;
                }
            }
            foreach (var conditionBase in controlGroup.Conditions)
            {
                if (conditionBase.Input == input)
                {
                    return true;
                }
            }

            foreach (var expression in controlGroup.MathematicalExpressions)
            {
                if (expression.Inputs.Contains(input))
                {
                    return true;
                }
            }
            return false;
        }

        public object Clone()
        {
            var controlGroup = (ControlGroup)Activator.CreateInstance(GetType());
            controlGroup.CopyFrom(this);
            return controlGroup;
        }

        public void CopyFrom(object source)
        {
            var controlGroup = source as ControlGroup;
            if (controlGroup != null)
            {
                Name = controlGroup.Name;
                CloneRtcObjectsAndSetRelationShips(controlGroup);
            }
        }

        private void CloneRtcObjectsAndSetRelationShips(ControlGroup controlGroup)
        {
            // clone objects
            foreach (var rule in controlGroup.Rules)
            {
                Rules.Add((RuleBase)rule.Clone());
            }
            
            foreach (var condition in controlGroup.Conditions)
            {
                Conditions.Add((ConditionBase)condition.Clone());
            }
            
            foreach (var input in controlGroup.Inputs)
            {
                Inputs.Add((Input)input.Clone());
            }

            foreach (var output in controlGroup.Outputs)
            {
                Outputs.Add((Output)output.Clone());
            }
            
            foreach (var signalBase in controlGroup.Signals)
            {
                Signals.Add((SignalBase)signalBase.Clone());
            }
            
            // clone connections
            var sourceObjects = controlGroup.GetAllItemsRecursive().ToList();
            var targetObjects = this.GetAllItemsRecursive().ToList();

            var j = 0;
            foreach (var sourceRule in controlGroup.Rules)
            {
                var rule = Rules[j];

                for (var i = 0; i < sourceRule.Inputs.Count; i++)
                {
                    var index = sourceObjects.IndexOf(sourceRule.Inputs[i]);
                    rule.Inputs.Add((Input)targetObjects[index]);

                }
                for (var i = 0; i < sourceRule.Outputs.Count; i++)
                {
                    var index = sourceObjects.IndexOf(sourceRule.Outputs[i]);
                    rule.Outputs.Add((Output)targetObjects[index]);
                }

                j++;
            }

            j = 0;
            foreach (var sourceCondition in controlGroup.Conditions)
            {
                var condition = Conditions[j];
                var index2 = sourceObjects.IndexOf(sourceCondition.Input);

                if (index2 != -1)
                {
                    condition.Input = (Input)targetObjects[index2];
                }

                for (var i = 0; i < sourceCondition.TrueOutputs.Count; i++)
                {
                    var index = sourceObjects.IndexOf(sourceCondition.TrueOutputs[i]);
                    condition.TrueOutputs.Add((RtcBaseObject)targetObjects[index]);
                }

                for (var i = 0; i < sourceCondition.FalseOutputs.Count; i++)
                {
                    var index = sourceObjects.IndexOf(sourceCondition.FalseOutputs[i]);
                    condition.FalseOutputs.Add((RtcBaseObject)targetObjects[index]);
                }

                j++;
            }
            
            j = 0;
            foreach (var sourceSignal in controlGroup.Signals)
            {
                var signal = Signals[j];

                for (var i = 0; i < sourceSignal.Inputs.Count; i++)
                {
                    var index = sourceObjects.IndexOf(sourceSignal.Inputs[i]);
                    signal.Inputs.Add((Input)targetObjects[index]);

                }
                for (var i = 0; i < sourceSignal.RuleBases.Count; i++)
                {
                    var index = sourceObjects.IndexOf(sourceSignal.RuleBases[i]);
                    signal.RuleBases.Add((RuleBase)targetObjects[index]);
                }

                j++;
            }
            
        }

        public IEnumerable<object> GetDirectChildren()
        {
            // probably we should just return rules here and let them return connection points
            foreach (var connectionPoint in Inputs.Cast<ConnectionPoint>().Concat(Outputs.Cast<ConnectionPoint>()))
            {
                yield return connectionPoint;
            }

            foreach (var rule in Rules)
            {
                yield return rule;
            }

            foreach (var condition in Conditions)
            {
                yield return condition;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
