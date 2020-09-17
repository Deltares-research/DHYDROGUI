using System;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// A StructureFeature is a placeholder for 1 or more structures.
    /// If the number of structures exceeds 1 it behaves as a compound
    /// structure
    /// </summary>
    [Entity]
    public class CompositeBranchStructure : BranchStructure, ICompositeBranchStructure
    {
        public CompositeBranchStructure() : this("StructureFeature", 0) {}

        public CompositeBranchStructure(string name, double offset) {}

        [NoNotifyPropertyChange]
        public override double Chainage { get; set; }

        public override StructureType GetStructureType() => throw new NotImplementedException();

        /// <summary>
        /// All structures in the StructureFeature
        /// </summary>
        /// Do not bubble Property changed event because structures are also member of branchFeatures in branch
        /// TODO: make it a composition, structures must be only part of the composite structure isn't it?
        [Aggregation]
        public virtual IEventedList<IStructure1D> Structures { get; set; }
    }
}