using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Parameters
{
    [TestFixture]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class BoundaryParametersFactoryTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var factory = new BoundaryParametersFactory();

            // Assert
            Assert.That(factory, Is.InstanceOf<IBoundaryParametersFactory>());
        }

        [Test]
        public void ConstructDefaultConstantParameters_ExpectedValues()
        {
            // Setup
            var factory = new BoundaryParametersFactory();

            // Call
            ConstantParameters<TSpreading> parameters = 
                factory.ConstructDefaultConstantParameters<TSpreading>();

            // Assert
            Assert.That(parameters.Height, Is.EqualTo(0.0));
            Assert.That(parameters.Period, Is.EqualTo(1.0));
            Assert.That(parameters.Direction, Is.EqualTo(0.0));
            Assert.That(parameters.Spreading, Is.Not.Null);
            Assert.That(parameters.Spreading, Is.InstanceOf<TSpreading>());
        }

        [Test]
        public void ConstructConstantParameters_ExpectedValues()
        {
            // Setup
            var factory = new BoundaryParametersFactory();

            const double expectedHeight = 1.5;
            const double expectedPeriod = 2.5;
            const double expectedDirection = 3.5;

            var expectedSpreading = new TSpreading();

            // Call
            ConstantParameters<TSpreading> parameters =
                factory.ConstructConstantParameters(expectedHeight, 
                                                    expectedPeriod, 
                                                    expectedDirection,
                                                    expectedSpreading);

            // Assert
            Assert.That(parameters.Height, Is.EqualTo(expectedHeight));
            Assert.That(parameters.Period, Is.EqualTo(expectedPeriod));
            Assert.That(parameters.Direction, Is.EqualTo(expectedDirection));
            Assert.That(parameters.Spreading, Is.SameAs(expectedSpreading));
        }
    }
}