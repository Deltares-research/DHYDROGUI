using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DeltaShell.Dimr.xsd
{
    public partial class dimrXML
    {
        [XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
        public string XsiSchemaLocation { get; set; } = "http://schemas.deltares.nl/dimr " +
                                                        "http://content.oss.deltares.nl/schemas/dimr-1.0.xsd";

        [XmlIgnore]
        public List<XmlAttribute> UnKnownAttributes { get; set; }

        [XmlIgnore]
        public List<XmlElement> UnKnownElements { get; set; }
    }
}