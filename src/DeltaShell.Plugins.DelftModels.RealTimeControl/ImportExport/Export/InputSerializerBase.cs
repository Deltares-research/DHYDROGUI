using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    /// <summary>
    /// Abstract class for serializers of different input types.
    /// </summary>
    public abstract class InputSerializerBase : RtcSerializerBase
    {
        private readonly IInput input;

        protected InputSerializerBase(IInput input) : base((RtcBaseObject)input)
        {
            this.input = input;
        }

        public XElement ToXmlInputReference(XNamespace xNamespace, string labelName, string labelSetpoint = null)
        {
            var result = new XElement(xNamespace + "input");
            result.Add(new XElement(xNamespace + labelName, GetXmlName()));
            string setpoint = input.SetPoint;
            if (!string.IsNullOrEmpty(setpoint) && !string.IsNullOrEmpty(labelSetpoint))
            {
                result.Add(new XElement(xNamespace + labelSetpoint, setpoint));
            }

            return result;
        }

        public abstract string GetXmlName();
    }
}