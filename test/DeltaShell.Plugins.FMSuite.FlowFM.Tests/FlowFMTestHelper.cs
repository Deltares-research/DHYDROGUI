using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    public static class FlowFMTestHelper
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

        public static Polygon GetInvalidGeometryForEnclosureExample()
        {
            /* Wrapper emulating the Interior ring. */
            return new Polygon(new LinearRing(new[]
            {
                /* 
                       (10.0, 20.0)   O------O (20.0, 20.0)             
                                       \    /   
                                        \  /   
                                         \/   
                                         /\
                                        /  \
                                       /    \
                     (10.0, 10.0)     O------O (20.0, 10.0)
                */
                new Coordinate(10.0, 10.0), new Coordinate(20.0, 20.0),
                new Coordinate(10.0, 20.0), new Coordinate(20.0, 10.0),
                new Coordinate(10.0, 10.0)
            }));
        }

        public static LinearRing GetJustLinearRing()
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

        public static GroupableFeature2DPolygon CreateFeature2DPolygonFromGeometry(string featureName, Geometry geometry)
        {
            var enclosureFeature = new GroupableFeature2DPolygon()
            {
                Name = featureName,
                Geometry = geometry
            };
            return enclosureFeature;
        }

        public static bool ContainsError(this ValidationReport report, string errorMessage)
        {
            return ContainsValidationIssue(report, errorMessage, ValidationSeverity.Error);
        }

        private static bool ContainsValidationIssue(this ValidationReport report, string errorMessage, ValidationSeverity severity)
        {
            foreach (var issue in report.Issues.Where(i => i.Severity == severity))
            {
                Console.WriteLine(issue.Message);

                if (issue.Message == errorMessage) return true;
            }

            return report.SubReports.Any(subReport => ContainsValidationIssue(subReport, errorMessage, severity));
        }
    }
}