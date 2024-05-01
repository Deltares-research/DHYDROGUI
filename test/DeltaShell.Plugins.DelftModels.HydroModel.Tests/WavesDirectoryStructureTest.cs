using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [ExcludeFromCodeCoverage]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.VerySlow)]
    [TestFixture]
    public class WavesDirectoryStructureTest
    {
        [Test]
        [TestCase("Obw.zip", "Obw.dsproj", "obw_wave")]
        [TestCase("WaddenZee.zip", "wad.dsproj", "wad")]
        [TestCase("Westerscheldt.zip", "Westerscheldt.dsproj", "Waves")]
        [TestCase("A4-Storm_wavecon.zip", "Project4.dsproj", "Waves")]
        public void GivenAPreviousModel_WhenThisModelIsOpened_ThenTheModelIsCorrectlyMigrated(string zipName, string dsprojName, string waveName)
        {
            // Given
            using (var temporaryDirectory = new TemporaryDirectory())
            using (var app = GetConfiguredHydroApplication(temporaryDirectory.Path))
            {
                // path setup
                string relativeModelDataPath = Path.Combine("WavesDirectoryStructureTest", zipName);
                string modelDataPath = TestHelper.GetTestFilePath(relativeModelDataPath);

                ZipFileUtils.Extract(modelDataPath, temporaryDirectory.Path);

                string dsprojPath = Path.Combine(temporaryDirectory.Path, "old_model", dsprojName);

                // When
                app.OpenProject(dsprojPath);
                // Execute SaveAs() manually (migrating through GUI does this already).
                app.SaveProjectAs(dsprojPath);

                // Then
                var modelWaveFolder =
                    new DirectoryInfo(Path.Combine(temporaryDirectory.Path, "old_model", dsprojName + "_data", waveName));
                var expectedWaveFolder =
                    new DirectoryInfo(Path.Combine(temporaryDirectory.Path, "expected_dsproj_data", dsprojName + "_data", waveName));

                AssertExpectedFolderStructure(modelWaveFolder, expectedWaveFolder);
            }
        }

        private static IApplication GetConfiguredHydroApplication(string temporaryDirectoryPath)
        {
            string workDir = Path.Combine(temporaryDirectoryPath, "workDir");
            Directory.CreateDirectory(workDir);

            var applicationSettingsMock = Substitute.For<ApplicationSettingsBase>();
            applicationSettingsMock["WorkDirectory"] = workDir;

            var app = CreateApplication();
            app.UserSettings = applicationSettingsMock;;
            
            app.Run();
            return app;
        }
        private static IApplication CreateApplication()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new WaveApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new HydroModelApplicationPlugin(),

            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }

        private static void AssertExpectedFolderStructure(DirectoryInfo modelWaveFolder, DirectoryInfo expectedWaveFolder)
        {
            var expectedDirectories = new Queue<DirectoryInfo>();
            expectedDirectories.Enqueue(expectedWaveFolder);

            var actualDirectories = new Queue<DirectoryInfo>();
            actualDirectories.Enqueue(modelWaveFolder);

            while (expectedDirectories.Count > 0)
            {
                DirectoryInfo nextExpectedDirectory = expectedDirectories.Dequeue();
                DirectoryInfo nextActualDirectory = actualDirectories.Dequeue();

                Assert.That(Path.GetFileName(nextActualDirectory.Name),
                            Is.EqualTo(Path.GetFileName(nextExpectedDirectory.Name)),
                            $"Expected the wave model (sub) directories to be equal, but found a difference in {nextActualDirectory.Name}:");

                AssertEqualFiles(nextExpectedDirectory, nextActualDirectory);

                DirectoryInfo[] expectedSubFolders = nextExpectedDirectory.GetDirectories();
                DirectoryInfo[] actualSubFolders = nextActualDirectory.GetDirectories();

                Assert.That(actualSubFolders.Length, Is.EqualTo(expectedSubFolders.Length),
                            $"Expected {nextActualDirectory.Name} to contain the expected number of subfolders:");

                foreach (DirectoryInfo expectedSubFolder in expectedSubFolders)
                {
                    expectedDirectories.Enqueue(expectedSubFolder);
                }

                foreach (DirectoryInfo actualSubFolder in actualSubFolders)
                {
                    actualDirectories.Enqueue(actualSubFolder);
                }
            }
        }

        private static void AssertEqualFiles(DirectoryInfo nextExpectedDirectory, DirectoryInfo nextActualDirectory)
        {
            FileInfo[] expectedFiles = nextExpectedDirectory.GetFiles();
            FileInfo[] actualFiles = nextActualDirectory.GetFiles();

            Assert.That(actualFiles.Length, Is.EqualTo(expectedFiles.Length),
                        $"Expected {nextActualDirectory.Name} to contain the same files, but found a different number of files:");

            for (var i = 0; i < expectedFiles.Length; i++)
            {
                byte[] expectedFileContent = File.ReadAllBytes(expectedFiles[i].FullName);
                byte[] actualFileContent = File.ReadAllBytes(actualFiles[i].FullName);

                Assert.That(actualFileContent, Is.EqualTo(expectedFileContent),
                            $"Expected the file {actualFiles[i].Name} to be equal to expected.");
            }
        }

        [TestCase("DWaves_1.1.0.0.zip", "DWaves_1.1.0.0.dsproj")]
        [TestCase("DWaves_1.2.0.0.zip", "DWaves_1.2.0.0.dsproj")]
        public void GivenAProject_WithPreviousWavePluginVersion_WhenMigrating_ThenNoErrorsAreGenerated(string zipName, string dsProjName)
        {
            // Given
            using (var temporaryDirectory = new TemporaryDirectory())
            using (var app = GetConfiguredHydroApplication(temporaryDirectory.Path))
            {
                string testData = TestHelper.GetTestFilePath(Path.Combine("WavesDirectoryStructureTest", zipName));
                ZipFileUtils.Extract(testData, temporaryDirectory.Path);

                string dsprojPath = Path.Combine(temporaryDirectory.Path, dsProjName);

                // When
                void Call()
                {
                    app.OpenProject(dsprojPath);
                    app.SaveProjectAs(dsprojPath);
                }

                // Then
                Assert.That(TestHelper.GetAllRenderedMessages(Call, Level.Error), Is.Empty);
            }
        }
    }
}