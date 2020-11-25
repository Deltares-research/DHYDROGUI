using System.Collections.Generic;
using System.Linq;
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
            List<Feature2DPoint> observationPoints = GetSome<Feature2DPoint>().ToList();
            List<Feature2D> observationCrossSections = GetSome<Feature2D>().ToList();
            List<WaveObstacle> obstacles = GetSome<WaveObstacle>().ToList();

            container.ObservationPoints.AddRange(observationPoints);
            container.ObservationCrossSections.AddRange(observationCrossSections);
            container.Obstacles.AddRange(obstacles);

            // Call
            List<IFeature> features = container.GetAllFeatures().ToList();

            // Assert
            CollectionAssert.IsSupersetOf(features, observationPoints);
            CollectionAssert.IsSupersetOf(features, observationCrossSections);
            CollectionAssert.IsSupersetOf(features, obstacles);
        }

        private IEnumerable<T> GetSome<T>() where T : new()
        {
            for (var i = 0; i < 3; i++)
            {
                yield return new T();
            }
        }
    }
}