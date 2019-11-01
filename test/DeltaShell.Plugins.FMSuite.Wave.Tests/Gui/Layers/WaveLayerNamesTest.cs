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
    }
}