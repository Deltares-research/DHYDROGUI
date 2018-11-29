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
    public abstract class RuleBase : RtcBaseObject
    {
        [Aggregation]
        public IEventedList<Input> Inputs { get; set; }

        [Aggregation]
        public IEventedList<Output> Outputs { get; set; }

        protected RuleBase() : base()
        {
            Name = RuleProvider.GetTitle(GetType());
            Inputs = new EventedList<Input>();
            Outputs = new EventedList<Output>();
        }

        public virtual bool CanBeLinkedFromSignal()
        {
            return false;
        }

        public virtual bool IsLinkedFromSignal()
        {
            return CanBeLinkedFromSignal();
        }

        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            return new XElement(xNamespace + "rule");
        }

        /// <summary>
        /// some rule might require their output logged
        /// eg. Integral part for PID rule
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<XElement> OutputAsInputToDataConfigXml(XNamespace xNamespace)
        {
            yield break;
        }

        ///// <summary>
        ///// some rule might require their output logged
        ///// eg. Integral part for PID rule
        ///// </summary>
        ///// <returns></returns>
        //public virtual IEnumerable<XElement> ToOutputXml(XNamespace xNamespace)
        //{
        //    yield break;
        //}

        /// <summary>
        /// implement this if the rule needs to write some state to the
        /// state_import.xml file.
        /// eg. Integral part for PID rule
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<XElement> ToImportState(XNamespace xNamespace)
        {
            yield break;
        }

        [ValidationMethod]
        public static void Validate(RuleBase ruleBase)
        {
            var exceptions = new List<ValidationException>();

            if (ruleBase.Outputs.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format("rule '{0}' has no output.", ruleBase.Name)));
            }
            foreach (var output in ruleBase.Outputs)
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

        public override object Clone()
        {
            var ruleBase = (RuleBase)Activator.CreateInstance(GetType());
            ruleBase.CopyFrom(this);
            return ruleBase;
        }

        public override void CopyFrom(object source)
        {
            var ruleBase = source as RuleBase;
            if (ruleBase != null)
            {
                base.CopyFrom(source);
            }
        }
    }
}