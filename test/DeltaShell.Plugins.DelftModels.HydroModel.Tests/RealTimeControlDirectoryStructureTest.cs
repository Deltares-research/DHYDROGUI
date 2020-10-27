using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class RealTimeControlDirectoryStructureTest
    {
        [Test]
        public void GivenAProjectWithRealTimeControlApplicationPluginVersion3_7_0_WhenOpened_ThenTheRTCModelIsCorrectlyMigrated()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            using (DeltaShellApplication app = ApplicationTestHelper.GetApplication(temp.Path,
                                                                                    new HydroModelApplicationPlugin(),
                                                                                    new RealTimeControlApplicationPlugin()))
            {
                string testData = temp.CopyDirectoryToTempDirectory(TestHelper.GetTestFilePath("RealTimeControlDirectoryStructureTest"));

                // Call
                app.OpenProject(Path.Combine(testData, "project.dsproj"));

                // Assert
                string expOutputDirPath = Path.Combine(testData, "project.dsproj_data", "rtc_model", "output");
                Assert.That(expOutputDirPath, Does.Exist);

                string[] outputFiles = Directory.GetFiles(expOutputDirPath);
                Assert.That(outputFiles, Has.Length.EqualTo(3));

                for (var i = 0; i < 3; i++)
                {
                    string expRestartFilePath = Path.Combine(expOutputDirPath, $"rtc_20200101_0{i}0000.xml");
                    Assert.That(expRestartFilePath, Does.Exist);

                    string content = File.ReadAllText(expRestartFilePath);
                    Assert.That(content, Is.EqualTo($"content_{i}"));
                }
            }
        }
    }
}