using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Hydro structure.
    /// </summary>
    public interface IStructure1D : IStructure, IHydroNetworkFeature, IBranchFeature, IPointFeature
    {
        ICompositeBranchStructure ParentStructure { get; set; }

        /// <summary>
        /// Y offset relative in the profile. This value is used by the structure view to display
        /// the structure in the cross section. It is not used by the 1d model engine.
        /// </summary>
        double OffsetY { get; set; }

        StructureType GetStructureType();
    }
}