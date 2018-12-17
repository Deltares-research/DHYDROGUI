using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveBoundaryConditionTest
    {
        [Test]
        public void WhenInstantiatingAWaveBoundaryCondition_ThenTheDefaultPeakEnhancementFactorValueIsEqualToExpectedValue()
        {
            // When
            var waveBoundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Harmonics);

            // Then
            Assert.That(waveBoundaryCondition.SpectralData.PeakEnhancementFactor, Is.EqualTo(3.3));
        }
    }
}