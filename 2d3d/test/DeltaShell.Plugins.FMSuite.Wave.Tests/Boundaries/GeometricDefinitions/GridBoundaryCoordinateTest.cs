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
            void Call() => new GridBoundaryCoordinate((GridSide) 0, random.Next());

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

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            // Setup
            var expectedSide = random.NextEnumValue<GridSide>();
            int expectedIndex = random.Next();

            var gridBoundaryCoordinate = new GridBoundaryCoordinate(expectedSide,
                                                                    expectedIndex);

            // Call
            bool result = gridBoundaryCoordinate.Equals(null);

            // Assert
            Assert.That(result, Is.False, "Expected comparison with null to be false:");
        }

        [Test]
        public void Equals_Object_ReturnsFalse()
        {
            // Setup
            var expectedSide = random.NextEnumValue<GridSide>();
            int expectedIndex = random.Next();

            var gridBoundaryCoordinate = new GridBoundaryCoordinate(expectedSide,
                                                                    expectedIndex);

            // Call
            bool result = gridBoundaryCoordinate.Equals(new object());

            // Assert
            Assert.That(result, Is.False, "Expected comparison with a different type to be false:");
        }

        [Test]
        public void Equals_DifferentIndex_ReturnsFalse()
        {
            // Setup
            var expectedSide1 = random.NextEnumValue<GridSide>();
            int expectedIndex1 = random.Next(int.MaxValue - 1);

            var gridBoundaryCoordinate1 = new GridBoundaryCoordinate(expectedSide1,
                                                                     expectedIndex1);

            var expectedSide2 = random.NextEnumValue<GridSide>();
            int expectedIndex2 = expectedIndex1 + 1;

            var gridBoundaryCoordinate2 = new GridBoundaryCoordinate(expectedSide2,
                                                                     expectedIndex2);

            // Call
            bool result1 = gridBoundaryCoordinate1.Equals(gridBoundaryCoordinate2);
            bool result2 = gridBoundaryCoordinate2.Equals(gridBoundaryCoordinate1);

            // Assert
            Assert.That(result1, Is.False, "Expected comparison with a different index to be false:");
            Assert.That(result2, Is.False, "Expected comparison with a different index to be false:");
        }

        [Test]
        public void Equals_DifferentGridSide_ReturnsFalse()
        {
            // Setup
            var expectedSide1 = random.NextEnumValue<GridSide>();
            int expectedIndex1 = random.Next();

            var gridBoundaryCoordinate1 = new GridBoundaryCoordinate(expectedSide1,
                                                                     expectedIndex1);

            GridSide expectedSide2;

            do
            {
                expectedSide2 = random.NextEnumValue<GridSide>();
            } while (expectedSide2 == expectedSide1);

            int expectedIndex2 = random.Next();

            var gridBoundaryCoordinate2 = new GridBoundaryCoordinate(expectedSide2,
                                                                     expectedIndex2);

            // Call
            bool result1 = gridBoundaryCoordinate1.Equals(gridBoundaryCoordinate2);
            bool result2 = gridBoundaryCoordinate2.Equals(gridBoundaryCoordinate1);

            // Assert
            Assert.That(result1, Is.False, "Expected comparison with a different index to be false:");
            Assert.That(result2, Is.False, "Expected comparison with a different index to be false:");
        }

        [Test]
        public void Equals_SameProperties_ReturnsTrue()
        {
            // Setup
            var expectedSide = random.NextEnumValue<GridSide>();
            int expectedIndex = random.Next();

            var gridBoundaryCoordinate1 = new GridBoundaryCoordinate(expectedSide,
                                                                     expectedIndex);

            var gridBoundaryCoordinate2 = new GridBoundaryCoordinate(expectedSide,
                                                                     expectedIndex);

            // Call
            bool result1 = gridBoundaryCoordinate1.Equals(gridBoundaryCoordinate2);
            bool result2 = gridBoundaryCoordinate2.Equals(gridBoundaryCoordinate1);

            // Assert
            Assert.That(result1, Is.True, "Expected comparison with a different index to be false:");
            Assert.That(result2, Is.True, "Expected comparison with a different index to be false:");
        }

        [Test]
        public void GetHashCode_SameProperties_ReturnsSameValue()
        {
            // Setup
            var expectedSide = random.NextEnumValue<GridSide>();
            int expectedIndex = random.Next();

            var gridBoundaryCoordinate1 = new GridBoundaryCoordinate(expectedSide,
                                                                     expectedIndex);

            var gridBoundaryCoordinate2 = new GridBoundaryCoordinate(expectedSide,
                                                                     expectedIndex);

            // Call
            int result1 = gridBoundaryCoordinate1.GetHashCode();
            int result2 = gridBoundaryCoordinate2.GetHashCode();

            // Assert
            Assert.That(result1, Is.EqualTo(result2), "Expected the same hash code:");
        }

        [Test]
        public void Deconstruct_ExpectedResults()
        {
            // Setup
            var expectedSide = random.NextEnumValue<GridSide>();
            int expectedIndex = random.Next();

            var gridBoundaryCoordinate = new GridBoundaryCoordinate(expectedSide,
                                                                    expectedIndex);

            // Call
            (GridSide gridSideResult, int indexResult) = gridBoundaryCoordinate;

            // Assert
            Assert.That(gridSideResult, Is.EqualTo(expectedSide));
            Assert.That(indexResult, Is.EqualTo(expectedIndex));
        }
    }
}