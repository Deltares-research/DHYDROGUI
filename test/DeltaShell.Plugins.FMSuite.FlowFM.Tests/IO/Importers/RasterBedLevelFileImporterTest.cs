using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class RasterBedLevelFileImporterTest
    {
        [Test]
        public void Given_A2x2Raster_When_ImportingBedLevels_Then_CorrectPointCloudIsReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\ahn_breach.asc");
            var importer = new RasterBedLevelFileImporter();

            var bedLevels = (importer.ImportItem(testFilePath, new WaterFlowFMModel()) as IEnumerable<IPointValue>).ToList();
            Assert.IsNotNull(bedLevels);

            // grid with values
            // 1 2
            // 5 6 

            Assert.IsTrue(bedLevels.Count == 252*173);
        }

        [Test]
        public void Given_AnRasterFileImporter_When_ImportingAnAscFileWithOnlyIntsAsBedLevelValues_Then_ErrorMessageIsThrown()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\dem2x2_ref_dike.asc");

            var importer = new RasterBedLevelFileImporter();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(testFilePath, new WaterFlowFMModel()), Resources.RasterBedLevelFileImporter_ConvertRegularGridToBedLevelValues_The_file_you_are_trying_to_import_only_contains_integers__This_is_not_yet_supported__Please_change_a_minimum_of_one_value_to_a_decimal_number_in_the_import_file);
        }
    }
}
