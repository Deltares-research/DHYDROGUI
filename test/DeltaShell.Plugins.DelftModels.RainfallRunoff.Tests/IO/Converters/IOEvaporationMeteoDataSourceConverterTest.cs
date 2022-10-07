using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Converters
{
    [TestFixture]
    public class IOEvaporationMeteoDataSourceConverterTest
    {
        [Test]
        public void FromIOMeteoDataSource_IOEvaporationMeteoDataSourceUndefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var converter = new IOEvaporationMeteoDataSourceConverter();

            // Call
            void Call() => converter.FromIOMeteoDataSource((IOEvaporationMeteoDataSource)99);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<InvalidEnumArgumentException>()
                                    .With.Message.EqualTo("ioEvaporationMeteoDataSource"));
        }

        [Test]
        [TestCase(IOEvaporationMeteoDataSource.UserDefined, MeteoDataSource.UserDefined)]
        [TestCase(IOEvaporationMeteoDataSource.LongTermAverage, MeteoDataSource.LongTermAverage)]
        [TestCase(IOEvaporationMeteoDataSource.GuidelineSewerSystems, MeteoDataSource.GuidelineSewerSystems)]
        public void FromIOMeteoDataSource_ReturnsTheCorrectResult(IOEvaporationMeteoDataSource ioEvaporationMeteoDataSource, MeteoDataSource expResult)
        {
            // Setup
            var converter = new IOEvaporationMeteoDataSourceConverter();

            // Call
            MeteoDataSource result = converter.FromIOMeteoDataSource(ioEvaporationMeteoDataSource);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        public void ToIOMeteoDataSource_EvaporationMeteoDataSourceUndefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var converter = new IOEvaporationMeteoDataSourceConverter();

            // Call
            void Call() => converter.ToIOMeteoDataSource((MeteoDataSource)99);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<InvalidEnumArgumentException>()
                                    .With.Message.EqualTo("evaporationMeteoDataSource"));
        }

        [Test]
        [TestCase(MeteoDataSource.UserDefined, IOEvaporationMeteoDataSource.UserDefined)]
        [TestCase(MeteoDataSource.LongTermAverage, IOEvaporationMeteoDataSource.LongTermAverage)]
        [TestCase(MeteoDataSource.GuidelineSewerSystems, IOEvaporationMeteoDataSource.GuidelineSewerSystems)]
        public void ToIOMeteoDataSource_ReturnsTheCorrectResult(MeteoDataSource meteoDataSource, IOEvaporationMeteoDataSource expResult)
        {
            // Setup
            var converter = new IOEvaporationMeteoDataSourceConverter();

            // Call
            IOEvaporationMeteoDataSource result = converter.ToIOMeteoDataSource(meteoDataSource);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }
    }
}