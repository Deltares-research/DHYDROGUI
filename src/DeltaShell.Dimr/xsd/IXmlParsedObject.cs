using System.Collections.Generic;
using System.Xml;

namespace DeltaShell.Dimr.xsd
{
    public interface IXmlParsedObject
    {
        List<XmlAttribute> UnKnownAttributes { get; set; }

        List<XmlElement> UnKnownElements { get; set; }
    }
}