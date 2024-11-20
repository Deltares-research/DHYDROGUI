using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using Deltares.Infrastructure.IO.Ini;
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

        public override IniSection CreateDefinitionRegion(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (!IsCorrectCrossSectionDefinitionForGenerator(standardDefinition)) return IniSection;

            AddProperties(standardDefinition, writeFrictionFromDefinition, defaultFrictionId);
            if (standardDefinition?.Shape is ICrossSectionStandardShapeOpenClosed shape)
            {
                IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.Closed, shape.Closed ? "yes" : "no");
            }
            return IniSection;
        }

        private bool IsCorrectCrossSectionDefinitionForGenerator(CrossSectionDefinitionStandard standardDefinition)
        {
            return standardDefinition != null 
                   && HasCorrectCrossSectionShape(standardDefinition);
        }

        private void AddProperties(
            CrossSectionDefinitionStandard standardDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            AddEnhancedCommonProperties(standardDefinition, writeFrictionFromDefinition, defaultFrictionId);
            AddShapeMeasurementProperties(standardDefinition.Shape);
            if (UseTabulatedProfile)
            {
                GenerateTabulatedProfile(standardDefinition.Shape.GetTabulatedDefinition());
            }
        }
        
        protected virtual void AddEnhancedCommonProperties(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            AddCommonProperties(crossSectionDefinition);
            AddFrictionData(crossSectionDefinition, writeFrictionFromDefinition, defaultFrictionId);
        }

        protected abstract bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition);

        protected abstract void AddShapeMeasurementProperties(ICrossSectionStandardShape shape);

        private void AddFrictionData(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            if (!writeFrictionFromDefinition)
            {
                IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FrictionId, defaultFrictionId);
                return;
            }

            var crossSectionSection = crossSectionDefinition.Sections.FirstOrDefault();
            if (crossSectionSection != null)
            {
                IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FrictionId, crossSectionSection.SectionType?.Name);
            }
        }
    }
}