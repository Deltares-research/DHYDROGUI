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

            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.PavedElmSet), AggregationOptions.None));
            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.UnpavedElmSet), AggregationOptions.Current));
            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.GreenhouseElmSet), AggregationOptions.None));
            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.OpenWaterElmSet), AggregationOptions.None));
            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.BoundaryElmSet), AggregationOptions.Current));
            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.WWTPElmSet), AggregationOptions.None));
            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.SacramentoElmSet), AggregationOptions.None));
            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.LinkElmSet), AggregationOptions.Current));
            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.BalanceModelElmSet), AggregationOptions.Current));
            Assert.That(Equals(model.OutputSettings.GetCommonAggregationOption(ElementSet.BalanceNodeElmSet), AggregationOptions.Current)); 

        }
    }
}
