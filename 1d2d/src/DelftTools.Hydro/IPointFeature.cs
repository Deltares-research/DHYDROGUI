using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    public interface IPointFeature : IFeature
    {
        ICompositeNetworkPointFeature ParentPointFeature { get; set; }
    }
}