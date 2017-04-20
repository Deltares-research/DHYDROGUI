using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorCrossSectionLocation : DefinitionGeneratorLocation
    {
        public DefinitionGeneratorCrossSectionLocation(string iniCategoryName)
            : base(iniCategoryName)
        {
        }

        public override DelftIniCategory CreateIniRegion(IBranchFeature branchFeature)
        {
            AddCommonRegionElements(branchFeature);
            var crossSection = branchFeature as ICrossSection;
            if (crossSection == null) return IniCategory;

            var shift = 0.0;

            if (crossSection.Definition.CrossSectionType == CrossSectionType.Standard)
            {
                shift = crossSection.LowestPoint;
            }
            else if (crossSection.Definition.IsProxy)
            {
                shift = ((CrossSectionDefinitionProxy)crossSection.Definition).LevelShift;
            }
            
            IniCategory.AddProperty(CrossSectionRegion.Shift.Key, shift, CrossSectionRegion.Shift.Description, CrossSectionRegion.Shift.Format);
            IniCategory.AddProperty(CrossSectionRegion.Definition.Key, crossSection.Definition.Name, CrossSectionRegion.Definition.Description);

            return IniCategory;
        }
    }
}