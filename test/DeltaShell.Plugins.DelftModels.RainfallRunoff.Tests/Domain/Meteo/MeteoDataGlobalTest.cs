using System;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class MeteoDataGlobalTest
    {
        [Test]
        public void GetMillimetersOfFractionTimeStep()
        {
            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
                {
                    DataDistributionType = MeteoDataDistributionType.Global
                };

            var firstDay = DateTime.Now;
            var secondDay = firstDay.AddDays(1);

            var secondHour = firstDay.AddHours(1);
            var thirdHour = firstDay.AddHours(2);

            meteoData.Data[firstDay] = 48.0; //mm
            meteoData.Data[secondDay] = 48.0; //mm

            var mm = meteoData.GetMeteoForPeriod(secondHour, thirdHour,
                                                                             new TimeSpan(0, 1, 0, 0), null);

            Assert.AreEqual(2.0, mm[0]);

        }

        [Test]
        public void GetMillimetersOfMultipleTimeSteps()
        {
            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
            {
                DataDistributionType = MeteoDataDistributionType.Global
            };

            var firstHour = DateTime.Now;
            var secondHour = firstHour.AddHours(1);
            var thirdHour = firstHour.AddHours(2);
            var fourthHour = firstHour.AddHours(3);
            var fifthhHour = firstHour.AddHours(4);



            meteoData.Data[firstHour] = 10.0; //mm
            meteoData.Data[secondHour] = 5.0; //mm
            meteoData.Data[thirdHour] = 12.0; //mm
            meteoData.Data[fourthHour] = 14.0; //mm
            meteoData.Data[fifthhHour] = 8.0; //mm

            var mm = meteoData.GetMeteoForPeriod(secondHour, fourthHour,
                                                                             new TimeSpan(0, 2, 0, 0), null);

            const double expected_mm = (5.0 + 12.0);
            Assert.AreEqual(expected_mm, mm[0]);
        }



        [Test]
        public void GetMillimetersOfMultipleTimeStepsAndTwoFractions()
        {
            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
            {
                DataDistributionType = MeteoDataDistributionType.Global
            };

            var firstHour = DateTime.Now;
            var secondHour = firstHour.AddHours(1);
            var secondHourAndAHalf = firstHour.AddHours(1.5);
            var thirdHour = firstHour.AddHours(2);
            var fourthHour = firstHour.AddHours(3);
            var fourthHourAndAHalf = firstHour.AddHours(3.5);
            var fifthhHour = firstHour.AddHours(4);



            meteoData.Data[firstHour] = 10.0; //mm
            meteoData.Data[secondHour] = 5.0; //mm
            meteoData.Data[thirdHour] = 12.0; //mm
            meteoData.Data[fourthHour] = 14.0; //mm
            meteoData.Data[fifthhHour] = 8.0; //mm

            var mm = meteoData.GetMeteoForPeriod(secondHourAndAHalf, fourthHourAndAHalf,
                                                                             new TimeSpan(0, 2, 0, 0), null);

            const double expected_mm = (5.0/2.0 + 12.0 + 14.0/2.0);
            Assert.AreEqual(expected_mm, mm[0]);
        }
    }
}
