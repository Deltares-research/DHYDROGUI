using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Features;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Features
{
    [TestFixture]
    public class BoundaryLineFeatureTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var feature = new BoundaryLineFeature();

            // Assert
            Assert.That(feature, Is.InstanceOf<Feature2D>());
        }
    }
}