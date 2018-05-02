using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public abstract class DefinitionGeneratorStructure : IDefinitionGeneratorStructure
    {
        private readonly CompoundStructureInfo compoundStructureInfo;
        protected DelftIniCategory IniCategory { get; private set; }

        protected DefinitionGeneratorStructure(CompoundStructureInfo compoundStructureInfo)
        {
            this.compoundStructureInfo = compoundStructureInfo;
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

            var compoundStructureId = compoundStructureInfo.Id;
            IniCategory.AddProperty(StructureRegion.Compound.Key, compoundStructureId, StructureRegion.Compound.Description);
            if (compoundStructureId > 0) IniCategory.AddProperty(StructureRegion.CompoundName.Key, compoundStructureInfo.Name, StructureRegion.CompoundName.Description);
            IniCategory.AddProperty(StructureRegion.DefinitionType.Key, definitionType, StructureRegion.DefinitionType.Description);
        }
    }

    public interface IDefinitionGeneratorStructure
    {
        DelftIniCategory CreateStructureRegion(IStructure structure);
    }
}