using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public abstract class DefinitionGeneratorStructure : IDefinitionGeneratorStructure
    {
        private readonly int compoundStructureId;
        protected DelftIniCategory IniCategory { get; private set; }

        protected DefinitionGeneratorStructure(int compoundStructureId)
        {
            this.compoundStructureId = compoundStructureId;
            IniCategory = new DelftIniCategory(StructureRegion.Header);
        }

        public abstract DelftIniCategory CreateStructureRegion(IStructure structure);

        protected void AddCommonRegionElements(IStructure structure, string definitionType)
        {
            if (structure.Branch == null) return;
            string nameWithoutHashSigns = structure.Name.Replace("##", "~~");
            IniCategory.AddProperty(StructureRegion.Id.Key, nameWithoutHashSigns, StructureRegion.Id.Description);
            IniCategory.AddProperty(StructureRegion.Name.Key, structure.LongName, StructureRegion.Name.Description);

            IniCategory.AddProperty(StructureRegion.BranchId.Key, structure.Branch.Name, StructureRegion.BranchId.Description);
            IniCategory.AddProperty(StructureRegion.Chainage.Key, structure.Chainage, StructureRegion.Chainage.Description, StructureRegion.Chainage.Format);
            IniCategory.AddProperty(StructureRegion.Compound.Key, compoundStructureId, StructureRegion.Compound.Description);
            IniCategory.AddProperty(StructureRegion.DefinitionType.Key, definitionType, StructureRegion.DefinitionType.Description);
        }
    }

    public interface IDefinitionGeneratorStructure
    {
        DelftIniCategory CreateStructureRegion(IStructure structure);
    }
}