using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Hydro structure.
    /// </summary>
    public interface IStructure1D : IStructure, IBranchFeature //TODO : get this inheritance out
    {
        /// <summary>
        /// Y offset relative in the profile. This value is used by the structure view to display
        /// the structure in the cross section. It is not used by the 1d model engine.
        /// </summary>
        double OffsetY { get; set; }
    }
}