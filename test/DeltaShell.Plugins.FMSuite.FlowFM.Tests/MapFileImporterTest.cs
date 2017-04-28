using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
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
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var grid = MapFileImporter.Import(mduPath, "bendprof_map.nc");
            Assert.AreEqual(400, grid.Cells.Count);
        }

        [Test]
        public void LoadModelGridPillarFromNetFileFails()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f07_horizontal_viscosity\c020_pillar\input\pillar.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var grid = MapFileImporter.Import(mduPath, "pillar_net.nc"); //net file, not map file
            Assert.IsNull(grid);
        }

        [Test]
        public void LoadGridPensioenIsSameAsApiGrid()
        {
            var mapPath = TestHelper.GetTestFilePath(@"data\pensioen\pensioen_map.nc");
            var gridNc = NetFileImporter.ImportModelGrid(mapPath);
            Assert.AreEqual(7897, gridNc.Cells.Count);

            var mduPath = TestHelper.GetTestFilePath(@"data\pensioen\pensioen.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var gridApi = MapFileImporter.Import(mduPath, mapPath);
            Assert.AreEqual(7897, gridApi.Cells.Count);
            Assert.AreEqual(gridNc.Vertices, gridApi.Vertices, "vertex order");
            Assert.AreEqual(gridNc.Cells, gridApi.Cells, "cell order");
            Assert.AreEqual(gridNc.Edges, gridApi.Edges, "edge order");
        }

        [Test, Ignore]
        public void ErrorReadingFile() //DELFT3DFM-908
        {
            var mapPath = TestHelper.GetTestFilePath(@"data\Testrun5000_mk1d2d_map.nc");
            var gridNc = NetFileImporter.ImportModelGrid(mapPath);
            Assert.AreEqual("No Error: The given key was not present in the dictionary", "No Error: The given key was not present in the dictionary");
        }
    }
}