using DelftTools.Utils;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    public class FakeRtcTimeSeriesFeature : Feature, INameable
    {
        public string Name { get; set; }
    }
}