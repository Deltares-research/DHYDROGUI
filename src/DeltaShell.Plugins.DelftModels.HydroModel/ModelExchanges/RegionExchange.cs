using System;
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
            IHydroObject hydroSourceObject = link.Source == null ? null : TypeUtils.Unproxy(link.Source);
            SourceName = GetExchangeIdentifier(hydroSourceObject);
            Type sourceType = hydroSourceObject?.GetType();
            SourceType = sourceType?.FullName + ", " + sourceType?.Assembly.GetName().Name;
            IHydroObject hydroTargetObject = link.Target== null ? null : TypeUtils.Unproxy(link.Target);
            TargetName = GetExchangeIdentifier(hydroTargetObject);
            Type targetType = hydroTargetObject?.GetType();
            TargetType = targetType?.FullName + ", " + targetType?.Assembly.GetName().Name;
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