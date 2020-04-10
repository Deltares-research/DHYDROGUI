using System;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class GeometryCreatorTest
    {
        [Test]
        public void GivenCollectionOfCoordinates_WhenCreatingPolyLineGeometry_ThenLineStringWithTheSameCoordinatesIsReturned()
        {
            // Given
            var coordinates = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1)
            };

            // When
            var lineString = GeometryCreator.CreatePolyLineGeometry(coordinates);

            // Then
            Assert.That(lineString, Is.InstanceOf<LineString>());
            Assert.That(lineString.Coordinates, Is.EqualTo(coordinates));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot create poly line with less than 2 points.")]
        public void GivenCollectionOfCoordinatesSmallerThan2_WhenCreatingPolyLineGeometry_ThenArgumentExceptionIsThrown()
        {
            // Given
            var coordinates = new[]
            {
                new Coordinate(0, 0)
            };

            // When/Then
            GeometryCreator.CreatePolyLineGeometry(coordinates);
        }
    }
}