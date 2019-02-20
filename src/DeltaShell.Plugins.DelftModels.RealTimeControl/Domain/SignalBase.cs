using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public abstract class SignalBase : RtcBaseObject
    {
        [Aggregation]
        public IEventedList<Input> Inputs { get; set; }

        [Aggregation]
        public IEventedList<RuleBase> RuleBases { get; set; }

        public bool StoreAsRule { get; set; }

        protected SignalBase()
        {
            Name = SignalProvider.GetTitle(GetType());
            Inputs = new EventedList<Input>();
            RuleBases = new EventedList<RuleBase>();
        }

        /// <summary>
        /// Converts the information the signal needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace">The x namespace.</param>
        /// <param name="prefix">The control group name.</param>
        /// <returns>The Xml Element.</returns>
        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            return StoreAsRule ? new XElement(xNamespace + "rule") : new XElement(xNamespace + "signal");
        }

        /// <summary>
        /// <returns></returns>
        public virtual IEnumerable<XElement> OutputAsInputToDataConfigXml(XNamespace xNamespace)
        {
            yield break;
        }

        public virtual IEnumerable<XElement> ToImportState(XNamespace xNamespace)
        {
            yield break;
        }

        [ValidationMethod]
        public static void Validate(SignalBase signalBase)
        {
            var exceptions = new List<ValidationException>();

            if (signalBase.RuleBases.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format("signal '{0}' has no rule.", signalBase.Name)));
            }
            foreach (var rulebase in signalBase.RuleBases)
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

        public override object Clone()
        {
            var signalBase = (SignalBase)Activator.CreateInstance(GetType());
            signalBase.CopyFrom(this);
            return signalBase;
        }

        public override void CopyFrom(object source)
        {
            var signalBase = source as SignalBase;
            if (signalBase != null)
            {
                base.CopyFrom(source);
            }
        }
    }
}