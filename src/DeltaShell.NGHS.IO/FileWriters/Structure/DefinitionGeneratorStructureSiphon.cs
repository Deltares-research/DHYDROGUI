using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureSiphon : DefinitionGeneratorStructureInvertedSiphon
    {
        public DefinitionGeneratorStructureSiphon(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Siphon);

            var culvert = hydroObject as Culvert;
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
