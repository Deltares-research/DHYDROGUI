using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.CommonTools.Functions;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class TimeSeriesGeneratorTest
    {
        [Test]
        public void GenerateSeries()
        {
            var timeSeries = new TimeSeries();

            var generator = new TimeSeriesGenerator();

            generator.GenerateTimeSeries(timeSeries, new DateTime(2001, 1, 1), new DateTime(2002, 1, 1),
                                         new TimeSpan(1, 0, 0, 0));

            Assert.AreEqual(366, timeSeries.Time.Values.Count);
        }

        [Test]
        public void ExpandSeries()
        {
            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>("value"));

            var generator = new TimeSeriesGenerator();

            var originalStartTime = new DateTime(2001, 3, 1);
            var originalEndTime = new DateTime(2001, 6, 1);
            
            generator.GenerateTimeSeries(timeSeries, originalStartTime, originalEndTime, new TimeSpan(1, 0, 0, 0));
            Assert.AreEqual(93, timeSeries.Time.Values.Count);

            //fill with values (that we dont' want to lose):
            timeSeries.Components[0].SetValues(Enumerable.Range(0, timeSeries.Time.Values.Count).Select(i => (double) i));
            
            generator.ResizeTimeSeries(timeSeries, new DateTime(2001, 1, 1), new DateTime(2002, 1, 1));
            Assert.AreEqual(366, timeSeries.Time.Values.Count);

            //assert values are not lost
            Assert.AreEqual(0.0, timeSeries.Components[0].Values[59]);
            Assert.AreEqual(1.0, timeSeries.Components[0].Values[60]);
            Assert.AreEqual(2.0, timeSeries.Components[0].Values[61]);
        }

        [Test]
        public void ResizeSeriesToSameSize()
        {
            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>("value"));

            var generator = new TimeSeriesGenerator();

            var originalStartTime = new DateTime(2001, 1, 1);
            var originalEndTime = new DateTime(2002, 1, 1);

            generator.GenerateTimeSeries(timeSeries, originalStartTime, originalEndTime, new TimeSpan(1, 0, 0, 0));
            Assert.AreEqual(366, timeSeries.Time.Values.Count);

            //fill with values (that we dont' want to lose):
            var values = Enumerable.Range(0, timeSeries.Time.Values.Count).Select(i => (double)i).ToList();
            timeSeries.Components[0].SetValues(values);

            generator.ResizeTimeSeries(timeSeries, new DateTime(2001, 1, 1), new DateTime(2002, 1, 1));
            Assert.AreEqual(366, timeSeries.Time.Values.Count);
            Assert.AreEqual(values, timeSeries.Components[0].Values);
        }

        [Test]
        public void ResizeSeries()
        {
            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>("value"));

            var generator = new TimeSeriesGenerator();

            var originalStartTime = new DateTime(2001, 1, 1);
            var originalEndTime = new DateTime(2002, 1, 1);

            generator.GenerateTimeSeries(timeSeries, originalStartTime, originalEndTime, new TimeSpan(1, 0, 0, 0));
            Assert.AreEqual(366, timeSeries.Time.Values.Count);

            //fill with values (that we dont' want to lose):
            timeSeries.Components[0].SetValues(Enumerable.Range(0, timeSeries.Time.Values.Count).Select(i => (double)i));

            generator.ResizeTimeSeries(timeSeries, new DateTime(2001, 3, 1), new DateTime(2002, 1, 1));
            Assert.AreEqual(307, timeSeries.Time.Values.Count);
            Assert.AreEqual(59.0, timeSeries.Components[0].Values[0]);
        }
    }
}