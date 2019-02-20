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
    public abstract class ConditionBase : RtcBaseObject
    {
        public double Value { get; set; }

        [Aggregation]
        public Input Input { get; set; }

        [Aggregation]
        public IEventedList<RtcBaseObject> TrueOutputs { get; protected set; }
        
        [Aggregation]
        public IEventedList<RtcBaseObject> FalseOutputs { get; protected set; }

        protected ConditionBase()
        {
            Name = ConditionProvider.GetTitle(GetType());
            TrueOutputs = new EventedList<RtcBaseObject>();
            FalseOutputs = new EventedList<RtcBaseObject>();
        }

        public abstract string GetDescription();

        /// <summary>
        /// Converts the information of the condition needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace">The x namespace.</param>
        /// <param name="prefix">The control group name.</param>
        /// <returns>The Xml Element.</returns>
        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            return new XElement(xNamespace + "trigger");
        }

        public virtual IEnumerable<XElement> ToDataConfigExportSeries(XNamespace xNamespace, string prefix)
        {
            yield return new XElement(xNamespace + "timeSeries", new XAttribute("id", RtcXmlTag.Status + GetXmlNameWithoutTag(prefix)));
        }

        public virtual IEnumerable<XElement> ToDataConfigImportSeries(string prefix, XNamespace xNamespace)
        {
            yield break;
        }
        
        [ValidationMethod]
        public static void Validate(ConditionBase conditionBase)
        {
            var exceptions = new List<ValidationException>();

            if ((conditionBase.TrueOutputs.Count + conditionBase.FalseOutputs.Count) == 0)
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

        /// <summary>
        /// All trigger should dump their state to te export file (mail DS )
        /// </summary>
        public string StatusOutputSeriesName
        {
            get { return string.Format("Status_{0}", Name); }
        }

        public override object Clone()
        {
            var conditionBase = (ConditionBase) Activator.CreateInstance(GetType());
            conditionBase.CopyFrom(this);
            return conditionBase;
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

        protected string GetInputName()
        {
            return (Input != null) ? Input.XmlName : "|no input|";
        }
    }
}