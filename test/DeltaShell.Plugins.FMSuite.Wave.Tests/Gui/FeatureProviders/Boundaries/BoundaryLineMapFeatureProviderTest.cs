using System;
using System.Collections;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Data.Providers;
using IGeometryFactory = DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.IGeometryFactory;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries
{
    [TestFixture]
    public class BoundaryLineMapFeatureProviderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup 
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var geometryFactory = Substitute.For<IGeometryFactory>();

            // Call
            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer,
                                                                            waveBoundaryFactory,
                                                                            geometryFactory))
            {
                // Assert
                Assert.That(featureProvider, Is.InstanceOf<Feature2DCollection>());
                Assert.That(featureProvider.Features, Is.Not.Null);
                Assert.That(featureProvider.Features, Is.Empty);
            }
        }

        [Test]
        public void Constructor_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            // Setup
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var geometryFactory = Substitute.For<IGeometryFactory>();

            // Call
            void Call() => new BoundaryLineMapFeatureProvider(null, waveBoundaryFactory, geometryFactory);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("boundaryContainer"));
        }

        [Test]
        public void Constructor_WaveBoundaryFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var geometryFactory = Substitute.For<IGeometryFactory>();

            // Call
            void Call() => new BoundaryLineMapFeatureProvider(boundaryContainer, null, geometryFactory);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveBoundaryFactory"));
        }

        [Test]
        public void Constructor_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();

            // Call
            void Call() => new BoundaryLineMapFeatureProvider(boundaryContainer, waveBoundaryFactory, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("geometryFactory"));
        }

        [Test]
        public void AddGeometry_GeometryNull_ReturnsNullAndDoesNotChangeBoundaries()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var geometryFactory = Substitute.For<IGeometryFactory>();


            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer, 
                                                                            waveBoundaryFactory, 
                                                                            geometryFactory))
            {
                IGeometry geometry = null;

                // Call
                IFeature result = featureProvider.Add(geometry);

                // Assert
                Assert.That(result, Is.Null);
                boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().Add(null);
                boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().AddRange(null);
                waveBoundaryFactory.DidNotReceiveWithAnyArgs().ConstructWaveBoundary(null);
            }
        }

        [Test]
        public void AddGeometry_ConstructedBoundaryNull_ReturnsNullAndDoesNotChangeBoundaries()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var geometryFactory = Substitute.For<IGeometryFactory>();

            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer, 
                                                                            waveBoundaryFactory, 
                                                                            geometryFactory))
            {

                var geometry = Substitute.For<ILineString>();
                IWaveBoundary boundary = null;

                waveBoundaryFactory.ConstructWaveBoundary(geometry).Returns(boundary);

                // Call
                IFeature result = featureProvider.Add(geometry);

                // Assert
                Assert.That(result, Is.Null);
                boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().Add(null);
                boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().AddRange(null);
                waveBoundaryFactory.Received(1).ConstructWaveBoundary(geometry);
            }
        }

        [Test]
        public void AddGeometry_ConstructedBoundaryValid_AddsBoundaryToContainer()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var geometryFactory = Substitute.For<IGeometryFactory>();

            var geometry = Substitute.For<ILineString>();
            var boundary = Substitute.For<IWaveBoundary>();

            waveBoundaryFactory.ConstructWaveBoundary(geometry).Returns(boundary);

            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer,
                                                                            waveBoundaryFactory,
                                                                            geometryFactory))
            {
                // Call
                IFeature result = featureProvider.Add(geometry);

                // Assert
                Assert.That(result, Is.Null);
                boundaryContainer.Boundaries.Received(1).Add(boundary);
                boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().AddRange(null);
                waveBoundaryFactory.Received(1).ConstructWaveBoundary(geometry);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenABoundaryLineMapFeatureProvider_WhenTheUnderlyingEventedListIsChanged_ThenFeaturesChangedIsCalled()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var geometryFactory = Substitute.For<IGeometryFactory>();

            var geometry = Substitute.For<ILineString>();
            var boundary = Substitute.For<IWaveBoundary>();

            var boundaries = new EventedList<IWaveBoundary>();
            boundaryContainer.Boundaries.Returns(boundaries);

            geometryFactory.ConstructBoundaryLineGeometry(boundary).Returns(geometry);

            var nCalls = 0;
            void observeFeatures(object sender, EventArgs e)
            {
                nCalls += 1;
            }

            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer,
                                                                            waveBoundaryFactory,
                                                                            geometryFactory))
            {
                featureProvider.FeaturesChanged += observeFeatures;

                // Call
                boundaries.Add(boundary);

                // Assert
                Assert.That(nCalls, Is.EqualTo(1));
                Assert.That(featureProvider.Features, Has.Count.EqualTo(1));

                var feature = featureProvider.Features[0] as BoundaryLineFeature;
                Assert.That(feature, Is.Not.Null);
                Assert.That(feature.ObservedWaveBoundary, Is.SameAs(boundary));
                Assert.That(feature.Geometry, Is.SameAs(geometry));
            }
        }

        [Test]
        public void Add_IFeature_ThrowsNotSupportedExpection()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var geometryFactory = Substitute.For<IGeometryFactory>();

            var feature = Substitute.For<IFeature>();

            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer,
                                                                            waveBoundaryFactory,
                                                                            geometryFactory))
            {
                void Call() => featureProvider.Add(feature);
                Assert.Throws<NotSupportedException>(Call);
            }
        }

        [Test]
        public void Features_Set_ThrowsNotSupportedExpection()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var geometryFactory = Substitute.For<IGeometryFactory>();

            var features = Substitute.For<IList>();

            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer,
                                                                            waveBoundaryFactory,
                                                                            geometryFactory))
            {
                void Call() => featureProvider.Features = features;
                Assert.Throws<NotSupportedException>(Call);
            }
        }
    }
}