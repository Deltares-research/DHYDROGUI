using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class MeteoDataTimeIntegratorTest
    {
        [Test]
        public void GetMillimetersOfFractionTimeStepAfterLastTimestep()
        {
            var baseDate = DateTime.Now;
            var secondDay = baseDate.AddDays(1);

            var times = new[] { baseDate, secondDay };
            var values = new[] {48.0, 48.0};

            var mm1 = MeteoDataTimeIntegrator.GetIntegralPerPeriod(times, values, secondDay, baseDate.AddDays(2), false);
            Assert.AreEqual(48.0, mm1, "mm1");

            var mm2 = MeteoDataTimeIntegrator.GetIntegralPerPeriod(times, values, secondDay, baseDate.AddDays(1.5), false);
            Assert.AreEqual(24.0, mm2, "mm2");
        }

        [Test]
        public void GetMillimetersOfOneAndAHalfFractionTimeStep()
        {
            var baseDate = DateTime.Now;

            var times = new[] {baseDate, baseDate.AddDays(1), baseDate.AddDays(2)};
            var values = new[] { 48.0, 48.0, 48.0 };

            var mm2 = MeteoDataTimeIntegrator.GetIntegralPerPeriod(times, values, baseDate.AddDays(1), baseDate.AddDays(1.5), false);
            Assert.AreEqual(24.0, mm2, "mm2");
        }

        [Test]
        public void GetMillimetersOfFractionTimeStep()
        {
            var baseDate = DateTime.Now;

            var values = new[] { 48.0, 48.0 };
            var times = new[] {baseDate, baseDate.AddDays(1)};

            var mm = MeteoDataTimeIntegrator.GetIntegralPerPeriod(times, values, baseDate.AddHours(1), baseDate.AddHours(2), false);

            Assert.AreEqual(2.0, mm);
        }
        
        [Test]
        public void GetMillimetersOfMultipleTimeSteps()
        {
            var baseDate = DateTime.Now;

            var times = new[] { baseDate, baseDate.AddHours(1), baseDate.AddHours(2), baseDate.AddHours(3), baseDate.AddHours(4)};
            var values = new[] {10.0, 5.0, 12.0, 14.0, 8.0};

            var mm = MeteoDataTimeIntegrator.GetIntegralPerPeriod(times, values, baseDate.AddHours(1), baseDate.AddHours(3), false);

            Assert.AreEqual(5.0 + 12.0, mm);
        }
        
        [Test]
        public void GetMillimetersOfMultipleTimeStepsAndTwoFractions()
        {
            var baseDate = DateTime.Now;

            var times = new[] { baseDate, baseDate.AddHours(1), baseDate.AddHours(2), baseDate.AddHours(3), baseDate.AddHours(4) };
            var values = new[] { 10.0, 5.0, 12.0, 14.0, 8.0 };

            var mm = MeteoDataTimeIntegrator.GetIntegralPerPeriod(times, values, baseDate.AddHours(1.5), baseDate.AddHours(3.5), false);
            Assert.AreEqual(5.0 / 2.0 + 12.0 + 14.0 / 2.0, mm);
        }

        [Test]
        public void GetMillimetersOfPeriodWhereExtrapolationIsNeeded()
        {
            var baseDate = DateTime.Now;

            var times = new[] { baseDate, baseDate.AddHours(1), baseDate.AddHours(2) };
            var values = new[] { 10.0, 20.0, 30.0 };

            const bool isPeriodic = true;
            var mm = MeteoDataTimeIntegrator.GetIntegralPerPeriod(times, values, baseDate.AddHours(3), baseDate.AddHours(4), isPeriodic);
            Assert.AreEqual(10.0, mm, 0.00001);

            mm = MeteoDataTimeIntegrator.GetIntegralPerPeriod(times, values, baseDate.AddHours(2), baseDate.AddHours(4), isPeriodic);
            Assert.AreEqual(40.0, mm, 0.00001);  //10 + 20
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void CalculateNonPeriodicTimeseriesWithTimeStepDifferentToSeriesTimeStep()
        {
            var baseDateTime = new DateTime(2000, 01, 01);
            var timeSpan = new TimeSpan(0, 0, 15);
            
            const int numberOfValues = 100000;

            var times = Enumerable.Range(0, numberOfValues).Select(n => baseDateTime.Add(new TimeSpan(timeSpan.Ticks * n))).ToList();
            var values = Enumerable.Repeat(3.0, numberOfValues).ToList();

            var lastTimeValue = times.ElementAt(numberOfValues - 1);

            TestHelper.AssertIsFasterThan(120, () => MeteoDataTimeIntegrator.GetMeteoForPeriodInternal(baseDateTime, lastTimeValue, new TimeSpan(0, 1, 0), times, values, false));
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void CalculatePeriodicTimeseriesWithTimeStepDifferentToSeriesTimeStep()
        {
            var baseDateTime = new DateTime(2000, 01, 01);
            var timeSpan = new TimeSpan(0, 0, 15);
            
            const int numberOfValues = 100000;
            var times = Enumerable.Range(0, numberOfValues).Select(n => baseDateTime.Add(new TimeSpan(timeSpan.Ticks * n))).ToList();
            var values = Enumerable.Repeat(3.0, numberOfValues).ToList();
           
            var lastTimeValue = times[numberOfValues - 1];

            TestHelper.AssertIsFasterThan(120, () =>
                                          MeteoDataTimeIntegrator.GetMeteoForPeriodInternal(baseDateTime.AddYears(1),
                                                                                  lastTimeValue.AddYears(1),
                                                                                  new TimeSpan(0, 1, 0), times, values,
                                                                                  true));
        }

        [Test]
        public void TestGetIndex()
        {
            var baseDateTime = new DateTime(2000, 01, 01);
            var timeSpan = new TimeSpan(1, 0, 0);

            const int numberOfValues = 3;
            var times = Enumerable.Range(0, numberOfValues).Select(n => baseDateTime.Add(new TimeSpan(timeSpan.Ticks * n))).ToList();
            var values = Enumerable.Range(0, numberOfValues).Select(n => (double) n).ToList();

            double fraction;
            Assert.AreEqual(1, MeteoDataTimeIntegrator.GetIndexFor(baseDateTime.Add(timeSpan), times, false, out fraction));

            // with periodic time series
            Assert.AreEqual(2, MeteoDataTimeIntegrator.GetIndexFor(baseDateTime.Subtract(timeSpan), times, true, out fraction));
            Assert.AreEqual(values[0], MeteoDataTimeIntegrator.GetIndexFor(times[0], times, true, out fraction));
            Assert.AreEqual(values[values.Count - 1], MeteoDataTimeIntegrator.GetIndexFor(times[times.Count - 1], times, true, out fraction));
            Assert.AreEqual(values[values.Count - 1], MeteoDataTimeIntegrator.GetIndexFor(times[times.Count - 1].Add(new TimeSpan(0, 50, 0)), times, true, out fraction));
            Assert.AreEqual(0, MeteoDataTimeIntegrator.GetIndexFor(baseDateTime.Add(new TimeSpan(3, 0, 0)), times, true, out fraction));
            Assert.AreEqual(0, MeteoDataTimeIntegrator.GetIndexFor(baseDateTime.Add(new TimeSpan(3, 50, 0)), times, true, out fraction));
            Assert.AreEqual((5.0/6.0), fraction);
            Assert.AreEqual(1, MeteoDataTimeIntegrator.GetIndexFor(baseDateTime.Add(new TimeSpan(4, 0, 0)), times, true, out fraction));
        }

        [Test]
        public void GetMillimetersFromLeapYearData()
        {
            var times = Enumerable.Range(0, 366).Select(i => new DateTime(1960, 1, 1).AddDays(i)).ToList();
                //leap year
            var values = Enumerable.Range(0, 366).Select(i => (double) i).ToList();

            var t1 = new DateTime(2000, 2, 28);
            var t2 = new DateTime(2000, 3, 2);

            var data = MeteoDataTimeIntegrator.GetMeteoForPeriodInternal(t1, t2, new TimeSpan(1, 0, 0, 0, 0), times,
                                                                         values, true);
            Assert.AreEqual(4,data.Length);
            Assert.AreEqual(58, data[0]);
            Assert.AreEqual(59, data[1]);
            Assert.AreEqual(60, data[2]);
            Assert.AreEqual(61, data[3]);

            t1 = new DateTime(2001, 2, 28);
            t2 = new DateTime(2001, 3, 2);

            data = MeteoDataTimeIntegrator.GetMeteoForPeriodInternal(t1, t2, new TimeSpan(1, 0, 0, 0, 0), times,
                                                                         values, true);
            Assert.AreEqual(3, data.Length);
            Assert.AreEqual(58, data[0]);
            Assert.AreEqual(60, data[1]);
            Assert.AreEqual(61, data[2]);
        }

        [Test]
        public void GetMillimetersFromNonLeapYearData()
        {
            var times = Enumerable.Range(0, 365).Select(i => new DateTime(1961, 1, 1).AddDays(i)).ToList();
            //leap year
            var values = Enumerable.Range(0, 365).Select(i => (double)i).ToList();

            var t1 = new DateTime(2000, 2, 28);
            var t2 = new DateTime(2000, 3, 2);

            var data = MeteoDataTimeIntegrator.GetMeteoForPeriodInternal(t1, t2, new TimeSpan(1, 0, 0, 0, 0), times,
                                                                         values, true);
            Assert.AreEqual(4, data.Length);
            Assert.AreEqual(58, data[0]);
            Assert.AreEqual(58, data[1]);
            Assert.AreEqual(59, data[2]);
            Assert.AreEqual(60, data[3]);

            t1 = new DateTime(2001, 2, 28);
            t2 = new DateTime(2001, 3, 2);

            data = MeteoDataTimeIntegrator.GetMeteoForPeriodInternal(t1, t2, new TimeSpan(1, 0, 0, 0, 0), times,
                                                                         values, true);
            Assert.AreEqual(3, data.Length);
            Assert.AreEqual(58, data[0]);
            Assert.AreEqual(59, data[1]);
            Assert.AreEqual(60, data[2]);
        }

        [Test]
        public void GetMillimetersFromNonLeapYearDataStartingInMay()
        {
            var times = Enumerable.Range(0, 365).Select(i => new DateTime(1961, 5, 1).AddDays(i)).ToList();
            //leap year
            var values = Enumerable.Range(0, 365).Select(i => (double)((i + 120) % 365)).ToList();

            var t1 = new DateTime(2000, 2, 28);
            var t2 = new DateTime(2000, 3, 2);

            var data = MeteoDataTimeIntegrator.GetMeteoForPeriodInternal(t1, t2, new TimeSpan(1, 0, 0, 0, 0), times,
                                                                         values, true);
            Assert.AreEqual(4, data.Length);
            Assert.AreEqual(58, data[0]);
            Assert.AreEqual(58, data[1]);
            Assert.AreEqual(59, data[2]);
            Assert.AreEqual(60, data[3]);

            t1 = new DateTime(2001, 2, 28);
            t2 = new DateTime(2001, 3, 2);

            data = MeteoDataTimeIntegrator.GetMeteoForPeriodInternal(t1, t2, new TimeSpan(1, 0, 0, 0, 0), times,
                                                                         values, true);
            Assert.AreEqual(3, data.Length);
            Assert.AreEqual(58, data[0]);
            Assert.AreEqual(59, data[1]);
            Assert.AreEqual(60, data[2]);
        }

    }
}
