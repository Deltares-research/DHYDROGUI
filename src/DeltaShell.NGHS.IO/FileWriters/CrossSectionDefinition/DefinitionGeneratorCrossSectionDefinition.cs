using System.IO;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public abstract class DefinitionGeneratorCrossSectionDefinition
    {
        protected BinaryWriter BinFileForLevelTables;

        protected DelftIniCategory IniCategory { get; }
        private readonly string definitiontype;

        protected DefinitionGeneratorCrossSectionDefinition(string definitiontype)
        {
            this.definitiontype = definitiontype;
            IniCategory = new DelftIniCategory(DefinitionPropertySettings.Header);
        }

        public abstract DelftIniCategory CreateDefinitionRegion(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId);

        protected void AddCommonProperties(ICrossSectionDefinition crossSectionDefinition)
        {
            IniCategory.AddProperty(DefinitionPropertySettings.Id, crossSectionDefinition.Name);
            IniCategory.AddProperty(DefinitionPropertySettings.DefinitionType, definitiontype);
            IniCategory.AddProperty(DefinitionPropertySettings.Thalweg, crossSectionDefinition.Thalweg);
        }
    }
}