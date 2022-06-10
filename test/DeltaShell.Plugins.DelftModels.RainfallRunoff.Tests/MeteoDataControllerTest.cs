using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class MeteoDataControllerTest
    {
        /// <summary>
        /// RainfallRunoff is using MeteoDataController
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void AddCatchmentToNetworkAddsDefaultDataToPrecipitationPerCatchment()
        {
            var now = DateTime.Now;

            var rrModel = new RainfallRunoffModel
                {
                    Name = "Test",
                    Precipitation = { DataDistributionType = MeteoDataDistributionType.PerFeature }
                };

            Assert.IsAssignableFrom(typeof(FeatureCoverage), rrModel.Precipitation.Data);

            var catchment = new Catchment {Name = "c1"};
            rrModel.Basin.Catchments.Add(catchment);

            rrModel.Precipitation.Data[now, catchment] = 1.1;
            rrModel.Precipitation.Data[now.AddHours(1), catchment] = 2.2;
            rrModel.Precipitation.Data[now.AddHours(2), catchment] = 3.3;

            Assert.AreEqual(3, rrModel.Precipitation.Data.GetValues().Count);

            rrModel.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Unpaved});

            Assert.AreEqual(6, rrModel.Precipitation.Data.GetValues().Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ChangeOneMeteoDataToPerStationChangesOtherAsWell()
        {
            var rrModel = new RainfallRunoffModel
                {
                    Name = "Test",
                    Precipitation = {DataDistributionType = MeteoDataDistributionType.PerStation},
                    Evaporation = { DataDistributionType = MeteoDataDistributionType.PerStation }
            };

            Assert.AreEqual(MeteoDataDistributionType.PerStation, rrModel.Precipitation.DataDistributionType);
            Assert.AreEqual(MeteoDataDistributionType.PerStation, rrModel.Evaporation.DataDistributionType);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ChangeOneMeteoDataToPerStationBackToGlobalChangesOtherAsWell()
        {
            var rrModel = new RainfallRunoffModel
                {
                    Name = "Test",
                    Precipitation = {DataDistributionType = MeteoDataDistributionType.PerStation},
                };

            rrModel.Precipitation.DataDistributionType = MeteoDataDistributionType.Global;

            Assert.AreEqual(MeteoDataDistributionType.Global, rrModel.Precipitation.DataDistributionType);
            Assert.AreEqual(MeteoDataDistributionType.Global, rrModel.Evaporation.DataDistributionType);
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void AddMeteoStationAddsSliceToPreciptationAndEvaporation()
        {
            var rrModel = new RainfallRunoffModel
                {
                    Name = "Test",
                    Precipitation = {DataDistributionType = MeteoDataDistributionType.PerStation},
                    Evaporation = { DataDistributionType = MeteoDataDistributionType.PerStation}
                };

            rrModel.MeteoStations.Add("a");
            Assert.AreEqual(new[] { "a" }, rrModel.Precipitation.Data.Arguments[1].Values.OfType<string>().ToArray());
            Assert.AreEqual(new[] { "a" }, rrModel.Evaporation.Data.Arguments[1].Values.OfType<string>().ToArray());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemoveMeteoStationRemovesSliceFromPreciptationAndEvaporation()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };

            rrModel.MeteoStations.Add("a");
            rrModel.MeteoStations.Add("b");

            rrModel.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;
            rrModel.Evaporation.DataDistributionType = MeteoDataDistributionType.PerStation;

            Assert.AreEqual(new[] { "a", "b" }, rrModel.Precipitation.Data.Arguments[1].Values.OfType<string>().ToArray());
            Assert.AreEqual(new[] { "a", "b" }, rrModel.Evaporation.Data.Arguments[1].Values.OfType<string>().ToArray());

            rrModel.MeteoStations.Remove("a");

            Assert.AreEqual(new[] { "b" }, rrModel.Precipitation.Data.Arguments[1].Values.OfType<string>().ToArray());
            Assert.AreEqual(new[] { "b" }, rrModel.Evaporation.Data.Arguments[1].Values.OfType<string>().ToArray());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemoveMeteoStationRemovesFromCatchmentData()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };

            rrModel.MeteoStations.Add("a");
            rrModel.MeteoStations.Add("b");

            var c = Catchment.CreateDefault();
            c.CatchmentType = CatchmentType.Paved;
            rrModel.Basin.Catchments.Add(c);

            var modelData = rrModel.GetAllModelData().First();

            modelData.MeteoStationName = "a";

            rrModel.MeteoStations.Remove("a");

            Assert.AreEqual("", modelData.MeteoStationName);
        }

        /// <summary>
        /// RainfallRunoff is using MeteoDataController
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void RemoveCatchmentToNetworkRemovesPrecipitationPerCatchment()
        {
            var now = DateTime.Now;

            var rrModel = new RainfallRunoffModel
                {
                    Name = "Test",
                    Precipitation = {DataDistributionType = MeteoDataDistributionType.PerFeature}
                };

            Assert.IsAssignableFrom(typeof(FeatureCoverage), rrModel.Precipitation.Data);

            var catchment = new Catchment {CatchmentType = CatchmentType.Unpaved};
            
            rrModel.Basin.Catchments.Add(catchment);

            rrModel.Precipitation.Data[now, catchment] = 1.1;
            rrModel.Precipitation.Data[now.AddHours(1), catchment] = 2.2;
            rrModel.Precipitation.Data[now.AddHours(2), catchment] = 3.3;

            Assert.AreEqual(3, rrModel.Precipitation.Data.GetValues().Count);

            var catchment2 = new Catchment {CatchmentType = CatchmentType.Unpaved, Name = "c2"};
            
            rrModel.Basin.Catchments.Add(catchment2);

            Assert.AreEqual(6, rrModel.Precipitation.Data.GetValues().Count);

            rrModel.Basin.Catchments.Remove(catchment2);

            Assert.AreEqual(3, rrModel.Precipitation.Data.GetValues().Count);
        }

        /// <summary>
        /// RainfallRunoff is using MeteoDataController
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void RemoveCatchmentToNetworkRemovesEvaporationPerCatchment()
        {
            var now = DateTime.Now;

            var rrModel = new RainfallRunoffModel
                {
                    Name = "Test",
                    Evaporation = {DataDistributionType = MeteoDataDistributionType.PerFeature}
                };

            Assert.IsAssignableFrom(typeof(FeatureCoverage), rrModel.Evaporation.Data);

            var catchment = new Catchment {CatchmentType = CatchmentType.Unpaved};

            rrModel.Basin.Catchments.Add(catchment);

            rrModel.Evaporation.Data[now, catchment] = 1.1;
            rrModel.Evaporation.Data[now.AddHours(1), catchment] = 2.2;
            rrModel.Evaporation.Data[now.AddHours(2), catchment] = 3.3;

            Assert.AreEqual(3, rrModel.Evaporation.Data.GetValues().Count);

            var catchment2 = new Catchment {CatchmentType = CatchmentType.Unpaved, Name = "c2"};
            rrModel.Basin.Catchments.Add(catchment2);

            Assert.AreEqual(6, rrModel.Evaporation.Data.GetValues().Count);

            rrModel.Basin.Catchments.Remove(catchment2);

            Assert.AreEqual(3, rrModel.Evaporation.Data.GetValues().Count);
        }

        /// <summary>
        /// RainfallRunoff is using MeteoDataController
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void ChangeToPrecipitationPerCatchmentShouldSetTheCatchments()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };

            Assert.AreEqual(MeteoDataDistributionType.Global, rrModel.Precipitation.DataDistributionType);

            rrModel.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Unpaved, Name = "c1;"});
            rrModel.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Unpaved, Name = "c2"});
            rrModel.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Unpaved, Name = "c3"});

            rrModel.Precipitation.DataDistributionType = MeteoDataDistributionType.PerFeature;

            Assert.AreEqual(0, rrModel.Precipitation.Data.GetValues().Count);

            ((IFeatureCoverage)rrModel.Precipitation.Data).Time.Values.Add(DateTime.Now);

            Assert.AreEqual(3, rrModel.Precipitation.Data.GetValues().Count);
        }
    }
}
