using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours;
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
            var featuresFromBoundaryBehaviour = Substitute.For<IFeaturesFromBoundaryBehaviour>();

            var addBehaviour = Substitute.For<IAddBehaviour>();

            var lineFeatureProvider = new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                                         coordSystem,
                                                                         geometryFactory,
                                                                         addBehaviour);

            var startPointFeatureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                   coordSystem,
                                                                                   featuresFromBoundaryBehaviour);

            var endPointFeatureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                 coordSystem,
                                                                                 featuresFromBoundaryBehaviour);

            var supportPointFeatureProvider = new BoundarySupportPointMapFeatureProvider(boundaryProvider,
                                                                                         coordSystem,
                                                                                         geometryFactory);

            // Call
            var boundaryMapFeaturesContainer = new BoundaryMapFeaturesContainer(lineFeatureProvider,
                                                                                startPointFeatureProvider,
                                                                                endPointFeatureProvider,
                                                                                supportPointFeatureProvider);

            // Assert
            Assert.That(boundaryMapFeaturesContainer, Is.InstanceOf<IBoundaryMapFeaturesContainer>());

            Assert.That(boundaryMapFeaturesContainer.BoundaryLineMapFeatureProvider,
                        Is.SameAs(lineFeatureProvider));
            Assert.That(boundaryMapFeaturesContainer.BoundaryStartPointMapFeatureProvider,
                        Is.SameAs(startPointFeatureProvider));
            Assert.That(boundaryMapFeaturesContainer.BoundaryEndPointMapFeatureProvider,
                        Is.SameAs(endPointFeatureProvider));
            Assert.That(boundaryMapFeaturesContainer.SupportPointMapFeatureProvider,
                        Is.SameAs(supportPointFeatureProvider));
        }

        [Test]
        [TestCaseSource(nameof(ConstructorParameterNullTestData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(BoundaryLineMapFeatureProvider lineMapFeatureProvider,
                                                                          BoundaryReadOnlyMapFeatureProvider startPointFeatureProvider,
                                                                          BoundaryReadOnlyMapFeatureProvider endPointFeatureProvider,
                                                                          BoundarySupportPointMapFeatureProvider supportPointFeatureProvider,
                                                                          string expectedParameterName)
        {
            void Call() => new BoundaryMapFeaturesContainer(lineMapFeatureProvider,
                                                            startPointFeatureProvider,
                                                            endPointFeatureProvider,
                                                            supportPointFeatureProvider);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        private static IEnumerable<TestCaseData> ConstructorParameterNullTestData()
        {
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            var coordSystem = Substitute.For<ICoordinateSystem>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var featuresFromBoundaryBehaviour = Substitute.For<IFeaturesFromBoundaryBehaviour>();

            var addBehaviour = Substitute.For<IAddBehaviour>();

            var lineFeatureProvider = new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                                         coordSystem,
                                                                         geometryFactory,
                                                                         addBehaviour);

            var startPointFeatureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                   coordSystem,
                                                                                   featuresFromBoundaryBehaviour);

            var endPointFeatureProvider = new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                                                 coordSystem,
                                                                                 featuresFromBoundaryBehaviour);

            var supportPointFeatureProvider = new BoundarySupportPointMapFeatureProvider(boundaryProvider,
                                                                                         coordSystem,
                                                                                         geometryFactory);

            yield return new TestCaseData(null, startPointFeatureProvider, endPointFeatureProvider, supportPointFeatureProvider, "boundaryLineMapFeatureProvider");
            yield return new TestCaseData(lineFeatureProvider, null, endPointFeatureProvider, supportPointFeatureProvider, "boundaryStartPointMapFeatureProvider");
            yield return new TestCaseData(lineFeatureProvider, startPointFeatureProvider, null, supportPointFeatureProvider, "boundaryEndPointMapFeatureProvider");
            yield return new TestCaseData(lineFeatureProvider, startPointFeatureProvider, endPointFeatureProvider, null, "supportPointMapFeatureProvider");
        }
    }
}