using DelftTools.Utils;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface IHydroNode : INode, IItemContainer, IHydroNetworkFeature
    {
        /// <summary>
        /// The name of the hydro node
        /// </summary>
        /// <remarks>Reintroduced just for data binding in a feature attribute table (HydroNetworkEditor)</remarks>
        string Name { get; set; }

        /// <summary>
        /// The long name of the hydro node
        /// </summary>
        string LongName { get; set; }
    }
}