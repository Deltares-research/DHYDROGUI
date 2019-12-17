using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Data.Providers;

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
            var factory = Substitute.For<IWaveBoundaryFactory>();

            // Call
            var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer, factory);

            // Assert
            Assert.That(featureProvider, Is.InstanceOf<Feature2DCollection>());
        }

        [Test]
        public void Constructor_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = Substitute.For<IWaveBoundaryFactory>();

            // Call
            void Call() => new BoundaryLineMapFeatureProvider(null, factory);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("boundaryContainer"));
        }

        [Test]
        public void Constructor_FactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();

            // Call
            void Call() => new BoundaryLineMapFeatureProvider(boundaryContainer, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("factory"));
        }


        [Test]
        public void AddGeometry_GeometryNull_ReturnsNullAndDoesNotChangeBoundaries()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var factory = Substitute.For<IWaveBoundaryFactory>();

            var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer, factory);

            IGeometry geometry = null;

            // Call
            IFeature result = featureProvider.Add(geometry);

            // Assert
            Assert.That(result, Is.Null);
            boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().Add(null);
            boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().AddRange(null);
            factory.DidNotReceiveWithAnyArgs().ConstructWaveBoundary(null);
        }

        [Test]
        public void AddGeometry_ConstructedBoundaryNull_ReturnsNullAndDoesNotChangeBoundaries()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var factory = Substitute.For<IWaveBoundaryFactory>();

            var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer, factory);

            var geometry = Substitute.For<ILineString>();
            IWaveBoundary boundary = null;

            factory.ConstructWaveBoundary(geometry).Returns(boundary);

            // Call
            IFeature result = featureProvider.Add(geometry);

            // Assert
            Assert.That(result, Is.Null);
            boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().Add(null);
            boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().AddRange(null);
            factory.Received(1).ConstructWaveBoundary(geometry);
        }

        [Test]
        public void AddGeometry_ConstructedBoundaryValid_AddsBoundaryToContainer()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var factory = Substitute.For<IWaveBoundaryFactory>();

            var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer, factory);

            var geometry = Substitute.For<ILineString>();
            var boundary = Substitute.For<IWaveBoundary>();

            factory.ConstructWaveBoundary(geometry).Returns(boundary);

            // Call
            IFeature result = featureProvider.Add(geometry);

            // Assert
            Assert.That(result, Is.Null);
            boundaryContainer.Boundaries.Received(1).Add(boundary);
            boundaryContainer.Boundaries.DidNotReceiveWithAnyArgs().AddRange(null);
            factory.Received(1).ConstructWaveBoundary(geometry);
        }
    }
}