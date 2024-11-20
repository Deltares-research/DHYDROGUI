using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    public class BndExtForceFileConstantsTest
    {
        [TestCase(BndExtForceFileConstants.BoundaryBlockKey, "boundary")]
        [TestCase(BndExtForceFileConstants.LateralBlockKey, "lateral")]
        [TestCase(BndExtForceFileConstants.QuantityKey, "quantity")]
        [TestCase(BndExtForceFileConstants.LocationFileKey, "locationFile")]
        [TestCase(BndExtForceFileConstants.ForcingFileKey, "forcingFile")]
        [TestCase(BndExtForceFileConstants.ThatcherHarlemanTimeLagKey, "returnTime")]
        [TestCase(BndExtForceFileConstants.OpenBoundaryToleranceKey, "OpenBoundaryTolerance")]
        [TestCase(BndExtForceFileConstants.IdKey, "id")]
        [TestCase(BndExtForceFileConstants.NameKey, "name")]
        [TestCase(BndExtForceFileConstants.TypeKey, "type")]
        [TestCase(BndExtForceFileConstants.LocationTypeKey, "locationType")]
        [TestCase(BndExtForceFileConstants.NumCoordinatesKey, "numCoordinates")]
        [TestCase(BndExtForceFileConstants.XCoordinatesKey, "xCoordinates")]
        [TestCase(BndExtForceFileConstants.YCoordinatesKey, "yCoordinates")]
        [TestCase(BndExtForceFileConstants.DischargeKey, "discharge")]
        public void ConstantField_ReturnsCorrectValue(string actualValue, string expectedValue)
        {
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
    }
}