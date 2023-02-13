using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DHYDRO.TestModels.DFlowFM;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    partial class WaterFlowFMModelTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void LoadFromMdu_WithRelativeRestartFile_LoadsCorrectRestartFile()
        {
            // Setup
            string testFolder = TestHelper.GetTestFilePath("MduFileWithRelativeRestart");

            using (var tempDir = new TemporaryDirectory())
            {
                var model = Substitute.ForPartsOf<WaterFlowFMModel>();
                string modelFolder = tempDir.CopyDirectoryToTempDirectory(testFolder);
                string mduFilePath = Path.Combine(modelFolder, "simplebox.mdu");
                string restartFilePath = Path.Combine(modelFolder, "original\\simplebox_20010101_000100_rst.nc");

                // Precondition
                Assert.That(model.UseRestart, Is.False);

                // Call
                model.LoadFromMdu(mduFilePath);

                // Assert
                Assert.That(model.UseRestart, Is.True);
                Assert.That(model.RestartInput.Path, Is.EqualTo(restartFilePath));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(true)]
        [TestCase(false)]
        public void LoadFromMdu_ForModelWithOutput_ShouldNotChangeOutOfSyncSetByDatabaseEarlier(bool outputOutOfSync)
        {
            // Setup
            string testFolder = TestHelper.GetTestFilePath(@"Model\Output\FlowFM");

            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                model.OutputOutOfSync = outputOutOfSync;

                string modelFolder = tempDir.CopyDirectoryToTempDirectory(testFolder);

                string mduFilePath = Path.Combine(modelFolder, "input", "FlowFM.mdu");

                // Precondition
                Assert.AreEqual(outputOutOfSync, model.OutputOutOfSync);

                // Call
                model.LoadFromMdu(mduFilePath);

                // Assert
                Assert.AreEqual(outputOutOfSync, model.OutputOutOfSync);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void LoadFromMdu_WithRelativeRestartFile_DoesNotExist_GivesWarning()
        {
            // Setup
            string testFolder = TestHelper.GetTestFilePath("MduFileWithRelativeRestart");

            using (var tempDir = new TemporaryDirectory())
            {
                var model = Substitute.ForPartsOf<WaterFlowFMModel>();
                string modelFolder = tempDir.CopyDirectoryToTempDirectory(testFolder);
                string mduFilePath = Path.Combine(modelFolder, "simplebox.mdu");
                string restartFilePath = Path.Combine(modelFolder, "original\\simplebox_20010101_000100_rst.nc");

                File.Delete(restartFilePath);

                // Call
                void Call() => model.LoadFromMdu(mduFilePath);

                // Assert
                List<string> messages = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToList();
                Assert.That(messages, Has.Count.EqualTo(1));
                Assert.That(messages[0], Is.EqualTo($"Restart file not found: {restartFilePath}."));

                Assert.That(model.UseRestart, Is.False);
                Assert.That(model.RestartInput.IsEmpty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void LoadFromMduWithoutRelativeRestartFile_DoesNothing()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                DFlowFMModelRepository.f005_boundary_conditions
                                      .c011_waterlevel_tim_varying
                                      .CopyTo(new DirectoryInfo(tempDir.Path));
                string mduFilePath = Path.Combine(tempDir.Path, "tfl.mdu");
                var model = Substitute.ForPartsOf<WaterFlowFMModel>();

                // Call
                model.LoadFromMdu(mduFilePath);

                // Assert
                Assert.That(model.UseRestart, Is.False);
                Assert.That(model.RestartInput.IsEmpty);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAModelWithNonDefaultBedlevType_WhenTheModelIsImported_ThenModelLoadsCorrectly()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                string srcTestDataPath = TestHelper.GetTestFilePath("WaterFlowFMModel.BedlevType");
                string testDataPath = tempDir.CopyDirectoryToTempDirectory(srcTestDataPath);
                string mduPath = Path.Combine(testDataPath, "bedlevtype_1.mdu");

                using (var model = new WaterFlowFMModel())
                {
                    // When | Then
                    void Call() => model.ImportFromMdu(mduPath, true);
                    Assert.DoesNotThrow(Call);
                }
            }
        }
    }
}