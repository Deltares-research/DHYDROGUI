using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    [TestFixture]
    public class WaterQualityModel352LegacyLoaderTest
    {
        private const string outputFolderName = nameof(WaterQualityModel.OutputFolder);

        private WaterQualityModel352LegacyLoader legacyLoader;
        private Project project;
        private WaterQualityModel model;

        [SetUp]
        public void Setup()
        {
            legacyLoader = new WaterQualityModel352LegacyLoader();
            model = new WaterQualityModel();
            var rootFolder = new Folder();
            rootFolder.Add(model);
            project = new Project {RootFolder = rootFolder};
        }

        [TearDown]
        public void TearDown()
        {
            legacyLoader = null;
            model.Dispose();
            project = null;
        }

        [Test]
        public void OnAfterProjectMigrated_WhenOutputDirectoryExistsButIsEmpty_ThenOutputFolderIsNull()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                model.ModelSettings.OutputDirectory = tempDirectory.Path;

                // Call
                legacyLoader.OnAfterProjectMigrated(project);

                // Assert
                Assert.That(model.OutputFolder, Is.Null,
                            $"After migrating, the {outputFolderName} should be null " +
                            "when the output directory does exist but there is no output.");
            }
        }

        [Test]
        public void OnAfterProjectMigrated_WhenOutputDirectoryExistsAndIsNotEmpty_ThenOutputFolderIsSetCorrectly()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string outputDirPath = tempDirectory.Path;

                model.ModelSettings.OutputDirectory = outputDirPath;

                File.WriteAllText(Path.Combine(outputDirPath, "file.txt"), "");

                // Call
                legacyLoader.OnAfterProjectMigrated(project);

                // Assert
                AssertThatOutputFolderIsSetWithCorrectPath(outputDirPath);
            }
        }

        [Test]
        public void OnAfterProjectMigrated_WhenWorkingDirectoryExistsAndOutputDirectoryDoesNotExist_ThenOutputDirectoryIsCreatedAndContentOfWorkingDirectoryIsMovedThere()
        {
            // Setup
            const string fileName = "file_in_wd.txt";
            const string directoryName = "directory_in_wd";

            using (var tempDirectory = new TemporaryDirectory())
            {
                string outputDirPath = Path.Combine(tempDirectory.Path, "output");
                model.ModelSettings.OutputDirectory = outputDirPath;

                string workingDir = model.ModelDataDirectory + "_output";
                PrepareWorkingDirectory(workingDir, fileName, directoryName);

                // Precondition
                Assert.That(!Directory.Exists(outputDirPath), "Precondition violated.");

                // Call
                legacyLoader.OnAfterProjectMigrated(project);

                // Assert
                AssertThatOutputFolderIsSetWithCorrectPath(outputDirPath);
                AssertWorkingDirectoryIsCorrectlyMoved(outputDirPath, directoryName, fileName);
            }
        }

        [Test]
        public void OnAfterProjectMigrated_WhenWorkingDirectoryExistsAndOutputDirectoryExists_ThenWorkingDirectoryIsMovedToAndMergedWithOutputDirectory()
        {
            // Setup
            const string fileName = "file_in_wd.txt";
            const string directoryName = "directory_in_wd";

            using (var tempDirectory = new TemporaryDirectory())
            {
                string outputDirPath = Path.Combine(tempDirectory.Path, "output");
                model.ModelSettings.OutputDirectory = outputDirPath;

                string workingDir = model.ModelDataDirectory + "_output";
                PrepareWorkingDirectory(workingDir, fileName, directoryName);

                string existingFilePath = CreateDirectoryWithFile(outputDirPath);

                // Call
                legacyLoader.OnAfterProjectMigrated(project);

                // Assert
                AssertThatOutputFolderIsSetWithCorrectPath(outputDirPath);
                AssertWorkingDirectoryIsCorrectlyMoved(outputDirPath, directoryName, fileName);
                Assert.That(File.Exists(existingFilePath),
                            "Files that were already in output folder should still exist.");
            }
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("folder")]
        public void OnAfterProjectMigrated_WhenOutputDirectoryAndWorkingDirectoryDoNotExist_ThenOutputFolderIsNull(
            string nonExistingDirectory)
        {
            // Setup
            model.ModelSettings.OutputDirectory = nonExistingDirectory;

            // Precondition
            Assert.That(!Directory.Exists(model.ModelDataDirectory + "_output"), "Precondition violated.");

            // Call
            legacyLoader.OnAfterProjectMigrated(project);

            // Assert
            Assert.That(model.OutputFolder, Is.Null,
                        $"After migrating, the {outputFolderName} should be null " +
                        "when the output directory does not exist.");
        }

        [Category(TestCategory.Integration)]
        [TestCase("deltashell-bal.prn", "BalanceOutputTag")]
        [TestCase("deltashell.mon", "MonitoringFileTag")]
        [TestCase("deltashell.lsp", "ProcessFileTag")]
        [TestCase("deltashell.lst", "ListFileTag")]
        public void OnAfterProjectMigrated_WhenDatabaseContainsDataItemsForOutputTextDocuments_ThenTheseDataItemsShouldBeRemovedAndCreatedWhenConnectingMovedOutputFiles(string fileName, string dataItemTag)
        {
            // Setup
            const string directoryName = "directory_in_wd";

            using (var tempDirectory = new TemporaryDirectory())
            {
                string outputDirPath = Path.Combine(tempDirectory.Path, "output");
                model.ModelSettings.OutputDirectory = outputDirPath;

                string workingDir = model.ModelDataDirectory + "_output";
                PrepareWorkingDirectory(workingDir, fileName, directoryName);

                model.DataItems.Add(new DataItem(new TextDocumentFromFile(), DataItemRole.Output, dataItemTag));

                IDataItem dataItem = null;

                // Precondition
                Assert.DoesNotThrow(() => dataItem = model.DataItems.Single(di => di.Tag == dataItemTag), "Precondition violated.");
                Assert.IsTrue(dataItem.Value is TextDocumentFromFile, "Precondition violated.");

                // Call
                legacyLoader.OnAfterProjectMigrated(project);

                // Assert
                Assert.DoesNotThrow(() => dataItem = model.DataItems.Single(di => di.Tag == dataItemTag));
                Assert.IsTrue(dataItem.Value is TextDocument,
                              $"Expected DataItem value type is {typeof(TextDocument)}, but was {dataItem.Value.GetType()}");
            }
        }

        private void AssertThatOutputFolderIsSetWithCorrectPath(string outputDirPath)
        {
            Assert.That(model.OutputFolder, Is.Not.Null,
                        $"After migrating, the {outputFolderName} should not be null " +
                        "when the output directory exists and has output.");
            Assert.That(model.OutputFolder.Path, Is.EqualTo(outputDirPath),
                        $"After migrating, the path of the {outputFolderName} should be set to the path of the output directory.");
        }

        private static void AssertWorkingDirectoryIsCorrectlyMoved(string outputDirPath, string directoryName,
                                                                   string fileName)
        {
            Assert.That(Directory.Exists(outputDirPath),
                        "After migrating, output folder in model directory should exist. ");
            Assert.That(Directory.Exists(Path.Combine(outputDirPath, directoryName)),
                        "After migrating, directories from working directory should be moved to output directory.");
            Assert.That(File.Exists(Path.Combine(outputDirPath, fileName)),
                        "After migrating, files from working directory should be moved to output directory.");
        }

        private static string CreateDirectoryWithFile(string outputDirPath)
        {
            FileUtils.CreateDirectoryIfNotExists(outputDirPath);
            string existingFilePath = Path.Combine(outputDirPath, "file_in_dir.txt");
            File.WriteAllText(existingFilePath, "");

            return existingFilePath;
        }

        private static void PrepareWorkingDirectory(string workingDir, string fileName, string directoryName)
        {
            FileUtils.CreateDirectoryIfNotExists(workingDir);
            File.WriteAllText(Path.Combine(workingDir, fileName), "");
            Directory.CreateDirectory(Path.Combine(workingDir, directoryName));
        }
    }
}