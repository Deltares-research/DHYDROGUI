using System.Collections.Generic;
using System.Linq;

namespace DelftTools.Hydro.Roughness
{
    public static class RoughnessSectionExtensions
    {
        public static RoughnessSection GetApplicableReverseRoughnessSection(this IEnumerable<RoughnessSection> sections, RoughnessSection normalRoughnessSection)
        {
            return sections.OfType<ReverseRoughnessSection>().FirstOrDefault(rs => rs.NormalSection == normalRoughnessSection) ??
                   normalRoughnessSection;
        }
    }
}