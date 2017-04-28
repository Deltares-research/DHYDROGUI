using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    class DefinitionGeneratorStructureSiphon : DefinitionGeneratorStructureInvertedSiphon
    {
        public DefinitionGeneratorStructureSiphon(int compoundStructureId)
            : base(compoundStructureId)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.Siphon);

            var culvert = structure as Culvert;
            if (culvert == null) return IniCategory;

            AddCommonCulvertElements(culvert);
            AddInvertedSiphonElements(culvert);
            AddSiphonElements(culvert);

            return IniCategory;
        }

        private void AddSiphonElements(ICulvert culvert)
        {
            IniCategory.AddProperty(StructureRegion.TurnOnLevel.Key, culvert.SiphonOnLevel, StructureRegion.TurnOnLevel.Description, StructureRegion.TurnOnLevel.Format);
            IniCategory.AddProperty(StructureRegion.TurnOffLevel.Key, culvert.SiphonOffLevel, StructureRegion.TurnOffLevel.Description, StructureRegion.TurnOffLevel.Format);
        }
    }
}
