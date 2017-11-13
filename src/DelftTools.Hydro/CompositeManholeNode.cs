using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    public class CompositeManholeNode : HydroNode
    {
        public CompositeManholeNode(string manholeId)
        {
            ManholeId = manholeId;
            Compartments = new List<Manhole>();
        }

        public string ManholeId { get; set; }

        public ICollection<Manhole> Compartments { get; set; }

        public ICollection<IStructure> Connections { get; set; }

        public override double XCoordinate
        {
            get
            {
                var firstManhole = Compartments.FirstOrDefault();
                return firstManhole?.Coordinates.X ?? -999.99;
            }
        }

        public override double YCoordinate
        {
            get
            {
                var firstManhole = Compartments.FirstOrDefault();
                return firstManhole?.Coordinates.Y ?? -999.99;
            }
        }
    }
}
