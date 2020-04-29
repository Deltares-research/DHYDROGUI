using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class KnownWaveBoundariesFileConstantsTest
    {
        [TestCase(KnownWaveBoundariesFileConstants.DegreesDefinedSpreading, "Degrees")]
        [TestCase(KnownWaveBoundariesFileConstants.PowerDefinedSpreading, "Power")]
        [TestCase(KnownWaveBoundariesFileConstants.GaussShape, "Gauss")]
        [TestCase(KnownWaveBoundariesFileConstants.JonswapShape, "Jonswap")]
        [TestCase(KnownWaveBoundariesFileConstants.PiersonMoskowitzShape, "Pierson-Moskowitz")]
        [TestCase(KnownWaveBoundariesFileConstants.PeakPeriodType, "peak")]
        [TestCase(KnownWaveBoundariesFileConstants.MeanPeriodType, "mean")]
        [TestCase(KnownWaveBoundariesFileConstants.CoordinatesDefinitionType, "xy-coordinates")]
        [TestCase(KnownWaveBoundariesFileConstants.OrientationDefinitionType, "orientation")]
        [TestCase(KnownWaveBoundariesFileConstants.FromFileSpectrumType, "from file")]
        [TestCase(KnownWaveBoundariesFileConstants.ParametrizedSpectrumType, "parametric")]
        public void ConstantField_ReturnsCorrectValue(string actualValue, string expectedValue)
        {
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
    }
}
