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

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            AddCommonProperties(crossSectionDefinition);

            IniCategory.AddProperty(DefinitionPropertySettings.SingleValuedZ, DefinitionPropertySettings.SingleValuedZ.DefaultValue);
            IniCategory.AddProperty(DefinitionPropertySettings.YZCount, crossSectionDefinition.Profile.ToList().Count);
            AddCoordinates(crossSectionDefinition);
            AddFrictionData(crossSectionDefinition);

            var yzCrossSectionDefinition = crossSectionDefinition.IsProxy ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition as CrossSectionDefinitionYZ : crossSectionDefinition as CrossSectionDefinitionYZ;
            if (yzCrossSectionDefinition == null) return IniCategory;
            var deltaZStorage = yzCrossSectionDefinition.YZDataTable.Select(row => row.DeltaZStorage);
            IniCategory.AddProperty(DefinitionPropertySettings.DeltaZStorage, deltaZStorage);

            return IniCategory;
        }

        protected void AddFrictionData(ICrossSectionDefinition crossSectionDefinition)
        {
            var crossSectionSections = crossSectionDefinition.Sections;
            if (crossSectionSections != null)
            {
                IniCategory.AddProperty(DefinitionPropertySettings.SectionCount, crossSectionSections.Count);
                /*IniCategory.AddProperty(DefinitionPropertySettings.FrictionPositions,
                    string.Join(";", crossSectionSections.Select(css => css.MinY.ToString("F3"))) + ";" +
                    crossSectionSections.Max(css => css.MaxY.ToString("F3")));*/
                IniCategory.AddProperty(DefinitionPropertySettings.FrictionIds, string.Join(";", crossSectionSections.Select(css => css.SectionType.Name)));
                IniCategory.AddProperty(DefinitionPropertySettings.FrictionPositions, crossSectionSections.Select(css => css.MinY).Plus(crossSectionSections.Max(css => css.MaxY)));
                
            }
        }
        private void AddCoordinates(ICrossSectionDefinition crossSectionDefinition)
        {
            var zCoordinates = crossSectionDefinition.IsProxy
                ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition.Profile.Select(p => p.Y)
                : crossSectionDefinition.Profile.Select(p => p.Y);

            var yCoordinates = crossSectionDefinition.Profile.Select(p => p.X);

            IniCategory.AddProperty(DefinitionPropertySettings.YCoors, yCoordinates);
            IniCategory.AddProperty(DefinitionPropertySettings.ZCoors, zCoordinates);
        }
    }
}