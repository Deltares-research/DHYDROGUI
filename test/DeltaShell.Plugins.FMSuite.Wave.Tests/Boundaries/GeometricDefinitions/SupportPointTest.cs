using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NUnit.Framework;
using System;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class SupportPointTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_InvalidDistance_ThrowsArgumentOutOfRangeException()
        {
            // Call
            void Call() => new SupportPoint(random.NextDouble() * -1);

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("distance"));
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            double expectedDistance = random.NextDouble();

            // Call
            var supportPoint = new SupportPoint(expectedDistance);

            // Assert
            Assert.That(supportPoint.Distance, Is.EqualTo(expectedDistance));
        }
    }
}
