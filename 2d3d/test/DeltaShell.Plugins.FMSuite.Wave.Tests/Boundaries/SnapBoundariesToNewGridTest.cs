using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
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

            GridBoundaryCoordinate GetGridBoundaryCoordinateArg(int index)
            {
                return Arg.Is<GridBoundaryCoordinate>(x =>
                                                          x.GridSide == geometricDefinition.GridSide &&
                                                          x.Index == index);
            }

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
            var factory = Substitute.For<IWaveBoundaryGeometricDefinitionFactory>();

            void Call() => SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(null, factory).ToArray();

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void RestoreBoundariesIfPossible_ThrowsArgumentNullException_WhenNullGeometricDefinitionFactory()
        {
            void Call() => SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(Enumerable.Empty<CachedBoundary>(), null).ToArray();

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void RestoreBoundariesIfPossible_ReturnsEmptyCollection_WhenCachedBoundariesEmpty()
        {
            // Setup
            var cachedBoundaries = new List<CachedBoundary>();
            var factory = Substitute.For<IWaveBoundaryGeometricDefinitionFactory>();

            // Call
            IEnumerable<IWaveBoundary> result = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, factory);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void RestoreBoundariesIfPossible_GeometricDefinitionCouldNotBeConstructed_LogsMessageReturnsNothing()
        {
            using (var messageLogger = new LogAppenderEntriesTester(typeof(SnapBoundariesToNewGrid)))
            {
                // Setup
                var cachedBoundaries = new List<CachedBoundary> {CachedBoundaryCreator(0, 5, 10, "one")};

                var factory = Substitute.For<IWaveBoundaryGeometricDefinitionFactory>();
                factory.ConstructWaveBoundaryGeometricDefinition(cachedBoundaries[0].StartingPointWorldCoordinate,
                                                                 cachedBoundaries[0].EndingPointWorldCoordinate).Returns((IWaveBoundaryGeometricDefinition) null);
                factory.ConstructWaveBoundaryGeometricDefinition(cachedBoundaries[0].EndingPointWorldCoordinate,
                                                                 cachedBoundaries[0].StartingPointWorldCoordinate).Returns((IWaveBoundaryGeometricDefinition) null);

                // Call
                IEnumerable<IWaveBoundary> result = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, factory);

                // Assert
                Assert.IsNotNull(result);
                Assert.That(result, Is.Empty);

                factory.ReceivedWithAnyArgs(1).ConstructWaveBoundaryGeometricDefinition(null, null);

                // Assert logging messages
                Assert.AreEqual(1, messageLogger.Messages.Count());
                Assert.That(messageLogger.Messages.First(), Is.EqualTo($"Boundary {cachedBoundaries[0].WaveBoundary.Name} could not snap to the new grid. Please inspect your boundaries."));
            }
        }

        [Test]
        public void RestoreBoundariesIfPossible_ReturnsSingleNewWaveBoundary_NoAdditionalSupportPoints()
        {
            // Arrange
            CachedBoundary cachedBoundary = CachedBoundaryCreator(1, 2, 10, "one");
            var cachedBoundaries = new List<CachedBoundary> {cachedBoundary};

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var factory = Substitute.For<IWaveBoundaryGeometricDefinitionFactory>();
            factory.ConstructWaveBoundaryGeometricDefinition(null, null).ReturnsForAnyArgs(geometricDefinition);

            // Act
            IEnumerable<IWaveBoundary> result =
                SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, factory)
                                       .ToArray();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(1, result.Count());

            IWaveBoundary boundary = result.First();
            Assert.That(boundary, Is.Not.SameAs(cachedBoundary.WaveBoundary));
            Assert.That(boundary.Name, Is.EqualTo(cachedBoundary.WaveBoundary.Name));
            Assert.That(boundary.ConditionDefinition, Is.SameAs(cachedBoundary.WaveBoundary.ConditionDefinition));
            Assert.That(boundary.GeometricDefinition, Is.SameAs(geometricDefinition));

            factory.ReceivedWithAnyArgs(1).ConstructWaveBoundaryGeometricDefinition(null, null);
        }

        [Test]
        public void RestoreBoundariesIfPossible_ReturnsIWaveBoundaryWithSupportPoints_WhenProvidingValidSupportPoints()
        {
            // Setup
            CachedBoundary cachedBoundary = CachedBoundaryCreator(1, 1, 10, "one");

            // Add a single support point falling within the lenth of the waveBoundary
            var supportPoint = new SupportPoint(5, cachedBoundary.WaveBoundary.GeometricDefinition);
            cachedBoundary.WaveBoundary.GeometricDefinition.SupportPoints.Add(supportPoint);

            var cachedBoundaries = new List<CachedBoundary> {cachedBoundary};

            var geometricDefinition = new WaveBoundaryGeometricDefinition(1, 10, GridSide.North, 10.0);

            var factory = Substitute.For<IWaveBoundaryGeometricDefinitionFactory>();
            factory.ConstructWaveBoundaryGeometricDefinition(null, null).ReturnsForAnyArgs(geometricDefinition);

            // Call
            IEnumerable<IWaveBoundary> result =
                SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, factory)
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
            // Setup
            var cachedBoundaries = new List<CachedBoundary>();
            CachedBoundary cachedBoundary = CachedBoundaryCreator(1, 1, 10, "one");
            var supportPoint = new SupportPoint(5, cachedBoundary.WaveBoundary.GeometricDefinition);
            cachedBoundary.WaveBoundary.GeometricDefinition.SupportPoints.Add(supportPoint);
            cachedBoundaries.Add(cachedBoundary);

            var geometricDefinition = new WaveBoundaryGeometricDefinition(1, 10, GridSide.North, 10.0);

            var factory = Substitute.For<IWaveBoundaryGeometricDefinitionFactory>();
            factory.ConstructWaveBoundaryGeometricDefinition(null, null).ReturnsForAnyArgs(geometricDefinition);

            // Call
            IEnumerable<IWaveBoundary> result =
                SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, factory)
                                       .ToArray();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            // Assert
            IWaveBoundary newWaveBoundary = result.First();
            Assert.IsFalse(newWaveBoundary.GeometricDefinition.SupportPoints.Contains(cachedBoundaries[0].WaveBoundary.GeometricDefinition.SupportPoints[0]));
            Assert.IsFalse(newWaveBoundary.GeometricDefinition.SupportPoints.Contains(cachedBoundaries[0].WaveBoundary.GeometricDefinition.SupportPoints[1]));
            Assert.IsFalse(newWaveBoundary.GeometricDefinition.SupportPoints.Contains(cachedBoundaries[0].WaveBoundary.GeometricDefinition.SupportPoints[2]));
        }

        [Test]
        public void RestoreBoundariesIfPossible_RemovesInvalidSupportPoints()
        {
            // Setup
            using (var messageLogger = new LogAppenderEntriesTester(typeof(SnapBoundariesToNewGrid)))
            {
                CachedBoundary cachedBoundary = CachedBoundaryCreator(1, 5, 70, "one");
                var supportPoint = new SupportPoint(50.0, cachedBoundary.WaveBoundary.GeometricDefinition);
                cachedBoundary.WaveBoundary.GeometricDefinition.SupportPoints.Add(supportPoint);
                cachedBoundary.WaveBoundary.GeometricDefinition.Length.Returns(60);

                var fileBasedParameters = new FileBasedParameters("mock");
                var component = new SpatiallyVaryingDataComponent<FileBasedParameters>();
                component.AddParameters(supportPoint, fileBasedParameters);

                var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
                conditionDefinition.DataComponent.Returns(component);
                cachedBoundary.WaveBoundary.ConditionDefinition.Returns(conditionDefinition);

                var cachedBoundaries = new List<CachedBoundary> {cachedBoundary};

                var geometricDefinition = new WaveBoundaryGeometricDefinition(1, 10, GridSide.North, 10.0);

                var factory = Substitute.For<IWaveBoundaryGeometricDefinitionFactory>();
                factory.ConstructWaveBoundaryGeometricDefinition(null, null).ReturnsForAnyArgs(geometricDefinition);

                // Call
                IEnumerable<IWaveBoundary> result =
                    SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cachedBoundaries, factory)
                                           .ToArray();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count(), Is.EqualTo(1));

                IWaveBoundary boundary = result.First();
                Assert.That(boundary.ConditionDefinition.DataComponent, Is.SameAs(component));
                Assert.That(component.Data, Is.Empty);
                Assert.That(messageLogger.Messages.First(), Is.EqualTo($"Support point at distance {supportPoint.Distance} does no longer fit on the snapped Boundary {cachedBoundary.WaveBoundary.Name}; Removed. Please inspect your support points"));
            }
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