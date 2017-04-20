using System.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ExtForceFileHelperTest
    {
        [Test]
        [TestCase("C:\\Folder\\AnotherFolder\\MoreFolder\\HW1995", "\\filename_something.xyz", "HW1995\\filename_something.xyz")]
        [TestCase("C:\\Folder\\AnotherFolder\\MoreFolder\\HW1995","\\YesThereIsAnotherFolder\\filename_something.xyz", "HW1995\\YesThereIsAnotherFolder\\filename_something.xyz")]
        public void WriteInitialConditionsSamplesTest(string extForceFilePath, string fileName, string expectedFileName)
        {
            var importSamplesOperation = new ImportSamplesSpatialOperationExtension
            {
                                FilePath = Path.GetFullPath(extForceFilePath + fileName),
            };
            ExtForceFileItem item = ExtForceFileHelper.WriteInitialConditionsSamples(extForceFilePath, "quantity", importSamplesOperation, null, true);
            Assert.AreEqual(expectedFileName, item.FileName);
        }
    }
}