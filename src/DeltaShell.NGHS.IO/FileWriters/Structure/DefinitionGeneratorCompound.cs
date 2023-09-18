using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorCompound : DefinitionGeneratorStructure
    {
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.CompoundStructure);

            var compositeBranchStructure = hydroObject as CompositeBranchStructure;
            if (compositeBranchStructure == null) return IniSection;

            AddCommonCompoundElements(compositeBranchStructure);

            return IniSection;
        }

        private void AddCommonCompoundElements(CompositeBranchStructure compositeBranchStructure)
        {
            IniSection.AddProperty(StructureRegion.NumberOfCompoundStructures.Key, compositeBranchStructure.Structures.Count, StructureRegion.AllowedFlowDir.Description);
            IniSection.AddPropertyWithOptionalComment(StructureRegion.StructureIds.Key, string.Join(";", compositeBranchStructure.Structures.Select(s => s.Name)), StructureRegion.AllowedFlowDir.Description);
        }
    }
}