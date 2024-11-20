using System.Collections.Generic;
using System.Linq;
using DelftTools.Units;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Meteo
{
    [TestFixture]
    public class MeteoDataDistributedGlobalTest
    {
        private readonly Unit unit = new Unit("TestName", "TestSymbol");
        
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call 
            var meteoDataDistributed = new MeteoDataDistributedGlobal(new MeteoTimeSeriesInstanceCreator(), unit);

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
        public void Constructor_ArgNull_ThrowsArgumentNullException(MeteoTimeSeriesInstanceCreator meteoTimeSeriesInstanceCreator, IUnit givenUnit)
        {
            void Call() => new MeteoDataDistributedGlobal(meteoTimeSeriesInstanceCreator, givenUnit);
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        private static IEnumerable<TestCaseData> ConstructorArgNullCases()
        {
            yield return new TestCaseData(null, new Unit());
            yield return new TestCaseData(new MeteoTimeSeriesInstanceCreator(), null);
        }
    }
}