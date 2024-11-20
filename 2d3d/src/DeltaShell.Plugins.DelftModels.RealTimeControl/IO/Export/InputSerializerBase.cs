using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Abstract class for serializers of different input types.
    /// </summary>
    public abstract class InputSerializerBase : RtcSerializerBase
    {
        private readonly IInput input;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSerializerBase"/> class.
        /// </summary>
        /// <param name="input"> The input to serialize. </param>
        protected InputSerializerBase(IInput input) : base((RtcBaseObject) input)
        {
            this.input = input;
        }

        /// <summary>
        /// Converts the input to an xml element used as a reference by other xml elements.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="labelName"> Name of the label. </param>
        /// <param name="labelSetpoint"> The set point label. </param>
        /// <returns> The reference xml element. </returns>
        public XElement ToXmlInputReference(XNamespace xNamespace, string labelName, string labelSetpoint = null)
        {
            var result = new XElement(xNamespace + "input");
            result.Add(new XElement(xNamespace + labelName, GetXmlName(string.Empty)));
            string setpoint = input.SetPoint;
            if (!string.IsNullOrEmpty(setpoint) && !string.IsNullOrEmpty(labelSetpoint))
            {
                result.Add(new XElement(xNamespace + labelSetpoint, setpoint));
            }

            return result;
        }

        /// <summary>
        /// Gets the xml name of the input.
        /// </summary>
        /// <param name="prefix">A string that can be used to prepend or append to the XmlName. </param>
        /// <returns> The xml name of the input. </returns>
        public abstract string GetXmlName(string prefix);
    }
}