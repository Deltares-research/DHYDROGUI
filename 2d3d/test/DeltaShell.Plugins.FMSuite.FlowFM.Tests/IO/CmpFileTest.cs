using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
            string cmpPath = TestHelper.GetTestFilePath(@"harlingen\FilesUsingOldFormat\071_03_0001.cmp");
            var cmpPathExport = "017_03_0001_export.cmp";
            IList<HarmonicComponent> harmonicComponents = cmpFile.Read(cmpPath);
            cmpFile.Write(cmpPathExport, harmonicComponents);
            IList<HarmonicComponent> harmonicComponentsExport = cmpFile.Read(cmpPathExport);

            Assert.AreEqual(harmonicComponents[0].Name, harmonicComponentsExport[0].Name);
            Assert.AreEqual(harmonicComponents[0].Frequency, harmonicComponentsExport[0].Frequency);
            Assert.AreEqual(harmonicComponents[0].Amplitude, harmonicComponentsExport[0].Amplitude);
            Assert.AreEqual(harmonicComponents[0].Phase, harmonicComponentsExport[0].Phase);
        }

        [Test]
        public void ReadCmpFileWithUnknownKeyShowsLogMessage()
        {
            var cmpFile = new CmpFile();
            string cmpPath = TestHelper.GetTestFilePath(@"CmpFileTest\cmpWithUnknownN41Key.cmp");
            var returnObjc = new List<HarmonicComponent>();
            string logMssg = string.Format(Resources.CmpFile_Read_Unknown_key__0__from_file__1___It_will_not_be_imported_, "N41", cmpPath);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => returnObjc = cmpFile.Read(cmpPath, BoundaryConditionDataType.AstroComponents).ToList(), logMssg);
            Assert.IsTrue(returnObjc.Any());
        }
    }
}