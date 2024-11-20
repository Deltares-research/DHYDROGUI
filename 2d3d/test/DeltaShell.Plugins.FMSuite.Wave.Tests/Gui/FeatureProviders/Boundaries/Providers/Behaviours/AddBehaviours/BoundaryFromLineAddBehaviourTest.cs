using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours
{
    [TestFixture]
    public class BoundaryFromLineAddBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var provider = Substitute.For<IBoundaryProvider>();
            var factory = Substitute.For<IWaveBoundaryFactory>();

            // Call
            var addBehaviour = new BoundaryFromLineAddBehaviour(provider,
                                                                factory);

            // Assert
            Assert.That(addBehaviour, Is.InstanceOf<IAddBehaviour>());
        }

        [Test]
        public void Constructor_BoundaryProviderNull_ThrowsArgumentNullException()
        {
            void Call() => new BoundaryFromLineAddBehaviour(null,
                                                            Substitute.For<IWaveBoundaryFactory>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryProvider"));
        }

        [Test]
        public void Constructor_WaveBoundaryFactoryNull_ThrowsArgumentNullException()
        {
            void Call() => new BoundaryFromLineAddBehaviour(Substitute.For<IBoundaryProvider>(),
                                                            null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundaryFactory"));
        }

        [Test]
        public void Execute_GeometryNull_ReturnsNullAndDoesNotChangeBoundaries()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var addBehaviour = new BoundaryFromLineAddBehaviour(boundaryProvider,
                                                                waveBoundaryFactory);

            IGeometry geometry = null;

            // Call
            addBehaviour.Execute(geometry);

            // Assert
            boundaryProvider.Boundaries.DidNotReceiveWithAnyArgs().Add(null);
            boundaryProvider.Boundaries.DidNotReceiveWithAnyArgs().AddRange(null);
            waveBoundaryFactory.DidNotReceiveWithAnyArgs().ConstructWaveBoundary(null);
        }

        [Test]
        public void Execute_ConstructedBoundaryNull_ReturnsNullAndDoesNotChangeBoundaries()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var addBehaviour = new BoundaryFromLineAddBehaviour(boundaryProvider,
                                                                waveBoundaryFactory);

            var geometry = Substitute.For<ILineString>();
            IWaveBoundary boundary = null;

            waveBoundaryFactory.ConstructWaveBoundary(geometry).Returns(boundary);

            // Call
            addBehaviour.Execute(geometry);

            // Assert
            boundaryProvider.Boundaries.DidNotReceiveWithAnyArgs().Add(null);
            boundaryProvider.Boundaries.DidNotReceiveWithAnyArgs().AddRange(null);
            waveBoundaryFactory.Received(1).ConstructWaveBoundary(geometry);
        }

        [Test]
        public void Execute_ConstructedBoundaryValid_AddsBoundaryToContainer()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var waveBoundaryFactory = Substitute.For<IWaveBoundaryFactory>();
            var addBehaviour = new BoundaryFromLineAddBehaviour(boundaryProvider,
                                                                waveBoundaryFactory);

            var geometry = Substitute.For<ILineString>();
            var boundary = Substitute.For<IWaveBoundary>();

            waveBoundaryFactory.ConstructWaveBoundary(geometry).Returns(boundary);

            // Call
            addBehaviour.Execute(geometry);

            // Assert
            boundaryProvider.Boundaries.Received(1).Add(boundary);
            boundaryProvider.Boundaries.DidNotReceiveWithAnyArgs().AddRange(null);
            waveBoundaryFactory.Received(1).ConstructWaveBoundary(geometry);
        }
    }
}