using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    public class BndExtForceFileConstantsTest
    {
        [TestCase(BndExtForceFileConstants.BoundaryBlockKey, "[boundary]")]
        [TestCase(BndExtForceFileConstants.QuantityKey, "quantity")]
        [TestCase(BndExtForceFileConstants.LocationFileKey, "locationfile")]
        [TestCase(BndExtForceFileConstants.ForcingFileKey, "forcingfile")]
        [TestCase(BndExtForceFileConstants.ThatcherHarlemanTimeLagKey, "return_time")]
        [TestCase(BndExtForceFileConstants.OpenBoundaryToleranceKey, "OpenBoundaryTolerance")]
        public void ConstantField_ReturnsCorrectValue(string actualValue, string expectedValue)
        {
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
    }
}