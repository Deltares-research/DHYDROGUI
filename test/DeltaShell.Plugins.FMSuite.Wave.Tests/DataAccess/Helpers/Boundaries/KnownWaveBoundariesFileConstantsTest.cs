using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
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
        [TestCase(KnownWaveBoundariesFileConstants.NorthBoundaryOrientationType, "north")]
        [TestCase(KnownWaveBoundariesFileConstants.NorthWestBoundaryOrientationType, "northwest")]
        [TestCase(KnownWaveBoundariesFileConstants.WestBoundaryOrientationType, "west")]
        [TestCase(KnownWaveBoundariesFileConstants.SouthWestBoundaryOrientationType, "southwest")]
        [TestCase(KnownWaveBoundariesFileConstants.SouthBoundaryOrientationType, "south")]
        [TestCase(KnownWaveBoundariesFileConstants.SouthEastBoundaryOrientationType, "southeast")]
        [TestCase(KnownWaveBoundariesFileConstants.EastBoundaryOrientationType, "east")]
        [TestCase(KnownWaveBoundariesFileConstants.ClockwiseDistanceDirType, "clockwise")]
        [TestCase(KnownWaveBoundariesFileConstants.CounterClockwiseDistanceDirType, "counter-clockwise")]
        [TestCase(KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType, "fromsp2file")]
        public void ConstantField_ReturnsCorrectValue(string actualValue, string expectedValue)
        {
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
    }
}