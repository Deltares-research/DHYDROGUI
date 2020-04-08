using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using GeoAPI.Extensions.CoordinateSystems;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Containers
{
    [TestFixture]
    public class BoundaryMapFeaturesContainerTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Assert
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            // Call
            var boundaryMapFeaturesContainer = new BoundaryMapFeaturesContainer(boundaryContainer, 
                                                                                coordinateSystem);

            // Assert
            Assert.That(boundaryMapFeaturesContainer.BoundaryEndPointMapFeatureProvider, Is.Not.Null);
            Assert.That(boundaryMapFeaturesContainer.BoundaryLineMapFeatureProvider, Is.Not.Null);
            Assert.That(boundaryMapFeaturesContainer.SupportPointMapFeatureProvider, Is.Not.Null);
        }

        [Test]
        public void Constructor_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            // Call
            void Call() => new BoundaryMapFeaturesContainer(null, coordinateSystem);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryContainer"));
        }
    }
}