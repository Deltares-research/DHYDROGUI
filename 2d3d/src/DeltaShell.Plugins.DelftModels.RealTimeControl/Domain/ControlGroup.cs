using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class ControlGroup : EditableObjectUnique<long>, ICloneable, IControlGroup, IItemContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ControlGroup));
        private string name;

        public ControlGroup()
        {
            Name = "Control Group";
            Conditions = new EventedList<ConditionBase>();
            Rules = new EventedList<RuleBase>();
            Inputs = new EventedList<Input>();
            Outputs = new EventedList<Output>();
            Signals = new EventedList<SignalBase>();
            MathematicalExpressions = new EventedList<MathematicalExpression>();
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    name = value;
                }
                else
                {
                    Log.Error(Resources.RealTimeControlGroupErrorLogEmptyValue);
                }
            }
        }

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

            foreach (Input input in controlGroup.Inputs)
            {
                if (string.IsNullOrEmpty(input.LocationName))
                {
                    exceptions.Add(new ValidationException(string.Format("Input at index {0} in control group '{1}' has empty location name.",
                                                                         controlGroup.Inputs.IndexOf(input), controlGroup.Name)));
                }

                if (!input.IsConnected)
                {
                    exceptions.Add(new ValidationException(string.Format("Input item at index {0} in control group '{1}' is not connected.",
                                                                         controlGroup.Inputs.IndexOf(input), controlGroup.Name)));
                }

                if (!InputUsedByRtcObjects(controlGroup, input))
                {
                    exceptions.Add(new ValidationException(string.Format("Input item at index {0} in control group '{1}' is not connected to a rule or condition.",
                                                                         controlGroup.Inputs.IndexOf(input), controlGroup.Name)));
                }
            }

            foreach (Output output in controlGroup.Outputs)
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

            foreach (ConditionBase condition in controlGroup.Conditions)
            {
                ValidationResult validationResult = condition.Validate();
                if (!validationResult.IsValid)
                {
                    AddAndPrefixExceptions(exceptions, string.Format("Control group '{0}' : ", controlGroup.Name), validationResult.ValidationException);
                }
            }

            foreach (SignalBase signal in controlGroup.Signals)
            {
                ValidationResult validationResult = signal.Validate();
                if (!validationResult.IsValid)
                {
                    AddAndPrefixExceptions(exceptions, string.Format("Control group '{0}' : ", controlGroup.Name), validationResult.ValidationException);
                }
            }

            foreach (RuleBase rule in controlGroup.Rules)
            {
                ValidationResult validationResult = rule.Validate();
                if (!validationResult.IsValid)
                {
                    AddAndPrefixExceptions(exceptions, string.Format("Control group '{0}' : ", controlGroup.Name), validationResult.ValidationException);
                }
            }

            if (exceptions.Any())
            {
                throw new ValidationContextException(exceptions);
            }
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

        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            var controlGroup = (ControlGroup) Activator.CreateInstance(GetType());
            controlGroup.CopyFrom(this);
            return controlGroup;
        }

        public IEnumerable<object> GetDirectChildren()
        {
            // probably we should just return rules here and let them return connection points
            foreach (ConnectionPoint connectionPoint in Inputs.Cast<ConnectionPoint>().Concat(Outputs.Cast<ConnectionPoint>()))
            {
                yield return connectionPoint;
            }

            foreach (RuleBase rule in Rules)
            {
                yield return rule;
            }

            foreach (ConditionBase condition in Conditions)
            {
                yield return condition;
            }
        }

        private static void AddAndPrefixExceptions(IList<ValidationException> exceptions, string prefix, ValidationException innerException)
        {
            foreach (string message in innerException.Messages)
            {
                exceptions.Add(new ValidationException(string.Format("{0}{1}", prefix, message)));
            }
        }

        private static bool InputUsedByRtcObjects(ControlGroup controlGroup, Input input)
        {
            if (controlGroup.Rules.Any(rule => rule.Inputs.Contains(input)))
            {
                return true;
            }

            if (controlGroup.Signals.Any(signal => signal.Inputs.Contains(input)))
            {
                return true;
            }

            if (controlGroup.Conditions.Any(condition => ReferenceEquals(condition.Input, input)))
            {
                return true;
            }

            if (controlGroup.MathematicalExpressions.Any(expression => expression.Inputs.Contains(input)))
            {
                return true;
            }

            return false;
        }

        private void CloneRtcObjectsAndSetRelationShips(ControlGroup controlGroup)
        {
            // clone objects
            foreach (RuleBase rule in controlGroup.Rules)
            {
                Rules.Add((RuleBase) rule.Clone());
            }

            foreach (ConditionBase condition in controlGroup.Conditions)
            {
                Conditions.Add((ConditionBase) condition.Clone());
            }

            foreach (Input input in controlGroup.Inputs)
            {
                Inputs.Add((Input) input.Clone());
            }

            foreach (Output output in controlGroup.Outputs)
            {
                Outputs.Add((Output) output.Clone());
            }

            foreach (SignalBase signalBase in controlGroup.Signals)
            {
                Signals.Add((SignalBase) signalBase.Clone());
            }

            // clone connections
            List<object> sourceObjects = controlGroup.GetAllItemsRecursive().ToList();
            List<object> targetObjects = this.GetAllItemsRecursive().ToList();

            var j = 0;
            foreach (RuleBase sourceRule in controlGroup.Rules)
            {
                RuleBase rule = Rules[j];

                for (var i = 0; i < sourceRule.Inputs.Count; i++)
                {
                    int index = sourceObjects.IndexOf(sourceRule.Inputs[i]);
                    rule.Inputs.Add((Input) targetObjects[index]);
                }

                for (var i = 0; i < sourceRule.Outputs.Count; i++)
                {
                    int index = sourceObjects.IndexOf(sourceRule.Outputs[i]);
                    rule.Outputs.Add((Output) targetObjects[index]);
                }

                j++;
            }

            j = 0;
            foreach (ConditionBase sourceCondition in controlGroup.Conditions)
            {
                ConditionBase condition = Conditions[j];
                int index2 = sourceObjects.IndexOf(sourceCondition.Input);

                if (index2 != -1)
                {
                    condition.Input = (Input) targetObjects[index2];
                }

                for (var i = 0; i < sourceCondition.TrueOutputs.Count; i++)
                {
                    int index = sourceObjects.IndexOf(sourceCondition.TrueOutputs[i]);
                    condition.TrueOutputs.Add((RtcBaseObject) targetObjects[index]);
                }

                for (var i = 0; i < sourceCondition.FalseOutputs.Count; i++)
                {
                    int index = sourceObjects.IndexOf(sourceCondition.FalseOutputs[i]);
                    condition.FalseOutputs.Add((RtcBaseObject) targetObjects[index]);
                }

                j++;
            }

            j = 0;
            foreach (SignalBase sourceSignal in controlGroup.Signals)
            {
                SignalBase signal = Signals[j];

                for (var i = 0; i < sourceSignal.Inputs.Count; i++)
                {
                    int index = sourceObjects.IndexOf(sourceSignal.Inputs[i]);
                    signal.Inputs.Add((Input) targetObjects[index]);
                }

                for (var i = 0; i < sourceSignal.RuleBases.Count; i++)
                {
                    int index = sourceObjects.IndexOf(sourceSignal.RuleBases[i]);
                    signal.RuleBases.Add((RuleBase) targetObjects[index]);
                }

                j++;
            }
        }
    }
}