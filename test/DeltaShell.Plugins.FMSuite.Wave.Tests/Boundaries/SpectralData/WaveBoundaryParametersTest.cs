using DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.SpectralData
{
    [TestFixture]
    public class WaveBoundaryParametersTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var parameters = new WaveBoundaryParameters();

            // Assert
            Assert.That(parameters.Height, Is.EqualTo(0.0));
            Assert.That(parameters.Period, Is.EqualTo(1.0));
            Assert.That(parameters.Direction, Is.EqualTo(0.0));
            Assert.That(parameters.Spreading, Is.EqualTo(0.0));
        }
    }
}