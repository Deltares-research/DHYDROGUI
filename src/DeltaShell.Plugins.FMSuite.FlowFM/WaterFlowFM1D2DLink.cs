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
    }
}
