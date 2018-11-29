using System;
using System.Xml.Linq;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity(FireOnCollectionChange=false)]
    public class Input : ConnectionPoint
    {
        /// <summary>
        /// Value of Setpoint in generated xml; will always be regenerated during ToXml process
        /// </summary>
        public string SetPoint { get; set; }

        public XElement ToXml(XNamespace xNamespace, string lableName)
        {
            return ToXml(xNamespace, lableName, null);
        }

        public XElement ToXml(XNamespace xNamespace, string lableName, string lableSetpoint)
        {
            var result = new XElement(xNamespace + "input");
            result.Add(new XElement(xNamespace + lableName, XmlName));
            if (!string.IsNullOrEmpty(SetPoint) && !string.IsNullOrEmpty(lableSetpoint))
            {
                result.Add(new XElement(xNamespace + lableSetpoint, SetPoint));
            }
            return result;
        }

        public override ConnectionType ConnectionType
        {
            get { return ConnectionType.Input; }
        }

        public override object Clone()
        {
            var input = (Input) Activator.CreateInstance(GetType());
            input.CopyFrom(this);
            return input;
        }

        public override void CopyFrom(object source)
        {
            var input = source as Input;
            if (input != null)
            {
                base.CopyFrom(source);
            }
        }

        public override string XmlName
        {
            get { return RtcXmlTag.Input + LocationName.Replace("##", "~~") + "/" + ParameterName; }
        }
    }
}
