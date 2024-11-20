using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class GridCoordinateTest
    {
        private readonly Random random = new Random(17);

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            int expectedX = random.Next();
            int expectedY = random.Next();

            // Call
            var coordinate = new GridCoordinate(expectedX, expectedY);

            // Assert
            Assert.That(coordinate.X, Is.EqualTo(expectedX));
            Assert.That(coordinate.Y, Is.EqualTo(expectedY));
        }

        [Test]
        [TestCase(true, false, "x")]
        [TestCase(false, true, "y")]
        public void Constructor_CallingNegativeX_ThrowsArgumentOutOfRangeException(bool IsNegativeX, bool IsNegativeY, string expectedParamName)
        {
            // Setup 
            int xVal = random.Next(1, int.MaxValue) * (IsNegativeX ? -1 : 1);
            int yVal = random.Next(1, int.MaxValue) * (IsNegativeY ? -1 : 1);

            // Call
            void Call() => new GridCoordinate(xVal, yVal);

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);

            Assert.That(exception, Has.Property("ParamName").EqualTo(expectedParamName));
        }
    }
}