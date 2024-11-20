using System.Collections.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public abstract class RuleBase : RtcBaseObject, IItemContainer
    {
        protected RuleBase()
        {
            Name = RuleProvider.GetTitle(GetType());
            Inputs = new EventedList<IInput>();
            Outputs = new EventedList<Output>();
        }

        [Aggregation]
        public IEventedList<IInput> Inputs { get; set; }

        [Aggregation]
        public IEventedList<Output> Outputs { get; set; }

        public virtual bool CanBeLinkedFromSignal()
        {
            return false;
        }

        public virtual bool IsLinkedFromSignal()
        {
            return CanBeLinkedFromSignal();
        }

        [ValidationMethod]
        public static void Validate(RuleBase ruleBase)
        {
            var exceptions = new List<ValidationException>();

            if (ruleBase.Outputs.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format("rule '{0}' has no output.", ruleBase.Name)));
            }

            foreach (Output output in ruleBase.Outputs)
            {
                if (string.IsNullOrEmpty(output.ParameterName))
                {
                    exceptions.Add(new ValidationException(string.Format("rule '{0}' has unlinked output.", ruleBase.Name)));
                }
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override void CopyFrom(object source)
        {
            var ruleBase = source as RuleBase;
            if (ruleBase != null)
            {
                base.CopyFrom(source);
            }
        }

        public abstract IEnumerable<object> GetDirectChildren();
    }
}