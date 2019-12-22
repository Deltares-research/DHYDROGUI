using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Shapes
{
    [TestFixture]
    public class JonswapShapeTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var shape = new JonswapShape();

            // Assert
            Assert.That(shape, Is.InstanceOf(typeof(JonswapShape)));
        }
    }
}