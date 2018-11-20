using System.Collections.Generic;
using System.Xml.Serialization;
using DeltaShell.Dimr.xsd;

namespace DeltaShell.NGHS.IO.Factories
{
    public class DelftConfigXmlSerializerSelector
    {
        private Dictionary<string, XmlSerializer> lookupSerializer;

        public XmlSerializer ReturnSerializer(string rootName) => lookupSerializer[rootName];

        public DelftConfigXmlSerializerSelector()
        {
            lookupSerializer = new Dictionary<string, XmlSerializer>();
            var dimrSerializer = new XmlSerializer(typeof(dimrXML));
            lookupSerializer.Add("dimrConfig", dimrSerializer);

            //Todo: Add Rtc Data object model with xsd2code tooling
            //var rtcDataConfigSerializer = new XmlSerializer(typeof(rtcDataConfig));
            //lookupSerializer.Add("dimrConfig", rtcDataConfigSerializer)          

            //var rtcRuntimeConfigSerializer = new XmlSerializer(typeof(rtcRuntimeConfig));
            //lookupSerializer.Add("dimrConfig", rtcDataConfigSerializer);       

            //var rtcToolsConfigSerializer = new XmlSerializer(typeof(rtcToolsConfig));
            //lookupSerializer.Add("dimrConfig", rtcDataConfigSerializer);

            //var state_importSerializer = new XmlSerializer(typeof(state_import));
            //lookupSerializer.Add("dimrConfig", rtcDataConfigSerializer);      

            //var timeseries_importSerializer = new XmlSerializer(typeof(timeseries_import));
            //lookupSerializer.Add("dimrConfig", rtcDataConfigSerializer);
        }

    }
}
