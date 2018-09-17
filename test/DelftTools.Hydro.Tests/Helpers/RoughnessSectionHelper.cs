using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Roughness;

namespace DelftTools.Hydro.Tests.Helpers
{
    public static class RoughnessSectionHelper
    {
        public static RoughnessSection GetMainRoughnessSection(this IEnumerable<RoughnessSection> sections)
        {
            return sections.FirstOrDefault(s => s.Name == RoughnessDataSet.MainSectionTypeName);
        }

        public static RoughnessSection GetFloodplain1(this IEnumerable<RoughnessSection> sections)
        {
            return sections.FirstOrDefault(s => s.Name == RoughnessDataSet.Floodplain1SectionTypeName);
        }

        public static RoughnessSection GetFloodplain2(this IEnumerable<RoughnessSection> sections)
        {
            return sections.FirstOrDefault(s => s.Name == RoughnessDataSet.Floodplain2SectionTypeName);
        }
    }
}
