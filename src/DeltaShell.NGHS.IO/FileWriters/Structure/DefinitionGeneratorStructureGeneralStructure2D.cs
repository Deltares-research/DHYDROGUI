using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureGeneralStructure2D : DefinitionGeneratorStructure2D
    {
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.GeneralStructure);

            var structureGenerator = new DefinitionGeneratorStructureGeneralStructure(new StructureBcFileNameGenerator());
            var generalStructureIniSection = structureGenerator.CreateStructureRegion(hydroObject);
            generalStructureIniSection.Properties.ForEach(p => IniSection.AddProperty(p));

            return IniSection;
        }
    }
}
