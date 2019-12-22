using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Shapes
{
    [TestFixture]
    public class GaussShapeTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var shape = new GaussShape();

            // Assert
            Assert.That(shape, Is.InstanceOf(typeof(IBoundaryConditionShape)));
        }
    }
}