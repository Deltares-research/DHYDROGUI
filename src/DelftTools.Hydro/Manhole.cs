using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    public class Manhole : Node
    {
        public Manhole(string manholeId) : base(manholeId)
        {
            Compartments = new List<Compartment>();
        }
        
        public IList<Compartment> Compartments { get; set; }

        public double XCoordinate
        {
            get
            {
                return Compartments.Count == 0 ? 0.0 : Compartments.Average(c => c.Geometry?.Coordinate.X ?? 0.0);
            }
        }

        public double YCoordinate
        {
            get
            {
                return Compartments.Count == 0 ? 0.0 : Compartments.Average(c => c.Geometry?.Coordinate.Y ?? 0.0);
            }
        }

        public override IGeometry Geometry
        {
            get { return new Point(XCoordinate, YCoordinate); }
        }
    }
}
