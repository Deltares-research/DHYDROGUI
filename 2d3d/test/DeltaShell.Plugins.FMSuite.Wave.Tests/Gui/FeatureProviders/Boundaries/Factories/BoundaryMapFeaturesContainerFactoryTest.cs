using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Extensions.CoordinateSystems;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Factories
{
    [TestFixture]
    public class BoundaryMapFeaturesContainerFactoryTest
    {
        [Test]
        public void ConstructEditableBoundaryMapFeaturesContainer_ExpectedResults()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordSystem = Substitute.For<ICoordinateSystem>();

            // Call
            IBoundaryMapFeaturesContainer result =
                BoundaryMapFeaturesContainerFactory.ConstructEditableBoundaryMapFeaturesContainer(boundaryContainer,
                                                                                                  coordSystem);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.DoesNotThrow(() => result.BoundaryLineMapFeatureProvider.Add(null));
        }

        [Test]
        public void ConstructEditableBoundaryMapFeaturesContainer_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => BoundaryMapFeaturesContainerFactory.ConstructEditableBoundaryMapFeaturesContainer(
                null,
                Substitute.For<ICoordinateSystem>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryContainer"));
        }

        [Test]
        public void ConstructReadOnlyBoundaryMapFeaturesContainer_ExpectedResults()
        {
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var coordSystem = Substitute.For<ICoordinateSystem>();

            // Call
            IBoundaryMapFeaturesContainer result =
                BoundaryMapFeaturesContainerFactory.ConstructReadOnlyBoundaryMapFeaturesContainer(boundaryProvider,
                                                                                                  geometryFactory,
                                                                                                  coordSystem);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Throws<NotSupportedException>(() => result.BoundaryLineMapFeatureProvider.Add(null));
        }

        [Test]
        public void ConstructReadOnlyBoundaryMapFeaturesContainer_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => BoundaryMapFeaturesContainerFactory.ConstructReadOnlyBoundaryMapFeaturesContainer(
                null,
                Substitute.For<IWaveBoundaryGeometryFactory>(),
                Substitute.For<ICoordinateSystem>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryProvider"));
        }

        [Test]
        public void ConstructReadOnlyBoundaryMapFeaturesContainer_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => BoundaryMapFeaturesContainerFactory.ConstructReadOnlyBoundaryMapFeaturesContainer(
                Substitute.For<IBoundaryProvider>(),
                null,
                Substitute.For<ICoordinateSystem>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometryFactory"));
        }
    }
}