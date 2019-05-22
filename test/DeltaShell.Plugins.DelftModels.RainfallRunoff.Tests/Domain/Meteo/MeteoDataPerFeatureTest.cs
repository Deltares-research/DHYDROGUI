using System;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class MeteoDataPerFeatureTest
    {
        [Test]
        public void GetMeteoForPeriodForOneFeature()
        {
            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
            {
                DataDistributionType = MeteoDataDistributionType.PerFeature
            };

            var catchment1 = new Catchment { Name = "Catchment1" };
            var catchment2 = new Catchment { Name = "Catchment2" };

            ((IFeatureCoverage)meteoData.Data).Features.Add(catchment1);
            ((IFeatureCoverage)meteoData.Data).FeatureVariable.Values.Add(catchment1);

            ((IFeatureCoverage)meteoData.Data).Features.Add(catchment2);
            ((IFeatureCoverage)meteoData.Data).FeatureVariable.Values.Add(catchment2);

            var firstDay = DateTime.Now;
            var secondDay = firstDay.AddDays(1);

            var secondHour = firstDay.AddHours(1);
            var thirdHour = firstDay.AddHours(2);

            meteoData.Data[firstDay, catchment1] = 24.0; //mm
            meteoData.Data[secondDay, catchment1] = 24.0; //mm
            meteoData.Data[firstDay, catchment2] = 48.0; //mm
            meteoData.Data[secondDay, catchment2] = 48.0; //mm

            var mm = meteoData.GetMeteoForPeriod(secondHour, thirdHour,
                                                                                 new TimeSpan(0, 1, 0, 0), catchment2);

            Assert.AreEqual(2.0, mm[0]);
        }
    }
}
