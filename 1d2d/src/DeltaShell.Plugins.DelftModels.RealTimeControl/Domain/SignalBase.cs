using System.Collections.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public abstract class SignalBase : RtcBaseObject
    {
        private const string defaultSignalName = "Lookup Table";

        protected SignalBase()
        {
            Name = defaultSignalName;
            Inputs = new EventedList<Input>();
            RuleBases = new EventedList<RuleBase>();
        }

        [Aggregation]
        public IEventedList<Input> Inputs { get; set; }

        [Aggregation]
        public IEventedList<RuleBase> RuleBases { get; set; }

        public bool StoreAsRule { get; set; }

        [ValidationMethod]
        public static void Validate(SignalBase signalBase)
        {
            var exceptions = new List<ValidationException>();

            if (signalBase.RuleBases.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format("signal '{0}' has no rule.", signalBase.Name)));
            }

            foreach (RuleBase rulebase in signalBase.RuleBases)
            {
                if (string.IsNullOrEmpty(rulebase.Name))
                {
                    exceptions.Add(new ValidationException(string.Format("signal '{0}' has unlinked rule.", signalBase.Name)));
                }
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }
    }
}