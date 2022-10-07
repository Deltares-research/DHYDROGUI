using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Importers
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class EvaporationDataImporterTest
    {
        [Test]
        public void ImportDataOnGlobalEvaporation()
        {
            var importer = new EvaporationDataImporter();
            var targetItem = new EvaporationMeteoData
            {
                DataDistributionType = MeteoDataDistributionType.Global
            };

            importer.ImportItem(TestHelper.GetTestFilePath("T25RRSA.EVP"), targetItem);

            Assert.AreEqual(6, targetItem.Data.Components.First().Values.Count);
        }

        [Test]
        public void ImportDataOnSingleStationEvaporation()
        {
            var importer = new EvaporationDataImporter();
            var targetItem = new EvaporationMeteoData
            {
                DataDistributionType = MeteoDataDistributionType.PerStation
            };
            targetItem.Data.Arguments[1].Values.AddRange(new[] {"station1"});
            importer.ImportItem(TestHelper.GetTestFilePath("T25RRSA.EVP"), targetItem);
            
            Assert.AreEqual(6, targetItem.Data.Components.First().Values.Count);
        }

        [Test]
        public void ImportDataOnTwoStationEvaporation()
        {
            var importer = new EvaporationDataImporter();
            var targetItem = new EvaporationMeteoData
            {
                DataDistributionType = MeteoDataDistributionType.PerStation
            };
            targetItem.Data.Arguments[1].Values.AddRange(new[] {"station1", "station2"});
            importer.ImportItem(TestHelper.GetTestFilePath("T25RRSA.EVP"), targetItem);

            Assert.AreEqual(12, targetItem.Data.Components.First().Values.Count);
        }

        [Test]
        public void ImportDataOnThreeStationEvaporation()
        {
            var importer = new EvaporationDataImporter();
            var targetItem = new EvaporationMeteoData
            {
                DataDistributionType = MeteoDataDistributionType.PerStation
            };
            targetItem.Data.Arguments[1].Values.AddRange(new[] {"station1", "station2", "station3"});
            importer.ImportItem(TestHelper.GetTestFilePath("T25RRSA.EVP"), targetItem);

            var component = targetItem.Data.Components.First();

            Assert.AreEqual(18, component.Values.Count);

            var nonzeroValues = component.Values.OfType<double>().Count(v => v != (double) component.DefaultValue);

            Assert.AreEqual(12, nonzeroValues);
        }
    }
}
