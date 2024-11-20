using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class FixedWeirSchemeExtensionsTest
    {
        [TestCase(FixedWeirSchemes.None, 0.0d)]
        [TestCase(FixedWeirSchemes.Scheme6, 0.0d)]
        [TestCase(FixedWeirSchemes.Scheme8, 0.10d)]
        [TestCase(FixedWeirSchemes.Scheme9, 0.0d)]
        public void GivenAFixedWeirScheme_WhenGetMinimalAllowedGroundHeightIsCalled_ThenTheCorrectValueIsReturned(
            FixedWeirSchemes scheme, double expectedValue)
        {
            // When
            double actualValue = scheme.GetMinimalAllowedGroundHeight();

            // Then
            Assert.That(actualValue, Is.EqualTo(expectedValue),
                        $"Minimal allowed value for fixed weir scheme {scheme.GetDescription()} was different than expected.");
        }
    }
}