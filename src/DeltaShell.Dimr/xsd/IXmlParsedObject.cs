using System.Collections.Generic;
using System.Xml;

namespace DeltaShell.Dimr.xsd
{
    public interface IXmlParsedObject
    {
        /// <summary>
        /// Unknown XML attributes that could not parsed to properties of this XML object
        /// </summary>
        List<XmlAttribute> UnKnownAttributes { get; set; }

        /// <summary>
        /// Unknown XML elements that could not parsed to properties of this XML object
        /// </summary>
        List<XmlElement> UnKnownElements { get; set; }
    }
}