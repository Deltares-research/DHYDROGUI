using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class TimeSeriesHelperTests
    {
        private IFunction timeSerie;

        [SetUp]
        public void SetUp()
        {
            var time = new Variable<DateTime>
                           {
                               Values =
                                   {
                                       new DateTime(2011, 1, 1, 0, 0, 0),
                                       new DateTime(2011, 1, 1, 12, 0, 0),
                                       new DateTime(2011, 1, 2, 0, 0, 0),
                                       new DateTime(2011, 1, 2, 12, 0, 0),
                                       new DateTime(2011, 1, 3, 0, 0, 0)
                                   }
                           };

            var values = new Variable<int>
                             {
                                 Values =
                                     {
                                         1,
                                         2,
                                         3,
                                         4,
                                         5
                                     }
                             };

            timeSerie = new Function { Components = { values }, Arguments = { time } };
        }

        [Test]
        public void PeriodicExtrapolationWithSobekPeriodSameLengthAsTimeSerieDefinedInSeconds()
        {
            Assert.AreEqual(ExtrapolationType.None, timeSerie.Arguments[0].ExtrapolationType);

            //PDIN 0 1 '172800' pdin
            TimeSeriesHelper.SetPeriodicExtrapolationSobek(timeSerie, "172800");

            Assert.AreEqual(ExtrapolationType.Periodic, timeSerie.Arguments[0].ExtrapolationType);
        }

        [Test]
        public void PeriodicExtrapolationWithSobekPeriodSameLengthAsTimeSerieDefinedInDays()
        {
            Assert.AreEqual(ExtrapolationType.None, timeSerie.Arguments[0].ExtrapolationType);

            //PDIN 0 1 '2;00:00:00' pdin
            TimeSeriesHelper.SetPeriodicExtrapolationSobek(timeSerie, "2;00:00:00");

            Assert.AreEqual(ExtrapolationType.Periodic, timeSerie.Arguments[0].ExtrapolationType);
        }

        [Test]
        public void PeriodicExtrapolationWithSobekPeriodLongerPeriodAsTimeSerie()
        {
            var seconds = 200000;
            Assert.AreEqual(ExtrapolationType.None, timeSerie.Arguments[0].ExtrapolationType);

            //PDIN 0 1 '172800' pdin
            TimeSeriesHelper.SetPeriodicExtrapolationSobek(timeSerie, seconds.ToString());

            Assert.AreEqual(ExtrapolationType.Periodic, timeSerie.Arguments[0].ExtrapolationType);

            var addedValue = ((DateTime) timeSerie.Arguments[0].MinValue).Add(new TimeSpan(0, 0, seconds));

            Assert.AreEqual(addedValue,(DateTime)timeSerie.Arguments[0].MaxValue);
        }


        [Test]
        public void PeriodicExtrapolationWithSobekPeriodShorterPeriodAsTimeSerie()
        {
            var seconds = 100000;
            Assert.AreEqual(ExtrapolationType.None, timeSerie.Arguments[0].ExtrapolationType);

            TimeSeriesHelper.SetPeriodicExtrapolationSobek(timeSerie, seconds.ToString());

            //Not supported yet: extrapolation will be set to None and a log message will be sent
            Assert.AreEqual(ExtrapolationType.None, timeSerie.Arguments[0].ExtrapolationType);


        }
    }
}
