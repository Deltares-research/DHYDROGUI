using System;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers
{
    [TestFixture]
    public class WaveLayerNamesTest
    {
        [Test]
        public void GetDomainLayerName_ValidDomainName_ReturnsExpectedResults()
        {
            // Call
            string result = WaveLayerNames.GetDomainLayerName("domainName");

            // Assert
            Assert.That(result, Is.EqualTo("Domain (domainName)"));
        }

        [Test]
        public void GetDomainLayerName_DomainNameNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveLayerNames.GetDomainLayerName(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("domainName"));
        }

        [Test]
        public void GetOutputLayerName_ValidDomainName_ReturnsExpectedResults()
        {
            // Call
            string result = WaveLayerNames.GetOutputLayerName("domainName");

            // Assert
            Assert.That(result, Is.EqualTo("Output (domainName)"));
        }

        [Test]
        public void GetOutputLayerName_DomainNameNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveLayerNames.GetOutputLayerName(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("domainName"));
        }

        [Test]
        public void BoundarySupportPointLayerName_ReturnsCorrectValue()
        {
            Assert.That(WaveLayerNames.BoundarySupportPointsLayerName, Is.EqualTo("Support points"));
        }
    }
}