using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class CmpFileTest
    {
        [Test]
        public void ReadWriteReadCmpTest()
        {
            var cmpFile = new CmpFile();
            var cmpPath = TestHelper.GetTestFilePath(@"harlingen\071_03_0001.cmp");
            var cmpPathExport = "017_03_0001_export.cmp";
            var harmonicComponents = cmpFile.Read(cmpPath);
            cmpFile.Write(cmpPathExport, harmonicComponents);
            var harmonicComponentsExport = cmpFile.Read(cmpPathExport);

            Assert.AreEqual(harmonicComponents[0].Name, harmonicComponentsExport[0].Name);
            Assert.AreEqual(harmonicComponents[0].Frequency, harmonicComponentsExport[0].Frequency);
            Assert.AreEqual(harmonicComponents[0].Amplitude, harmonicComponentsExport[0].Amplitude);
            Assert.AreEqual(harmonicComponents[0].Phase, harmonicComponentsExport[0].Phase);
        }
    }
}
