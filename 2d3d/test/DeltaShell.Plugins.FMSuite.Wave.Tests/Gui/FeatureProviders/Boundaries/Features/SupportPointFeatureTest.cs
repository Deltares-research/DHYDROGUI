using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Features;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Features
{
    [TestFixture]
    public class SupportPointFeatureTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var feature = new SupportPointFeature();

            // Assert
            Assert.That(feature, Is.InstanceOf<Feature2D>());
        }
    }
}