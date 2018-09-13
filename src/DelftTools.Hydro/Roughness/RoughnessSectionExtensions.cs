using System.Collections.Generic;
using System.Linq;

namespace DelftTools.Hydro.Roughness
{
    public static class RoughnessSectionExtensions
    {
        /// <summary>
        /// Returns main section based on name
        /// </summary>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static RoughnessSection MainChannel(this IEnumerable<RoughnessSection> sections)
        {
            return sections.FirstOrDefault(s => s.Name == RoughnessDataSet.MainSectionTypeName);
        }

        public static RoughnessSection Floodplain1(this IEnumerable<RoughnessSection> sections)
        {
            return sections.FirstOrDefault(s => s.Name == RoughnessDataSet.Floodplain1SectionTypeName);
        }

        public static RoughnessSection Floodplain2(this IEnumerable<RoughnessSection> sections)
        {
            return sections.FirstOrDefault(s => s.Name == RoughnessDataSet.Floodplain2SectionTypeName);
        }

        public static RoughnessSection GetApplicableReverseRoughnessSection(this IEnumerable<RoughnessSection> sections, RoughnessSection normalRoughnessSection)
        {
            return sections.OfType<ReverseRoughnessSection>().FirstOrDefault(rs => rs.NormalSection == normalRoughnessSection) ??
                   normalRoughnessSection;
        }
    }
}