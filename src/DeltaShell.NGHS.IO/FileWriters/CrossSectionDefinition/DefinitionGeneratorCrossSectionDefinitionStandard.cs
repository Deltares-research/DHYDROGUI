using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public abstract class DefinitionGeneratorCrossSectionDefinitionStandard : DefinitionGeneratorCrossSectionDefinitionZw
    {
        protected DefinitionGeneratorCrossSectionDefinitionStandard(string definitionType) : base(definitionType)
        {
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (!IsCorrectCrossSectionDefinitionForGenerator(standardDefinition)) return IniCategory;

            AddProperties(standardDefinition);
            return IniCategory;
        }

        private bool IsCorrectCrossSectionDefinitionForGenerator(CrossSectionDefinitionStandard standardDefinition)
        {
            return standardDefinition != null 
                   && HasCorrectCrossSectionShape(standardDefinition);
        }

        private void AddProperties(CrossSectionDefinitionStandard standardDefinition)
        {
            AddCommonProperties(standardDefinition);
            AddShapeMeasurementProperties(standardDefinition.Shape);
            AddCrossSectionStandardProperties();
        }

        private void AddCrossSectionStandardProperties()
        {
            IniCategory.AddProperty(DefinitionPropertySettings.Closed, 1);
        }

        protected override void AddCommonProperties(ICrossSectionDefinition crossSectionDefinition)
        {
            base.AddCommonProperties(crossSectionDefinition);
            var crossSectionSection = crossSectionDefinition.Sections.FirstOrDefault();
            if (crossSectionSection != null) IniCategory.AddProperty(DefinitionPropertySettings.RoughnessNames, crossSectionSection.SectionType.Name);
        }

        protected abstract bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition);
        protected abstract void AddShapeMeasurementProperties(ICrossSectionStandardShape shape);
    }
}