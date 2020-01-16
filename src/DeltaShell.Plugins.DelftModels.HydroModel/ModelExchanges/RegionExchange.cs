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
            SourceType = link.Source?.GetType().AssemblyQualifiedName;
            TargetName = GetExchangeIdentifier(link.Target);
            TargetType = link.Target?.GetType().AssemblyQualifiedName;
            LinkName = link.Name;
            LinkGeometryWkt = new WKTWriter().Write(link.Geometry);
        }

        [DataMember]
        public string TargetType { get; set; }

        [DataMember]
        public string SourceType { get; set; }

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