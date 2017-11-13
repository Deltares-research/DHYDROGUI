using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    public class Manhole : HydroNode
    {
        public Manhole(string manholeId, Coordinate coords)
        {
            ManholeId = manholeId;
            Compartments = new List<ManholeCompartment>();
            Geometry = new Point(coords);
        }

        public string ManholeId { get; set; }

        public ICollection<ManholeCompartment> Compartments { get; set; }

        public ICollection<IStructure> Connections { get; set; }
    }
}
