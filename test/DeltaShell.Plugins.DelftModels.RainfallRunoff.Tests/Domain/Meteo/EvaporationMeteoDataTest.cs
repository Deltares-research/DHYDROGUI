using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
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
        }
    }
}