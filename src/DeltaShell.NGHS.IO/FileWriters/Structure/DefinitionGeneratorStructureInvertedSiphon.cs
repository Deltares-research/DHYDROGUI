using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureInvertedSiphon : DefinitionGeneratorStructureCulvert
    {
        public DefinitionGeneratorStructureInvertedSiphon(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.InvertedSiphon);

            var culvert = hydroObject as Culvert;
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
