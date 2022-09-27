using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureGeneralStructure2D : DefinitionGeneratorStructure2D
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.GeneralStructure);

            var structureGenerator = new DefinitionGeneratorStructureGeneralStructure(new StructureBcFileNameGenerator());
            var generalStructureCategory = structureGenerator.CreateStructureRegion(hydroObject);
            generalStructureCategory.Properties.ForEach(p => IniCategory.Properties.Add(p));

            return IniCategory;
        }
    }
}
