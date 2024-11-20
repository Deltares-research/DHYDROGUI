using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class WaveBoundaryGeometricDefinitionTest
    {
        private readonly Random random = new Random();

        private static IEnumerable<TestCaseData> EnumValues =>
            Enum.GetValues(typeof(GridSide))
                .Cast<GridSide>()
                .Select(x => new TestCaseData(x));

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            int startingIndex = random.Next(int.MaxValue - 1);
            int endingIndex = random.Next(startingIndex, int.MaxValue);
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            // Call
            var geometricDefinition =
                new WaveBoundaryGeometricDefinition(startingIndex,
                                                    endingIndex,
                                                    gridSide,
                                                    length);

            // Assert
            Assert.That(geometricDefinition, Is.InstanceOf<IWaveBoundaryGeometricDefinition>());

            Assert.That(geometricDefinition.StartingIndex, Is.EqualTo(startingIndex),
                        "Expected a different StartingIndex.");
            Assert.That(geometricDefinition.EndingIndex, Is.EqualTo(endingIndex),
                        "Expected a different EndingIndex.");
            Assert.That(geometricDefinition.GridSide, Is.EqualTo(gridSide),
                        "Expected a different GridSide.");
            Assert.That(geometricDefinition.Length, Is.EqualTo(length),
                        "Expected a different Length.");

            IEventedList<SupportPoint> supportPoints = geometricDefinition.SupportPoints;
            Assert.That(supportPoints, Is.Not.Null,
                        "Expected that SupportPoints was not null.");
            Assert.That(supportPoints, Has.Count.EqualTo(2),
                        "Expected that the SupportsPoints holds two instances.");
            Assert.That(supportPoints[0].Distance, Is.EqualTo(0),
                        "Expected that first SupportPoint has a Distance of 0");
            Assert.That(supportPoints[1].Distance, Is.EqualTo(length),
                        $"Expected that second SupportPoint is equal to the Length {length}.");
        }

        [Test]
        public void Constructor_InvalidGridSideValue_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            int startingIndex = random.Next(int.MaxValue - 1);
            int endingIndex = random.Next(startingIndex, int.MaxValue);
            const GridSide gridSide = (GridSide) int.MaxValue;
            double length = random.NextDouble();

            // Call
            void Call() => new WaveBoundaryGeometricDefinition(startingIndex,
                                                               endingIndex,
                                                               gridSide,
                                                               length);

            // Assert
            var exception = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo($"Value '{gridSide}' is not a defined GridSide enum."));
        }

        [Test]
        public void Constructor_StartingIndexSmallerThanZero_ThrowsArgumentException()
        {
            // Setup
            int startingIndex = -1 * random.Next(1, int.MaxValue);
            int endingIndex = random.Next(1, int.MaxValue);
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            // Call
            void Call() => new WaveBoundaryGeometricDefinition(startingIndex,
                                                               endingIndex,
                                                               gridSide,
                                                               length);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message,
                        Is.EqualTo($"StartingIndex: '{startingIndex}' should be larger or equal to zero."));
        }

        [Test]
        public void Constructor_StartingIndexEqualToEndingIndex_ThrowsArgumentException()
        {
            // Setup
            int startingIndex = random.Next();
            int endingIndex = startingIndex;
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            // Call
            void Call() => new WaveBoundaryGeometricDefinition(startingIndex,
                                                               endingIndex,
                                                               gridSide,
                                                               length);

            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message,
                        Is.EqualTo($"StartingIndex: '{startingIndex}' should be smaller than EndingIndex: {endingIndex}."));
        }

        [Test]
        public void Constructor_StartingIndexGreaterThanEndingIndex_ThrowsArgumentException()
        {
            // Setup
            int endingIndex = random.Next(int.MaxValue - 1);
            int startingIndex = random.Next(endingIndex, int.MaxValue);
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            // Call
            void Call() => new WaveBoundaryGeometricDefinition(startingIndex,
                                                               endingIndex,
                                                               gridSide,
                                                               length);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message,
                        Is.EqualTo($"StartingIndex: '{startingIndex}' should be smaller than EndingIndex: {endingIndex}."));
        }

        [Test]
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(4)]
        public void StartingIndex_SetValidValue_ExpectedValue(int startingIndex)
        {
            // Setup
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            var geometricDefinition = new WaveBoundaryGeometricDefinition(2, 5, gridSide, length);

            // Call
            geometricDefinition.StartingIndex = startingIndex;

            // Assert
            Assert.That(geometricDefinition.StartingIndex, Is.EqualTo(startingIndex),
                        "Expected a different StartingIndex:");
        }

        [Test]
        [TestCase(5, 10, 10)]
        [TestCase(5, 10, 12)]
        public void StartingIndex_SetValueGreaterOrEqualToEndingIndex_ThrowsArgumentException(int startingIndex,
                                                                                              int endingIndex,
                                                                                              int nextStartingIndex)
        {
            // Setup
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            var geometricDefinition = new WaveBoundaryGeometricDefinition(startingIndex,
                                                                          endingIndex,
                                                                          gridSide,
                                                                          length);

            // Call
            void Call() => geometricDefinition.StartingIndex = nextStartingIndex;

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message,
                        Is.EqualTo($"StartingIndex should be smaller than EndingIndex."));
        }

        [Test]
        public void StartingIndex_SetValueToSmallerThanZero_ThrowsArgumentException()
        {
            // Setup
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            var geometricDefinition = new WaveBoundaryGeometricDefinition(5, 10, gridSide, length);

            // Call
            void Call() => geometricDefinition.StartingIndex = -5;

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message,
                        Is.EqualTo($"StartingIndex should be greater or equal to zero."));
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(8)]
        public void EndingIndex_SetValidValue_ExpectedValue(int endingIndex)
        {
            // Setup
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            var geometricDefinition =
                new WaveBoundaryGeometricDefinition(0, 5, gridSide, length);

            // Call
            geometricDefinition.EndingIndex = endingIndex;

            // Assert
            Assert.That(geometricDefinition.EndingIndex, Is.EqualTo(endingIndex),
                        "Expected a different EndingIndex:");
        }

        [Test]
        [TestCase(5, 10, 5)]
        [TestCase(5, 10, 2)]
        public void EndingIndex_SetValueSmallerOrEqualToStartingIndex_ThrowsArgumentException(int startingIndex,
                                                                                              int endingIndex,
                                                                                              int nextEndingIndex)
        {
            // Setup
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            var geometricDefinition = new WaveBoundaryGeometricDefinition(startingIndex,
                                                                          endingIndex,
                                                                          gridSide,
                                                                          length);

            // Call
            void Call() => geometricDefinition.EndingIndex = nextEndingIndex;

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message,
                        Is.EqualTo($"EndingIndex should be greater than StartingIndex."));
        }

        [Test]
        [TestCaseSource(nameof(EnumValues))]
        public void GridSide_SetValidValue_ExpectedValue(GridSide gridSide)
        {
            double length = random.NextDouble();

            // Setup
            var geometricDefinition =
                new WaveBoundaryGeometricDefinition(0,
                                                    5,
                                                    GridSide.East,
                                                    length);

            // Call
            geometricDefinition.GridSide = gridSide;

            // Assert
            Assert.That(geometricDefinition.GridSide, Is.EqualTo(gridSide),
                        "Expected a different GridSide:");
        }

        [Test]
        public void GridSide_SetInvalidEnum_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            int startingIndex = random.Next(int.MaxValue - 1);
            int endingIndex = random.Next(startingIndex, int.MaxValue);
            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            var geometricDefinition = new WaveBoundaryGeometricDefinition(startingIndex,
                                                                          endingIndex,
                                                                          gridSide,
                                                                          length);

            // Call
            void Call() => geometricDefinition.GridSide = (GridSide) int.MaxValue;

            // Assert
            var exception = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo($"Value '{int.MaxValue}' is not a defined GridSide enum."));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_InvalidLengthValue_ThrowsArgumentException(int factor)
        {
            // Setup
            int startingIndex = random.Next(int.MaxValue - 1);
            int endingIndex = random.Next(startingIndex, int.MaxValue);
            var gridSide = random.NextEnumValue<GridSide>();
            double length = factor * random.NextDouble();

            void Call() => new WaveBoundaryGeometricDefinition(startingIndex,
                                                               endingIndex,
                                                               gridSide,
                                                               length);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo($"Length: '{length}' should be larger than zero."));
        }
    }
}