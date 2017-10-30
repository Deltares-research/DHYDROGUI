using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Converters
{
    [TestFixture]
    class XmlTimeSeriesTruncaterTest
    {
        [Test]
        public void StartTimeAndEndTimeExistInTruncatedSeries()
        {
            var startTime = DateTime.Now;
            var endTime = startTime.AddDays(1);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            var timeSeries = new TimeSeries
            {
                Components = { new Variable<double>("SetPoint") },
                Name = "SetPoint"
            };

            timeSeries[startTime] = 1.0;
            timeSeries[endTime] = 31.0;

            var xmlTimeSeries = new XmlTimeSeries
            {
                StartTime = startTime,
                EndTime = endTime,
                TimeStep = timeStep,
                TimeSeries = (TimeSeries)timeSeries.Clone()
            };

            var truncateStart = startTime.AddHours(1);
            var truncateEnd = endTime.AddHours(-1);
            XmlTimeSeriesTruncater.Truncate(xmlTimeSeries, truncateStart, truncateEnd);

            var allTimeValues = xmlTimeSeries.TimeSeries.Time.AllValues.ToList();
            Assert.IsFalse(allTimeValues.Contains(startTime));
            Assert.IsTrue(allTimeValues.Contains(truncateStart));
            Assert.IsTrue(allTimeValues.Contains(truncateEnd));
            Assert.IsFalse(allTimeValues.Contains(endTime));
        }
    }
}
