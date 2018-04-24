using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Feature;
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

        [DisplayName("Name")]
        [FeatureAttribute(Order = 1, ExportName = "Name")]
        public string Name { get; set; }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2, ExportName = "Long name")]
        public string LongName { get; set; }

        [DisplayName("Type of link")]
        [FeatureAttribute(Order = 3, ExportName = "Type")]
        public GridApiDataSet.LinkType TypeOfLink { get; set; }

        public int FaceIndex { get; set; }

        public int DiscretisationPointIndex { get; set; }
    }
}
