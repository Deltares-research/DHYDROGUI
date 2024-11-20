using DelftTools.Utils;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Hydro object is any object contained in the <see cref="IHydroRegion"/>.
    /// </summary>
    public interface IHydroObject : IFeature, INameable
    {
        IHydroRegion Region { get; }
    }
}