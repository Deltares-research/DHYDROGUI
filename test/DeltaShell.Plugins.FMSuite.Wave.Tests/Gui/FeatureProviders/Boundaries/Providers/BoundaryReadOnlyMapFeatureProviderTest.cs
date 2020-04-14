using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
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
            IEnumerable<IFeature> f(IWaveBoundary boundary) => new[] {Substitute.For<IFeature>()};

            // Call
            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider, 
                                                                                coordinateSystem,
                                                                                f))
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
            IEnumerable<IFeature> f(IWaveBoundary boundary) => new[] {Substitute.For<IFeature>()};

            // Call
            void Call() => new BoundaryReadOnlyMapFeatureProvider(null, 
                                                                  coordinateSystem, 
                                                                  f);

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
            Assert.That(exception.ParamName, Is.EqualTo("constructFeaturesFromBoundaryFunc"));
        }

        [Test]
        public void GivenABoundaryEndPointMapFeatureProviderWithBoundaries_WhenFeaturesAreRetrieved_ThenTheCorrectEndPointsAreGenerated()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            var boundaries = new EventedList<IWaveBoundary>
            {
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>(),
            };

            boundaryProvider.Boundaries.Returns(boundaries);

            List<Point> endPoints = Enumerable.Range(0, 6)
                                              .Select(x => new Point(new Coordinate(x + 0.5, -x + 0.5)))
                                              .ToList();

            for (var i = 0; i < boundaries.Count; i++)
            {
                geometryFactory.ConstructBoundaryEndPoints(boundaries[i])
                               .Returns(endPoints.Skip(i * 2).Take(2));
            }

            IEnumerable<IFeature> ConstructEndPointFeatures(IWaveBoundary boundary) => 
                geometryFactory.ConstructBoundaryEndPoints(boundary)
                               .Select(p => new Feature2DPoint { Geometry = p});

            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                coordinateSystem,
                                                                                ConstructEndPointFeatures))
            {
                // Call
                List<Feature2DPoint> endPointFeatures = featureProvider.Features.Cast<Feature2DPoint>().ToList();

                // Assert
                Assert.That(endPointFeatures, Has.Count.EqualTo(endPoints.Count));
                Assert.That(endPointFeatures.Distinct().Count(), Is.EqualTo(endPointFeatures.Count));

                foreach (Feature2DPoint feat in endPointFeatures)
                {
                    Assert.That(endPoints.Contains(feat.Geometry),
                                $"Expected {feat.Geometry} to be contained in endPoints.");
                }
            }
        }

        [Test]
        public void Features_Set_ThrowsNotSupportedException()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            IEnumerable<IFeature> f(IWaveBoundary boundary) => new[] {Substitute.For<IFeature>()};

            // Call
            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                coordinateSystem,
                                                                                f))
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
            IEnumerable<IFeature> f(IWaveBoundary boundary) => new[] {Substitute.For<IFeature>()};

            // Call
            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                coordinateSystem,
                                                                                f))
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
            IEnumerable<IFeature> f(IWaveBoundary boundary) => new[] {Substitute.For<IFeature>()};

            // Call
            using (var featureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                coordinateSystem,
                                                                                f))
            {
                void Call() => featureProvider.Add(Substitute.For<IFeature>());

                Assert.Throws<NotSupportedException>(Call);
            }
        }
    }
}