using System.Runtime.Serialization;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    [DataContract]
    public class RealTimeControlXmlDirectoryLookup
    {
        [DataMember(Name = "xmlDir")]
        public string XmlDirectory { get; set; }

        [DataMember(Name = "schemaDir")]
        public string SchemaDirectory { get; set; }
    }
}