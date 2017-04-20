using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekRRMeteoDataImporterTest
    {
        IPartialSobekImporter importer;
        IPartialSobekImporter importerTholen29;
        RainfallRunoffModel rrModel;
        RainfallRunoffModel tholen29Model;

        private void SetImporterForFile(string filePath)
        {
            rrModel = new RainfallRunoffModel();
            importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(filePath, rrModel,
                                                                             new IPartialSobekImporter[]
                                                                                 {
                                                                                     new SobekRRDrainageBasinImporter(),
                                                                                     new SobekRRSettingsImporter(), 
                                                                                     new SobekRRMeteoDataImporter(),
                                                                                     new SobekRRPavedImporter(), 
                                                                                     new SobekRRUnpavedImporter(), 
                                                                                     new SobekRRGreenhouseImporter()
                                                                                 });
            importerTholen29 = PartialSobekImporterBuilder.BuildPartialSobekImporter(filePath, rrModel,
                                                                             new IPartialSobekImporter[]
                                                                                 {
                                                                                     new SobekRRDrainageBasinImporter(),
                                                                                     new SobekRRSettingsImporter(), 
                                                                                     new SobekRRMeteoDataImporter(), //no catchments etc
                                                                                 });
        }

        private void ImportTholenCase29()
        {
            // sorry, but this is for performance (load once)
            if (tholen29Model == null)
            {
                SetImporterForFile(TestHelper.GetDataDir() + @"\Tholen.lit\29\NETWORK.TP");
                importerTholen29.Import();
                tholen29Model = rrModel;
            }
            else
            {
                rrModel = tholen29Model;
            }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            ImportTholenCase29(); //takes time!
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportTholenCheckPrecipitationData()
        {
            ImportTholenCase29(); //cached!
            Assert.AreEqual(MeteoDataDistributionType.Global, rrModel.Precipitation.DataDistributionType);
            Assert.AreEqual(145, rrModel.Precipitation.Data.Arguments[0].Values.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportTholenCheckEvaporationData()
        {
            ImportTholenCase29(); //cached!
            Assert.AreEqual(MeteoDataDistributionType.Global, rrModel.Evaporation.DataDistributionType);
            Assert.AreEqual(6, rrModel.Evaporation.Data.Arguments[0].Values.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportTholenCheckTimeSettings()
        {
            ImportTholenCase29(); //cached!
            Assert.AreEqual(new DateTime(2005, 12, 30), rrModel.StartTime);
            Assert.AreEqual(new DateTime(2006, 1, 5, 0, 0, 0), rrModel.StopTime);
            Assert.AreEqual(new TimeSpan(0, 15, 0), rrModel.TimeStep);
            Assert.AreEqual(new TimeSpan(0, 15, 0), rrModel.OutputTimeStep);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportMultipleMeteoStationsDataEvaporationForOneStationMissing()
        {
            SetImporterForFile(TestHelper.GetDataDir() + @"\RRMiniTestModels\DRRSA.lit\9\NETWORK.TP");

            importer.Import();
            Assert.AreEqual(MeteoDataDistributionType.PerStation, rrModel.Evaporation.DataDistributionType);
            Assert.AreEqual(2, rrModel.Evaporation.Data.Arguments.Count); // time and station
            Assert.AreEqual(6, rrModel.Evaporation.Data.Arguments.First(a => a.ValueType == typeof(DateTime)).Values.Cast<DateTime>().Count());
            Assert.AreEqual(2, rrModel.Evaporation.Data.Arguments[1].Values.OfType<string>().Count());

            var function = rrModel.Evaporation.Data as Function;
            
            var firstStation = function.Arguments[1].CreateValueFilter(rrModel.MeteoStations.First());
            Assert.AreEqual(new []{0.03, 0.03, 0.03, 0.03, 0.03, 0.03}, function.GetValues<double>(firstStation));
            var secondStation = function.Arguments[1].CreateValueFilter(rrModel.MeteoStations.Last());
            Assert.AreEqual(new[] {0.02, 0.025, 0.028, 0.015, 0.011, 0.008}, function.GetValues<double>(secondStation));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ImportMultipleMeteoStationsData()
        {
            SetImporterForFile(TestHelper.GetDataDir() + @"\Tholen.lit\30\NETWORK.TP");

            importer.Import();
            Assert.AreEqual(MeteoDataDistributionType.PerStation, rrModel.Precipitation.DataDistributionType);
            Assert.AreEqual(2, rrModel.Precipitation.Data.Arguments.Count);
            Assert.AreEqual(274, rrModel.Precipitation.Data.Arguments[1].Values.Cast<object>().Distinct().Count());
        }
    }
}
