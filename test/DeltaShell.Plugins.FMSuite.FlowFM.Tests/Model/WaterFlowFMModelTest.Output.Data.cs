using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.TestUtils;
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
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                FileUtils.CopyDirectory(testDataDirectory, tempDirectory.Path);
                string mduFilePath = Path.Combine(tempDirectory.Path, "input", "FlowFM.mdu");

                var waterFlowFmModel = new WaterFlowFMModel(mduFilePath);
                string hisFilePath = waterFlowFmModel.OutputHisFileStore.Path;
                string mapFilePath = waterFlowFmModel.OutputMapFileStore.Path;
                string classMapFilePath = waterFlowFmModel.OutputClassMapFileStore.Path;

                // Pre-condition
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Not.Null);
                Assert.That(File.Exists(hisFilePath), Is.True);
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Not.Null);
                Assert.That(File.Exists(mapFilePath), Is.True);
                Assert.That(waterFlowFmModel.OutputClassMapFileStore, Is.Not.Null);
                Assert.That(File.Exists(classMapFilePath), Is.True);

                // Call
                waterFlowFmModel.ClearOutput();

                // Assert
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Null);
                Assert.That(File.Exists(hisFilePath), Is.False);
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Null);
                Assert.That(File.Exists(mapFilePath), Is.False);
                Assert.That(waterFlowFmModel.OutputClassMapFileStore, Is.Null);
                Assert.That(File.Exists(classMapFilePath), Is.False);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ClearOutput_WithFunctionStoresThatHaveLockedFiles_ThenStoreIsClearedButFilesAreNotRemoved()
        {
            string testDataDirectory = TestHelper.GetTestFilePath(@"Model\Output\Data\FlowFM");
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                FileUtils.CopyDirectory(testDataDirectory, tempDirectory.Path);
                string mduFilePath = Path.Combine(tempDirectory.Path, "input", "FlowFM.mdu");

                var waterFlowFmModel = new WaterFlowFMModel(mduFilePath);
                string hisFilePath = waterFlowFmModel.OutputHisFileStore.Path;
                string mapFilePath = waterFlowFmModel.OutputMapFileStore.Path;
                string classMapFilePath = waterFlowFmModel.OutputClassMapFileStore.Path;

                // Pre-condition
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Not.Null);
                Assert.That(File.Exists(hisFilePath), Is.True);
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Not.Null);
                Assert.That(File.Exists(mapFilePath), Is.True);
                Assert.That(waterFlowFmModel.OutputClassMapFileStore, Is.Not.Null);
                Assert.That(File.Exists(classMapFilePath), Is.True);

                using (File.Open(hisFilePath, FileMode.Open))
                using (File.Open(mapFilePath, FileMode.Open))
                using (File.Open(classMapFilePath, FileMode.Open))
                {
                    // Call
                    void Call() => waterFlowFmModel.ClearOutput();

                    // Assert
                    string[] logMessages =
                    {
                        $"Unable to remove output file '{hisFilePath}':{Environment.NewLine}{$@"The process cannot access the file '{hisFilePath}' because it is being used by another process."}",
                        $"Unable to remove output file '{mapFilePath}':{Environment.NewLine}{$@"The process cannot access the file '{mapFilePath}' because it is being used by another process."}",
                        $"Unable to remove output file '{classMapFilePath}':{Environment.NewLine}{$@"The process cannot access the file '{classMapFilePath}' because it is being used by another process."}"
                    };
                    TestHelper.AssertLogMessagesAreGenerated(Call, logMessages, 3);
                    Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Null);
                    Assert.That(File.Exists(hisFilePath), Is.True);
                    Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Null);
                    Assert.That(File.Exists(mapFilePath), Is.True);
                    Assert.That(waterFlowFmModel.OutputClassMapFileStore, Is.Null);
                    Assert.That(File.Exists(classMapFilePath), Is.True);
                }
            }
        }
    }
}