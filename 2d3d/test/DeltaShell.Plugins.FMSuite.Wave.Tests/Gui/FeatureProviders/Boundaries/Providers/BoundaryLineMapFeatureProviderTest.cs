using System;
using System.Collections;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Features;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Providers
{
    [TestFixture]
    public class BoundaryLineMapFeatureProviderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup 
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var addBehaviour = Substitute.For<IAddBehaviour>();

            // Call
            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                                            coordinateSystem,
                                                                            geometryFactory,
                                                                            addBehaviour))
            {
                // Assert
                Assert.That(featureProvider, Is.InstanceOf<Feature2DCollection>());
                Assert.That(featureProvider.Features, Is.Not.Null);
                Assert.That(featureProvider.Features, Is.Empty);
                Assert.That(featureProvider.CoordinateSystem, Is.SameAs(coordinateSystem));
            }
        }

        [Test]
        public void Constructor_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            // Setup
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var addBehaviour = Substitute.For<IAddBehaviour>();

            // Call
            void Call() => new BoundaryLineMapFeatureProvider(null,
                                                              coordinateSystem,
                                                              geometryFactory,
                                                              addBehaviour);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("boundaryProvider"));
        }

        [Test]
        public void Constructor_AddBehaviourNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            // Call
            void Call() => new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                              coordinateSystem,
                                                              geometryFactory,
                                                              null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("addBehaviour"));
        }

        [Test]
        public void Constructor_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var addBehaviour = Substitute.For<IAddBehaviour>();

            // Call
            void Call() => new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                              coordinateSystem,
                                                              null,
                                                              addBehaviour);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundaryGeometryFactory"));
        }

        [Test]
        public void AddGeometry_CallsAddBehaviour()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var addBehaviour = Substitute.For<IAddBehaviour>();

            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                                            coordinateSystem,
                                                                            geometryFactory,
                                                                            addBehaviour))
            {
                var geometry = Substitute.For<IGeometry>();

                // Call
                IFeature result = featureProvider.Add(geometry);

                // Assert
                Assert.That(result, Is.Null);
                addBehaviour.Received(1).Execute(geometry);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenABoundaryLineMapFeatureProviderWithAddBehaviour_WhenTheUnderlyingEventedListIsChanged_ThenFeaturesChangedIsCalled()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            var geometry = Substitute.For<ILineString>();
            var boundary = Substitute.For<IWaveBoundary>();

            var boundaries = new EventedList<IWaveBoundary>();
            boundaryProvider.Boundaries.Returns(boundaries);

            geometryFactory.ConstructBoundaryLineGeometry(boundary).Returns(geometry);

            var nCalls = 0;

            void ObserveFeatures(object sender, EventArgs e)
            {
                nCalls += 1;
            }

            var addBehaviour = new BoundaryFromLineAddBehaviour(boundaryProvider,
                                                                waveBoundaryFactory);

            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                                            coordinateSystem,
                                                                            geometryFactory,
                                                                            addBehaviour))
            {
                featureProvider.FeaturesChanged += ObserveFeatures;

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
        public void Add_IFeature_ThrowsNotSupportedException()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var addBehaviour = Substitute.For<IAddBehaviour>();

            var feature = Substitute.For<IFeature>();

            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                                            coordinateSystem,
                                                                            geometryFactory,
                                                                            addBehaviour))
            {
                void Call() => featureProvider.Add(feature);
                Assert.Throws<NotSupportedException>(Call);
            }
        }

        [Test]
        public void Features_Set_ThrowsNotSupportedException()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var addBehaviour = Substitute.For<IAddBehaviour>();

            var features = Substitute.For<IList>();

            using (var featureProvider = new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                                            coordinateSystem,
                                                                            geometryFactory,
                                                                            addBehaviour))
            {
                void Call() => featureProvider.Features = features;
                Assert.Throws<NotSupportedException>(Call);
            }
        }
    }
}