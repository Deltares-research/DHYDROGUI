using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using GisSharpBlog.NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture, Category(TestCategory.DataAccess)]
    public class UnstrucGridSnapApiTest
    {
        [Test]
        public void VerifySnapping()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);
            using (var snap = new UnstrucGridSnapApi(model))
            {
                var snappedGeometry = snap.GetGridSnappedGeometry(UnstrucGridSnapApi.ThinDams,
                                                                   model.Area.ThinDams[0].Geometry);

                Assert.AreEqual(43, snappedGeometry.Coordinates.Length);
                Assert.AreEqual(156211.35, snappedGeometry.Coordinates[0].X, 0.001);
                Assert.AreEqual(577728.079, snappedGeometry.Coordinates[42].Y, 0.001);
            }
        }

        [Test]
        public void VerifyPointSnapping()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);
            using (var snap = new UnstrucGridSnapApi(model))
            {
                var geometries = model.Area.ObservationPoints.Select(o => o.Geometry).ToArray();
                var snappedGeometries = snap.GetGridSnappedGeometry(UnstrucGridSnapApi.ObsPoint,
                                                                    geometries).ToList();

                Assert.AreEqual(189, snappedGeometries.Count());

                var snappedGeometry = snappedGeometries[0];
                Assert.AreEqual(1, snappedGeometry.Coordinates.Length);
                Assert.AreNotEqual(geometries[0].Coordinates[0].X, snappedGeometry.Coordinates[0].X);
                Assert.AreNotEqual(geometries[0].Coordinates[0].Y, snappedGeometry.Coordinates[0].Y);
            }
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void VerifyBoundarySnapping()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);

            using (var snap = new UnstrucGridSnapApi(model))
            {
                var geometries = model.Boundaries.Select(o => o.Geometry).ToArray();
                var snappedBoundaries = snap.GetGridSnappedGeometry(UnstrucGridSnapApi.Boundary, geometries).ToList();

                Assert.AreEqual(4, snappedBoundaries.Count());

                // snappedBoundaries[0] isn't very neat. Maybe have some other condition to find the right boundary.
                var snappedBoundary = snappedBoundaries[0];
                Assert.AreEqual(49, snappedBoundary.Coordinates.Length);
                Assert.AreNotEqual(geometries[0].Coordinates[0].X, snappedBoundary.Coordinates[0].X);
                Assert.AreNotEqual(geometries[0].Coordinates[0].Y, snappedBoundary.Coordinates[0].Y);
            }
        }

        [Test]
        public void VerifyBoundaryWaterLevelPointSnapping()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);

            var boundary = model.Boundaries[0];
            var coordinates = boundary.Geometry.Coordinates.ToList();
            var lastCoordinate = coordinates[coordinates.Count - 1];
            coordinates.Add(new Coordinate(lastCoordinate.X + 5000, lastCoordinate.Y));
            coordinates.Add(new Coordinate(156597.144695918, 573322.452946329));
            coordinates.Add(new Coordinate(156610.39561194, 573439.90424791));

            boundary.Geometry = new LineString(coordinates.ToArray());

            using (var snap = new UnstrucGridSnapApi(model))
            {
                var geometries = model.Boundaries.Select(o => o.Geometry).ToArray();
                var snappedBoundaries = snap.GetGridSnappedGeometry(UnstrucGridSnapApi.Boundary, geometries).ToList();
                var snappedWaterLevelBnds = snap.GetGridSnappedGeometry(UnstrucGridSnapApi.WaterLevelBnd, geometries).ToList();

                Assert.AreEqual(4, snappedBoundaries.Count());
                Assert.AreEqual(4, snappedWaterLevelBnds.Count());

                var snappedBoundary = snappedBoundaries[0];
                var snappedWaterLevelBnd = snappedWaterLevelBnds[0];
                Assert.IsInstanceOf<MultiPoint>(snappedWaterLevelBnd);
                Assert.AreEqual(49, snappedWaterLevelBnd.Coordinates.Length);
                Assert.AreNotEqual(geometries[0].Coordinates[0].X, snappedWaterLevelBnd.Coordinates[0].X);
                Assert.AreNotEqual(geometries[0].Coordinates[0].Y, snappedWaterLevelBnd.Coordinates[0].Y);
                Assert.AreNotEqual(snappedBoundary.Coordinates[0].X, snappedWaterLevelBnd.Coordinates[0].X);
                Assert.AreNotEqual(snappedBoundary.Coordinates[0].Y, snappedWaterLevelBnd.Coordinates[0].Y);
            }
        }

        // Overlaps are allowed since dflowfm build 35502...
        [Test]
        public void VerifySnappingWithOverlap()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);
            using (var snap = new UnstrucGridSnapApi(model))
            {
                var geometry = new LineString(new[]
                    {
                        model.Area.ThinDams[0].Geometry.Coordinates[0],
                        model.Area.ThinDams[0].Geometry.Coordinates[1]
                    });

                var snappedGeometries = snap.GetGridSnappedGeometry(UnstrucGridSnapApi.ThinDams,
                                                                     new[] {geometry, geometry}).ToArray();

                var firstGeometry = snappedGeometries[0];
                Assert.AreEqual(8, firstGeometry.Coordinates.Length);

                var secondGeometry = snappedGeometries[1];
                Assert.AreEqual(8, secondGeometry.Coordinates.Length);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void VerifyWaalFixedWeirs()
        {
            var mduPath = TestHelper.GetTestFilePath(@"waal\waal9.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);
            using (var snap = new UnstrucGridSnapApi(model))
            {
                var snappedFixedWeirs = snap.GetGridSnappedGeometry(UnstrucGridSnapApi.FixedWeir,
                                                                   model.Area.FixedWeirs.Select(t => t.Geometry)
                                                                        .Take(5000)
                                                                        .ToArray());

                Assert.AreEqual(95, snappedFixedWeirs.Count(g => g.IsEmpty));
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void VerifySnappingWaalFeaturesDoesntCrash()
        {
            var mduPath = TestHelper.GetTestFilePath(@"waal\waal9.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);
            using (var snap = new UnstrucGridSnapApi(model))
            {
                snap.GetGridSnappedGeometry(UnstrucGridSnapApi.ObsPoint, model.Area.ObservationPoints.Select(t => t.Geometry).ToArray());
                snap.GetGridSnappedGeometry(UnstrucGridSnapApi.Boundary, model.Boundaries.Select(t => t.Geometry).ToArray());
                snap.GetGridSnappedGeometry(UnstrucGridSnapApi.FixedWeir, model.Area.FixedWeirs.Select(t => t.Geometry).Take(2000).ToArray());
                snap.GetGridSnappedGeometry(UnstrucGridSnapApi.ThinDams, model.Area.ThinDams.Select(t => t.Geometry).ToArray());
                snap.GetGridSnappedGeometry(UnstrucGridSnapApi.ObsCrossSection, model.Area.ObservationCrossSections.Select(t => t.Geometry).ToArray());
                snap.GetGridSnappedGeometry(UnstrucGridSnapApi.Pump, model.Area.Pumps.Select(t => t.Geometry).ToArray());
                snap.GetGridSnappedGeometry(UnstrucGridSnapApi.Weir, model.Area.Weirs.Select(t => t.Geometry).ToArray());
                snap.GetGridSnappedGeometry(UnstrucGridSnapApi.Gate, model.Area.Gates.Select(t => t.Geometry).ToArray());
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void VerifyGeometryOutsideGrid()
        {
            var mduPath = TestHelper.GetTestFilePath(@"waal\waal9.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);
            using (var snap = new UnstrucGridSnapApi(model))
            {
                var geometryOutsideGrid = model.Boundaries.First().Geometry;
                Assert.IsTrue(snap.GetGridSnappedGeometry(UnstrucGridSnapApi.ThinDams, geometryOutsideGrid).IsEmpty,
                              "thindam outside grid should be empty");
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Ignore("Fails on build server")]
        public void VerifySnappingIsFast()
        {
            var mduPath = TestHelper.GetTestFilePath(@"dcsm\par16.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);
            using (var snap = new UnstrucGridSnapApi(model))
            {
                // takes 150000ms
                TestHelper.AssertIsFasterThan(1000, () =>
                    {
                        var snappedThinDams = snap.GetGridSnappedGeometry(UnstrucGridSnapApi.ThinDams,
                                                                          model.Area.ThinDams.Select(t => t.Geometry)
                                                                               .ToArray());
                        Assert.AreEqual(1916, snappedThinDams.Count());
                    });
            }
        }
    }
}