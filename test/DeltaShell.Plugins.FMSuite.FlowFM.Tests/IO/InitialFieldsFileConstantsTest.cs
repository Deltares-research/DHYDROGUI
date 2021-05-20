using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class InitialFieldsFileConstantsTest
    {
        [TestCase(InitialFieldsFileConstants.WaterLevel, "waterlevel")]
        [TestCase(InitialFieldsFileConstants.WaterDepth, "waterdepth")]
        [TestCase(InitialFieldsFileConstants.BedLevel, "bedlevel")]
        [TestCase(InitialFieldsFileConstants.Infiltration, "InfiltrationCapacity")]
        public void ConstantFields(string actual, string expected)
        {
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}