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

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryContainer"));
        }

        [Test]
        public void ConstructReadOnlyBoundaryMapFeaturesContainer_ExpectedResults()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordSystem = Substitute.For<ICoordinateSystem>();

            // Call
            IBoundaryMapFeaturesContainer result =
                BoundaryMapFeaturesContainerFactory.ConstructReadOnlyBoundaryMapFeaturesContainer(boundaryContainer,
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
                Substitute.For<ICoordinateSystem>());

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryContainer"));
        }
        
    }
}