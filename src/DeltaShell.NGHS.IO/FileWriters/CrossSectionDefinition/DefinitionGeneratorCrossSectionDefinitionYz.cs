using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionYz : DefinitionGeneratorCrossSectionDefinition
    {
        protected DefinitionGeneratorCrossSectionDefinitionYz(string definitionType)
            : base(definitionType)
        {
        }

        public DefinitionGeneratorCrossSectionDefinitionYz()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Yz)
        {
        }

        public override IniSection CreateDefinitionRegion(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            AddCommonProperties(crossSectionDefinition);

            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SingleValuedZ, DefinitionPropertySettings.SingleValuedZ.DefaultValue);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.YZCount, crossSectionDefinition.GetProfile().ToList().Count);
            
            AddCoordinates(crossSectionDefinition);
            
            AddFrictionData(crossSectionDefinition, writeFrictionFromDefinition, defaultFrictionId);

            return IniSection;
        }

        protected void AddFrictionData(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            if (!writeFrictionFromDefinition)
            {
                IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SectionCount, 1);
                IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FrictionIds, defaultFrictionId);
                return;
            }

            var crossSectionSections = crossSectionDefinition.Sections;
            if (crossSectionSections != null && crossSectionSections.Any())
            {
                IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SectionCount, crossSectionSections.Count);
                IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.FrictionIds, string.Join(";", crossSectionSections.Select(css => css.SectionType.Name)));
                IniSection.AddPropertyFromConfigurationWithMultipleValues(DefinitionPropertySettings.FrictionPositions, crossSectionSections.Select(css => css.MinY).Plus(crossSectionSections.Max(css => css.MaxY)));
            }
        }

        private void AddCoordinates(ICrossSectionDefinition crossSectionDefinition)
        {
            var zCoordinates = crossSectionDefinition.IsProxy
                ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition.GetProfile().Select(p => p.Y)
                : crossSectionDefinition.GetProfile().Select(p => p.Y);

            var yCoordinates = crossSectionDefinition.GetProfile().Select(p => p.X);

            IniSection.AddPropertyFromConfigurationWithMultipleValues(DefinitionPropertySettings.YCoors, yCoordinates);
            IniSection.AddPropertyFromConfigurationWithMultipleValues(DefinitionPropertySettings.ZCoors, zCoordinates);
        }
    }
}