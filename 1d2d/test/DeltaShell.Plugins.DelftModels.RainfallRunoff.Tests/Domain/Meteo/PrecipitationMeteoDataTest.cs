using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class PrecipitationMeteoDataTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var precipitationMeteoData = new PrecipitationMeteoData();

            // Assert
            Assert.That(precipitationMeteoData.Name, Is.EqualTo(RainfallRunoffModelDataSet.PrecipitationName));
            Assert.That(precipitationMeteoData.DataAggregationType, Is.EqualTo(MeteoDataAggregationType.Cumulative));
            Assert.That(precipitationMeteoData.DataDistributionType, Is.EqualTo(MeteoDataDistributionType.Global));
            Assert.That(precipitationMeteoData.Data.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Constant));
            Assert.That(precipitationMeteoData.Data.Components[0].Unit.Symbol, Is.EqualTo("mm"));
            
            Assert.AreEqual(1, precipitationMeteoData.Data.Arguments.Count);
            Assert.IsNotNull(precipitationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime)));
            Assert.AreEqual(1, precipitationMeteoData.Data.Components.Count);

            precipitationMeteoData.DataDistributionType = MeteoDataDistributionType.PerFeature;
            
            Assert.AreEqual(2, precipitationMeteoData.Data.Arguments.Count);
            Assert.IsNotNull(precipitationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime)));
            Assert.IsNotNull(precipitationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(IFeature)));
            Assert.AreEqual(1, precipitationMeteoData.Data.Components.Count);
        }
    }
}