using System.IO;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils.Builders;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.VerySlow)]
    [TestFixture]
    public class RealTimeControlDirectoryStructureTest
    {
        [Test]
        public void GivenAProjectWithRealTimeControlApplicationPluginVersion3_7_0_WhenOpened_ThenTheRTCModelIsCorrectlyMigrated()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            using (var app = GetApplication())
            {
                string testData = temp.CopyDirectoryToTempDirectory(TestHelper.GetTestFilePath("RealTimeControlDirectoryStructureTest"));

                // Call
                app.ProjectService.OpenProject(Path.Combine(testData, "rtc_37_project.dsproj"));

                // Assert
                string expOutputDirPath = Path.Combine(testData, "rtc_37_project.dsproj_data", "rtc_model", "output");
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

        private static IApplication GetApplication()
        {
            IApplication app = new DHYDROApplicationBuilder().WithRealTimeControl().WithHydroModel().Build();
            app.Run();

            return app;
        }
    }
}