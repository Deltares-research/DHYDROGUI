using System.Collections.Generic;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;

namespace DeltaShell.NGHS.IO.Factories
{
    public class DelftConfigXmlSerializerSelector
    {
        private readonly Dictionary<string, XmlSerializer> lookupSerializer;

        public XmlSerializer ReturnSerializer(string rootName) => lookupSerializer[rootName];

        public DelftConfigXmlSerializerSelector()
        {
            lookupSerializer = new Dictionary<string, XmlSerializer>();
            var dimrSerializer = new XmlSerializer(typeof(dimrXML));
            lookupSerializer.Add("dimrConfig", dimrSerializer);

            var rtcDataConfigSerializer = new XmlSerializer(typeof(RTCDataConfigXML));
            lookupSerializer.Add("rtcDataConfig", rtcDataConfigSerializer);

            var rtcRuntimeConfigSerializer = new XmlSerializer(typeof(RtcRuntimeConfigXML));
            lookupSerializer.Add("rtcRuntimeConfig", rtcRuntimeConfigSerializer);

            var rtcToolsConfigSerializer = new XmlSerializer(typeof(RtcToolsConfigXML));
            lookupSerializer.Add("rtcToolsConfig", rtcToolsConfigSerializer);

            var state_importSerializer = new XmlSerializer(typeof(TreeVectorFileXML));
            lookupSerializer.Add("treeVectorFile", state_importSerializer);
        }
    }
}
