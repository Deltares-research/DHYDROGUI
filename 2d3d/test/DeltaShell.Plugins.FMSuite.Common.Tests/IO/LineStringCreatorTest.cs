using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class LineStringCreatorTest
    {
        [Test]
        public void GivenCollectionOfCoordinates_WhenCreatingLineString_ThenLineStringWithTheSameCoordinatesIsReturned()
        {
            // Given
            var coordinates = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1)
            };

            // When
            LineString lineString = LineStringCreator.CreateLineString(coordinates);

            // Then
            Assert.That(lineString.Coordinates, Is.EqualTo(coordinates));
        }

        [Test]
        public void GivenCollectionOfCoordinatesSmallerThan2_WhenCreatingLineString_ThenArgumentExceptionIsThrown()
        {
            // Given
            var coordinates = new[]
            {
                new Coordinate(0, 0)
            };

            // When/Then
            Assert.That(() => LineStringCreator.CreateLineString(coordinates),
                Throws.ArgumentException.With.Message.EqualTo("Cannot create poly line with less than 2 points."));
        }
    }
}