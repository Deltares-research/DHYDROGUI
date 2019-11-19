using System;
using System.ComponentModel;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class GridBoundaryCoordinateTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var expectedSide = random.NextEnumValue<GridSide>();
            int expectedIndex = random.Next();

            // Call
            var gridBoundaryCoordinate = new GridBoundaryCoordinate(expectedSide,
                                                                    expectedIndex);

            // Assert
            Assert.That(gridBoundaryCoordinate.GridSide, Is.EqualTo(expectedSide));
            Assert.That(gridBoundaryCoordinate.Index, Is.EqualTo(expectedIndex));
        }

        [Test]
        public void Constructor_InvalidGridSide_ThrowsInvalidEnumArgumentException()
        {
            // Call
            void Call() => new GridBoundaryCoordinate((GridSide)0, random.Next());

            // Assert
            Assert.Throws<InvalidEnumArgumentException>(Call);
        }

        [Test]
        public void Constructor_InvalidIndex_ThrowsArgumentOutOfRangeException()
        {
            // Call
            void Call() => new GridBoundaryCoordinate(random.NextEnumValue<GridSide>(),
                                                      random.Next() * -1);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(Call);
        }
    }
}