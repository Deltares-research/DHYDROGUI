using System.Linq;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public abstract class DefinitionGeneratorCrossSectionDefinitionStandard : DefinitionGeneratorCrossSectionDefinitionZw
    {
        protected DefinitionGeneratorCrossSectionDefinitionStandard(string definitionType)
            : base(definitionType)
        {
        }

        protected override void AddCommonRegionElements(ICrossSectionDefinition crossSectionDefinition)
        {
            base.AddCommonRegionElements(crossSectionDefinition);
            var crossSectionSection = crossSectionDefinition.Sections.FirstOrDefault();
            if (crossSectionSection != null)
                IniCategory.AddProperty(DefinitionRegion.RoughnessNames.Key, crossSectionSection.SectionType.Name, DefinitionRegion.RoughnessNames.Description);
        }

        protected CrossSectionDefinitionZW ConverStandardToZw(CrossSectionDefinitionStandard standardDefinition)
        {
            CrossSectionDefinitionZW crossSectionDefinitionZw = standardDefinition.Shape.GetTabulatedDefinition();

            crossSectionDefinitionZw.ShiftLevel(standardDefinition.LevelShift);
            var section = standardDefinition.Sections.FirstOrDefault();
            crossSectionDefinitionZw.Sections.Add(new CrossSectionSection
            {
                SectionType = section == null ? new CrossSectionSectionType() {Name = CrossSectionDefinitionZW.MainSectionName} : section.SectionType,
                MinY = 0,
                MaxY = crossSectionDefinitionZw.Width / 2
            });
            return crossSectionDefinitionZw;
        }
    }
}