using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class TemperatureDataImporterTest
    {
        [Test]
        public void ImportDefaultTemperatureData()
        {
            var importer = new TemperatureDataImporter();
            var targetItem = new MeteoData(MeteoDataAggregationType.NonCumulative) { Name = RainfallRunoffModelDataSet.TemperatureName };

            importer.ImportItem(TestHelper.GetTestFilePath("DEFAULT.TMP"), targetItem);

            Assert.Greater(targetItem.Data.Components.First().Values.Count, 1);
        }

        [Test]
        public void ImportDefaultTemperatureDataSetsGlobalData()
        {
            var importer = new TemperatureDataImporter();
            var targetItem = new MeteoData(MeteoDataAggregationType.NonCumulative)
                {
                    Name = RainfallRunoffModelDataSet.TemperatureName,
                    DataDistributionType = MeteoDataDistributionType.PerStation
                };

            targetItem.Data.Arguments[1].AddValues(new[] {"station1", "station2"});
            importer.ImportItem(TestHelper.GetTestFilePath("DEFAULT.TMP"), targetItem);

            Assert.AreEqual(MeteoDataDistributionType.Global, targetItem.DataDistributionType);
            Assert.AreEqual(25, targetItem.Data.Components.First().Values.Count);
        }
    }
}
