using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
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

            UnstructuredGrid grid = MapFileImporter.Import(mduPath, "bendprof_map.nc");
            Assert.AreEqual(400, grid.Cells.Count);
        }

        [Test]
        public void LoadModelGridPillarFromNetFileFails()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f07_horizontal_viscosity\c020_pillar\input\pillar.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            UnstructuredGrid grid = MapFileImporter.Import(mduPath, "pillar_net.nc"); //net file, not map file
            Assert.IsNull(grid);
        }

        [Test]
        public void LoadGridPensioenIsSameAsApiGrid()
        {
            string mapPath = TestHelper.GetTestFilePath(@"data\pensioen\pensioen_map.nc");
            UnstructuredGrid gridNc = NetFileImporter.ImportModelGrid(mapPath);
            Assert.AreEqual(7897, gridNc.Cells.Count);

            string mduPath = TestHelper.GetTestFilePath(@"data\pensioen\pensioen.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            UnstructuredGrid gridApi = MapFileImporter.Import(mduPath, mapPath);
            Assert.AreEqual(7897, gridApi.Cells.Count);
            Assert.AreEqual(gridNc.Vertices, gridApi.Vertices, "vertex order");
            Assert.AreEqual(gridNc.Cells, gridApi.Cells, "cell order");
            Assert.AreEqual(gridNc.Edges, gridApi.Edges, "edge order");
        }
    }
}