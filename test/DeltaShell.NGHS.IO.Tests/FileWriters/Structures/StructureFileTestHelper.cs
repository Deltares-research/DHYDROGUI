using DelftTools.Hydro.Structures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    public static class StructureFileTestHelper
    {
        private static LineString CreateDefaultLineString() => new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) });
        
        public static Weir2D CreateDefaultWeir2D() =>
            new Weir2D(nameof(Weir2D), true)
            {
                Geometry = CreateDefaultLineString(),
            };
        
        public static Pump2D CreateDefaultPump2D() =>
            new Pump2D(nameof(Pump2D), true)
            {
                Geometry = CreateDefaultLineString(),
            };

        public static Gate2D CreateDefaultGate2D() =>
            new Gate2D(nameof(Gate2D))
            {
                Geometry = CreateDefaultLineString(),
            };

        public static LeveeBreach CreateDefaultLeveeBreach() =>
            new LeveeBreach
            {
                Geometry = CreateDefaultLineString(),
            };
    }
}