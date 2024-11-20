using DelftTools.Utils.Collections.Generic;

namespace DelftTools.Hydro.Structures
{
    public interface ICompositeBranchStructure : IStructure1D, ICompositeNetworkPointFeature
    {
        IEventedList<IStructure1D> Structures { get; set; }

        /// <summary>
        /// Placeholder for meta data
        /// </summary>
        object Tag { get; set; }
    }
}