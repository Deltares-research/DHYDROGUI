using System;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.FeatureData
{
    [TestFixture]
    public class BoundaryConditionsPointsSyncedListTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var list = new BoundaryConditionsPointsSyncedList();

            // Assert
            Assert.IsInstanceOf<GeometryPointsSyncedList<string>>(list);
        }

        [Test]
        public void ToString_WithCreationMethodSet_ReturnsExpectedStringRepresentation()
        {
            // Setup
            var feature2D = new Feature2D
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(2, 0)
                }),
                Name = "Feature"
            };

            Func<IFeature, int, string> creationMethod = (feature, i) => $"Creation_{i}";
            var boundaryConditionPoints = new BoundaryConditionsPointsSyncedList
            {
                CreationMethod = creationMethod,
                Feature = feature2D
            };

            // Call
            var stringRepresentation = boundaryConditionPoints.ToString();

            // Assert
            Assert.AreEqual("Creation_0, Creation_1, Creation_2", stringRepresentation);
        }

        [Test]
        public void ToString_WithoutCreationMethodSet_ReturnsDefaultToString()
        {
            // Setup
            var feature2D = new Feature2D
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(2, 0)
                }),
                Name = "Feature"
            };

            var boundaryConditionPoints = new BoundaryConditionsPointsSyncedList {Feature = feature2D};

            // Call
            var stringRepresentation = boundaryConditionPoints.ToString();

            // Assert
            Assert.AreEqual(boundaryConditionPoints.GetType().FullName, stringRepresentation);
        }
    }
}