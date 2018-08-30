using System.Linq;
using DelftTools.Hydro.CrossSections;
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

            IniCategory.AddProperty(DefinitionPropertySettings.YZCount, crossSectionDefinition.Profile.ToList().Count);
            AddValuesYz(crossSectionDefinition);

            var yzCrossSectionDefinition = crossSectionDefinition.IsProxy ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition as CrossSectionDefinitionYZ : crossSectionDefinition as CrossSectionDefinitionYZ;
            if (yzCrossSectionDefinition == null) return IniCategory;
            var deltaZStorage = yzCrossSectionDefinition.YZDataTable.Select(row => row.DeltaZStorage);
            IniCategory.AddProperty(DefinitionPropertySettings.DeltaZStorage, deltaZStorage);

            return IniCategory;
        }

        protected void AddValuesYz(ICrossSectionDefinition crossSectionDefinition)
        {
            var zValues = crossSectionDefinition.IsProxy
                ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition.Profile.Select(p => p.Y)
                : crossSectionDefinition.Profile.Select(p => p.Y);

            var yValues = crossSectionDefinition.Profile.Select(p => p.X);

            IniCategory.AddProperty(DefinitionPropertySettings.YValues, yValues);
            IniCategory.AddProperty(DefinitionPropertySettings.ZValues, zValues);
        }
    }
}