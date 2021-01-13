using System.Xml.Schema;
using System.Xml.Serialization;

namespace DeltaShell.Dimr.RtcXsd
{
    // The Xsd.exe tool does not generate schema location attributes. As such,    
    // these attributes are not recognised when parsing an xml and will be 
    // reported as incorrect behaviour. In order to circumvent this, these 
    // attributes need to be added in these partial classes.
    // See https://stackoverflow.com/questions/1408336/xmlserialization-and-xsischemalocation-xsd-exe
    // and D3DFMIQ-2034 for more information.

    public partial class RtcRuntimeConfigComplexType
    {
        [XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
        public string xsiSchemaLocation = "http://www.wldelft.nl/fews " +
                                          ".\\rtcRuntimeConfig.xsd";
    }

    public partial class RTCDataConfigComplexType
    {
        [XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
        public string xsiSchemaLocation = "http://www.wldelft.nl/fews " +
                                          ".\\rtcDataConfig.xsd";
    }

    public partial class RtcToolsConfigComplexType
    {
        [XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
        public string xsiSchemaLocation = "http://www.wldelft.nl/fews " +
                                          ".\\rtcToolsConfig.xsd";
    }

    public partial class TimeSeriesCollectionComplexType
    {
        [XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
        public string xsiSchemaLocation = "http://www.wldelft.nl/fews " +
                                          ".\\pi_timeseries.xsd";
    }
}