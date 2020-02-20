using System.Runtime.Serialization;
using DelftTools.Hydro;
using DelftTools.Utils.Reflection;
using NetTopologySuite.Extensions.IO;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges
{
    [DataContract]
    public class RegionExchange
    {
        public RegionExchange(HydroLink link)
        {
            var hydroSourceObject = link.Source == null ? null : TypeUtils.Unproxy(link.Source);
            SourceName = GetExchangeIdentifier(hydroSourceObject);
            SourceType = hydroSourceObject?.GetType().AssemblyQualifiedName;
            var hydroTargetObject = link.Target== null ? null : TypeUtils.Unproxy(link.Target);
            TargetName = GetExchangeIdentifier(hydroTargetObject);
            TargetType = hydroTargetObject?.GetType().AssemblyQualifiedName;
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