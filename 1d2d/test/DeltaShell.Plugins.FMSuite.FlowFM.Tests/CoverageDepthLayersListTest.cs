using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class CoverageDepthLayersListTest
    {
        [Test]
        public void SetDefinitionUniformOnceTest()
        {
            CoverageDepthLayersList list = new CoverageDepthLayersList(CreateUnstructuredGridVertexCoverage);

            Assert.AreEqual(0, list.Coverages.Count);

            list.VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.Uniform);

            Assert.AreEqual(1, list.Coverages.Count);
        }

        [Test]
        public void ChangeDefinitionUniformToTopBottomTest()
        {
            CoverageDepthLayersList list = new CoverageDepthLayersList(CreateUnstructuredGridVertexCoverage);
            list.VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.Uniform);

            Assert.AreEqual(1, list.Coverages.Count);

            list.VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.TopBottom);

            Assert.AreEqual(2, list.Coverages.Count);
        }

        [Test]
        public void ChangeDefinitionTopBottomToUniformTest()
        {
            CoverageDepthLayersList list = new CoverageDepthLayersList(CreateUnstructuredGridVertexCoverage);
            list.VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.TopBottom);

            Assert.AreEqual(2, list.Coverages.Count);

            list.VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.Uniform);

            Assert.AreEqual(1, list.Coverages.Count);
        }

        private UnstructuredGridVertexCoverage CreateUnstructuredGridVertexCoverage(string name)
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
            var coverage = new UnstructuredGridVertexCoverage(grid, false) {Name = name};
            coverage.SetValues(new[] { 1.0, 2.0, 3.0, 4.0 });
            coverage.Components[0].NoDataValue = -999.0;

            return coverage;
        }
    }
}
