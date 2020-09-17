using DelftTools.Utils;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface IChannel : IBranch, IHydroNetworkFeature, IItemContainer
    {
        //reintroducing members for databinding.
        string Name { get; }

        INode Source { set; }
        INode Target { get; set; }
    }
}