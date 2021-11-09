using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class MeteoDataIntegrationTest
    {
        [Test]
        public void BackwardPeriodicExtrapolationShouldWork()
        {
            RainfallRunoffModel GetModelFunc(MeteoData evaporation)
            {
                return new RainfallRunoffModel
                {
                    StartTime = new DateTime(DateTime.Today.Year, 1, 1),
                    StopTime = new DateTime(DateTime.Today.Year, 12, 31)
                };
            }
            var targetItem = new MeteoData(MeteoDataAggregationType.Cumulative)
                {
                    Name = RainfallRunoffModelDataSet.EvaporationName,
                    DataDistributionType = MeteoDataDistributionType.Global
                };
            var importer = new EvaporationDataImporter(GetModelFunc);
            importer.ImportItem(TestHelper.GetTestFilePath("EVAPOR.GEM"), targetItem);
            Assert.AreEqual(DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365, targetItem.Data.Arguments[0].Values.Count);

            var values = targetItem.GetMeteoForPeriod(new DateTime(1960, 10, 1), new DateTime(1960, 10, 31),
                                                      new TimeSpan(1, 0, 0, 0), null);
            Assert.AreEqual(31, values.Length);

            var checkList = new[] {1.327, 1.257, 1.163, 1.213, 1.09}; 

            var list = new List<double>(values);

            Assert.IsTrue(list.Any(x => Math.Abs(x - checkList[0]) < 0.0001));

            var index = list.FindIndex(0, 31, x => Math.Abs(x - checkList[0]) < 0.0001);

            Assert.IsTrue(index < 25);

            Assert.AreEqual(checkList[1], list[index + 1], 1e-3);
            Assert.AreEqual(checkList[2], list[index + 2], 1e-3);
            Assert.AreEqual(checkList[3], list[index + 3], 1e-3);
            Assert.AreEqual(checkList[4], list[index + 4], 1e-3);
        }
    }
}
