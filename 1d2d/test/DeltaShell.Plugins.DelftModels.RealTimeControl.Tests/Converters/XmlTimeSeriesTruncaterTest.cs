using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Converters
{
    [TestFixture]
    internal class XmlTimeSeriesTruncaterTest
    {
        [Test]
        public void StartTimeAndEndTimeExistInTruncatedSeries()
        {
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddDays(1);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            var timeSeries = new TimeSeries
            {
                Components = {new Variable<double>("SetPoint")},
                Name = "SetPoint"
            };

            timeSeries[startTime] = 1.0;
            timeSeries[endTime] = 31.0;

            var xmlTimeSeries = new XmlTimeSeries
            {
                StartTime = startTime,
                EndTime = endTime,
                TimeStep = timeStep,
                TimeSeries = (TimeSeries) timeSeries.Clone()
            };

            DateTime truncateStart = startTime.AddHours(1);
            DateTime truncateEnd = endTime.AddHours(-1);
            XmlTimeSeriesTruncater.Truncate(xmlTimeSeries, truncateStart, truncateEnd);

            List<DateTime> allTimeValues = xmlTimeSeries.TimeSeries.Time.AllValues.ToList();
            Assert.IsFalse(allTimeValues.Contains(startTime));
            Assert.IsTrue(allTimeValues.Contains(truncateStart));
            Assert.IsTrue(allTimeValues.Contains(truncateEnd));
            Assert.IsFalse(allTimeValues.Contains(endTime));
        }
    }
}