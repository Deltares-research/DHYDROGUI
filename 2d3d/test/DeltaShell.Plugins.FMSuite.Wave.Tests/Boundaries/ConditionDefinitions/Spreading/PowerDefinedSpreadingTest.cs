using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Spreading
{
    [TestFixture]
    public class PowerDefinedSpreadingTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var spreading = new PowerDefinedSpreading();

            // Assert
            Assert.That(spreading, Is.InstanceOf<IBoundaryConditionSpreading>());
            Assert.That(spreading.SpreadingPower, Is.EqualTo(4.0));
        }

        [Test]
        public void AcceptVisitor_VisitorNull_ThrowsArgumentNullExceptionForPowerSpreading()
        {
            // Setup
            var spreading = new PowerDefinedSpreading();

            // Call
            void Call() => spreading.AcceptVisitor(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("visitor"));
        }

        [Test]
        public void AcceptVisitor_CallsCorrectVisitorMethodForPowerSpreading()
        {
            // Setup
            var spreading = new PowerDefinedSpreading();
            var visitor = Substitute.For<ISpreadingVisitor>();

            // Call
            spreading.AcceptVisitor(visitor);

            // Assert
            visitor.Received(1).Visit(spreading);
        }
    }
}