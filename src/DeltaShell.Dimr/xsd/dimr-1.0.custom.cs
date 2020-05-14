using System.Xml.Schema;
using System.Xml.Serialization;

namespace DeltaShell.Dimr.xsd
{
    public partial class dimrXML
    {
        [XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
        public string XsiSchemaLocation { get; } = "http://schemas.deltares.nl/dimr " +
                                                   "http://content.oss.deltares.nl/schemas/dimr-1.0.xsd";
    }
}