using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// The trigger reads
    /// if y(k) > 0 then 1  |<-- 0 seems invalid 
    ///             else 0
    /// The following operators are supported: > >= = <= <
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class StandardCondition : ConditionBase
    {
        private bool inputRequired;
        private static readonly ILog Log = LogManager.GetLogger(typeof(StandardCondition));

        /// <summary>
        /// valid values are "EXPLICIT" "IMPLICIT"; default is EXPLICIT
        /// </summary>
        public virtual string Reference { get; set; }

        public Operation Operation { get; set; }

        public StandardCondition(): this(true){}

        public StandardCondition(bool inputRequired)
        {
            Operation = Operation.Equal;
            Reference = ReferenceType.Explicit; // = default EXPLICIT
            this.inputRequired = inputRequired;
            if (string.IsNullOrEmpty(XmlTag))
            {
                XmlTag = RtcXmlTag.StandardCondition;
            }
        }

        public override string GetDescription()
        {
            return new OperationConverter().OperationToString(Operation) + Value;
        }

        /// <summary>
        /// Generates the xml for standard condition. 
        /// RTC supports either
        ///  "x1Series" operator "x2Value" 
        ///  "x1Series" operator "x2Series" 
        ///  "x1Value" operator "x2Value" 
        ///  "x1Value" operator "x2Series" 
        /// Delta only supports
        ///  "x1Series" operator "x2Value" where x1Series is base.Input and x2Value is base.Value
        /// </summary>
        /// <returns></returns>
        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            return ToXml(xNamespace, prefix, GetInputName());
        }

        protected virtual XElement GetX2Element(XNamespace xNamespace, string inputName)
        {
            return new XElement(xNamespace + "x2Value", Value);
        }

        public XElement ToXml(XNamespace xNamespace, string prefix, string inputName)
        {
            var result = base.ToXml(xNamespace, prefix);
            var standard = new XElement(xNamespace + "standard", new XAttribute("id", XmlTag + prefix + Name));
            standard.Add(new XElement(xNamespace + "condition",
                                new XElement(xNamespace + "x1Series", Reference == string.Empty ? null : new XAttribute("ref", Reference), inputName),
                                new XElement(xNamespace + "relationalOperator", Operation.ToString()),
                                // see above comment
                                GetX2Element(xNamespace, inputName)));
            if (TrueOutputs.OfType<RuleBase>().Any())
            {
                //rules
                standard.Add(new XElement(xNamespace + "true",
                                          TrueOutputs.OfType<RuleBase>().Select(
                                              rule => rule.ToXmlReference(xNamespace, prefix))));
            }

            if (TrueOutputs.OfType<ConditionBase>().Any())
            {
                //conditions
                standard.Add(new XElement(xNamespace + "true", TrueOutputs.OfType<ConditionBase>().Select(condition => condition.ToXml(xNamespace, prefix))));
            }

            if (FalseOutputs.OfType<RuleBase>().Any())
            {
                //rules
                standard.Add(new XElement(xNamespace + "false",
                                          FalseOutputs.OfType<RuleBase>().Select(
                                              rule => rule.ToXmlReference(xNamespace, prefix))));
            }

            if (FalseOutputs.OfType<ConditionBase>().Any())
            {
                //conditions
                standard.Add(new XElement(xNamespace + "false", FalseOutputs.OfType<ConditionBase>().Select(condition => condition.ToXml(xNamespace, prefix))));
            }

            // output series with status info is required by RTC
            standard.Add(new XElement(xNamespace + "output", new XElement(xNamespace + "status", RtcXmlTag.Status + prefix + Name)));
            result.Add(standard);
            return result;
        }
        
        [ValidationMethod]
        public static void Validate(StandardCondition standardCondition)
        {
            var exceptions = new List<ValidationException>();

            if ((standardCondition.Input == null) && (standardCondition.inputRequired))
            {
                exceptions.Add(new ValidationException(string.Format("Condition '{0}' has no input; this is required for standard conditions.", standardCondition.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }
    
        public override object Clone()
        {
            var standardCondition = (StandardCondition)Activator.CreateInstance(GetType());
            standardCondition.CopyFrom(this);
            return standardCondition;
        }

        public override void CopyFrom(object source)
        {
            var standardCondition = source as StandardCondition;
            if (standardCondition != null)
            {
                base.CopyFrom(source);
                Reference = standardCondition.Reference;
                Operation = standardCondition.Operation;
            }
        }

        public static class ReferenceType
        {
            public const string Explicit = "EXPLICIT";
            public const string Implicit = "IMPLICIT";
        }
    }

}
