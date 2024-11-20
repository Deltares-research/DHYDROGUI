using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class WaveBoundaryGeometricDefinitionFactoryTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_SnappingCalculatorProviderNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WaveBoundaryGeometricDefinitionFactory(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("snappingCalculatorProvider"));
        }

        [Test]
        public void Constructor_DoesNotThrow()
        {
            // Call
            void Call() => new WaveBoundaryGeometricDefinitionFactory(Substitute.For<IBoundarySnappingCalculatorProvider>());

            // Assert
            Assert.DoesNotThrow(Call);
        }

        [Test]
        public void ConstructWaveBoundaryGeometricDefinition_StartCoordinateNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = new WaveBoundaryGeometricDefinitionFactory(Substitute.For<IBoundarySnappingCalculatorProvider>());

            // Call
            void Call() => factory.ConstructWaveBoundaryGeometricDefinition(null, new Coordinate());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("startCoordinate"));
        }

        [Test]
        public void ConstructWaveBoundaryGeometricDefinition_EndCoordinateNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = new WaveBoundaryGeometricDefinitionFactory(Substitute.For<IBoundarySnappingCalculatorProvider>());

            // Call
            void Call() => factory.ConstructWaveBoundaryGeometricDefinition(new Coordinate(), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("endCoordinate"));
        }

        [Test]
        public void ConstructWaveBoundaryGeometricDefinition_ReturnsCorrectResult()
        {
            // Setup
            int startIndex = random.Next(0, 9);
            int endIndex = random.Next(10, 19);
            int length = random.Next(1, 10);
            var gridSide = random.NextEnumValue<GridSide>();

            var startCoordinate = new Coordinate(0.0, 0.0);
            var endCoordinate = new Coordinate(0.0, random.Next(1, 10));

            var firstGridCoordinates = new[]
            {
                new GridBoundaryCoordinate(gridSide, startIndex)
            };

            var lastGridCoordinates = new[]
            {
                new GridBoundaryCoordinate(gridSide, endIndex)
            };

            var calculator = Substitute.For<IBoundarySnappingCalculator>();

            calculator.SnapCoordinateToGridBoundaryCoordinate(startCoordinate)
                      .Returns(firstGridCoordinates);
            calculator.SnapCoordinateToGridBoundaryCoordinate(endCoordinate)
                      .Returns(lastGridCoordinates);
            calculator.CalculateDistanceBetweenBoundaryIndices(startIndex, endIndex, gridSide).Returns(length);

            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            calculatorProvider.GetBoundarySnappingCalculator().Returns(calculator);

            var factory = new WaveBoundaryGeometricDefinitionFactory(calculatorProvider);

            // Call
            IWaveBoundaryGeometricDefinition result = factory.ConstructWaveBoundaryGeometricDefinition(startCoordinate,
                                                                                                       endCoordinate);

            // Assert
            Assert.That(result.StartingIndex, Is.EqualTo(startIndex));
            Assert.That(result.EndingIndex, Is.EqualTo(endIndex));
            Assert.That(result.GridSide, Is.EqualTo(gridSide));
            Assert.That(result.Length, Is.EqualTo(length));
        }

        [Test]
        public void ConstructWaveBoundaryGeometricDefinition_OrientationNotDefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var factory = new WaveBoundaryGeometricDefinitionFactory(calculatorProvider);

            // Call | Assert
            void Call() => factory.ConstructWaveBoundaryGeometricDefinition((BoundaryOrientationType) int.MaxValue);

            Assert.Throws<InvalidEnumArgumentException>(Call);
        }

        [Test]
        public void ConstructWaveBoundaryGeometricDefinition_CalculatorNull_ReturnsNull()
        {
            // Setup
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            calculatorProvider.GetBoundarySnappingCalculator().Returns((IBoundarySnappingCalculator) null);

            var factory = new WaveBoundaryGeometricDefinitionFactory(calculatorProvider);

            // Call
            IWaveBoundaryGeometricDefinition result =
                factory.ConstructWaveBoundaryGeometricDefinition(BoundaryOrientationType.East);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ConstructWaveBoundaryGeometricDefinition_ExpectedResults()
        {
            // Setup
            const BoundaryOrientationType orientation = BoundaryOrientationType.North;

            var calculator = Substitute.For<IBoundarySnappingCalculator>();
            calculator.GridBoundary.GetSideAlignedWithNormal(Arg.Is<Vector2D>(n => Math.Abs(n.X) < 0.005 &&
                                                                                   Math.Abs(n.Y - 1) < 0.005))
                      .Returns(GridSide.North);
            calculator.GridBoundary[GridSide.North].Returns(new[]
            {
                new GridBoundaryCoordinate(GridSide.North, 0),
                new GridBoundaryCoordinate(GridSide.North, 1)
            });

            const double length = 5.0;
            calculator.CalculateDistanceBetweenBoundaryIndices(0, 1, GridSide.North)
                      .Returns(length);

            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            calculatorProvider.GetBoundarySnappingCalculator().Returns(calculator);

            var factory = new WaveBoundaryGeometricDefinitionFactory(calculatorProvider);

            // Call
            IWaveBoundaryGeometricDefinition result = factory.ConstructWaveBoundaryGeometricDefinition(orientation);

            // Assert
            Assert.That(result.GridSide, Is.EqualTo(GridSide.North));
            Assert.That(result.StartingIndex, Is.EqualTo(0));
            Assert.That(result.EndingIndex, Is.EqualTo(1));
            Assert.That(result.Length, Is.EqualTo(length));
        }

        [Test]
        public void HasInvertedOrderingCoordinates_CalculatorNull_ReturnsFalse()
        {
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var inputCoordinate = new Coordinate(10.0, 10.0);

            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();

            calculatorProvider.GetBoundarySnappingCalculator().ReturnsForAnyArgs((IBoundarySnappingCalculator) null);
            var factory = new WaveBoundaryGeometricDefinitionFactory(calculatorProvider);

            // Call
            bool result = factory.HasInvertedOrderingCoordinates(geometricDefinition, inputCoordinate);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [TestCaseSource(nameof(GetHasInvertedOrderingCoordinatesTestData))]
        public void HasInvertedOrderingCoordinates_ExpectedResults(Coordinate inputCoordinate, Coordinate currentCoordinate, bool expectedResult)
        {
            // Setup
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.GridSide.ReturnsForAnyArgs(GridSide.North);
            geometricDefinition.StartingIndex.ReturnsForAnyArgs(5);

            var calculator = Substitute.For<IBoundarySnappingCalculator>();
            calculator.GridBoundary.GetWorldCoordinateFromBoundaryCoordinate(
                          Arg.Is<GridBoundaryCoordinate>(c => c.GridSide == GridSide.North && c.Index == 5))
                      .Returns(currentCoordinate);

            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();

            calculatorProvider.GetBoundarySnappingCalculator().ReturnsForAnyArgs(calculator);
            var factory = new WaveBoundaryGeometricDefinitionFactory(calculatorProvider);

            // Call
            bool result = factory.HasInvertedOrderingCoordinates(geometricDefinition, inputCoordinate);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCaseSource(nameof(GetHasInvertedOrderingCoordinatesParamNullTestData))]
        public void HasInvertedOrderingCoordinates_ParameterNull_ThrowsArgumentNullException(IWaveBoundaryGeometricDefinition geometricDefinition,
                                                                                             Coordinate startCoordinate,
                                                                                             string expectedParamName)
        {
            // Setup
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var factory = new WaveBoundaryGeometricDefinitionFactory(calculatorProvider);

            // Call | Assert
            void Call() => factory.HasInvertedOrderingCoordinates(geometricDefinition, startCoordinate);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        private static IEnumerable<TestCaseData> GetHasInvertedOrderingCoordinatesTestData()
        {
            var coord1 = new Coordinate(5.0, 20.0);
            var coord2 = new Coordinate(25.0, 20.0);

            yield return new TestCaseData(coord1, coord1, false);
            yield return new TestCaseData(coord1, coord2, true);
        }

        private static IEnumerable<TestCaseData> GetHasInvertedOrderingCoordinatesParamNullTestData()
        {
            var coord = new Coordinate(5.0, 20.0);
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            yield return new TestCaseData(geometricDefinition, null, "startCoordinate");
            yield return new TestCaseData(null, coord, "geometricDefinition");
        }
    }
}