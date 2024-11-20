using System.Collections.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public abstract class ConditionBase : RtcBaseObject
    {
        protected ConditionBase()
        {
            Name = ConditionProvider.GetTitle(GetType());
            TrueOutputs = new EventedList<RtcBaseObject>();
            FalseOutputs = new EventedList<RtcBaseObject>();
        }

        public double Value { get; set; }

        [Aggregation]
        public IInput Input { get; set; }

        [Aggregation]
        public IEventedList<RtcBaseObject> TrueOutputs { get; protected set; }

        [Aggregation]
        public IEventedList<RtcBaseObject> FalseOutputs { get; protected set; }

        public abstract string GetDescription();

        [ValidationMethod]
        public static void Validate(ConditionBase conditionBase)
        {
            var exceptions = new List<ValidationException>();

            if (conditionBase.TrueOutputs.Count + conditionBase.FalseOutputs.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format("Condition '{0}' item has no output rules.", conditionBase.Name)));
            }

            if (conditionBase.TrueOutputs.Count > 1)
            {
                exceptions.Add(new ValidationException(string.Format(
                                                           "Condition '{0}' item has multiple outputs if the condition is true; this is not supported.",
                                                           conditionBase.Name)));
            }

            if (conditionBase.FalseOutputs.Count > 1)
            {
                exceptions.Add(new ValidationException(string.Format(
                                                           "Condition '{0}' item has multiple outputs if the condition is false; this is not supported.",
                                                           conditionBase.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override void CopyFrom(object source)
        {
            var conditionBase = source as ConditionBase;
            if (conditionBase != null)
            {
                base.CopyFrom(source);
                Value = conditionBase.Value;
            }
        }
    }
}