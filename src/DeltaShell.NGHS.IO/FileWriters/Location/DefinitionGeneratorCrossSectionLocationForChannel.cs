using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorCrossSectionLocationForChannel : DefinitionGeneratorLocation
    {
        public DefinitionGeneratorCrossSectionLocationForChannel(string iniCategoryName)
            : base(iniCategoryName)
        {
        }

        public override IEnumerable<DelftIniCategory> CreateIniRegion(IBranchFeature branchFeature)
        {
            AddCommonRegionElements(branchFeature);
            var crossSection = branchFeature as ICrossSection;
            if (crossSection == null)
            {
                yield return IniCategory;
                yield break;
            }

            var shift = 0.0;

            if (crossSection.Definition.CrossSectionType == CrossSectionType.Standard)
            {
                shift = crossSection.LowestPoint;
            }
            else if (crossSection.Definition.IsProxy)
            {
                shift = ((CrossSectionDefinitionProxy)crossSection.Definition).LevelShift;
            }
            
            IniCategory.AddProperty(LocationRegion.Shift.Key, shift, LocationRegion.Shift.Description, LocationRegion.Shift.Format);
            IniCategory.AddProperty(LocationRegion.Definition.Key, crossSection.Definition.Name, LocationRegion.Definition.Description);

            yield return IniCategory;
        }
    }
}