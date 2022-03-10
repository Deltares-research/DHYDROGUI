using System;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.Tests
{
    public class FMSuiteCommonTestHelper
    {
        private static string expectedEnclosurePolFileContent = "\r\n    6    2\r\n                      10                      10\r\n                      22                      10\r\n                      20                      15\r\n                      20                      20\r\n                      12                      20\r\n                      10                      15\r\n";

        public static string GetExpectedEnclosurePolFileContent(string featureName)
        {
            return String.Concat(featureName, expectedEnclosurePolFileContent);
        }

        public static Polygon GetValidGeometryForEnclosureExample()
        {
            /* Wrapper emulating the Interior ring. */
            return new Polygon(GetJustLinearRing());
        }

        private static LinearRing GetJustLinearRing()
        {
            return new LinearRing(new[]
            {
                /* 
                       (12.0, 20.0) O----------O (20.0, 20.0)             
                                  /           |
                                 /            |
                     (10.0, 15.0) O             O (20.0, 15.0)
                                |              \
                                |               \
                     (10.0, 10.0) O----------------O (22.0, 10.0)
                */
                new Coordinate(10.0, 10.0), new Coordinate(22.0, 10.0),
                new Coordinate(20.0, 15.0), new Coordinate(20.0, 20.0),
                new Coordinate(12.0, 20.0), new Coordinate(10.0, 15.0),
                new Coordinate(10.0, 10.0)
            });
        }

        public static Feature2DPolygon CreateFeature2DPolygonFromGeometry(string featureName, Geometry geometry)
        {
            var enclosureFeature = new Feature2DPolygon()
            {
                Name = featureName,
                Geometry = geometry
            };
            return enclosureFeature;
        }
    }
}