using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    internal class GridWizardMapToolHelperTest
    {
        [Test]
        public void TestDensify()
        {
            var coordinates = new[]
            {
                new Coordinate(0.0, 0.0, 0.0),
                new Coordinate(30.0, 40.0, 0.0)
            };
            var lineString = new LineString(coordinates);
            const double desiredLength = 5.0;
            Type t = typeof(GridWizardMapToolHelper);
            object result = t.InvokeMember("Densify", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[]
            {
                lineString,
                desiredLength,
                double.PositiveInfinity
            });

            var lengthIndexedLine = new LengthIndexedLine(lineString);
            IEnumerable<Coordinate> denseCoordinates = Enumerable.Range(0, 11).Select(i => lengthIndexedLine.ExtractPoint(i * desiredLength));
            var reference = new LineString(denseCoordinates.ToArray());

            Assert.IsTrue(reference.Equals(reference));
        }

        [Test]
        public void TestMakeBoundaryLineStrings()
        {
            var embankmentCoordinates = new[]
            {
                new Coordinate(0.0, 0.0, 0.0),
                new Coordinate(1000.0, 0.0, 0.0)
            };
            var embankment = new LineString(embankmentCoordinates);
            var embankments = new List<LineString> {embankment};

            var branchCoordinates = new[]
            {
                new Coordinate(0.0, 100.0, 0.0),
                new Coordinate(1000.0, 100.0, 0.0)
            };
            var branch = new LineString(branchCoordinates);
            IList<IGeometry> branches = new List<IGeometry> {branch};

            IGeometry polygon = new Polygon(new LinearRing(new[]
            {
                new Coordinate(100.0, -1000.0, 0.0),
                new Coordinate(100.0, 1000.0, 0.0),
                new Coordinate(900.0, 1000.0, 0.0),
                new Coordinate(900.0, -1000.0, 0.0),
                new Coordinate(100.0, -1000.0, 0.0)
            }));

            var result = (IEnumerable<LineString>) TypeUtils.CallPrivateStaticMethod(typeof(GridWizardMapToolHelper), "MakeBoundaryLineStrings", embankments, polygon, branches);

            var reference = new LineString(new[]
            {
                new Coordinate(100.0, 0.0, 0.0),
                new Coordinate(100.0, -1000.0, 0.0),
                new Coordinate(900.0, -1000.0, 0.0),
                new Coordinate(900.0, 0.0, 0.0)
            });

            LineString lineString = result.ElementAt(0);
            Assert.IsTrue(lineString.Equals(reference));
        }

        [Test]
        public void TestGetEmbankmentLineStrings()
        {
            var embankmentCoordinates = new[]
            {
                new Coordinate(0.0, 0.0, 0.0),
                new Coordinate(1000.0, 0.0, 0.0)
            };
            var embankment = new LineString(embankmentCoordinates);
            var embankments = new List<LineString> {embankment};

            var userLineString = new LineString(new[]
            {
                new Coordinate(100.0, -1000.0, 0.0),
                new Coordinate(100.0, 1000.0, 0.0),
                new Coordinate(900.0, 1000.0, 0.0),
                new Coordinate(900.0, -1000.0, 0.0),
                new Coordinate(100.0, -1000.0, 0.0)
            });
            var userPolygon = new Polygon(new LinearRing(userLineString.Coordinates));

            object result = TypeUtils.CallPrivateStaticMethod(typeof(GridWizardMapToolHelper), "GetEmbankmentLineStrings", embankments, userPolygon);
            var reference = new List<ILineString>
            {
                new LineString(new[]
                {
                    new Coordinate(100.0, 0.0, 0.0),
                    new Coordinate(900.0, 0.0, 0.0)
                })
            };

            Assert.AreEqual(result, reference);
        }

        [Test]
        public void TestProjectLineStrings()
        {
            var embankmentCoordinates = new[]
            {
                new Coordinate(0.0, 0.0, 0.0),
                new Coordinate(1000.0, 0.0, 0.0)
            };
            var embankment = new LineString(embankmentCoordinates);

            var velocityCoordinates = new[]
            {
                new Coordinate(0.0, 100.0, 0.0),
                new Coordinate(1000.0, 100.0, 0.0)
            };
            var velocities = new LineString(velocityCoordinates);

            var associations = new Dictionary<ILineString, IList<ILineString>>() {{embankment, new List<ILineString> {velocities}}};

            var result = (IEnumerable<ILineString>) TypeUtils.CallPrivateStaticMethod(typeof(GridWizardMapToolHelper), "ProjectLineStrings", associations, 20, 100);

            var reference = new LineString(new[]
            {
                new Coordinate(0.0, 0.0, 0.0),
                new Coordinate(500.0, 0.0, 0.0),
                new Coordinate(1000.0, 0.0, 0.0)
            });

            ILineString lineStrings = result.ElementAt(0);
            Assert.IsTrue(lineStrings.Equals(reference));
        }

        [Test]
        public void TestClusterLineStrings()
        {
            var polygon1 = new Polygon(new LinearRing(new[]
            {
                new Coordinate(100.0, -1000.0, 0.0),
                new Coordinate(100.0, 1000.0, 0.0),
                new Coordinate(900.0, 1000.0, 0.0),
                new Coordinate(900.0, -1000.0, 0.0),
                new Coordinate(100.0, -1000.0, 0.0)
            }));

            var polygon2 = new Polygon(new LinearRing(new[]
            {
                new Coordinate(1100.0, -1000.0, 0.0),
                new Coordinate(1100.0, 1000.0, 0.0),
                new Coordinate(1900.0, 1000.0, 0.0),
                new Coordinate(1900.0, -1000.0, 0.0),
                new Coordinate(1100.0, -1000.0, 0.0)
            }));

            var lineStrings = new List<ILineString>();
            lineStrings.AddRange(polygon1.ExteriorRing.Coordinates.Skip(1).Zip(polygon1.ExteriorRing.Coordinates, (second, first) => new LineString(new[]
            {
                first,
                second
            })));
            lineStrings.AddRange(polygon2.ExteriorRing.Coordinates.Skip(1).Zip(polygon2.ExteriorRing.Coordinates, (second, first) => new LineString(new[]
            {
                first,
                second
            })));

            IList<ILineString> projectedLineStrings = new List<ILineString>
            {
                lineStrings[0],
                lineStrings[4],
                lineStrings[7]
            };

            IEnumerable<ILineString> densifiedLineStrings = new[]
            {
                2,
                6
            }.Select(j => lineStrings[j]);
            IEnumerable<ILineString> denseBoundaryLineStrings = new[]
            {
                1,
                3,
                5
            }.Select(j => lineStrings[j]);

            object toMergeLists = TypeUtils.CallPrivateStaticMethod(typeof(GridWizardMapToolHelper), "ClusterLineStrings", projectedLineStrings, densifiedLineStrings, denseBoundaryLineStrings);
            object mergedLineStrings = TypeUtils.CallPrivateStaticMethod(typeof(GridWizardMapToolHelper), "MergeLineStrings", toMergeLists);
            var polygons = (IList<IPolygon>) TypeUtils.CallPrivateStaticMethod(typeof(GridWizardMapToolHelper), "MakePolygons", mergedLineStrings);

            Assert.IsTrue(polygon1.Equals(polygons[0]));
            Assert.IsTrue(polygon2.Equals(polygons[1]));
        }

        [Test]
        public void GivenEmbankmentsWithTooLessCoordinatesInItsGeometry_WhenComputingPolygons_ThenNullIsReturned()
        {
            // Given
            var embankment = new Embankment {Geometry = new Point(0, 0)};
            IList<Feature2D> embankments = new List<Feature2D> {embankment};

            var linearRing = new LinearRing(new[]
            {
                new Coordinate(-1, -1),
                new Coordinate(-1, 1),
                new Coordinate(1, 1),
                new Coordinate(1, -1),
                new Coordinate(-1, -1)
            });
            var userPolygon = new Polygon(linearRing);

            // When
            IList<IPolygon> polygons =
                GridWizardMapToolHelper.ComputePolygons(new Discretization(), embankments, userPolygon, 0.0, 0.0);

            // Then
            const string failureMessage = "Polygons should not have been created, because of the embankment with just one point in its geometry.";
            Assert.IsNull(polygons, failureMessage);
        }
    }
}