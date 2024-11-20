using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Providers
{
    [TestFixture]
    public class BoundaryReadOnlyMapFeatureProviderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var behaviour = Substitute.For<IFeaturesFromBoundaryBehaviour>();

            // Call
            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                coordinateSystem,
                                                                                behaviour))
            {
                // Assert
                Assert.That(featureProvider, Is.InstanceOf(typeof(FeatureCollection)));
                Assert.That(featureProvider.FeatureType, Is.EqualTo(typeof(Feature2DPoint)));
                Assert.That(featureProvider.Features, Is.Empty);
                Assert.That(featureProvider.CoordinateSystem, Is.SameAs(coordinateSystem));
            }
        }

        [Test]
        public void Constructor_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            // Setup
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var behaviour = Substitute.For<IFeaturesFromBoundaryBehaviour>();

            // Call
            void Call() => new BoundaryReadOnlyMapFeatureProvider(null,
                                                                  coordinateSystem,
                                                                  behaviour);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryProvider"));
        }

        [Test]
        public void Constructor_GeometryFactory_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            // Call
            void Call() => new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                  coordinateSystem,
                                                                  null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featuresFromBoundaryBehaviour"));
        }

        [Test]
        public void GivenABoundaryReadOnlyMapFeatureProviderWithBoundaries_WhenFeaturesAreRetrieved_ThenTheCorrectFeaturesAreGenerated()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var behaviour = Substitute.For<IFeaturesFromBoundaryBehaviour>();

            var boundaries = new EventedList<IWaveBoundary>
            {
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>()
            };

            boundaryProvider.Boundaries.Returns(boundaries);

            List<IFeature> endPoints = Enumerable.Range(0, 6)
                                                 .Select(x => Substitute.For<IFeature>())
                                                 .ToList();

            for (var i = 0; i < boundaries.Count; i++)
            {
                behaviour.Execute(boundaries[i])
                         .Returns(endPoints.Skip(i * 2).Take(2));
            }

            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                coordinateSystem,
                                                                                behaviour))
            {
                // Call
                List<IFeature> endPointFeatures = featureProvider.Features.Cast<IFeature>().ToList();

                // Assert
                Assert.That(endPointFeatures, Has.Count.EqualTo(endPoints.Count));
                Assert.That(endPointFeatures.Distinct().Count(), Is.EqualTo(endPointFeatures.Count));

                foreach (IFeature feat in endPointFeatures)
                {
                    Assert.That(endPoints.Contains(feat), $"Expected {feat} to be contained in endPoints.");
                }
            }
        }

        [Test]
        public void Features_Set_ThrowsNotSupportedException()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var behaviour = Substitute.For<IFeaturesFromBoundaryBehaviour>();

            // Call
            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                coordinateSystem,
                                                                                behaviour))
            {
                void Call() => featureProvider.Features = Substitute.For<IList>();

                Assert.Throws<NotSupportedException>(Call);
            }
        }

        [Test]
        public void Add_Geometry_ThrowsNotSupportedException()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var behaviour = Substitute.For<IFeaturesFromBoundaryBehaviour>();

            // Call
            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                coordinateSystem,
                                                                                behaviour))
            {
                void Call() => featureProvider.Add(Substitute.For<IGeometry>());

                Assert.Throws<NotSupportedException>(Call);
            }
        }

        [Test]
        public void Add_Feature_ThrowsNotSupportedException()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var behaviour = Substitute.For<IFeaturesFromBoundaryBehaviour>();

            // Call
            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                coordinateSystem,
                                                                                behaviour))
            {
                void Call() => featureProvider.Add(Substitute.For<IFeature>());

                Assert.Throws<NotSupportedException>(Call);
            }
        }
    }
}