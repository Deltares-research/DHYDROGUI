using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
   public class RasterFileImporterTest
    {

        [Test]
        public void Given_AnRasterFileImporter_When_ImportingAnAscFileAsGrid_Then_AGridIsReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\dem100x100_ref_dike.asc");

            var importer = new RasterFileImporter();
            var expectedGrid =  importer.ImportItem(testFilePath, new WaterFlowFMModel()) as UnstructuredGrid;
            Assert.IsNotNull(expectedGrid);
            Assert.AreEqual(319*324,expectedGrid.Cells.Count);
            Assert.AreEqual((319+1)*(324+1), expectedGrid.Vertices.Count);
            Assert.AreEqual(207355,expectedGrid.Edges.Count);
        }
    }
}
