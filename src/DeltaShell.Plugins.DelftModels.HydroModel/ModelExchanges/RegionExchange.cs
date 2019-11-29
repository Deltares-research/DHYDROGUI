using System.Runtime.Serialization;
using DelftTools.Hydro;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges
{
    [DataContract]
    public class RegionExchange
    {
        public RegionExchange(HydroLink link)
        {
            SourceName = GetExchangeIdentifier(link.Source);
            TargetName = GetExchangeIdentifier(link.Target);
        }

        private static string GetExchangeIdentifier(IHydroObject hydroObject)
        {
            return hydroObject.Name;
        }

        [DataMember]
        public string SourceName { get; set; }

        [DataMember]
        public string TargetName { get; set; }
    }
}