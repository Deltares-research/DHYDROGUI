using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    class DefinitionGeneratorStructureInvertedSiphon : DefinitionGeneratorStructureCulvert
    {
        public DefinitionGeneratorStructureInvertedSiphon(int compoundStructureId)
            : base(compoundStructureId)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.InvertedSiphon);

            var culvert = structure as Culvert;
            if (culvert == null) return IniCategory;

            AddCommonCulvertElements(culvert);
            AddInvertedSiphonElements(culvert);
            
            return IniCategory;
        }

        protected void AddInvertedSiphonElements(ICulvert culvert)
        {
            IniCategory.AddProperty(StructureRegion.BendLossCoef.Key, culvert.BendLossCoefficient, StructureRegion.BendLossCoef.Description, StructureRegion.BendLossCoef.Format);
        }
    }
}
