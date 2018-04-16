using DelftTools.Utils;
using DeltaShell.NGHS.IO.Grid;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class WaterFlowFM1D2DLink : Feature, INameable
    {
        public WaterFlowFM1D2DLink(int fromPoint, int toCell)
        {
            FaceIndex = toCell;
            DiscretisationPointIndex = fromPoint;
            TypeOfLink = GridApiDataSet.LinkType.Mesh1DMesh2D;
        }

        public string Name { get; set; }

        public string LongName { get; set; }

        public GridApiDataSet.LinkType TypeOfLink { get; set; }

        public int FaceIndex { get; set; }

        public int DiscretisationPointIndex { get; set; }
    }
}
