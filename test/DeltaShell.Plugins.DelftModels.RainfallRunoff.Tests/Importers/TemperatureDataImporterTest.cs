using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Units;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Importers
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class TemperatureDataImporterTest
    {
        private readonly Unit unit = new Unit(RainfallRunoffModelDataSet.TemperatureName, "°C");
        
        [Test]
        public void ImportDefaultTemperatureData()
        {
            var importer = new TemperatureDataImporter();
            var targetItem = new TemperatureMeteoData();

            importer.ImportItem(TestHelper.GetTestFilePath("DEFAULT.TMP"), targetItem);

            Assert.Greater(targetItem.Data.Components.First().Values.Count, 1);
            IUnit meteoUnit = targetItem.Data.Components.First().Unit;
            Assert.That(meteoUnit.Name, Is.EqualTo(unit.Name));
            Assert.That(meteoUnit.Symbol, Is.EqualTo(unit.Symbol));
        }

        [Test]
        public void ImportDefaultTemperatureDataSetsGlobalData()
        {
            var importer = new TemperatureDataImporter();
            var targetItem = new TemperatureMeteoData
                {
                    Name = RainfallRunoffModelDataSet.TemperatureName,
                    DataDistributionType = MeteoDataDistributionType.PerStation
                };

            targetItem.Data.Arguments[1].AddValues(new[] {"station1", "station2"});
            importer.ImportItem(TestHelper.GetTestFilePath("DEFAULT.TMP"), targetItem);

            Assert.AreEqual(MeteoDataDistributionType.Global, targetItem.DataDistributionType);
            Assert.AreEqual(25, targetItem.Data.Components.First().Values.Count);
            IUnit meteoUnit = targetItem.Data.Components.First().Unit;
            Assert.That(meteoUnit.Name, Is.EqualTo(unit.Name));
            Assert.That(meteoUnit.Symbol, Is.EqualTo(unit.Symbol));
        }
    }
}
