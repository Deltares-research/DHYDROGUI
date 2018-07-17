using System.Collections;
using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class WaterFlowFM1D2DLink : Feature, INameable, IComparer
    {
        public WaterFlowFM1D2DLink(int fromPoint, int toCell)
        {
            FaceIndex = toCell;
            DiscretisationPointIndex = fromPoint;
            TypeOfLink = GridApiDataSet.LinkType.Embedded;
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

        public int DiscretisationPointIndex { get; set; }

        public int FaceIndex { get; set; }

        public int Compare(object object1, object object2)
        {
            var link1 = (WaterFlowFM1D2DLink)object1;
            var link2 = (WaterFlowFM1D2DLink)object2;
            if ((link1.DiscretisationPointIndex == link2.DiscretisationPointIndex) && (link1.FaceIndex == link2.FaceIndex))
                return 0;
            if ((link1.DiscretisationPointIndex < link2.DiscretisationPointIndex) || ((link1.DiscretisationPointIndex == link2.DiscretisationPointIndex) && (link1.FaceIndex < link2.FaceIndex)))
                return -1;

            return 1;
        }
    }
}
