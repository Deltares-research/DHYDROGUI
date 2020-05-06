using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class CachedBoundaryTests
    {
        [Test]
        public void WhenNewInstance_ConstructorThrowsArgumentNullException_WithNullCoordinateParameter()
        {
            void Call() => new CachedBoundary(null, null, null);

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void WhenNewInstance_ConstructorThrowsArgumentNullException_WithNullCoordinateSecondParameter()
        {
            var coordinate = new Coordinate(0, 0);
            void Call() => new CachedBoundary(coordinate, null, null);

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void WhenNewInstance_ConstructorThrowsArgumentNullException_WithNullWaveBoundaryParameter()
        {
            var coordinate = new Coordinate(0, 0);
            void Call() => new CachedBoundary(coordinate, coordinate, null);

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void NewInstance_ConstructorAssignsPropertiesCorrectly()
        {
            var coordinate = new Coordinate(0, 0);
            var coordinate1 = new Coordinate(0, 0);

            var waveBoundaryMock = NSubstitute.Substitute.For<IWaveBoundary>();
            var sut = new CachedBoundary(coordinate, coordinate1, waveBoundaryMock);

            Assert.IsNotNull(sut);
            Assert.AreSame(coordinate, sut.StartingPointWorldCoordinate);
            Assert.AreSame(coordinate1, sut.EndingPointWorldCoordinate);
            Assert.AreSame(waveBoundaryMock, sut.WaveBoundary);
        }
    }
}

