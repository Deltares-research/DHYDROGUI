using System;
using System.Xml.Linq;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity(FireOnCollectionChange=false)]
    public class Output : ConnectionPoint
    {
        public string IntegralPart { get; set; }
        public string DifferentialPart { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xNamespace"></param>
        /// <param name="lableName">
        /// For most rules this should be y; for relative time rule x
        /// </param>
        /// 
        /// <param name="integralName"></param>
        /// <returns></returns>
        public XElement ToXml(XNamespace xNamespace, string lableName, string integralName)
        {
            return ToXml(xNamespace, lableName, integralName, null);
        }

        public XElement ToXml(XNamespace xNamespace, string lableName, string integralName, string differentialName)
        {
            var result = new XElement(xNamespace + "output");
            result.Add(new XElement(xNamespace + lableName, XmlName));
            if (!string.IsNullOrEmpty(IntegralPart) && !string.IsNullOrEmpty(integralName))
            {
                result.Add(new XElement(xNamespace + integralName, IntegralPart));
            }
            if (!string.IsNullOrEmpty(DifferentialPart) && !string.IsNullOrEmpty(differentialName))
            {
                result.Add(new XElement(xNamespace + differentialName, DifferentialPart));
            }
            return result;
        }

        public override ConnectionType ConnectionType
        {
            get { return ConnectionType.Output; }
        }

        public override object Clone()
        {
            var output = (Output)Activator.CreateInstance(GetType());
            output.CopyFrom(this);
            return output;
        }

        public override void CopyFrom(object source)
        {
            var output = source as Output;
            if (output != null)
            {
                base.CopyFrom(output);
                IntegralPart = output.IntegralPart;
            }
        }

        public override string XmlName
        {
            get
            {
                {
                    string nameWithoutHashTags = Name.Replace("##", "~~");
                    return "output_" + nameWithoutHashTags;
                }
            }
        }
    }
}
