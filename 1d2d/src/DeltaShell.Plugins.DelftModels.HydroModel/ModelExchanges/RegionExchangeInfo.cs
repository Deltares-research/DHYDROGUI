using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges
{
    [DataContract]
    public class RegionExchangeInfo : IHydroModelExchangeInfo
    {
        public RegionExchangeInfo(IRegion from, IRegion to)
        {
            SourceRegionName = GetModelIdentifier(from);
            TargetRegionName = GetModelIdentifier(to);

            Exchanges = new List<RegionExchange>();
        }

        private static string GetModelIdentifier(IRegion region)
        {
            return region.Name;
        }

        [DataMember]
        public string SourceRegionName { get; set; }

        [DataMember]
        public string TargetRegionName { get; set; }

        [DataMember]
        public IList<RegionExchange> Exchanges { get; set; }

        public bool HasExchanges
        {
            get { return Exchanges.Any(); }
        }
    }
}