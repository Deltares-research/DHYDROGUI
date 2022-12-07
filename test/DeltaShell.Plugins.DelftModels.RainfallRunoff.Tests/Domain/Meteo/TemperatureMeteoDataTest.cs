using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class TemperatureMeteoDataTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var temperatureMeteoData = new TemperatureMeteoData();

            // Assert
            Assert.That(temperatureMeteoData.Name, Is.EqualTo(RainfallRunoffModelDataSet.TemperatureName));
            Assert.That(temperatureMeteoData.DataAggregationType, Is.EqualTo(MeteoDataAggregationType.NonCumulative));
            Assert.That(temperatureMeteoData.DataDistributionType, Is.EqualTo(MeteoDataDistributionType.Global));
            Assert.That(temperatureMeteoData.Data.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Constant));
            Assert.That(temperatureMeteoData.Data.Components[0].Unit.Symbol, Is.EqualTo("°C"));
            
            Assert.AreEqual(1, temperatureMeteoData.Data.Arguments.Count);
            Assert.IsNotNull(temperatureMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime)));
            Assert.AreEqual(1, temperatureMeteoData.Data.Components.Count);

            temperatureMeteoData.DataDistributionType = MeteoDataDistributionType.PerFeature;
            
            Assert.AreEqual(2, temperatureMeteoData.Data.Arguments.Count);
            Assert.IsNotNull(temperatureMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime)));
            Assert.IsNotNull(temperatureMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(IFeature)));
            Assert.AreEqual(1, temperatureMeteoData.Data.Components.Count);
        }
    }
}