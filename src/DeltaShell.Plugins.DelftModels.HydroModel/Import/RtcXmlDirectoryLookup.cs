using System.Runtime.Serialization;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    [DataContract]
    public class RtcXmlDirectoryLookup
    {
        [DataMember]
        public string xmlDir { get; set; }

        [DataMember]
        public string schemaDir { get; set; }
    }
}