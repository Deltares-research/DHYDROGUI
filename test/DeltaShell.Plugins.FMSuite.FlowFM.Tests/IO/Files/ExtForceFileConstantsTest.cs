using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    public class ExtForceFileConstantsTest
    {
        [TestCase(ExtForceFileConstants.AreaKey, "AREA")]
        [TestCase(ExtForceFileConstants.AveragingTypeKey, "AVERAGINGTYPE")]
        [TestCase(ExtForceFileConstants.RelSearchCellSizeKey, "RELATIVESEARCHCELLSIZE")]
        [TestCase(ExtForceFileConstants.SedimentConcentrationPostfix, "_SedConc")]
        public void ConstantField_ReturnsCorrectValue(string actualValue, string expectedValue)
        {
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
    }
}