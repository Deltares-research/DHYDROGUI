using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    public interface ICompositeBranchStructure : IBranchFeature, IStructure
    {
        IEventedList<IStructure> Structures { get; set; }
    }
}