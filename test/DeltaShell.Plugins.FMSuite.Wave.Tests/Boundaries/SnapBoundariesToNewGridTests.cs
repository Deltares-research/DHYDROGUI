using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;
using log4net.Config;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class SnapBoundariesToNewGridTests
    {
        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenBoundariesIsNull()
        {
            var gridBoundaryMock = Substitute.For<IGridBoundary>();
            void Call() => SnapBoundariesToNewGrid.CreateCachedBoundaries(null, gridBoundaryMock);
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenGridBoundaryIsNull()
        {
            var boundaries = new List<IWaveBoundary>();
            void Call() => SnapBoundariesToNewGrid.CreateCachedBoundaries(boundaries, null);
            Assert.DoesNotThrow(Call);
        }

        [Test]
        public void CreateCachedBoundariesReturnsEmptyList_WhenProvidingEmptyList1()
        {
            // Arrange
            var gridBoundaryMock = Substitute.For<IGridBoundary>();
            var boundaries = new List<IWaveBoundary>();
            
            // Act
            IEnumerable<CachedBoundary> result = SnapBoundariesToNewGrid.CreateCachedBoundaries(boundaries, gridBoundaryMock);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.Count(), 0);
        }

        [Test]
        public void CreateCachedBoundaries_ReturnsCachedBoundary_WhenWaveBoundaryProvided()
        {
            // Arrange
            var gridBoundaryMock = Substitute.For<IGridBoundary>();
            gridBoundaryMock.GetWorldCoordinateFromBoundaryCoordinate(Arg.Any<GridBoundaryCoordinate>()).ReturnsForAnyArgs(new Coordinate(0, 0));
            var boundaries = new List<IWaveBoundary>();
            boundaries.Add(WaveBoundaryMockCreator());

            // Act
            var result = SnapBoundariesToNewGrid.CreateCachedBoundaries(boundaries, gridBoundaryMock);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.Count(), 1);
            gridBoundaryMock.Received(2).GetWorldCoordinateFromBoundaryCoordinate(Arg.Any<GridBoundaryCoordinate>());
        }

        [Test]
        public void RestoreBoundariesIfPossible_ThrowsArgumentNullException_WhenNullCachedBoundariesProvided()
        {
            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            void Call() => SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(null, boundarySnappingCalculator);
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void RestoreBoundariesIfPossible_ThrowsArgumentNullException_WhenNullSnappingCalculatorProvided()
        {
            var cachedBoundaries = new List<CachedBoundary>();
            void Call() => SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, null);
            Assert.DoesNotThrow(Call);
        }

        [Test]
        public void RestoreBoundariesIfPossible_ReturnsEmptyCollection_WhenProvidedEmptyCollection()
        {
            // Arrange
            var cachedBoundaries = new List<CachedBoundary>();
            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();

            // Act
            var result = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, boundarySnappingCalculator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void RestoreBoundariesIfPossible_LogsMessageReturnsNothing_OnlyOnePointSnapped()
        {
            var appender = new log4net.Appender.MemoryAppender();
            var appenders = BasicConfigurator.Configure(appender);

            // Arrange
            var cachedBoundaries = new List<CachedBoundary>();
            cachedBoundaries.Add(CachedBoundaryCreator(0, 0, 10,"one"));
            //cachedBoundaries.Add(CachedBoundaryCreator(20, 20, 20, "two"));

            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            var beginPointGridCoordinates = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.North,0)};
            var endPointGridCoordinates = new List<GridBoundaryCoordinate>() ;
            boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(Arg.Any<Coordinate>()).Returns( x=> beginPointGridCoordinates, x=> endPointGridCoordinates);

            // Act
            var result = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, boundarySnappingCalculator);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());

            // Assert logging messages
            LoggingEvent[] logEntries = appender.GetEvents();
            Assert.AreEqual(1, logEntries.Length);
            Assert.That(logEntries.First().RenderedMessage, Is.EqualTo("Boundary one could not snap to the new grid (begin and or end point problematic). Please inspect your boundaries."));
        }


        [Test]
        public void RestoreBoundariesIfPossible_ReturnsSingleNewWaveBoundary_NoAdditionalSupportPoints()
        {
            // Arrange
            var cachedBoundaries = new List<CachedBoundary>();
            cachedBoundaries.Add(CachedBoundaryCreator(1, 1, 10, "one"));

            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            var beginPointGridCoordinates = new List<GridBoundaryCoordinate>() { new GridBoundaryCoordinate(GridSide.North, 0) };
            var endPointGridCoordinates = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.North, 1)};
            boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(Arg.Any<Coordinate>()).Returns(x => beginPointGridCoordinates, x => endPointGridCoordinates);
            boundarySnappingCalculator.CalculateDistanceBetweenBoundaryIndices(Arg.Any<int>(), Arg.Any<int>(),Arg.Any<GridSide>()).ReturnsForAnyArgs(10);

            // Act
            var result = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, boundarySnappingCalculator);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreNotSame(result.First(), cachedBoundaries.First().WaveBoundary);
            Assert.AreEqual(cachedBoundaries.First().WaveBoundary.Name, result.First().Name);

        }

        [Test]
        public void RestoreBoundariesIfPossible_SupportPoints()
        {
            // Arrange
            var cachedBoundaries = new List<CachedBoundary>();
            var cachedBoundary = CachedBoundaryCreator(1, 1, 10, "one");
            
            // Add a single support point falling within the lenth of the waveBoundary
            var supportPoint = new SupportPoint(5, cachedBoundary.WaveBoundary.GeometricDefinition );
            cachedBoundary.WaveBoundary.GeometricDefinition.SupportPoints.Add(supportPoint);
            cachedBoundaries.Add(cachedBoundary);

            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            var beginPointGridCoordinates = new List<GridBoundaryCoordinate>() { new GridBoundaryCoordinate(GridSide.North, 0) };
            var endPointGridCoordinates = new List<GridBoundaryCoordinate>() { new GridBoundaryCoordinate(GridSide.North, 1) };
            boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(Arg.Any<Coordinate>()).Returns(x => beginPointGridCoordinates, x => endPointGridCoordinates);
            boundarySnappingCalculator.CalculateDistanceBetweenBoundaryIndices(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<GridSide>()).ReturnsForAnyArgs(10);

            // Act
            var result = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, boundarySnappingCalculator);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreNotSame(result.First(), cachedBoundaries.First().WaveBoundary);
            Assert.AreEqual(cachedBoundaries.First().WaveBoundary.Name, result.First().Name);
            Assert.AreEqual(3, result.First().GeometricDefinition.SupportPoints.Count());
            var points = result.First().GeometricDefinition.SupportPoints;
            Assert.AreEqual(0,points[0].Distance);
            Assert.AreEqual(10, points[1].Distance);
            Assert.AreEqual(5, points[2].Distance);
        }

        [Ignore]
        [Test]
        public void RestoreBoundariesIfPossible_SupportPointsIncludingToBig()
        {
            // Arrange
            var cachedBoundaries = new List<CachedBoundary>();
            var cachedBoundary = CachedBoundaryCreator(1, 1, 10, "one");

            // Add a single support point falling within the lenth of the waveBoundary
            var supportPoint = new SupportPoint(5, cachedBoundary.WaveBoundary.GeometricDefinition);
            var supportPointTooBig = new SupportPoint(30, cachedBoundary.WaveBoundary.GeometricDefinition);
            cachedBoundary.WaveBoundary.GeometricDefinition.SupportPoints.Add(supportPoint);
            cachedBoundary.WaveBoundary.GeometricDefinition.SupportPoints.Add(supportPointTooBig);
            cachedBoundaries.Add(cachedBoundary);

            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            var beginPointGridCoordinates = new List<GridBoundaryCoordinate>() { new GridBoundaryCoordinate(GridSide.North, 0) };
            var endPointGridCoordinates = new List<GridBoundaryCoordinate>() { new GridBoundaryCoordinate(GridSide.North, 1) };
            boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(Arg.Any<Coordinate>()).Returns(x => beginPointGridCoordinates, x => endPointGridCoordinates);
            boundarySnappingCalculator.CalculateDistanceBetweenBoundaryIndices(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<GridSide>()).ReturnsForAnyArgs(10);

            // Act
            var result = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, boundarySnappingCalculator);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreNotSame(result.First(), cachedBoundaries.First().WaveBoundary);
            Assert.AreEqual(cachedBoundaries.First().WaveBoundary.Name, result.First().Name);
            Assert.AreEqual(3, result.First().GeometricDefinition.SupportPoints.Count());
            var points = result.First().GeometricDefinition.SupportPoints;
            Assert.AreEqual(0, points[0].Distance);
            Assert.AreEqual(10, points[1].Distance);
            Assert.AreEqual(5, points[2].Distance);
        }



        private static CachedBoundary CachedBoundaryCreator(double begin, double end, int length, string name)
        {
            var startCoordinate = new Coordinate(begin,end);
            var endCoordinate = new Coordinate(begin + 10,end + 10);
            var waveBoundaryMock = WaveBoundaryMockCreator();
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
            var waveBoundaryMock = Substitute.For<IWaveBoundary>();
            waveBoundaryMock.GeometricDefinition.Returns(waveBoundaryGeometricDefinitionMock);
            var waveBoundaryConditionDefinitionMock = Substitute.For<IWaveBoundaryConditionDefinition>();
            waveBoundaryMock.ConditionDefinition.Returns(waveBoundaryConditionDefinitionMock);

            return waveBoundaryMock;
        }
    }
}