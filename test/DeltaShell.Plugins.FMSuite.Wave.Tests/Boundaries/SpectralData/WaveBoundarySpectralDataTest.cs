using DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveBoundarySpectralDataTest
    {
        [Test]
        public void WhenInstantiatingWaveBoundarySpectralDataObject_ThenDefaultValuesAreSet()
        {
            // Call
            var waveBoundarySpectralData = new WaveBoundarySpectralData();

            // Assert
            Assert.That(waveBoundarySpectralData.GaussianSpreadingValue, Is.EqualTo(0.1));

            Assert.That(waveBoundarySpectralData.ShapeType, Is.EqualTo((WaveSpectrumShapeType)0),
                        $"Expected ShapeType to be initialised with 0");
            Assert.That(waveBoundarySpectralData.PeriodType, Is.EqualTo((WavePeriodType)0),
                        $"Expected PeriodType to be initialised with 0");
            Assert.That(waveBoundarySpectralData.DirectionalSpreadingType, Is.EqualTo((WaveDirectionalSpreadingType)0),
                        $"Expected DirectionalSpreadingType to be initialised with 0");
            Assert.That(waveBoundarySpectralData.PeakEnhancementFactor, Is.EqualTo(0.0),
                        $"Expected the PeakEnhancementFactor to be initialised with 0.0");
        }
    }
}
