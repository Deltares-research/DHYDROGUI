using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface IHydroNetworkFeature : INetworkFeature, IHydroObject
    {
        IHydroNetwork HydroNetwork { get; }

        string Description { get; set; }

        string LongName { get; set; }
    }
}