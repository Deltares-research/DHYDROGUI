using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class SnapBoundariesToNewGridTest
    {
        [Test]
        public void CreateCachedBoundaries_ThrowsArgumentNullException_WhenBoundariesIsNull()
        {
            // Setup
            var gridBoundaryMock = Substitute.For<IGridBoundary>();

            // Call | Assert
            void Call() => SnapBoundariesToNewGrid.CreateCachedBoundaries(null, gridBoundaryMock).ToArray();
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void CreateCachedBoundaries_ReturnsEmptyList_WhenGridBoundaryIsNull()
        {
            // Setup
            var boundaries = new List<IWaveBoundary> {Substitute.For<IWaveBoundary>()};

            // Call
            IEnumerable<CachedBoundary> result =
                SnapBoundariesToNewGrid.CreateCachedBoundaries(boundaries, null);

            // Assert
            Assert.NotNull(result);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void CreateCachedBoundaries_ReturnsEmptyList_WhenProvidingEmptyList1()
        {
            // Setup
            var gridBoundaryMock = Substitute.For<IGridBoundary>();
            var boundaries = new List<IWaveBoundary>();

            // Call
            IEnumerable<CachedBoundary> result =
                SnapBoundariesToNewGrid.CreateCachedBoundaries(boundaries, gridBoundaryMock);

            // Assert
            Assert.NotNull(result);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void CreateCachedBoundaries_ReturnsCachedBoundary_WhenWaveBoundariesProvided()
        {
            // GridBoundaries coordinate are defined using the (GridSide, StartingIndex)
            // Setup
            var startingCoordinate = new Coordinate(5, 6);
            var endingCoordinate = new Coordinate(7, 8);

            var gridBoundaryMock = Substitute.For<IGridBoundary>();

            var boundaries = new List<IWaveBoundary> {WaveBoundaryMockCreator()};
            IWaveBoundaryGeometricDefinition geometricDefinition = boundaries[0].GeometricDefinition;

            GridBoundaryCoordinate GetGridBoundaryCoordinateArg(int index) =>
                Arg.Is<GridBoundaryCoordinate>(x =>
                                                   x.GridSide == geometricDefinition.GridSide &&
                                                   x.Index == index);

            gridBoundaryMock.GetWorldCoordinateFromBoundaryCoordinate(GetGridBoundaryCoordinateArg(geometricDefinition.StartingIndex))
                            .Returns(startingCoordinate);
            gridBoundaryMock.GetWorldCoordinateFromBoundaryCoordinate(GetGridBoundaryCoordinateArg(geometricDefinition.EndingIndex))
                            .Returns(endingCoordinate);

            // Call
            IEnumerable<CachedBoundary> result =
                SnapBoundariesToNewGrid.CreateCachedBoundaries(boundaries, gridBoundaryMock)
                                       .ToArray();

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.Count(), 1);

            CachedBoundary first = result.First();
            Assert.That(first.StartingPointWorldCoordinate.X, Is.EqualTo(startingCoordinate.X));
            Assert.That(first.StartingPointWorldCoordinate.Y, Is.EqualTo(startingCoordinate.Y));

            Assert.That(first.EndingPointWorldCoordinate.X, Is.EqualTo(endingCoordinate.X));
            Assert.That(first.EndingPointWorldCoordinate.Y, Is.EqualTo(endingCoordinate.Y));

            gridBoundaryMock.Received(2).GetWorldCoordinateFromBoundaryCoordinate(Arg.Any<GridBoundaryCoordinate>());
        }

        [Test]
        public void RestoreBoundariesIfPossible_ThrowsArgumentNullException_WhenNullCachedBoundariesProvided()
        {
            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            void Call() => SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(null, boundarySnappingCalculator).ToArray();
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void RestoreBoundariesIfPossible_ReturnsEmptyCollection_WhenCachedBoundariesEmpty()
        {
            // Arrange
            var cachedBoundaries = new List<CachedBoundary>();

            // Act
            IEnumerable<IWaveBoundary> result = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void RestoreBoundariesIfPossible_ReturnsEmptyCollection_WhenCachedBoundariesNotEmptyAndSnappingCalculatorNull()
        {
            // Arrange
            var cachedBoundaries = new List<CachedBoundary>();
            cachedBoundaries.Add(CachedBoundaryCreator(0, 0, 10, "test1"));

            // Act
            IEnumerable<IWaveBoundary> result =
                SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, null)
                                       .ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void RestoreBoundariesIfPossible_LogsMessageReturnsNothing_OnlyOnePointSnapped()
        {
            using (var messageLogger = new LogAppenderEntriesTester(typeof(SnapBoundariesToNewGrid)))
            {
                // Arrange
                var cachedBoundaries = new List<CachedBoundary>();
                cachedBoundaries.Add(CachedBoundaryCreator(0, 0, 10, "one"));

                var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
                var beginPointGridCoordinates = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.North, 0)};
                var endPointGridCoordinates = new List<GridBoundaryCoordinate>();
                // First call returns a coordinate for the begin point, second call returns an empty list (no coordinates.
                boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(Arg.Any<Coordinate>()).Returns(x => beginPointGridCoordinates, x => endPointGridCoordinates);

                // Act
                IEnumerable<IWaveBoundary> result = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, boundarySnappingCalculator);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsFalse(result.Any());

                // Assert logging messages
                Assert.AreEqual(1, messageLogger.Messages.Count());
                Assert.That(messageLogger.Messages.First(), Is.EqualTo("Boundary one could not snap to the new grid (begin and or end point problematic). Please inspect your boundaries."));
            }
        }

        [Test]
        public void RestoreBoundariesIfPossible_ReturnsSingleNewWaveBoundary_NoAdditionalSupportPoints()
        {
            // Arrange
            var cachedBoundaries = new List<CachedBoundary>();
            cachedBoundaries.Add(CachedBoundaryCreator(1, 1, 10, "one"));

            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            var beginPointGridCoordinates = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.North, 0)};
            var endPointGridCoordinates = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.North, 1)};
            boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(Arg.Any<Coordinate>()).Returns(x => beginPointGridCoordinates, x => endPointGridCoordinates);
            boundarySnappingCalculator.CalculateDistanceBetweenBoundaryIndices(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<GridSide>()).ReturnsForAnyArgs(10);

            // Act
            IEnumerable<IWaveBoundary> result =
                SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, boundarySnappingCalculator)
                                       .ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreNotSame(result.First(), cachedBoundaries.First().WaveBoundary);
            Assert.AreEqual(cachedBoundaries.First().WaveBoundary.Name, result.First().Name);
        }

        [Test]
        public void RestoreBoundariesIfPossible_ReturnsIWaveBoundaryWithSupportPoints_WhenProvidingValidSupportPoints()
        {
            // Arrange
            var cachedBoundaries = new List<CachedBoundary>();
            CachedBoundary cachedBoundary = CachedBoundaryCreator(1, 1, 10, "one");

            // Add a single support point falling within the lenth of the waveBoundary
            var supportPoint = new SupportPoint(5, cachedBoundary.WaveBoundary.GeometricDefinition);
            cachedBoundary.WaveBoundary.GeometricDefinition.SupportPoints.Add(supportPoint);
            cachedBoundaries.Add(cachedBoundary);

            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            var beginPointGridCoordinates = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.North, 0)};
            var endPointGridCoordinates = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.North, 1)};
            boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(Arg.Any<Coordinate>()).Returns(x => beginPointGridCoordinates, x => endPointGridCoordinates);
            boundarySnappingCalculator.CalculateDistanceBetweenBoundaryIndices(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<GridSide>()).ReturnsForAnyArgs(10);

            // Act
            IEnumerable<IWaveBoundary> result =
                SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, boundarySnappingCalculator)
                                       .ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreNotSame(result.First(), cachedBoundaries.First().WaveBoundary);
            Assert.AreEqual(cachedBoundaries.First().WaveBoundary.Name, result.First().Name);
            Assert.AreEqual(3, result.First().GeometricDefinition.SupportPoints.Count());
            IEventedList<SupportPoint> points = result.First().GeometricDefinition.SupportPoints;
            Assert.AreEqual(0, points[0].Distance);
            Assert.AreEqual(10, points[1].Distance);
            Assert.AreEqual(5, points[2].Distance);
        }

        [Test]
        public void RestoreBoundariesIfPossible_ReplacesAllValidSupportPointsSavingSupportPointSettings()
        {
            var cachedBoundaries = new List<CachedBoundary>();
            CachedBoundary cachedBoundary = CachedBoundaryCreator(1, 1, 10, "one");
            var supportPoint = new SupportPoint(5, cachedBoundary.WaveBoundary.GeometricDefinition);
            cachedBoundary.WaveBoundary.GeometricDefinition.SupportPoints.Add(supportPoint);
            cachedBoundaries.Add(cachedBoundary);

            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            var beginPointGridCoordinates = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.North, 0)};
            var endPointGridCoordinates = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.North, 1)};
            boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(Arg.Any<Coordinate>()).Returns(x => beginPointGridCoordinates, x => endPointGridCoordinates);
            boundarySnappingCalculator.CalculateDistanceBetweenBoundaryIndices(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<GridSide>()).ReturnsForAnyArgs(10);

            IEnumerable<IWaveBoundary> result =
                SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, boundarySnappingCalculator)
                                       .ToArray();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            // Assert
            IWaveBoundary newWaveBoundary = result.First();
            Assert.IsFalse(newWaveBoundary.GeometricDefinition.SupportPoints.Contains(cachedBoundaries[0].WaveBoundary.GeometricDefinition.SupportPoints[0]));
            Assert.IsFalse(newWaveBoundary.GeometricDefinition.SupportPoints.Contains(cachedBoundaries[0].WaveBoundary.GeometricDefinition.SupportPoints[1]));
            Assert.IsFalse(newWaveBoundary.GeometricDefinition.SupportPoints.Contains(cachedBoundaries[0].WaveBoundary.GeometricDefinition.SupportPoints[2]));
        }

        private static CachedBoundary CachedBoundaryCreator(double begin, double end, int length, string name)
        {
            var startCoordinate = new Coordinate(begin, end);
            var endCoordinate = new Coordinate(begin + 10, end + 10);
            IWaveBoundary waveBoundaryMock = WaveBoundaryMockCreator();
            var supportPoints = new EventedList<SupportPoint>();
            supportPoints.Add(new SupportPoint(0, waveBoundaryMock.GeometricDefinition));
            supportPoints.Add(new SupportPoint(length, waveBoundaryMock.GeometricDefinition));
            waveBoundaryMock.GeometricDefinition.SupportPoints.Returns(supportPoints);
            waveBoundaryMock.Name.Returns(name);

            return new CachedBoundary(startCoordinate, endCoordinate, waveBoundaryMock);
        }

        private static IWaveBoundary WaveBoundaryMockCreator()
        {
            var waveBoundaryGeometricDefinitionMock = Substitute.For<IWaveBoundaryGeometricDefinition>();
            waveBoundaryGeometricDefinitionMock.GridSide.Returns(GridSide.North);
            waveBoundaryGeometricDefinitionMock.StartingIndex.Returns(3);
            waveBoundaryGeometricDefinitionMock.EndingIndex.Returns(6);

            var waveBoundaryMock = Substitute.For<IWaveBoundary>();
            waveBoundaryMock.GeometricDefinition.Returns(waveBoundaryGeometricDefinitionMock);
            var waveBoundaryConditionDefinitionMock = Substitute.For<IWaveBoundaryConditionDefinition>();
            waveBoundaryMock.ConditionDefinition.Returns(waveBoundaryConditionDefinitionMock);

            return waveBoundaryMock;
        }
    }
}