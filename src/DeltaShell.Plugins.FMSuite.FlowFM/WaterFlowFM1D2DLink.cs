using DelftTools.Utils;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class WaterFlowFM1D2DLink : Feature, INameable
    {
        public WaterFlowFM1D2DLink(int fromCell, int toNode )
        {
            cell2dlink = fromCell;
            flow1dLink = toNode;
        }

        public string Name { get; set; }

        public int cell2dlink { get; set; }

        public int flow1dLink { get; set; }
    }
}
