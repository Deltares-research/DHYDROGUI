using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveGridOperationApiTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void SnapBoundaryToCurvilinearGrid()
        {
            string gridFilePath = TestHelper.GetTestFilePath("boundaryFromSP2/wave_detail.grd");
            CurvilinearGrid grid = Delft3DGridFileReader.Read(gridFilePath);

            Assert.AreEqual(52, grid.Arguments[0].Values.Count);
            Assert.AreEqual(56, grid.Arguments[1].Values.Count);

            var geometry = new LineString(new[]
            {
                new Coordinate(147759.0, 620922.0),
                new Coordinate(151709.0, 602081.0),
                new Coordinate(157585.0, 586786.0)
            });

            IGeometry snappedGeometry = new WaveGridOperationApi(grid).GetGridSnappedGeometry("boundaries", geometry);

            Assert.AreNotEqual(geometry, snappedGeometry);
            Assert.AreEqual(3, snappedGeometry.Coordinates.Length);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SnapBoundaryToGridWithDryPoints()
        {
            string gridFilePath = TestHelper.GetTestFilePath("bcwTimeseries/outer.grd");
            CurvilinearGrid grid = Delft3DGridFileReader.Read(gridFilePath);

            // boundary on south side
            var geometry = new LineString(new[]
            {
                new Coordinate(4.34e+005, 3.256e+006),
                new Coordinate(5.34e+005, 3.258e+006),
                new Coordinate(6.68e+005, 3.257e+006)
            });

            IGeometry snappedGeometry = new WaveGridOperationApi(grid).GetGridSnappedGeometry("boundaries", geometry);

            Assert.IsNotNull(snappedGeometry);
            Assert.AreEqual(3, snappedGeometry.Coordinates.Length);
            Assert.AreEqual(snappedGeometry.Coordinates[0].Y, snappedGeometry.Coordinates[1].Y, float.Epsilon);
            Assert.AreEqual(snappedGeometry.Coordinates[1].Y, snappedGeometry.Coordinates[2].Y, float.Epsilon);

            // boundary on north side, cannot snap due to dry points
            geometry = new LineString(
                new[]
                {
                    new Coordinate(4.34e+005, 3.3956e+006),
                    new Coordinate(5.34e+005, 3.3958e+006),
                    new Coordinate(6.68e+005, 3.3957e+006)
                }
            );

            snappedGeometry = new WaveGridOperationApi(grid).GetGridSnappedGeometry("boundaries", geometry);
            Assert.IsNull(snappedGeometry);
        }
    }
}