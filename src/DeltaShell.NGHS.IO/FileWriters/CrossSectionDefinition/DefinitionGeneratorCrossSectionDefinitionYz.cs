using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
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

        public override DelftIniCategory CreateDefinitionRegion(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            AddCommonProperties(crossSectionDefinition);

            IniCategory.AddProperty(DefinitionPropertySettings.SingleValuedZ, DefinitionPropertySettings.SingleValuedZ.DefaultValue);
            IniCategory.AddProperty(DefinitionPropertySettings.YZCount, crossSectionDefinition.GetProfile().ToList().Count);
            
            AddCoordinates(crossSectionDefinition);
            
            AddFrictionData(crossSectionDefinition, writeFrictionFromDefinition, defaultFrictionId);

            return IniCategory;
        }

        protected void AddFrictionData(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            if (!writeFrictionFromDefinition)
            {
                IniCategory.AddProperty(DefinitionPropertySettings.SectionCount, 1);
                IniCategory.AddProperty(DefinitionPropertySettings.FrictionIds, defaultFrictionId);
                return;
            }

            var crossSectionSections = crossSectionDefinition.Sections;
            if (crossSectionSections != null && crossSectionSections.Any())
            {
                IniCategory.AddProperty(DefinitionPropertySettings.SectionCount, crossSectionSections.Count);
                IniCategory.AddProperty(DefinitionPropertySettings.FrictionIds, string.Join(";", crossSectionSections.Select(css => css.SectionType.Name)));
                IniCategory.AddProperty(DefinitionPropertySettings.FrictionPositions, crossSectionSections.Select(css => css.MinY).Plus(crossSectionSections.Max(css => css.MaxY)));
            }
        }

        private void AddCoordinates(ICrossSectionDefinition crossSectionDefinition)
        {
            var zCoordinates = crossSectionDefinition.IsProxy
                ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition.GetProfile().Select(p => p.Y)
                : crossSectionDefinition.GetProfile().Select(p => p.Y);

            var yCoordinates = crossSectionDefinition.GetProfile().Select(p => p.X);

            IniCategory.AddProperty(DefinitionPropertySettings.YCoors, yCoordinates);
            IniCategory.AddProperty(DefinitionPropertySettings.ZCoors, zCoordinates);
        }
    }
}