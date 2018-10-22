using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
   public class RasterFileImporterTest
    {

        [Test]
        public void Given_AnRasterFileImporter_When_ImportingATwoDecimalAscFileAsGrid_Then_AGridIsReturned()
        {
            try
            {
                var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\ahn_breach.asc");
                var mduFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM.mdu");

                var model = new WaterFlowFMModel();
                model.MduFilePath = mduFilePath;

                var importer = new RasterFileImporter();
                var expectedGrid = importer.ImportItem(testFilePath, model) as UnstructuredGrid;
                Assert.IsNotNull(expectedGrid);

                const int expectedCells = 252 * 173;
                Assert.AreEqual(expectedCells, expectedGrid.Cells.Count);

                const int expectedVertices = (252 + 1) * (173 + 1);
                Assert.AreEqual(expectedVertices, expectedGrid.Vertices.Count);

                const int expectedEdges = 87617;
                Assert.AreEqual(expectedEdges, expectedGrid.Edges.Count);
            }
            finally
            {
                var netFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM_net.nc");
                FileUtils.DeleteIfExists(netFilePath);
            }

        }

        [Test]
        public void Given_AnRasterFileImporter_When_ImportingAFourDecimalAscFileAsGrid_Then_AGridIsReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\dem100x100_ref_dike.asc");
            var mduFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM.mdu");

            var model = new WaterFlowFMModel {MduFilePath = mduFilePath};

            try
            {
                var importer = new RasterFileImporter();
                var expectedGrid = importer.ImportItem(testFilePath, model) as UnstructuredGrid;
                Assert.IsNotNull(expectedGrid);

                const int expectedCells = 319 * 324;
                Assert.AreEqual(expectedCells, expectedGrid.Cells.Count);

                const int expectedVertices = (319 + 1) * (324 + 1);
                Assert.AreEqual(expectedVertices, expectedGrid.Vertices.Count);

                const int expectedEdges = 207355;
                Assert.AreEqual(expectedEdges, expectedGrid.Edges.Count);
            }
            finally
            {
                var netFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM_net.nc");
                FileUtils.DeleteIfExists(netFilePath);
            }
        }



    }
}
