using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours
{
    [TestFixture]
    public class StartingPointFromBoundaryBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            var behaviour = new StartingPointFromBoundaryBehaviour(geometryFactory);

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IFeaturesFromBoundaryBehaviour>());
        }

        [Test]
        public void Constructor_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            void Call() => new StartingPointFromBoundaryBehaviour(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometryFactory"));
        }

        [Test]
        public void Execute_ValidBoundary_ExpectedFeature()
        {
            // Setup
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var waveBoundary = Substitute.For<IWaveBoundary>();
            var point = Substitute.For<IPoint>();

            geometryFactory.ConstructBoundaryStartPoint(waveBoundary).Returns(point);

            var behaviour = new StartingPointFromBoundaryBehaviour(geometryFactory);

            // Call
            List<IFeature> result = behaviour.Execute(waveBoundary)?.ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));

            IFeature feature = result.First();
            Assert.That(feature.Geometry, Is.SameAs(point));

            geometryFactory.Received(1).ConstructBoundaryStartPoint(waveBoundary);
        }

        [Test]
        public void Execute_NoPointGenerated_NoFeature()
        {
            // Setup
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var waveBoundary = Substitute.For<IWaveBoundary>();

            geometryFactory.ConstructBoundaryStartPoint(waveBoundary).Returns((IPoint) null);

            var behaviour = new StartingPointFromBoundaryBehaviour(geometryFactory);

            // Call
            List<IFeature> result = behaviour.Execute(waveBoundary)?.ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);

            geometryFactory.Received(1).ConstructBoundaryStartPoint(waveBoundary);
        }

        [Test]
        public void Execute_WaveBoundaryNull_ThrowsArgumentNullException()
        {
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var behaviour = new StartingPointFromBoundaryBehaviour(geometryFactory);

            void Call() => behaviour.Execute(null).ToList();

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }
    }
}