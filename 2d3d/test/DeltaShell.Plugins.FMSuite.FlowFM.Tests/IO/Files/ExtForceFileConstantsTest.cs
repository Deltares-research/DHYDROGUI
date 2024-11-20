using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DHYDRO.Common.IO.ExtForce;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    public class ExtForceFileConstantsTest
    {
        [TestCase(ExtForceFileConstants.Keys.Area, "AREA")]
        [TestCase(ExtForceFileConstants.Keys.AveragingType, "AVERAGINGTYPE")]
        [TestCase(ExtForceFileConstants.Keys.RelativeSearchCellSize, "RELATIVESEARCHCELLSIZE")]
        [TestCase(ExtForceQuantNames.SedimentConcentrationPostfix, "_SedConc")]
        public void ConstantField_ReturnsCorrectValue(string actualValue, string expectedValue)
        {
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
    }
}