using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public class DefinitionGeneratorCrossSectionLocationForChannel : DefinitionGeneratorLocation
    {
        public DefinitionGeneratorCrossSectionLocationForChannel(string iniSectionName)
            : base(iniSectionName)
        {
        }

        public override IEnumerable<IniSection> CreateIniRegion(IBranchFeature branchFeature)
        {
            AddCommonRegionElements(branchFeature);
            var crossSection = branchFeature as ICrossSection;
            if (crossSection == null)
            {
                yield return IniSection;
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
            
            IniSection.AddPropertyWithOptionalCommentAndFormat(LocationRegion.Shift.Key, shift, LocationRegion.Shift.Description, LocationRegion.Shift.Format);
            IniSection.AddPropertyWithOptionalComment(LocationRegion.Definition.Key, crossSection.Definition.Name, LocationRegion.Definition.Description);

            yield return IniSection;
        }
    }
}