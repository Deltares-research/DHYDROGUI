using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
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

            // Call
            var featureProvider = new BoundaryLineMapFeatureProvider(boundaryContainer);

            // Assert
            Assert.That(featureProvider, Is.InstanceOf<Feature2DCollection>());
        }

        [Test]
        public void Constructor_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BoundaryLineMapFeatureProvider(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("boundaryContainer"));
        }
    }
}