using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries
{
    [TestFixture]
    public class GeometryFactoryTest
    {
        private readonly Random random = new Random(37);

        [Test]
        public void Constructor_GridBoundaryProviderNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new GeometryFactory(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("gridBoundaryProvider"));
        }

        [Test]
        public void ConstructBoundaryLineGeometry_GridBoundaryNull_ReturnsNull()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            IGridBoundary gridBoundary = null;

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new GeometryFactory(gridBoundaryProvider);

            var waveBoundary = Substitute.For<IWaveBoundary>();

            // Call
            ILineString result = factory.ConstructBoundaryLineGeometry(waveBoundary);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ConstructBoundaryLineGeometry_CallingArgumentNameNull_ThrowsArgumentNullException()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            IGridBoundary gridBoundary = null;

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new GeometryFactory(gridBoundaryProvider);

            // Call
            void Call() => factory.ConstructBoundaryLineGeometry(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }

        [Test]
        public void ConstructBoundaryLineGeometry_ValidInput_ReturnsCorrectLineString()
        {
            // TODO: (MWT) make this neat
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var gridBoundary = Substitute.For<IGridBoundary>();

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);

            var factory = new GeometryFactory(gridBoundaryProvider);

            var gridSide = random.NextEnumValue<GridSide>();
            const int expectedStartingIndex = 3;
            const int expectedEndingIndex = 6;

            const int gridBoundarySize = 9;
            List<Coordinate> coordinates = Enumerable.Range(0, gridBoundarySize)
                                                     .Select(x => new Coordinate(x + 0.5, x * x))
                                                     .ToList();
            List<GridBoundaryCoordinate> gridCoordinates = Enumerable.Range(0, gridBoundarySize)
                                                                     .Select(x => new GridBoundaryCoordinate(gridSide, x))
                                                                     .ToList();

            gridBoundary[gridSide].Returns(gridCoordinates);
            for (var i = 0; i < gridBoundarySize; i++)
            {
                gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(gridCoordinates[i]).Returns(coordinates[i]);
            }

            var waveBoundaryGeomDef = new WaveBoundaryGeometricDefinition(expectedStartingIndex, expectedEndingIndex, gridSide);
            var waveBoundary = Substitute.For<IWaveBoundary>();
            waveBoundary.GeometricDefinition.Returns(waveBoundaryGeomDef);

            // Call
            ILineString result = factory.ConstructBoundaryLineGeometry(waveBoundary);

            // Assert
            const int expectedSize = expectedEndingIndex - expectedStartingIndex + 1;
            Assert.That(result.NumPoints, Is.EqualTo(expectedSize));

            var coordinateComparer = new Coordinate2DEqualityComparer();
            for (var i = 0; i < expectedSize; i++)
            {

                Assert.That(coordinateComparer.Equals(result.GetCoordinateN(i), 
                                                      coordinates[i + expectedStartingIndex]));
            }
        }


    }
}