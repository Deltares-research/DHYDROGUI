using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorCompound : DefinitionGeneratorStructure
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.CompoundStructure);

            var compositeBranchStructure = hydroObject as CompositeBranchStructure;
            if (compositeBranchStructure == null) return IniCategory;

            AddCommonCompoundElements(compositeBranchStructure);

            return IniCategory;
        }

        private void AddCommonCompoundElements(CompositeBranchStructure compositeBranchStructure)
        {
            IniCategory.AddProperty(StructureRegion.NumberOfCompoundStructures.Key, compositeBranchStructure.Structures.Count, StructureRegion.AllowedFlowDir.Description);
            IniCategory.AddProperty(StructureRegion.StructureIds.Key, string.Join(";", compositeBranchStructure.Structures.Select(s => s.Name)), StructureRegion.AllowedFlowDir.Description);
        }
    }
}