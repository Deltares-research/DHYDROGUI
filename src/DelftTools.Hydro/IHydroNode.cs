using DelftTools.Utils;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface IHydroNode : INode, IItemContainer, IHydroNetworkFeature
    {
    }
}