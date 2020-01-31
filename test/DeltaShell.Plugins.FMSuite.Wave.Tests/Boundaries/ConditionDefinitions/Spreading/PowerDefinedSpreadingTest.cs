using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Spreading
{
    [TestFixture]
    public class PowerDefinedSpreadingTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var spreading = new PowerDefinedSpreading();

            // Assert
            Assert.That(spreading, Is.InstanceOf<IBoundaryConditionSpreading>());
            Assert.That(spreading.SpreadingPower, Is.EqualTo(4.0));
        }
    }
}