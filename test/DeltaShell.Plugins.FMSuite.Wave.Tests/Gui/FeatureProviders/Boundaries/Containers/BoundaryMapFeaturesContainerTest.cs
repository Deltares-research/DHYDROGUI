using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours;
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
            // Setup
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            var addBehaviour = Substitute.For<IAddBehaviour>();

            var lineFeatureProvider = new BoundaryLineMapFeatureProvider(boundaryProvider, 
                                                                         coordSystem, 
                                                                         geometryFactory, 
                                                                         addBehaviour);

            var endPointFeatureProvider = new BoundaryEndPointMapFeatureProvider(boundaryProvider, 
                                                                                 coordSystem, 
                                                                                 geometryFactory);

            var supportPointFeatureProvider = new BoundarySupportPointMapFeatureProvider(boundaryProvider, 
                                                                                         coordSystem, 
                                                                                         geometryFactory);

            // Call
            var boundaryMapFeaturesContainer = new BoundaryMapFeaturesContainer(lineFeatureProvider, 
                                                                                endPointFeatureProvider, 
                                                                                supportPointFeatureProvider);

            // Assert
            Assert.That(boundaryMapFeaturesContainer, Is.InstanceOf<IBoundaryMapFeaturesContainer>());

            Assert.That(boundaryMapFeaturesContainer.BoundaryEndPointMapFeatureProvider, 
                        Is.SameAs(lineFeatureProvider));
            Assert.That(boundaryMapFeaturesContainer.BoundaryLineMapFeatureProvider, 
                        Is.SameAs(endPointFeatureProvider));
            Assert.That(boundaryMapFeaturesContainer.SupportPointMapFeatureProvider, 
                        Is.SameAs(supportPointFeatureProvider));
        }

        private static IEnumerable<TestCaseData> ConstructorParameterNullTestData()
        {
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            var addBehaviour = Substitute.For<IAddBehaviour>();

            var lineFeatureProvider = new BoundaryLineMapFeatureProvider(boundaryProvider, 
                                                                         coordSystem, 
                                                                         geometryFactory, 
                                                                         addBehaviour);

            var endPointFeatureProvider = new BoundaryEndPointMapFeatureProvider(boundaryProvider, 
                                                                                 coordSystem, 
                                                                                 geometryFactory);

            var supportPointFeatureProvider = new BoundarySupportPointMapFeatureProvider(boundaryProvider, 
                                                                                         coordSystem, 
                                                                                         geometryFactory);

            yield return new TestCaseData(null, endPointFeatureProvider, supportPointFeatureProvider, "boundaryLineMapFeatureProvider");
            yield return new TestCaseData(lineFeatureProvider, null, supportPointFeatureProvider, "boundaryEndPointMapFeatureProvider");
            yield return new TestCaseData(lineFeatureProvider, endPointFeatureProvider, null, "supportPointMapFeatureProvider");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorParameterNullTestData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(BoundaryLineMapFeatureProvider lineMapFeatureProvider,
                                                                          BoundaryEndPointMapFeatureProvider endPointFeatureProvider,
                                                                          BoundarySupportPointMapFeatureProvider supportPointFeatureProvider,
                                                                          string expectedParameterName)
        {
            void Call() => new BoundaryMapFeaturesContainer(lineMapFeatureProvider,
                                                            endPointFeatureProvider,
                                                            supportPointFeatureProvider);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }
    }
}