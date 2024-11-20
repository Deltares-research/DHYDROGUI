using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using NSubstitute;
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
            Assert.That(shape, Is.InstanceOf(typeof(IBoundaryConditionShape)));
        }

        [Test]
        public void AcceptVisitor_VisitorNull_ThrowsArgumentNullExceptionForJonswapShape()
        {
            // Setup
            var shape = new JonswapShape();

            // Call
            void Call() => shape.AcceptVisitor(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("visitor"));
        }

        [Test]
        public void AcceptVisitor_CallsCorrectVisitorMethodForJonswapShape()
        {
            // Setup
            var shape = new JonswapShape();
            var visitor = Substitute.For<IShapeVisitor>();

            // Call
            shape.AcceptVisitor(visitor);

            // Assert
            visitor.Received(1).Visit(shape);
        }
    }
}