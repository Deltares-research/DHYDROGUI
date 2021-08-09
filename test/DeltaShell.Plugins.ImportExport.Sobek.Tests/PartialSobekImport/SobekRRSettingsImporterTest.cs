using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    class SobekRRSettingsImporterTest
    {

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportOutputSettings()
        {
            var pathToSobekNetwork = TestHelper.GetTestFilePath(@"400_000.lit\12\NETWORK.TP");
            var model = new RainfallRunoffModel(); 
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, model, new IPartialSobekImporter[] { new SobekRRSettingsImporter() });

            importer.Import();

            Assert.That(model.OutputSettings.AggregationOption, Is.EqualTo(AggregationOptions.None));
        }
    }
}
