using DelftTools.TestUtils;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGrid2DIntegrationTests
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write2DSimpleGridTest()
        {
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 100, 100);
            Assert.AreEqual(9, grid.Vertices.Count);
            Assert.AreEqual(12, grid.Edges.Count);
            Assert.AreEqual(4, grid.Cells.Count);
        }
    }
}