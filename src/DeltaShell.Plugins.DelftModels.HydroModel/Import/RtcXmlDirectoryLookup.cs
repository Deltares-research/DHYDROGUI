using System.Runtime.Serialization;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    [DataContract]
    public class RtcXmlDirectoryLookup
    {
        [DataMember(Name = "xmlDir")]
        public string XmlDirectory { get; set; }

        [DataMember(Name = "schemaDir")]
        public string SchemaDirectory { get; set; }
    }
}