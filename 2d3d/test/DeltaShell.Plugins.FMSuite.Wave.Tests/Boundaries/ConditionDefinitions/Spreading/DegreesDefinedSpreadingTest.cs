using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NSubstitute;
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

        [Test]
        public void AcceptVisitor_VisitorNull_ThrowsArgumentNullExceptionForDegreesSpreading()
        {
            // Setup
            var spreading = new DegreesDefinedSpreading();

            // Call
            void Call() => spreading.AcceptVisitor(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("visitor"));
        }

        [Test]
        public void AcceptVisitor_CallsCorrectVisitorMethodForDegreesSpreading()
        {
            // Setup
            var spreading = new DegreesDefinedSpreading();
            var visitor = Substitute.For<ISpreadingVisitor>();

            // Call
            spreading.AcceptVisitor(visitor);

            // Assert
            visitor.Received(1).Visit(spreading);
        }
    }
}