using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class UnstructuredGridCoverageExtensionsTest
    {
        [Test]
        public void VertexCoverageToPointCloudTest()
        {
            // unstructured grid: two triangles in a square
            // 2 +-----+ 3
            //   |   / |
            //   | /   |
            // 1 +-----+ 4

            IList<Coordinate> vertices = new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 10),
                    new Coordinate(10, 10),
                    new Coordinate(10, 0)
                };

            var edges = new[,]
                {
                    {1, 2}, {2, 3}, {3, 4}, {4, 1}, {1, 3}
                };

            var grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges);

            var coverage = new UnstructuredGridVertexCoverage(grid, false);
            coverage.SetValues(new[] { 1.0, 2.0, 3.0, 4.0 });
            coverage.Components[0].NoDataValue = -999.0;

            IPointCloud cloud = coverage.ToPointCloud();
            Assert.AreEqual(4, cloud.PointValues.Count);
        }

        [Test]
        public void CellCenterCoverageToPointCloudTest()
        {
            // unstructured grid: two triangles in a square
            // 2 +-----+ 3
            //   |   / |
            //   | /   |
            // 1 +-----+ 4

            IList<Coordinate> vertices = new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 10),
                    new Coordinate(10, 10),
                    new Coordinate(10, 0),
                };

            var edges = new[,]
                {
                    {1, 2}, {2, 3}, {3, 4}, {4, 1}, {1, 3},
                };

            var cellIndices = new[,]
                {
                    {1,2,3}, {1,3,4},
                };

            var grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges, cellIndices);
            var coverage = new UnstructuredGridCellCoverage(grid, false);
            coverage[0] = 1.0;
            coverage[1] = 2.0;
            coverage.Components[0].NoDataValue = -999.0;

            IPointCloud cloud = coverage.ToPointCloud();
            Assert.AreEqual(2, cloud.PointValues.Count);
            Assert.AreEqual(cloud.PointValues[0].Value, 1.0);
            Assert.AreEqual(coverage.Coordinates.First(), new Coordinate(cloud.PointValues[0].X, cloud.PointValues[0].Y));
            Assert.AreEqual(cloud.PointValues[1].Value, 2.0);
            Assert.AreEqual(coverage.Coordinates.Last(), new Coordinate(cloud.PointValues[1].X, cloud.PointValues[1].Y));
        }
    }
}
