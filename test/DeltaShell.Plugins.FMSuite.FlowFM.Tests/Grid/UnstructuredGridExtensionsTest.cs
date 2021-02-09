using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Grid
{
    [TestFixture]
    public class UnstructuredGridExtensionsTest
    {
        [Test]
        public void GenerateFlowLinks_GridNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((UnstructuredGrid) null).GenerateFlowLinks();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("grid"));
        }

        [Test]
        public void GenerateFlowLinks_GeneratesTheCorrectFlowLinks()
        {
            // Setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(3, 3, 1, 1);
            grid.FlowLinks.Clear();

            // Call
            grid.GenerateFlowLinks();

            // Assert
            Assert.That(grid.FlowLinks, Has.Count.EqualTo(12));
            AssertFlowLinksExists(grid.FlowLinks, 0, 1);
            AssertFlowLinksExists(grid.FlowLinks, 0, 3);
            AssertFlowLinksExists(grid.FlowLinks, 1, 2);
            AssertFlowLinksExists(grid.FlowLinks, 1, 4);
            AssertFlowLinksExists(grid.FlowLinks, 2, 5);
            AssertFlowLinksExists(grid.FlowLinks, 3, 4);
            AssertFlowLinksExists(grid.FlowLinks, 3, 6);
            AssertFlowLinksExists(grid.FlowLinks, 4, 5);
            AssertFlowLinksExists(grid.FlowLinks, 4, 7);
            AssertFlowLinksExists(grid.FlowLinks, 5, 8);
            AssertFlowLinksExists(grid.FlowLinks, 6, 7);
            AssertFlowLinksExists(grid.FlowLinks, 7, 8);
        }

        private static void AssertFlowLinksExists(IEnumerable<FlowLink> flowLinks, int fromIndex, int toIndex)
        {
            FlowLink flowLink = flowLinks.FirstOrDefault(l => l.CellFromIndex == fromIndex &&
                                                              l.CellToIndex == toIndex);
            Assert.That(flowLink, Is.Not.Null);
        }
    }
}