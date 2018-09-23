using System.Collections;
using System.ComponentModel;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using PostSharp.Aspects.Advices;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    [Entity]
    public class WaterFlowFM1D2DLink : Feature, INameable, IComparer
    {
        public WaterFlowFM1D2DLink(int fromPoint, int toCell, string name = null)
        {
            FaceIndex = toCell;
            DiscretisationPointIndex = fromPoint;
            TypeOfLink = GridApiDataSet.LinkType.Embedded;
            if (string.IsNullOrEmpty(name))
            {
                Name = string.Format("1D2Dlink_{0}_{1}", fromPoint, toCell);
            }
        }

        /// <summary>
        /// Geometry
        /// Don't throw this redundant property away: needed for NotifyPropertyChannge event [Entity]
        /// </summary>
        public override IGeometry Geometry
        {
            get
            {
                return base.Geometry;
            }
            set
            {
                base.Geometry = value;
                var IsThisOverrideNeededForNotifyPropertyChangeEvent = true;
            }
        }

        /// <summary>
        /// The snap tolerance used during creation on map -> for reproducing
        /// </summary>
        public double SnapToleranceUsed { get; set; }

        [DisplayName("Name")]
        [FeatureAttribute(Order = 1, ExportName = "Name")]
        public string Name { get; set; }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2, ExportName = "Long name")]
        public string LongName { get; set; }

        [DisplayName("Type of link")]
        [FeatureAttribute(Order = 3, ExportName = "Type")]
        public GridApiDataSet.LinkType TypeOfLink { get; set; }

        [DisplayName("Point index")]
        [FeatureAttribute(Order = 4, ExportName = "Point index")]
        [ReadOnly(true)]
        public int DiscretisationPointIndex { get; set; }

        [DisplayName("Cell index")]
        [FeatureAttribute(Order = 5, ExportName = "Cell index")]
        [ReadOnly(true)]
        public int FaceIndex { get; set; }

        public int Compare(object object1, object object2)
        {
            var link1 = (WaterFlowFM1D2DLink)object1;
            var link2 = (WaterFlowFM1D2DLink)object2;
            if ((link1.DiscretisationPointIndex == link2.DiscretisationPointIndex) && (link1.FaceIndex == link2.FaceIndex) && (link1.TypeOfLink == link2.TypeOfLink))
                return 0;
            if ((link1.DiscretisationPointIndex < link2.DiscretisationPointIndex) || ((link1.DiscretisationPointIndex == link2.DiscretisationPointIndex) && (link1.FaceIndex < link2.FaceIndex)))
                return -1;
            return 1;
        }

    }
}
