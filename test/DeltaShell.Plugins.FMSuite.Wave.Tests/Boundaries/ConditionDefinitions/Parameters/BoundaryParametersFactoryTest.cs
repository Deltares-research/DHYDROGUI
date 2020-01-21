using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Parameters
{
    [TestFixture]
    public class BoundaryParametersFactoryTest
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
            ConstantParameters parameters = factory.ConstructDefaultConstantParameters();

            // Assert
            Assert.That(parameters.Height, Is.EqualTo(0.0));
            Assert.That(parameters.Period, Is.EqualTo(0.0));
            Assert.That(parameters.Direction, Is.EqualTo(0.0));
            Assert.That(parameters.Spreading, Is.EqualTo(0.0));
        }

        [Test]
        public void ConstructConstantParameters_ExpectedValues()
        {
            // Setup
            var factory = new BoundaryParametersFactory();

            const double expectedHeight = 1.5;
            const double expectedPeriod = 2.5;
            const double expectedDirection = 3.5;
            const double expectedSpreading = 4.5;

            // Call
            ConstantParameters parameters =
                factory.ConstructConstantParameters(expectedHeight, 
                                                    expectedPeriod, 
                                                    expectedDirection,
                                                    expectedSpreading);

            // Assert
            Assert.That(parameters.Height, Is.EqualTo(expectedHeight));
            Assert.That(parameters.Period, Is.EqualTo(expectedPeriod));
            Assert.That(parameters.Direction, Is.EqualTo(expectedDirection));
            Assert.That(parameters.Spreading, Is.EqualTo(expectedSpreading));
        }
    }
}