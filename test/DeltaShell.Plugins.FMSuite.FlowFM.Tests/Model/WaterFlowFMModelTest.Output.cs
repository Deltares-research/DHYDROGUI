using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DHYDRO.TestModels.DFlowFM;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        [Category(TestCategory.Slow)]
        public void Constructor_WithFilePathToSupportedMDUFiles_SetsOutputProperties()
        {
            // Setup
            string testDataDirectory = TestHelper.GetTestFilePath(@"Model\Output\FlowFM");
            using (var tempDirectory = new TemporaryDirectory())
            {
                FileUtils.CopyDirectory(testDataDirectory, tempDirectory.Path);
                string mduFilePath = Path.Combine(tempDirectory.Path, "input", "FlowFM.mdu");

                // Call
                using (var model = new WaterFlowFMModel())
                {
                    model.LoadFromMdu(mduFilePath);

                    // Assert
                    Assert.That(model.OutputHisFileStore, Is.Not.Null, "Output files should be loaded.");
                    Assert.That(model.OutputMapFileStore, Is.Not.Null, "Output files should be loaded.");
                    Assert.That(model.OutputClassMapFileStore, Is.Not.Null, "Output files should be loaded.");
                }
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void Constructor_WithFilePathToSupportedMDUFiles_DoesNotSetOutputProperties()
        {
            // Setup
            string testDataDirectory = TestHelper.GetTestFilePath(@"Model\UnsupportedModelWithOutput");
            using (var tempDirectory = new TemporaryDirectory())
            {
                FileUtils.CopyDirectory(testDataDirectory, tempDirectory.Path);
                string mduFilePath = Path.Combine(tempDirectory.Path, "EINDH.mdu");

                // Call
                using (var model = new WaterFlowFMModel())
                {
                    model.LoadFromMdu(mduFilePath);

                    // Assert
                    Assert.That(model.OutputHisFileStore, Is.Null, "Output files should not be loaded.");
                    Assert.That(model.OutputMapFileStore, Is.Null, "Output files should not be loaded.");
                    Assert.That(model.OutputClassMapFileStore, Is.Null, "Output files should not be loaded.");
                }
            }
        }

        [Test]
        public void ClearOutput_WithRestartOutput_ThenRestartOutputIsRemovedFromModel()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string[] restartFiles = CreateRestartFiles(tempDir).ToArray();
                var model = new WaterFlowFMModel();
                model.ConnectOutput(tempDir.Path);

                TypeUtils.SetField(model, "outputIsEmpty", false);

                // Precondition
                Assert.That(model.RestartOutput.Count(), Is.EqualTo(restartFiles.Length));

                // Call
                model.ClearOutput();

                // Assert
                Assert.That(model.RestartOutput, Is.Empty);
            }
        }

        [Test]
        public void ClearOutput_WithTextDocumentOutput_ThenOutputIsRemovedFromModel()
        {
            // Setup
            const string textDocumentTag = "myTextDocument";
            var waterFlowFmModel = new WaterFlowFMModel();
            waterFlowFmModel.DataItems.Add(new DataItem(new TextDocument(), DataItemRole.Output, textDocumentTag));

            // Private field outputIsEmpty is set to false after a successful model run. This field should be false when clearing model output.
            // As we do not focus on model run, we use reflection to set this field and omit the model run.
            TypeUtils.SetField(waterFlowFmModel, "outputIsEmpty", false);

            // Call
            waterFlowFmModel.ClearOutput();

            // Assert
            Assert.That(waterFlowFmModel.GetDataItemByTag(textDocumentTag), Is.Null,
                        "Text Document data item should have been removed at model output clearance.");
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ClearOutput_WithFunctionsStoreOutput_ThenStoresAreCleared()
        {
            string testDataDirectory = TestHelper.GetTestFilePath(@"Model\Output\FlowFM");
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                FileUtils.CopyDirectory(testDataDirectory, tempDirectory.Path);
                string mduFilePath = Path.Combine(tempDirectory.Path, "input", "FlowFM.mdu");

                var waterFlowFmModel = new WaterFlowFMModel();
                waterFlowFmModel.LoadFromMdu(mduFilePath);

                string hisFilePath = waterFlowFmModel.OutputHisFileStore.Path;
                string mapFilePath = waterFlowFmModel.OutputMapFileStore.Path;
                string classMapFilePath = waterFlowFmModel.OutputClassMapFileStore.Path;

                // Pre-condition
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Not.Null, "Test pre-condition failure.");
                Assert.That(File.Exists(hisFilePath), Is.True, "Test pre-condition failure.");
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Not.Null, "Test pre-condition failure.");
                Assert.That(File.Exists(mapFilePath), Is.True, "Test pre-condition failure.");
                Assert.That(waterFlowFmModel.OutputClassMapFileStore, Is.Not.Null, "Test pre-condition failure.");
                Assert.That(File.Exists(classMapFilePath), Is.True, "Test pre-condition failure.");

                // Call
                waterFlowFmModel.ClearOutput();

                // Assert
                Assert.That(waterFlowFmModel.OutputHisFileStore, Is.Null, "His file store should be set to null at model output clearance.");
                Assert.That(File.Exists(hisFilePath), Is.True, "Model output files should not be removed at model output clearance.");
                Assert.That(waterFlowFmModel.OutputMapFileStore, Is.Null, "Output map file store should be set to null at model output clearance.");
                Assert.That(File.Exists(mapFilePath), Is.True, "Model output files should not be removed at model output clearance.");
                Assert.That(waterFlowFmModel.OutputClassMapFileStore, Is.Null, "Class map file store should be set to null at model output clearance.");
                Assert.That(File.Exists(classMapFilePath), Is.True, "Model output files should not be removed at model output clearance.");
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ConnectOutput_WithUnsupportedMDUFiles_OutputNotSetAndLogMessageGenerated()
        {
            // Setup
            string unsupportedOutputPath = TestHelper.GetTestFilePath(@"Model\UnsupportedOutput");
            using (var tempDirectory = new TemporaryDirectory())
            {
                string tempDirectoryPath = tempDirectory.Path;
                FileUtils.CopyDirectory(unsupportedOutputPath, tempDirectoryPath);

                using (var model = new WaterFlowFMModel())
                {
                    // Call
                    Action call = () => model.ConnectOutput(tempDirectoryPath);

                    // Assert
                    TestHelper.AssertLogMessageIsGenerated(call, "Associated output files are unsupported, these will not be loaded");

                    Assert.That(model.OutputHisFileStore, Is.Null, "Output files should not be loaded.");
                    Assert.That(model.OutputMapFileStore, Is.Null, "Output files should not be loaded.");
                    Assert.That(model.OutputClassMapFileStore, Is.Null, "Output files should not be loaded.");
                }
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ConnectOutput_WithSupportedMDUFiles_OutputSet()
        {
            // Setup
            string testDataDirectory = TestHelper.GetTestFilePath(@"Model\Output\FlowFM");
            using (var tempDirectory = new TemporaryDirectory())
            {
                FileUtils.CopyDirectory(testDataDirectory, tempDirectory.Path);
                string outputFilePath = Path.Combine(tempDirectory.Path, "Output");

                using (var model = new WaterFlowFMModel())
                {
                    // Call
                    model.ConnectOutput(outputFilePath);

                    // Assert
                    Assert.That(model.OutputHisFileStore, Is.Not.Null, "Output files should be loaded.");
                    Assert.That(model.OutputMapFileStore, Is.Not.Null, "Output files should be loaded.");
                    Assert.That(model.OutputClassMapFileStore, Is.Not.Null, "Output files should be loaded.");
                }
            }
        }

        [Test]
        public void ConnectOutput_RestartFiles_ReconnectsTheRestartFiles()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                string[] restartFiles = CreateRestartFiles(tempDir).ToArray();

                var model = new WaterFlowFMModel();

                // Call
                model.ConnectOutput(tempDir.Path);

                // Assert
                RestartFile[] restartOutput = model.RestartOutput.ToArray();
                Assert.That(restartOutput, Has.Length.EqualTo(5));

                for (var i = 0; i < 5; i++)
                {
                    Assert.That(restartOutput[i].Path, Is.EqualTo(restartFiles[i]));
                }
            }
        }

        [Test]
        [Category(NghsTestCategory.PerformanceDotTrace)]
        public void ConnectOutput_Performance()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                DFlowFMModelRepository.f012_inout
                                      .c032_alloutrealistic
                                      .CopyTo(new DirectoryInfo(tempDir.Path));
                string mduFilePath = GetMduFilePath(tempDir.Path);
                string outputDir = GenerateOutput(mduFilePath);

                var model = new WaterFlowFMModel();

                // Call
                model.ConnectOutput(outputDir);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(true)]
        [TestCase(false)]
        public void ExportTo_WhenSwitchToIsTrueAndModelHasOutput_ShouldNotChangeOutOfSync(bool outOfSync)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                CreateRestartFiles(tempDir).ToArray();
              
                model.ConnectOutput(tempDir.Path);
                model.OutputOutOfSync = outOfSync;
                
                Assert.AreEqual(outOfSync, model.OutputOutOfSync);
                Assert.AreEqual(false, model.OutputIsEmpty);

                // Call
                model.ExportTo(Path.Combine(tempDir.Path, "export"), true, true, true);

                // Assert
                Assert.AreEqual(outOfSync, model.OutputOutOfSync);
            }
        }

        private static IEnumerable<string> CreateRestartFiles(TemporaryDirectory tempDir)
        {
            for (var i = 0; i < 5; i++)
            {
                yield return tempDir.CreateFile($"{i}_rst.nc");
            }
        }

        private static string GenerateOutput(string mduFilePath)
        {
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduFilePath);

            ActivityRunner.RunActivity(model);

            Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned));

            return model.WorkingOutputDirectoryPath;
        }

        private static string GetMduFilePath(string path)
        {
            return new DirectoryInfo(path).EnumerateFiles()
                                          .First(f => f.Name.EndsWith(".mdu", StringComparison.Ordinal))
                                          .FullName;
        }
    }
}