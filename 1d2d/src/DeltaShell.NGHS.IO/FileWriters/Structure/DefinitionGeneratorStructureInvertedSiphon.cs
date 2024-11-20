using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureInvertedSiphon : DefinitionGeneratorStructureCulvert
    {
        public DefinitionGeneratorStructureInvertedSiphon(IStructureFileNameGenerator structureFileNameGenerator) : base(structureFileNameGenerator) {}
        
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Culvert);

            var culvert = hydroObject as Culvert;
            if (culvert == null) return IniSection;

            AddCommonCulvertElements(culvert);
            AddInvertedSiphonElements(culvert);
            
            return IniSection;
        }

        protected void AddInvertedSiphonElements(ICulvert culvert)
        {
            IniSection.AddPropertyWithOptionalComment(StructureRegion.SubType.Key, StructureRegion.StructureTypeName.InvertedSiphon, StructureRegion.SubType.Description);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.BendLossCoef.Key, culvert.BendLossCoefficient, StructureRegion.BendLossCoef.Description, StructureRegion.BendLossCoef.Format);
        }
    }
}
