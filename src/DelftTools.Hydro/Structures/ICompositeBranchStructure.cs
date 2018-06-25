using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    public interface ICompositeBranchStructure : IBranchFeature, IStructure1D
    {
        IEventedList<IStructure1D> Structures { get; set; }
    }
}