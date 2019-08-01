using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        [Category(TestCategory.Slow)]
        public void ClearOutput_WithFunctionsStores_ThenStoresAreClearedAndRelatedFilesRemoved()
        {
            string testDataDirectory = TestHelper.GetTestFilePath(@"Model\Output\Data\FlowFM");
            string testDirectory = TestHelper.CreateLocalCopy(testDataDirectory);
            string mduFilePath = Path.Combine(testDirectory, "input", "FlowFM.mdu");
            try
            {
                var waterFlowFmModel = new WaterFlowFMModel(mduFilePath);
                string hisFilePath = waterFlowFmModel.OutputHisFileStore.Path;
                string mapFilePath = waterFlowFmModel.OutputMapFileStore.Path;

                // Pre-condition
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Not.Null);
                Assert.That(File.Exists(hisFilePath), Is.True);
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Not.Null);
                Assert.That(File.Exists(mapFilePath), Is.True);

                // Call
                waterFlowFmModel.ClearOutput();

                // Assert
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Null);
                Assert.That(File.Exists(hisFilePath), Is.False);
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Null);
                Assert.That(File.Exists(mapFilePath), Is.False);
            }
            finally
            {
                FileUtils.DeleteIfExists(testDirectory);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ClearOutput_WithFunctionStoresThatHaveLockedFiles_ThenStoreIsClearedButFilesAreNotRemoved()
        {
            string testDataDirectory = TestHelper.GetTestFilePath(@"Model\Output\Data\FlowFM");
            string testDirectory = TestHelper.CreateLocalCopy(testDataDirectory);
            string mduFilePath = Path.Combine(testDirectory, "input", "FlowFM.mdu");
            try
            {
                var waterFlowFmModel = new WaterFlowFMModel(mduFilePath);
                string hisFilePath = waterFlowFmModel.OutputHisFileStore.Path;
                string mapFilePath = waterFlowFmModel.OutputMapFileStore.Path;

                // Pre-condition
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Not.Null);
                Assert.That(File.Exists(hisFilePath), Is.True);
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Not.Null);
                Assert.That(File.Exists(mapFilePath), Is.True);

                using (File.Open(hisFilePath, FileMode.Open))
                using (File.Open(mapFilePath, FileMode.Open))
                {
                    // Call
                    void Call () => waterFlowFmModel.ClearOutput();

                    // Assert
                    string[] logMessages = new[]
                    {
                        $"Unable to remove output file '{hisFilePath}':{Environment.NewLine}{$@"The process cannot access the file '{hisFilePath}' because it is being used by another process."}",
                        $"Unable to remove output file '{mapFilePath}':{Environment.NewLine}{$@"The process cannot access the file '{mapFilePath}' because it is being used by another process."}"
                    };
                    TestHelper.AssertLogMessagesAreGenerated(Call, logMessages, 2);
                    Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Null);
                    Assert.That(File.Exists(hisFilePath), Is.True);
                    Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Null);
                    Assert.That(File.Exists(mapFilePath), Is.True);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDirectory);
            }
        }
    }
}