using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class SupportPointTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_InvalidDistance_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            // Call
            void Call() => new SupportPoint(random.NextDouble() * -1, geometricDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("distance"));
        }

        [Test]
        public void Constructor_GeometricDefinitionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SupportPoint(random.NextDouble(), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometricDefinition"));
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            double expectedDistance = random.NextDouble();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            // Call
            var supportPoint = new SupportPoint(expectedDistance, geometricDefinition);

            // Assert
            Assert.That(supportPoint.Distance, Is.EqualTo(expectedDistance));
            Assert.That(supportPoint.GeometricDefinition, Is.SameAs(geometricDefinition));
        }

        [Test]
        public void SetDistance_InvalidDistance_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            // Call
            var supportPoint = new SupportPoint(0, geometricDefinition);

            void Call() => supportPoint.Distance = -1 * random.NextDouble();
            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("distance"));
        }
    }
}