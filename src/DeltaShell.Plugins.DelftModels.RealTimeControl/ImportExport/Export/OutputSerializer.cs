using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public class OutputSerializer
    {
        private readonly Output output;

        public OutputSerializer(Output output)
        {
            this.output = output;
        }

        /// <summary>
        /// </summary>
        /// <param name="xNamespace"> </param>
        /// <param name="lableName">
        /// For most rules this should be y; for relative time rule x
        /// </param>
        /// <param name="integralName"> </param>
        /// <returns> </returns>
        public XElement ToXmlOutputReference(XNamespace xNamespace, string lableName, string integralName)
        {
            return ToXmlOutputReference(xNamespace, lableName, integralName, null);
        }

        public XElement ToXmlOutputReference(XNamespace xNamespace, string lableName, string integralName,
                                     string differentialName)
        {
            var result = new XElement(xNamespace + "output");
            result.Add(new XElement(xNamespace + lableName, GetXmlName()));
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

        public string GetXmlName()
        {
            return RtcXmlTag.Output + output.LocationName.Replace("##", "~~") + "/" + output.ParameterName;
        }
    }
}