using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekRRMeteoDataImporterTest
    {
        private static IPartialSobekImporter SetImporterForFile(string filePath, RainfallRunoffModel rrModel)
        {
            return PartialSobekImporterBuilder.BuildPartialSobekImporter(filePath, rrModel,
                                                                             new IPartialSobekImporter[]
                                                                                 {
                                                                                     new SobekRRDrainageBasinImporter(),
                                                                                     new SobekRRSettingsImporter(), 
                                                                                     new SobekRRMeteoDataImporter(),
                                                                                     new SobekRRPavedImporter(), 
                                                                                     new SobekRRUnpavedImporter(), 
                                                                                     new SobekRRGreenhouseImporter()
                                                                                 });
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void ImportTholenCheckMeteoData()
        {
            using (var rrModel = new RainfallRunoffModel())
            {
                var tholenLitNetworkTp = TestHelper.GetTestDataDirectory() + @"\Tholen.lit\29\NETWORK.TP";
                var importerTholen29 = PartialSobekImporterBuilder.BuildPartialSobekImporter(tholenLitNetworkTp, rrModel,
                                                                                             new IPartialSobekImporter[]
                                                                                             {
                                                                                                 new SobekRRDrainageBasinImporter(),
                                                                                                 new SobekRRSettingsImporter(),
                                                                                                 new SobekRRMeteoDataImporter(), //no catchments etc
                                                                                             });
                importerTholen29.Import();

                Assert.AreEqual(MeteoDataDistributionType.PerStation, rrModel.Precipitation.DataDistributionType);
                Assert.AreEqual(145, rrModel.Precipitation.Data.Arguments[0].Values.Count);

                Assert.AreEqual(MeteoDataDistributionType.Global, rrModel.Evaporation.DataDistributionType);
                Assert.AreEqual(6, rrModel.Evaporation.Data.Arguments[0].Values.Count);
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void ImportMultipleMeteoStationsDataEvaporationForOneStationMissing()
        {
            using (var rrModel = new RainfallRunoffModel())
            {
                var importer = SetImporterForFile(TestHelper.GetTestDataDirectory() + @"\RRMiniTestModels\DRRSA.lit\9\NETWORK.TP", rrModel);

                importer.Import();
                Assert.AreEqual(MeteoDataDistributionType.PerStation, rrModel.Evaporation.DataDistributionType);
                Assert.AreEqual(2, rrModel.Evaporation.Data.Arguments.Count); // time and station
                Assert.AreEqual(6, rrModel.Evaporation.Data.Arguments.First(a => a.ValueType == typeof(DateTime)).Values.Cast<DateTime>().Count());
                Assert.AreEqual(2, rrModel.Evaporation.Data.Arguments[1].Values.OfType<string>().Count());

                var function = rrModel.Evaporation.Data as Function ;
                
                var firstStation = function.Arguments[1].CreateValueFilter(rrModel.MeteoStations.First());
                Assert.AreEqual(new []{0.03, 0.03, 0.03, 0.03, 0.03, 0.03}, function.GetValues<double>(firstStation));
                var secondStation = function.Arguments[1].CreateValueFilter(rrModel.MeteoStations.Last());
                Assert.AreEqual(new[] {0.02, 0.025, 0.028, 0.015, 0.011, 0.008}, function.GetValues<double>(secondStation));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ImportMultipleMeteoStationsData()
        {
            using (var rrModel = new RainfallRunoffModel())
            {
                var importer = SetImporterForFile(TestHelper.GetTestDataDirectory() + @"\Tholen.lit\30\NETWORK.TP", rrModel);

                importer.Import();
                Assert.AreEqual(MeteoDataDistributionType.PerStation, rrModel.Precipitation.DataDistributionType);
                Assert.AreEqual(2, rrModel.Precipitation.Data.Arguments.Count);
                Assert.AreEqual(274, rrModel.Precipitation.Data.Arguments[1].Values.Cast<object>().Distinct().Count());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenSobekRRMeteoDataImporter_ImportingOnMeteoDataGlobal_ShouldWorkCorrectly()
        {
            //Arrange
            var model = Substitute.For<IRainfallRunoffModel>();
            var basin = Substitute.For<IDrainageBasin>();
            var meteoData = new MeteoData
            {
                Name = RainfallRunoffModelDataSet.PrecipitationName,
                DataDistributionType = MeteoDataDistributionType.Global
            };

            model.Basin.Returns(basin);
            model.Precipitation.Returns(meteoData);

            var fnmFilePath = TestHelper.GetTestFilePath(@"meteo\Global.fnm");
            var importer = new SobekRRMeteoDataImporter
            {
                PathSobek = fnmFilePath,
                TargetObject = model
            };

            // Act
            importer.Import();

            // Assert

            var timeVariable = meteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));

            Assert.NotNull(timeVariable, "Missing time variable");

            Assert.AreEqual(25, timeVariable.Values.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenSobekRRMeteoDataImporter_ImportingOnMeteoDataPerFeature_ShouldWorkCorrectly()
        {
            //Arrange
            var model = Substitute.For<IRainfallRunoffModel>();
            var basin = Substitute.For<IDrainageBasin>();
            var meteoData = new MeteoData
            {
                Name = RainfallRunoffModelDataSet.PrecipitationName,
                DataDistributionType = MeteoDataDistributionType.PerFeature
            };

            model.Basin.Returns(basin);
            model.Precipitation.Returns(meteoData);
            
            var catchments = new EventedList<Catchment>
            {
                new Catchment { Name = "Catchment_1D_1" },
                new Catchment { Name = "Catchment_1D_2" },
                new Catchment { Name = "Catchment_1D_3" }
            };

            basin.Catchments.Returns(catchments);

            var fnmFilePath = TestHelper.GetTestFilePath(@"meteo\PerFeature.fnm");
            var importer = new SobekRRMeteoDataImporter
            {
                PathSobek = fnmFilePath,
                TargetObject = model
            };
            
            // Act
            importer.Import();

            // Assert

            var timeVariable = meteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));
            var catchmentVariable = meteoData.Data.Arguments.FirstOrDefault(a => a.ValueType.Implements(typeof(IFeature)));

            Assert.NotNull(timeVariable, "Missing time variable");
            Assert.NotNull(catchmentVariable, "Missing catchment variable");

            Assert.AreEqual(25, timeVariable.Values.Count);
            Assert.AreEqual(3, catchmentVariable.Values.Count);
            
            Assert.Contains(catchments[0], catchmentVariable.Values);
            Assert.Contains(catchments[1], catchmentVariable.Values);
            Assert.Contains(catchments[2], catchmentVariable.Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenSobekRRMeteoDataImporter_ImportingOnMeteoDataPerFeature_ShouldNotWorkIfCatchmentsAreMissing()
        {
            //Arrange
            var model = Substitute.For<IRainfallRunoffModel>();
            var basin = Substitute.For<IDrainageBasin>();
            var meteoData = new MeteoData
            {
                Name = RainfallRunoffModelDataSet.PrecipitationName,
                DataDistributionType = MeteoDataDistributionType.PerFeature
            };

            model.Basin.Returns(basin);
            model.Precipitation.Returns(meteoData);

            var catchments = new EventedList<Catchment>
            {
                // missing Catchment_1D_2
                new Catchment { Name = "Catchment_1D_1" },
                new Catchment { Name = "Catchment_1D_3" }
            };

            basin.Catchments.Returns(catchments);

            var fnmFilePath = TestHelper.GetTestFilePath(@"meteo\PerFeature.fnm");
            var importer = new SobekRRMeteoDataImporter
            {
                PathSobek = fnmFilePath,
                TargetObject = model
            };

            // Act
            TestHelper.AssertAtLeastOneLogMessagesContains(()=> importer.Import(), "Could not find a catchment for the following stations");

            // Assert
            var timeVariable = meteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));
            var catchmentVariable = meteoData.Data.Arguments.FirstOrDefault(a => a.ValueType.Implements(typeof(IFeature)));

            Assert.NotNull(timeVariable, "Missing time variable");
            Assert.NotNull(catchmentVariable, "Missing catchment variable");

            Assert.AreEqual(25, timeVariable.Values.Count);
            Assert.AreEqual(0, catchmentVariable.Values.Count, "No data should be set when there are missing features during import");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenSobekRRMeteoDataImporter_ImportingOnMeteoDataPerStation_ShouldWorkCorrectly()
        {
            //Arrange
            var model = Substitute.For<IRainfallRunoffModel>();
            var basin = Substitute.For<IDrainageBasin>();
            var meteoStations = Substitute.For<IEventedList<string>>();
            var temperatureStations = Substitute.For<IEventedList<string>>();

            var precipitationMeteoData = new MeteoData
            {
                Name = RainfallRunoffModelDataSet.PrecipitationName,
                DataDistributionType = MeteoDataDistributionType.PerStation
            };
            var evaporationMeteoData = new MeteoData
            {
                Name = RainfallRunoffModelDataSet.EvaporationName,
                DataDistributionType = MeteoDataDistributionType.PerStation
            };
            var temperatureMeteoData = new MeteoData
            {
                Name = RainfallRunoffModelDataSet.TemperatureName,
                DataDistributionType = MeteoDataDistributionType.PerStation
            };

            model.Basin.Returns(basin);
            model.Precipitation.Returns(precipitationMeteoData);
            model.Evaporation.Returns(evaporationMeteoData);
            model.Temperature.Returns(temperatureMeteoData);

            model.MeteoStations.Returns(meteoStations);
            model.TemperatureStations.Returns(temperatureStations);

            meteoStations.Count.Returns(2);

            var fnmFilePath = TestHelper.GetTestFilePath(@"meteo\PerStation.fnm");
            var importer = new SobekRRMeteoDataImporter
            {
                PathSobek = fnmFilePath,
                TargetObject = model
            };

            // Act
            importer.Import();

            // Assert

            var timeVariable = precipitationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));
            var stationVariable = precipitationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(string));

            Assert.NotNull(timeVariable, "Missing precipitation time variable");
            Assert.NotNull(stationVariable, "Missing precipitation station variable");

            Assert.AreEqual(25, timeVariable.Values.Count);
            Assert.AreEqual(2, stationVariable.Values.Count);

            // check if meteo stations are also added to model meteo stations
            meteoStations.Received(1).Add("A");
            meteoStations.Received(1).Add("B");

            timeVariable = evaporationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));
            stationVariable = evaporationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(string));

            Assert.NotNull(timeVariable, "Missing evaporation time variable");
            Assert.NotNull(stationVariable, "Missing evaporation station variable");

            Assert.AreEqual(26, timeVariable.Values.Count);
            Assert.AreEqual(2, stationVariable.Values.Count);

            timeVariable = temperatureMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));
            stationVariable = temperatureMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(string));

            Assert.NotNull(timeVariable, "Missing evaporation time variable");
            Assert.NotNull(stationVariable, "Missing evaporation station variable");

            Assert.AreEqual(25, timeVariable.Values.Count);
            Assert.AreEqual(2, stationVariable.Values.Count);

            // check if temperature stations are also added to model temperature stations
            temperatureStations.Received(1).Add("temp1");
            temperatureStations.Received(1).Add("temp2");
        }
    }
}
