using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class MapFileImporterTest
    {
        [Test]
        public void LoadModelGridBendProf()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);
            Assert.AreEqual(400, grid.Cells.Count);
        }

        [Test]
        public void LoadModelGridPillarFromNetFileFails()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f07_horizontal_viscosity\c020_pillar\input\pillar.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "pillar_net.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);
            Assert.IsNull(grid);
        }

        [Test]
        public void LoadGridPensioenIsSameAsApiGrid()
        {
            string mapPath = TestHelper.GetTestFilePath(@"data\pensioen\pensioen_map.nc");
            UnstructuredGrid gridNc = NetFileImporter.ImportModelGrid(mapPath);
            Assert.AreEqual(7897, gridNc.Cells.Count);

            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapPath);
            Assert.AreEqual(7897, grid.Cells.Count);
            Assert.AreEqual(gridNc.Vertices, grid.Vertices, "vertex order");
            Assert.AreEqual(gridNc.Cells, grid.Cells, "cell order");
            Assert.AreEqual(gridNc.Edges, grid.Edges, "edge order");
        }
    }
}