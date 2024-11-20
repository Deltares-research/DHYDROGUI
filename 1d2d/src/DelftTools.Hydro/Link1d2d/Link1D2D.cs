using System.Linq;
using DelftTools.Utils.Aop;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro.Link1d2d
{
    [Entity]
    public class Link1D2D : Feature, ILink1D2D
    {
        public Link1D2D(int fromPoint, int toCell, string name = null)
        {
            FaceIndex = toCell;
            DiscretisationPointIndex = fromPoint;
            TypeOfLink = LinkStorageType.Embedded;

            Name = string.IsNullOrEmpty(name) 
                ? $"1D2Dlink_{fromPoint}_{toCell}" 
                : name;
        }

        /// <summary>
        /// The snap tolerance used during creation on map -> for reproducing
        /// </summary>
        public double SnapToleranceUsed { get; set; }
        
        public string Name { get; set; }
        
        public string LongName { get; set; }

        public LinkStorageType TypeOfLink { get; set; }
        
        public int DiscretisationPointIndex { get; set; }
        
        public int FaceIndex { get; set; }
        public int Link1D2DIndex { get; set; }

        public Coordinate GetCenter()
        {
            var c1 = Geometry?.Coordinates.FirstOrDefault();
            if (c1 == null) return null;
            var c2 = Geometry?.Coordinates.LastOrDefault();
            if (c2 == null) return null;
            return new Coordinate((c1.X + c2.X) / 2.0, (c1.Y + c2.Y) / 2.0);
        }

        public int Compare(object x, object y)
        {
            var link1 = (Link1D2D)x;
            var link2 = (Link1D2D)y;
            if ((link1.DiscretisationPointIndex == link2.DiscretisationPointIndex) && (link1.FaceIndex == link2.FaceIndex) && (link1.TypeOfLink == link2.TypeOfLink))
                return 0;
            if ((link1.DiscretisationPointIndex < link2.DiscretisationPointIndex) || ((link1.DiscretisationPointIndex == link2.DiscretisationPointIndex) && (link1.FaceIndex < link2.FaceIndex)))
                return -1;
            return 1;
        }

    }
}
