using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Spreading
{
    [TestFixture]
    public class DegreesDefinedSpreadingTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var spreading = new DegreesDefinedSpreading();

            // Assert
            Assert.That(spreading, Is.InstanceOf<IBoundaryConditionSpreading>());
            Assert.That(spreading.DegreesSpreading, Is.EqualTo(20.0));
        }
    }
}