using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroTimeSeriesFactoryTest
    {
        [Test]
        public void CreateTimeSeries()
        {
            TimeSeries timeSeries = HydroTimeSeriesFactory.CreateTimeSeries("Series name", "Component name", "unit");

            Assert.AreEqual("Series name", timeSeries.Name);
            Assert.AreEqual(new DateTime(2000, 1, 1), timeSeries.Time.DefaultValue);
            Assert.AreEqual(InterpolationType.Linear, timeSeries.Time.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, timeSeries.Time.ExtrapolationType);
            Assert.AreEqual("Component name", timeSeries.Components[0].Name);
            Assert.AreEqual("unit", timeSeries.Components[0].Unit.Symbol);
        }
    }
}