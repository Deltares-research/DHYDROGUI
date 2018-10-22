using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class RasterBedLevelFileImporterTest
    {
        [Test]
        public void Given_A2x2Raster_When_ImportingBedLevels_Then_CorrectPointCloudIsReturned()
        {

            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\dem100x100_ref_dike2x2.asc");
            var importer = new RasterBedLevelFileImporter();

            var bedLevels = importer.ImportItem(testFilePath, new WaterFlowFMModel()) as IList<PointValue>;
            Assert.IsNotNull(bedLevels);

            // grid with values
            // 1 2
            // 5 6 

            Assert.IsTrue(bedLevels.Count == 4);
            Assert.IsTrue(bedLevels[0].X == 50);
            Assert.IsTrue(bedLevels[0].Y == 50);
            Assert.IsTrue(bedLevels[0].Value == 5);

            Assert.IsTrue(bedLevels[1].X == 150);
            Assert.IsTrue(bedLevels[1].Y == 50);
            Assert.IsTrue(bedLevels[1].Value == 6);

            Assert.IsTrue(bedLevels[2].X == 50);
            Assert.IsTrue(bedLevels[2].Y == 150);
            Assert.IsTrue(bedLevels[2].Value == 1);

            Assert.IsTrue(bedLevels[3].X == 150);
            Assert.IsTrue(bedLevels[3].Y == 150);
            Assert.IsTrue(bedLevels[3].Value == 2);

        }



    }
}
