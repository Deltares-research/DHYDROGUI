using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness
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
            return sections.FirstOrDefault(s => s.Name == WaterFlowModel1DDataSet.MainChannelName);
        }

        public static RoughnessSection Floodplain1(this IEnumerable<RoughnessSection> sections)
        {
            return sections.FirstOrDefault(s => s.Name == WaterFlowModel1DDataSet.Floodplain1Name);
        }

        public static RoughnessSection Floodplain2(this IEnumerable<RoughnessSection> sections)
        {
            return sections.FirstOrDefault(s => s.Name == WaterFlowModel1DDataSet.Floodplain2Name);
        }

        public static RoughnessSection GetApplicableReverseRoughnessSection(this IEnumerable<RoughnessSection> sections, RoughnessSection normalRoughnessSection)
        {
            return sections.OfType<ReverseRoughnessSection>().FirstOrDefault(rs => rs.NormalSection == normalRoughnessSection) ??
                   normalRoughnessSection;
        }
    }
}