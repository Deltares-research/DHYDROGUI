using System.ComponentModel;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    [Entity]
    public class Link1D2D : Feature, ILink1D2D
    {
        public Link1D2D(int fromPoint, int toCell, string name = null)
        {
            FaceIndex = toCell;
            DiscretisationPointIndex = fromPoint;
            TypeOfLink = LinkType.Embedded;
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
        public LinkType TypeOfLink { get; set; }

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
            var link1 = (Link1D2D)object1;
            var link2 = (Link1D2D)object2;
            if ((link1.DiscretisationPointIndex == link2.DiscretisationPointIndex) && (link1.FaceIndex == link2.FaceIndex) && (link1.TypeOfLink == link2.TypeOfLink))
                return 0;
            if ((link1.DiscretisationPointIndex < link2.DiscretisationPointIndex) || ((link1.DiscretisationPointIndex == link2.DiscretisationPointIndex) && (link1.FaceIndex < link2.FaceIndex)))
                return -1;
            return 1;
        }

    }
}
