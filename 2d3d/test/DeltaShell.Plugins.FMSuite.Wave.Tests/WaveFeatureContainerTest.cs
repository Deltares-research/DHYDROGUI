using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveFeatureContainerTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var container = new WaveFeatureContainer();

            // Assert
            Assert.That(container, Is.InstanceOf<IWaveFeatureContainer>());
            Assert.That(container.Obstacles, Is.Empty);
            Assert.That(container.ObservationPoints, Is.Empty);
            Assert.That(container.ObservationCrossSections, Is.Empty);
        }

        [Test]
        public void GetAllFeatures_ContainsAllFeatures()
        {
            var container = new WaveFeatureContainer();
            var observationPoints = Create.For<List<Feature2DPoint>>();
            var observationCrossSections = Create.For<List<Feature2D>>();
            var obstacles = Create.For<List<WaveObstacle>>();

            container.ObservationPoints.AddRange(observationPoints);
            container.ObservationCrossSections.AddRange(observationCrossSections);
            container.Obstacles.AddRange(obstacles);

            // Call
            List<IFeature> features = container.GetAllFeatures().ToList();

            // Assert
            Assert.That(features, Has.Count.EqualTo(9));
            CollectionAssert.IsSupersetOf(features, observationPoints);
            CollectionAssert.IsSupersetOf(features, observationCrossSections);
            CollectionAssert.IsSupersetOf(features, obstacles);
        }
    }
}