using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Shapes
{
    [TestFixture]
    public class PiersonMoskowitzShapeTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var shape = new PiersonMoskowitzShape();

            // Assert
            Assert.That(shape, Is.InstanceOf(typeof(IBoundaryConditionShape)));
        }

        [Test]
        public void AcceptVisitor_VisitorNull_ThrowsArgumentNullExceptionForPiersonMoskowitzShape()
        {
            // Setup
            var shape = new GaussShape();

            // Call
            void Call() => shape.AcceptVisitor(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("visitor"));
        }

        [Test]
        public void AcceptVisitor_CallsCorrectVisitorMethodForPiersonMoskowitzShape()
        {
            // Setup
            var shape = new PiersonMoskowitzShape();
            var visitor = Substitute.For<IShapeVisitor>();

            // Call
            shape.AcceptVisitor(visitor);

            // Assert
            visitor.Received(1).Visit(shape);
        }
    }
}