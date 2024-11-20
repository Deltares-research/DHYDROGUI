using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData.Laterals
{
    [TestFixture]
    public class LateralTest
    {
        [Test]
        public void Constructor_InitializesCorrectly()
        {
            // Call
            var lateral = new Lateral();

            // Assert
            Assert.That(lateral.Data, Is.Not.Null);
        }

        [Test]
        public void SetName_FeatureNotSet_RemainsNull()
        {
            // Setup
            var lateral = new Lateral();
            const string name = "some_name";

            // Call
            lateral.Name = name;

            // Assert
            Assert.That(lateral.Name, Is.Null);
        }

        [Test]
        public void SetName_SetsTheNameOfTheFeature()
        {
            // Setup
            var lateral = new Lateral();
            var feature = new Feature2D();
            lateral.Feature = feature;
            const string name = "some_name";

            // Call
            lateral.Name = name;

            // Assert
            Assert.That(feature.Name, Is.EqualTo(name));
        }

        [Test]
        public void GetName_GetsTheNameOfTheFeature()
        {
            // Setup
            var lateral = new Lateral();
            var feature = new Feature2D();
            lateral.Feature = feature;
            const string name = "some_name";

            feature.Name = name;

            // Assert
            Assert.That(lateral.Name, Is.EqualTo(name));
        }
    }
}