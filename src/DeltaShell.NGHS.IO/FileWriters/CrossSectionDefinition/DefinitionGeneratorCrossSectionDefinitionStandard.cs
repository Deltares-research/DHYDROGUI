using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public abstract class DefinitionGeneratorCrossSectionDefinitionStandard : DefinitionGeneratorCrossSectionDefinitionZw
    {
        private bool UseTabulatedProfile { get; }
        
        protected DefinitionGeneratorCrossSectionDefinitionStandard(string definitionType, bool useTabulatedProfile = true) : base(definitionType)
        {
            UseTabulatedProfile = useTabulatedProfile;
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (!IsCorrectCrossSectionDefinitionForGenerator(standardDefinition)) return IniCategory;

            AddProperties(standardDefinition);
            if (standardDefinition?.Shape is ICrossSectionStandardShapeOpenClosed shape)
            {
                IniCategory.AddProperty(DefinitionPropertySettings.Closed, shape.Closed ? "yes" : "no");
            }
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
            if (UseTabulatedProfile)
                GenerateTabulatedProfile(standardDefinition.Shape.GetTabulatedDefinition());
        }
        
        protected override void AddCommonProperties(ICrossSectionDefinition crossSectionDefinition)
        {
            base.AddCommonProperties(crossSectionDefinition);
            var crossSectionSection = crossSectionDefinition.Sections.FirstOrDefault();
            if (crossSectionSection != null) IniCategory.AddProperty(DefinitionPropertySettings.FrictionId, crossSectionSection.SectionType?.Name);
        }

        protected abstract bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition);
        protected abstract void AddShapeMeasurementProperties(ICrossSectionStandardShape shape);
    }
}