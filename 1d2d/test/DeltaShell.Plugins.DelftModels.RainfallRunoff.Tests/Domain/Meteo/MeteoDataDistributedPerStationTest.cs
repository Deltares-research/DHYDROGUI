using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Units;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class MeteoDataDistributedPerStationTest
    {
        private readonly Unit unit = new Unit("TestName", "TestSymbol");
        
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var splitter = Substitute.For<ITimeDependentFunctionSplitter>();

            // Call 
            var meteoDataDistributed = new MeteoDataDistributedPerStation(splitter, unit);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(meteoDataDistributed, Is.InstanceOf<IMeteoDataDistributed>());
                Assert.That(meteoDataDistributed.Data, Is.Not.Null);
                IUnit meteoUnit = meteoDataDistributed.Data.Components.First().Unit;
                Assert.That(meteoUnit.Name, Is.EqualTo(unit.Name));
                Assert.That(meteoUnit.Symbol, Is.EqualTo(unit.Symbol));
            });
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(ITimeDependentFunctionSplitter splitter, IUnit givenUnit)
        {
            void Call() => new MeteoDataDistributedPerStation(splitter, givenUnit);
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetTimeSeries_ExpectedResult()
        {
            // Setup
            var splitter = Substitute.For<ITimeDependentFunctionSplitter>();
            var meteoDataDistributed = new MeteoDataDistributedPerStation(splitter, unit);
            const string item = "item";
            
            var expectedResult = Substitute.For<IFunction>();
            splitter.ExtractSeriesForArgumentValue(meteoDataDistributed.Data, item).Returns(expectedResult);

            // Call
            IFunction result = meteoDataDistributed.GetTimeSeries(item);

            // Assert
            Assert.That(result, Is.SameAs(expectedResult));
            splitter.Received(1).ExtractSeriesForArgumentValue(meteoDataDistributed.Data, item);
            IUnit meteoUnit = meteoDataDistributed.Data.Components.First().Unit;
            Assert.That(meteoUnit.Name, Is.EqualTo(unit.Name));
            Assert.That(meteoUnit.Symbol, Is.EqualTo(unit.Symbol));
        }

        [Test]
        public void Clone_ExpectedResults()
        {
            // Setup
            var splitter = Substitute.For<ITimeDependentFunctionSplitter>();
            var meteoDataDistributed = new MeteoDataDistributedPerStation(splitter, unit);

            // Call
            object result = meteoDataDistributed.Clone();

            // Assert
            Assert.That(result, Is.InstanceOf<MeteoDataDistributedPerStation>());
            var cloned = (MeteoDataDistributedPerStation)result;
            Assert.That(cloned, Is.Not.SameAs(meteoDataDistributed));
            Assert.That(cloned.Data, Is.Not.Null);
            Assert.That(cloned.Data, Is.Not.SameAs(meteoDataDistributed.Data));
            IUnit meteoUnit = meteoDataDistributed.Data.Components.First().Unit;
            Assert.That(meteoUnit.Name, Is.EqualTo(unit.Name));
            Assert.That(meteoUnit.Symbol, Is.EqualTo(unit.Symbol));
        }
        
        private static IEnumerable<TestCaseData> ConstructorArgNullCases()
        {
            yield return new TestCaseData(null, new Unit());
            yield return new TestCaseData(Substitute.For<ITimeDependentFunctionSplitter>(), null);
        }
    }
}