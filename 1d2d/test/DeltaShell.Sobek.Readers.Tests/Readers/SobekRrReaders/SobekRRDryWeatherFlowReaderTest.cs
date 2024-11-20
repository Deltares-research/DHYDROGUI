using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRDryWeatherFlowReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadDWALineFromManual()
        {
            string line =
                @"DWA id '125_lcd'  nm  '125 liter per capita per day'  do 1 wc 12. wd 125. wh 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 sc 50. dwa ";

            var sobekRRDryWeatherFlow = Enumerable.First<SobekRRDryWeatherFlow>(new SobekRRDryWeatherFlowReader().Parse(line));

            Assert.AreEqual("125_lcd", sobekRRDryWeatherFlow.Id);
            Assert.AreEqual("125 liter per capita per day", sobekRRDryWeatherFlow.Name);
            Assert.AreEqual(DWAComputationOption.NrPeopleTimesConstantPerHour, sobekRRDryWeatherFlow.ComputationOption);
            Assert.AreEqual(12.0, sobekRRDryWeatherFlow.WaterUsePerHourForConstant);
            Assert.AreEqual(125.0, sobekRRDryWeatherFlow.WaterUsePerDayForVariable);
            Assert.AreEqual(new[] {1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0, 20.0, 21.0, 22.0, 23.0, 24.0},
                            sobekRRDryWeatherFlow.WaterCapacityPerHour);
            Assert.AreEqual(50.0, sobekRRDryWeatherFlow.SaltConcentration);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadDWALineFromTholen()
        {
            string line =
                @"DWA id '1' nm 'DWF2' do 2 wc 0 wd 125 wh 1.5 1.5 1.5 1.5 1.5 3 4 5 6 6.5 7.5 8.5 7.5 6.5 6 5 5 5 4 3.5 3 2.5 2 2 dwa";

            var sobekRRDryWeatherFlow = Enumerable.First<SobekRRDryWeatherFlow>(new SobekRRDryWeatherFlowReader().Parse(line));

            Assert.AreEqual("1", sobekRRDryWeatherFlow.Id);
            Assert.AreEqual("DWF2", sobekRRDryWeatherFlow.Name);
            Assert.AreEqual(DWAComputationOption.NrPeopleTimesVariablePerHour, sobekRRDryWeatherFlow.ComputationOption);
            Assert.AreEqual(0.0, sobekRRDryWeatherFlow.WaterUsePerHourForConstant);
            Assert.AreEqual(125.0, sobekRRDryWeatherFlow.WaterUsePerDayForVariable);
            Assert.AreEqual(new[] { 1.5, 1.5, 1.5, 1.5, 1.5, 3.0, 4.0, 5.0, 6.0, 6.5, 7.5, 8.5, 7.5, 6.5, 6.0, 5.0, 5.0, 5.0, 4.0, 3.5, 3.0, 2.5, 2.0, 2.0 },
                            sobekRRDryWeatherFlow.WaterCapacityPerHour);
            Assert.AreEqual(400.0, sobekRRDryWeatherFlow.SaltConcentration); // = default value
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadDWAFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Tholen.lit\29\Paved.dwa");
            var lstDWAFile = new SobekRRDryWeatherFlowReader().Read(path);
            Assert.AreEqual(2, lstDWAFile.Count());
        }
    }
}