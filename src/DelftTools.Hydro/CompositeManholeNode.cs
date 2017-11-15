using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    public class CompositeManholeNode : HydroNode
    {
        public CompositeManholeNode(string manholeId) : base("manhole node")
        {
            ManholeId = manholeId;
            Compartments = new List<Manhole>();
        }

        public string ManholeId { get; }

        public IList<Manhole> Compartments { get; set; }

        public ICollection<IStructure> Connections { get; set; }

        public override double XCoordinate
        {
            get
            {
                var firstManhole = Compartments.FirstOrDefault();
                return firstManhole?.Coordinates?.X ?? 0.0;
            }
        }

        public override double YCoordinate
        {
            get
            {
                var firstManhole = Compartments.FirstOrDefault();
                return firstManhole?.Coordinates?.Y ?? 0.0;
            }
        }

        public override IGeometry Geometry
        {
            get { return new Point(XCoordinate, YCoordinate); }
        }
    }
}
