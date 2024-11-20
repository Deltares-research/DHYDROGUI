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
    public class PrecipitationDataImporterTest
    {
        private readonly Unit unit = new Unit(RainfallRunoffModelDataSet.PrecipitationName, "mm");
        
        [Test]
        public void ImportDefaultData()
        {
            var importer = new PrecipitationDataImporter();
            var targetItem = new PrecipitationMeteoData()
                {
                    Name = RainfallRunoffModelDataSet.PrecipitationName
                };

            importer.ImportItem(TestHelper.GetTestFilePath("DEFAULT.BUI"), targetItem);

            Assert.Greater(targetItem.Data.Components.First().Values.Count, 1);
            IUnit meteoUnit = targetItem.Data.Components.First().Unit;
            Assert.That(meteoUnit.Name, Is.EqualTo(unit.Name));
            Assert.That(meteoUnit.Symbol, Is.EqualTo(unit.Symbol));
        }

        [Test]
        public void ImportGlobalDataSetsDistributionType()
        {
            var importer = new PrecipitationDataImporter();
            var targetItem = new PrecipitationMeteoData()
                {
                    Name = RainfallRunoffModelDataSet.PrecipitationName,
                    DataDistributionType = MeteoDataDistributionType.PerFeature
                };

            importer.ImportItem(TestHelper.GetTestFilePath("DEFAULT.BUI"), targetItem);
            Assert.AreEqual(MeteoDataDistributionType.Global, targetItem.DataDistributionType);
            Assert.AreEqual(41, targetItem.Data.Components.First().Values.Count);
            IUnit meteoUnit = targetItem.Data.Components.First().Unit;
            Assert.That(meteoUnit.Name, Is.EqualTo(unit.Name));
            Assert.That(meteoUnit.Symbol, Is.EqualTo(unit.Symbol));
        }

        [Test]
        public void ImportStationsData()
        {
            var importer = new PrecipitationDataImporter();
            var targetItem = new PrecipitationMeteoData()
                {
                    Name = RainfallRunoffModelDataSet.PrecipitationName,
                    DataDistributionType = MeteoDataDistributionType.Global
                };

            importer.ImportItem(TestHelper.GetTestFilePath("DEFAULT2.BUI"), targetItem);
            Assert.AreEqual(MeteoDataDistributionType.PerStation, targetItem.DataDistributionType);
            Assert.AreEqual(new[] {"GFE1021", "GFE1022"}, targetItem.Data.Arguments[1].Values);
            Assert.AreEqual(145, targetItem.Data.Arguments[0].Values.Count);
            Assert.AreEqual(2*145, targetItem.Data.Components.First().Values.Count);
            IUnit meteoUnit = targetItem.Data.Components.First().Unit;
            Assert.That(meteoUnit.Name, Is.EqualTo(unit.Name));
            Assert.That(meteoUnit.Symbol, Is.EqualTo(unit.Symbol));
        }

    }
}
