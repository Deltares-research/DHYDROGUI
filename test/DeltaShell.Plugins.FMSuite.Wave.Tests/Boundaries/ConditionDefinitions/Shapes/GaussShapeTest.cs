using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using NSubstitute;
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

        [Test]
        public void AcceptVisitorTest()
        {
            // Setup
            var shape = new GaussShape();
            var visitor = Substitute.For<IBoundaryConditionVisitor>();

            // Call
            shape.AcceptVisitor(visitor);

            // Assert
            visitor.Received().Visit(shape);
        }
    }
}