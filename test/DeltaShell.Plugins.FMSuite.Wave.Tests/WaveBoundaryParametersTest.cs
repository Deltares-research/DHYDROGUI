using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveBoundaryParametersTest
    {
        [Test]
        public void WhenInstantiatingWaveBoundaryParametersObject_ThenDefaultValuesAreSet()
        {
            // When
            var waveBoundaryParameters = new WaveBoundaryParameters();

            // Then
            Assert.That(waveBoundaryParameters.Period, Is.EqualTo(1.0));
        }
    }
}
