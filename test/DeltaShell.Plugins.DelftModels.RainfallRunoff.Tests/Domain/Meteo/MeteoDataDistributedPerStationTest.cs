using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class MeteoDataDistributedPerStationTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var splitter = Substitute.For<ITimeDependentFunctionSplitter>();

            // Call 
            var meteoDataDistributed = new MeteoDataDistributedPerStation(splitter);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(meteoDataDistributed, Is.InstanceOf<IMeteoDataDistributed>());
                Assert.That(meteoDataDistributed.Data, Is.Not.Null);
            });
        }

        [Test]
        public void Constructor_FunctionSplitterNull_ThrowsArgumentNullException()
        {
            void Call() => new MeteoDataDistributedPerStation(null);
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetTimeSeries_ExpectedResult()
        {
            // Setup
            var splitter = Substitute.For<ITimeDependentFunctionSplitter>();
            var meteoDataDistributed = new MeteoDataDistributedPerStation(splitter);
            const string item = "item";
            
            var expectedResult = Substitute.For<IFunction>();
            splitter.ExtractSeriesForArgumentValue(meteoDataDistributed.Data, item).Returns(expectedResult);

            // Call
            IFunction result = meteoDataDistributed.GetTimeSeries(item);

            // Assert
            Assert.That(result, Is.SameAs(expectedResult));
            splitter.Received(1).ExtractSeriesForArgumentValue(meteoDataDistributed.Data, item);
        }

        [Test]
        public void Clone_ExpectedResults()
        {
            // Setup
            var splitter = Substitute.For<ITimeDependentFunctionSplitter>();
            var meteoDataDistributed = new MeteoDataDistributedPerStation(splitter);

            // Call
            object result = meteoDataDistributed.Clone();

            // Assert
            Assert.That(result, Is.InstanceOf<MeteoDataDistributedPerStation>());
            var cloned = (MeteoDataDistributedPerStation)result;
            Assert.That(cloned, Is.Not.SameAs(meteoDataDistributed));
            Assert.That(cloned.Data, Is.Not.Null);
            Assert.That(cloned.Data, Is.Not.SameAs(meteoDataDistributed.Data));
        }
    }
}