using System;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class MeteoDataTest
    {
        [Test]
        public void ChangeMeteoDataType()
        {
            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
                {
                    DataDistributionType = MeteoDataDistributionType.Global
                };
            
            Assert.AreEqual(1, meteoData.Data.Arguments.Count);
            Assert.IsNotNull(meteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime)));
            Assert.AreEqual(1, meteoData.Data.Components.Count);

            meteoData.DataDistributionType = MeteoDataDistributionType.PerFeature;
            
            Assert.AreEqual(2, meteoData.Data.Arguments.Count);
            Assert.IsNotNull(meteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime)));
            Assert.IsNotNull(meteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof(IFeature)));
            Assert.AreEqual(1, meteoData.Data.Components.Count);
        }
    }
}
