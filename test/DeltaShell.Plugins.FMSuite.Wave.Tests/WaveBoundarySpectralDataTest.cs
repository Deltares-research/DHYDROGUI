using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveBoundarySpectralDataTest
    {
        [Test]
        public void WhenInstantiatingWaveBoundarySpectralDataObject_ThenDefaultValuesAreSet()
        {
            // When
            var waveBoundarySpectralData = new WaveBoundarySpectralData();

            // Then
            Assert.That(waveBoundarySpectralData.GaussianSpreadingValue, Is.EqualTo(0.1));
        }
    }
}