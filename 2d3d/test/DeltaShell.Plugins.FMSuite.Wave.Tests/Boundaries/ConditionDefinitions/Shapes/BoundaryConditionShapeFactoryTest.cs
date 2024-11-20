using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Shapes
{
    [TestFixture]
    public class BoundaryConditionShapeFactoryTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var factory = new BoundaryConditionShapeFactory();

            // Assert
            Assert.That(factory, Is.InstanceOf<IBoundaryConditionShapeFactory>());
        }

        [Test]
        public void ConstructDefaultGaussShape_ReturnsExpectedValue()
        {
            // Setup
            var factory = new BoundaryConditionShapeFactory();

            // Call
            GaussShape result = factory.ConstructDefaultGaussShape();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GaussianSpread, Is.EqualTo(0.1));
        }

        [Test]
        public void ConstructDefaultJonswapShape_ReturnsExpectedValue()
        {
            // Setup
            var factory = new BoundaryConditionShapeFactory();

            // Call
            JonswapShape result = factory.ConstructDefaultJonswapShape();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PeakEnhancementFactor, Is.EqualTo(3.3));
        }

        [Test]
        public void ConstructDefaultPiersonMoskowitzShape_ReturnsExpectedValue()
        {
            // Setup
            var factory = new BoundaryConditionShapeFactory();

            // Call
            PiersonMoskowitzShape result = factory.ConstructDefaultPiersonMoskowitzShape();

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}