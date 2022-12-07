using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class EvaporationMeteoDataTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var evaporationMeteoData = new EvaporationMeteoData();

            // Assert
            Assert.That(evaporationMeteoData.Name, Is.EqualTo(RainfallRunoffModelDataSet.EvaporationName));
            Assert.That(evaporationMeteoData.DataAggregationType, Is.EqualTo(MeteoDataAggregationType.Cumulative));
            Assert.That(evaporationMeteoData.SelectedMeteoDataSource, Is.EqualTo(MeteoDataSource.UserDefined));
            Assert.That(evaporationMeteoData.DataDistributionType, Is.EqualTo(MeteoDataDistributionType.Global));
            Assert.That(evaporationMeteoData.Data.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Constant));
            Assert.That(evaporationMeteoData.Data.Components[0].Unit.Symbol, Is.EqualTo("mm"));
            
            Assert.AreEqual(1, evaporationMeteoData.Data.Arguments.Count);
            Assert.IsNotNull(evaporationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime)));
            Assert.AreEqual(1, evaporationMeteoData.Data.Components.Count);

            evaporationMeteoData.DataDistributionType = MeteoDataDistributionType.PerFeature;
            
            Assert.AreEqual(2, evaporationMeteoData.Data.Arguments.Count);
            Assert.IsNotNull(evaporationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime)));
            Assert.IsNotNull(evaporationMeteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(IFeature)));
            Assert.AreEqual(1, evaporationMeteoData.Data.Components.Count);
        }
    }
}