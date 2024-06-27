using System.IO;
using DelftTools.Hydro.CrossSections;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public abstract class DefinitionGeneratorCrossSectionDefinition
    {
        protected BinaryWriter BinFileForLevelTables;

        protected IniSection IniSection { get; }
        private readonly string definitiontype;

        protected DefinitionGeneratorCrossSectionDefinition(string definitiontype)
        {
            this.definitiontype = definitiontype;
            IniSection = new IniSection(DefinitionPropertySettings.Header);
        }

        public abstract IniSection CreateDefinitionRegion(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId);

        protected void AddCommonProperties(ICrossSectionDefinition crossSectionDefinition)
        {
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.Id, crossSectionDefinition.Name);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.DefinitionType, definitiontype);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.Thalweg, crossSectionDefinition.Thalweg);
        }
    }
}