using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for an <see cref="Output"/>.
    /// </summary>
    public class OutputSerializer
    {
        private readonly Output output;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputSerializer"/> class.
        /// </summary>
        /// <param name="output"> The output to serialize. </param>
        public OutputSerializer(Output output)
        {
            this.output = output;
        }

        /// <summary>
        /// Converts the output to an xml element used as a reference by other xml elements.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="labelName">
        /// The name of the label. For most rules this should be y; for relative time rule x.
        /// </param>
        /// <param name="integralName"> The integral name. </param>
        /// <returns> The reference xml element. </returns>
        public XElement ToXmlOutputReference(XNamespace xNamespace, string labelName, string integralName)
        {
            return ToXmlOutputReference(xNamespace, labelName, integralName, null);
        }

        /// <summary>
        /// Converts the output to an xml element used as a reference by other xml elements.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="labelName">
        /// The name of the label. For most rules this should be y; for relative time rule x
        /// </param>
        /// <param name="integralName"> The integral name. </param>
        /// <param name="differentialName"> The differential name. </param>
        /// <returns> The reference xml element. </returns>
        public XElement ToXmlOutputReference(XNamespace xNamespace, string labelName, string integralName,
                                             string differentialName)
        {
            var result = new XElement(xNamespace + "output");
            result.Add(new XElement(xNamespace + labelName, GetXmlName()));
            if (!string.IsNullOrEmpty(output.IntegralPart) && !string.IsNullOrEmpty(integralName))
            {
                result.Add(new XElement(xNamespace + integralName, output.IntegralPart));
            }

            if (!string.IsNullOrEmpty(output.DifferentialPart) && !string.IsNullOrEmpty(differentialName))
            {
                result.Add(new XElement(xNamespace + differentialName, output.DifferentialPart));
            }

            return result;
        }

        /// <summary>
        /// Gets the xml name of the output.
        /// </summary>
        /// <returns> The xml name of the output. </returns>
        public string GetXmlName()
        {
            return RtcXmlTag.Output + output.LocationName.Replace("##", "~~") + "/" + output.ParameterName;
        }
    }
}