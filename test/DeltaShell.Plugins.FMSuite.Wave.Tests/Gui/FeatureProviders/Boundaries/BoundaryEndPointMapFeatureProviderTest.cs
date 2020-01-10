using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries
{
    [TestFixture]
    public class BoundaryEndPointMapFeatureProviderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            using (var featureProvider = new BoundaryEndPointMapFeatureProvider(boundaryContainer, 
                                                                                coordinateSystem,
                                                                                geometryFactory))
            {
                // Assert
                Assert.That(featureProvider, Is.InstanceOf(typeof(FeatureCollection)));
                Assert.That(featureProvider.FeatureType, Is.EqualTo(typeof(Feature2DPoint)));
                Assert.That(featureProvider.Features, Is.Empty);
            }
        }

        [Test]
        public void Constructor_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            // Setup
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            void Call() => new BoundaryEndPointMapFeatureProvider(null, 
                                                                  coordinateSystem, 
                                                                  geometryFactory);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryContainer"));
        }

        [Test]
        public void Constructor_GeometryFactory_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            // Call
            void Call() => new BoundaryEndPointMapFeatureProvider(boundaryContainer, 
                                                                  coordinateSystem, 
                                                                  null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundaryGeometryFactory"));
        }

        [Test]
        public void GivenABoundaryEndPointMapFeatureProviderWithBoundaries_WhenFeaturesAreRetrieved_ThenTheCorrectEndPointsAreGenerated()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            var boundaries = new EventedList<IWaveBoundary>
            {
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>(),
            };

            boundaryContainer.Boundaries.Returns(boundaries);

            List<Point> endPoints = Enumerable.Range(0, 6)
                                              .Select(x => new Point(new Coordinate(x + 0.5, -x + 0.5)))
                                              .ToList();

            for (var i = 0; i < boundaries.Count; i++)
            {
                geometryFactory.ConstructBoundaryEndPoints(boundaries[i])
                               .Returns(endPoints.Skip(i * 2).Take(2));
            }

            using (var featureProvider = new BoundaryEndPointMapFeatureProvider(boundaryContainer,
                                                                                coordinateSystem,
                                                                                geometryFactory))
            {
                // Call
                List<Feature2DPoint> endPointFeatures = featureProvider.Features.Cast<Feature2DPoint>().ToList();

                // Assert
                Assert.That(endPointFeatures, Has.Count.EqualTo(endPoints.Count));
                Assert.That(endPointFeatures.Distinct().Count(), Is.EqualTo(endPointFeatures.Count));

                foreach (var feat in endPointFeatures)
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
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            using (var featureProvider = new BoundaryEndPointMapFeatureProvider(boundaryContainer,
                                                                                coordinateSystem,
                                                                                geometryFactory))
            {
                void Call() => featureProvider.Features = Substitute.For<IList>();

                Assert.Throws<NotSupportedException>(Call);
            }
        }

        [Test]
        public void Add_Geometry_ThrowsNotSupportedException()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            using (var featureProvider = new BoundaryEndPointMapFeatureProvider(boundaryContainer,
                                                                                coordinateSystem,
                                                                                geometryFactory))
            {
                void Call() => featureProvider.Add(Substitute.For<IGeometry>());

                Assert.Throws<NotSupportedException>(Call);
            }
        }

        [Test]
        public void Add_Feature_ThrowsNotSupportedException()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            using (var featureProvider = new BoundaryEndPointMapFeatureProvider(boundaryContainer,
                                                                                coordinateSystem,
                                                                                geometryFactory))
            {
                void Call() => featureProvider.Add(Substitute.For<IFeature>());

                Assert.Throws<NotSupportedException>(Call);
            }
        }
    }
}