using System.Runtime.Serialization;
using DelftTools.Hydro;
using NetTopologySuite.Extensions.IO;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges
{
    [DataContract]
    public class RegionExchange
    {
        public RegionExchange(HydroLink link)
        {
            SourceName = GetExchangeIdentifier(link.Source);
            TargetName = GetExchangeIdentifier(link.Target);
            LinkName = link.Name;
            LinkGeometryWkt = new WKTWriter().Write(link.Geometry);
        }

        private static string GetExchangeIdentifier(IHydroObject hydroObject)
        {
            return hydroObject.Name;
        }

        [DataMember]
        public string SourceName { get; set; }

        [DataMember]
        public string TargetName { get; set; }

        [DataMember]
        public string LinkName { get; set; }

        [DataMember]
        public string LinkGeometryWkt { get; set; }
    }
}