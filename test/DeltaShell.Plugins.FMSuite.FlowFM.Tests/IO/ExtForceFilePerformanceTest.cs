using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ExtForceFilePerformanceTest
    {
        [Test]
        [Category(TestCategory.Performance)]
        public void ReadExtForcingsShouldBeFast()
        {
            var def = new WaterFlowFMModelDefinition();
            var testDataPath = TestHelper.GetTestFilePath(@"dcsm");
            var externalForcingZipFileName = "dcsm.zip";
            var externalForcingZipFilePath = Path.Combine(testDataPath, externalForcingZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataPath, tempDir);
                ZipFileUtils.Extract(externalForcingZipFilePath, tempDir);

                var externalForcingFileName = "dcsmv6.ext";
                var mduFileName = "dcsmv6.mdu";

                var externalForcingFile = Path.Combine(tempDir, externalForcingFileName);
                var extSubFilesReferenceFilePath = Path.Combine(tempDir, mduFileName);

                var extForceFile = new ExtForceFile();
                TestHelper.AssertIsFasterThan(30000, () => extForceFile.Read(externalForcingFile, def, extSubFilesReferenceFilePath));
            });
        }
    }
}