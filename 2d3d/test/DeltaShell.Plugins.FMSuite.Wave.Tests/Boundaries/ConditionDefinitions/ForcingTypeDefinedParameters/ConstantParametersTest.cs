using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    [TestFixture]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class ConstantParametersTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        private readonly Random random = new Random(37);

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            double expectedHeight = random.NextDouble() * 1000.0;
            double expectedPeriod = random.NextDouble() * 1000.0;
            double expectedDirection = random.NextDouble() * 1000.0;
            var expectedSpreading = new TSpreading();

            // Call
            var boundaryConditionParameters = new ConstantParameters<TSpreading>(expectedHeight,
                                                                                 expectedPeriod,
                                                                                 expectedDirection,
                                                                                 expectedSpreading);

            // Assert
            Assert.That(boundaryConditionParameters, Is.InstanceOf<IForcingTypeDefinedParameters>());

            Assert.That(boundaryConditionParameters.Height, Is.EqualTo(expectedHeight),
                        "Expected a different Height:");
            Assert.That(boundaryConditionParameters.Period, Is.EqualTo(expectedPeriod),
                        "Expected a different Period:");
            Assert.That(boundaryConditionParameters.Direction, Is.EqualTo(expectedDirection),
                        "Expected a different Direction:");
            Assert.That(boundaryConditionParameters.Spreading, Is.SameAs(expectedSpreading),
                        "Expected a different Spreading:");
        }

        [Test]
        public void Constructor_SpreadingNull_ThrowsArgumentNullException()
        {
            // Setup
            double height = random.NextDouble() * 1000.0;
            double period = random.NextDouble() * 1000.0;
            double direction = random.NextDouble() * 1000.0;
            TSpreading spreading = null;

            // Call | Assert
            void Call() => new ConstantParameters<TSpreading>(height,
                                                              period,
                                                              direction,
                                                              spreading);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("spreading"));
        }

        [Test]
        public void SpreadingSet_ValueNull_ThrowsArgumentNullException()
        {
            // Setup
            double expectedHeight = random.NextDouble() * 1000.0;
            double expectedPeriod = random.NextDouble() * 1000.0;
            double expectedDirection = random.NextDouble() * 1000.0;
            var expectedSpreading = new TSpreading();

            var boundaryConditionParameters = new ConstantParameters<TSpreading>(expectedHeight,
                                                                                 expectedPeriod,
                                                                                 expectedDirection,
                                                                                 expectedSpreading);

            // Call | Assert
            void Call() => boundaryConditionParameters.Spreading = null;

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("value"));
        }

        [Test]
        public void AcceptVisitor_VisitorNull_ThrowsArgumentNullExceptionForConstantParameters()
        {
            // Setup
            var boundaryConditionParameters = new ConstantParameters<TSpreading>(random.NextDouble(),
                                                                                 random.NextDouble(),
                                                                                 random.NextDouble(),
                                                                                 new TSpreading());

            // Call
            void Call() => boundaryConditionParameters.AcceptVisitor(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("visitor"));
        }

        [Test]
        public void AcceptVisitor_CallsCorrectVisitorMethodForConstantParameters()
        {
            // Setup
            var boundaryConditionParameters = new ConstantParameters<TSpreading>(random.NextDouble(),
                                                                                 random.NextDouble(),
                                                                                 random.NextDouble(),
                                                                                 new TSpreading());

            var visitor = Substitute.For<IForcingTypeDefinedParametersVisitor>();

            // Call
            boundaryConditionParameters.AcceptVisitor(visitor);

            // Assert
            visitor.Received(1).Visit(boundaryConditionParameters);
        }
    }
}