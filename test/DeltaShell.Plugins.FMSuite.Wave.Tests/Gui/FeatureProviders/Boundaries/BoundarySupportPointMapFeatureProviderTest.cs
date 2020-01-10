using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Data.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries
{
    [TestFixture]
    public class BoundarySupportPointMapFeatureProviderTest
    {
        private readonly Random random = new Random();
        private IBoundaryContainer boundaryContainer;
        private IWaveBoundaryGeometryFactory waveBoundaryGeometryFactory;

        [SetUp]
        public void SetUp()
        {
            boundaryContainer = Substitute.For<IBoundaryContainer>();
            waveBoundaryGeometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            using (var featureProvider = new BoundarySupportPointMapFeatureProvider(boundaryContainer,
                                                                                    waveBoundaryGeometryFactory))
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
            // Call
            void Call() => new BoundarySupportPointMapFeatureProvider(null, waveBoundaryGeometryFactory);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("boundaryContainer"));
        }

        [Test]
        public void Constructor_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BoundarySupportPointMapFeatureProvider(boundaryContainer, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveBoundaryGeometryFactory"));
        }

        [Test]
        public void AddGeometry_Add_IFeature_ThrowsNotSupportedException()
        {
            using (var featureProvider = new BoundarySupportPointMapFeatureProvider(boundaryContainer,
                                                                                    waveBoundaryGeometryFactory))
            {
                // Call
                void Call() => featureProvider.Add(Substitute.For<IGeometry>());

                // Assert
                Assert.That(Call, Throws.TypeOf<NotImplementedException>()
                                        .With.Message.EqualTo("Should be implemented when support points can be added from the User Interface."));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenABoundarySupportPointMapFeatureProvider_WhenTheUnderlyingEventedListIsChanged_ThenFeaturesChangedIsCalled()
        {
            // Setup
            IWaveBoundary boundary = CreateBoundary();
            boundaryContainer.Boundaries.Returns(new EventedList<IWaveBoundary> {boundary});

            var supportPoint = new SupportPoint(random.NextDouble(), boundary.GeometricDefinition);

            var geometry = Substitute.For<IPoint>();
            waveBoundaryGeometryFactory.ConstructBoundarySupportPoint(supportPoint).Returns(geometry);

            using (var featureProvider = new BoundarySupportPointMapFeatureProvider(boundaryContainer,
                                                                                    waveBoundaryGeometryFactory))
            {
                // Call
                void Call() => boundary.GeometricDefinition.SupportPoints.Add(supportPoint);

                // Assert
                Assert.That(CountFeaturesChangedFired(featureProvider, Call), Is.EqualTo(1));
                AssertCorrectFeature(featureProvider, supportPoint, geometry);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCaseSource(nameof(RemoveBoundaryCalls))]
        public void GivenABoundarySupportPointMapFeatureProvider_WhenABoundaryIsRemovedFromTheBoundaryContainerAndNewSupportPointIsAdded_ThenFeaturesChangedIsNotCalled(Action<IBoundaryContainer> removeBoundary)
        {
            // Setup
            IWaveBoundary boundary = CreateBoundary();
            boundaryContainer.Boundaries.Returns(new EventedList<IWaveBoundary> {boundary});

            var supportPoint = new SupportPoint(random.NextDouble(), boundary.GeometricDefinition);

            using (var featureProvider = new BoundarySupportPointMapFeatureProvider(boundaryContainer,
                                                                                    waveBoundaryGeometryFactory))
            {
                removeBoundary.Invoke(boundaryContainer);

                // Call
                void Call() => boundary.GeometricDefinition.SupportPoints.Add(supportPoint);

                // Assert
                Assert.That(CountFeaturesChangedFired(featureProvider, Call), Is.EqualTo(0));
                Assert.That(featureProvider.Features, Has.Count.EqualTo(0));
            }
        }

        private IEnumerable<Action<IBoundaryContainer>> RemoveBoundaryCalls()
        {
            yield return bc => bc.Boundaries.Clear();
            yield return bc => bc.Boundaries.RemoveAt(0);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenABoundarySupportPointMapFeatureProvider_WhenABoundaryIsAddedToTheBoundaryContainerAndNewSupportPointIsAdded_ThenFeaturesChangedIsCalled()
        {
            // Setup
            IWaveBoundary boundary = CreateBoundary();

            boundaryContainer.Boundaries.Returns(new EventedList<IWaveBoundary>());

            var supportPoint = new SupportPoint(random.NextDouble(), boundary.GeometricDefinition);

            var geometry = Substitute.For<IPoint>();
            waveBoundaryGeometryFactory.ConstructBoundarySupportPoint(supportPoint).Returns(geometry);

            using (var featureProvider = new BoundarySupportPointMapFeatureProvider(boundaryContainer,
                                                                                    waveBoundaryGeometryFactory))
            {
                boundaryContainer.Boundaries.Add(boundary);

                // Precondition
                Assert.That(featureProvider.Features, Is.Empty);

                // Call
                void Call() => boundary.GeometricDefinition.SupportPoints.Add(supportPoint);

                // Assert
                Assert.That(CountFeaturesChangedFired(featureProvider, Call), Is.EqualTo(1));
                AssertCorrectFeature(featureProvider, supportPoint, geometry);
            }
        }

        [Test]
        public void Add_IFeature_ThrowsNotSupportedException()
        {
            // Setup
            var feature = Substitute.For<IFeature>();

            using (var featureProvider = new BoundarySupportPointMapFeatureProvider(boundaryContainer,
                                                                                    waveBoundaryGeometryFactory))
            {
                void Call() => featureProvider.Add(feature);
                Assert.Throws<NotSupportedException>(Call);
            }
        }

        [Test]
        public void Features_Set_ThrowsNotSupportedException()
        {
            // Setup
            var features = Substitute.For<IList>();

            using (var featureProvider = new BoundarySupportPointMapFeatureProvider(boundaryContainer,
                                                                                    waveBoundaryGeometryFactory))
            {
                void Call() => featureProvider.Features = features;
                Assert.Throws<NotSupportedException>(Call);
            }
        }

        private static IWaveBoundary CreateBoundary()
        {
            var boundary = Substitute.For<IWaveBoundary>();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var supportPoints = new EventedList<SupportPoint>();

            boundary.GeometricDefinition.Returns(geometricDefinition);
            geometricDefinition.SupportPoints.Returns(supportPoints);

            return boundary;
        }

        private static int CountFeaturesChangedFired(IFeatureProvider fc, Action action)
        {
            var count = 0;

            fc.FeaturesChanged += CountFeaturesChanged;

            action.Invoke();

            void CountFeaturesChanged(object sender, EventArgs e)
            {
                count++;
            }

            return count;
        }

        private static void AssertCorrectFeature(IFeatureProvider featureProvider, SupportPoint supportPoint, IPoint geometry)
        {
            Assert.That(featureProvider.Features, Has.Count.EqualTo(1));
            var feature = featureProvider.Features[0] as SupportPointFeature;
            Assert.That(feature, Is.Not.Null);
            Assert.That(feature.ObservedSupportPoint, Is.SameAs(supportPoint));
            Assert.That(feature.Geometry, Is.SameAs(geometry));
        }
    }
}