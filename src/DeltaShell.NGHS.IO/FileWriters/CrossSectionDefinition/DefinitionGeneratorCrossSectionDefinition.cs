using System.IO;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public abstract class DefinitionGeneratorCrossSectionDefinition
    {
        protected BinaryWriter BinFileForLevelTables;

        public void SetBinFileForLevelTables(BinaryWriter binaryWriter)
        {
            BinFileForLevelTables = binaryWriter;
        }

        protected DelftIniCategory IniCategory { get; private set; }
        private readonly string definitiontype;

        protected DefinitionGeneratorCrossSectionDefinition(string definitiontype)
        {
            this.definitiontype = definitiontype;
            IniCategory = new DelftIniCategory(DefinitionRegion.Header);
        }

        public abstract DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition);

        protected virtual void AddCommonRegionElements(ICrossSectionDefinition crossSectionDefinition)
        {
            IniCategory.AddProperty(DefinitionRegion.Id.Key, crossSectionDefinition.Name, DefinitionRegion.Id.Description);
            IniCategory.AddProperty(DefinitionRegion.DefinitionType.Key, definitiontype, DefinitionRegion.DefinitionType.Description);
            IniCategory.AddProperty(DefinitionRegion.Thalweg.Key, crossSectionDefinition.Thalweg, DefinitionRegion.Thalweg.Description, DefinitionRegion.Thalweg.Format);
        }
    }
}