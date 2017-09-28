using DelftTools.Utils;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class WaterFlowFM1D2DLink : Feature, INameable
    {
        public WaterFlowFM1D2DLink(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public int linksCount { get; set; }

        public int linkId { get; set; }

        public int cell2dlink { get; set; }

        public int flow1dLink { get; set; }
    }
}
