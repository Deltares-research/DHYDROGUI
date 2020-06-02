using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    [TestFixture]
    public class WaterFlowFMModel110LegacyLoaderTest
    {
        private const string ProjectFileExtension = ".dsproj";
        private const string ProjectDirExtension = ".dsproj_data";

        private const string OutputDirName = "output";

        private const string SnappedDirectoryName = "snapped";

        [TestCase("run_with_save_and_default_output", "DFM_OUTPUT_TestModel")]
        [TestCase("run_with_save_and_custom_output", "myCustomOutput")]
        [TestCase("run_with_save_and_flat_output", "", Category = TestCategory.Jira)] // Jira-Issue: D3DFMIQ-793
        public void TestDirectoryRestructuring_OutputIsMovedToTheCorrectLocation(string testCaseDir, string outputFMDirName)
        {
            var testDataDirInfo = new DirectoryInfo(TestHelper.GetTestFilePath(
                                                        Path.Combine(@"LegacyLoaderOutput", testCaseDir)));
            Assert.That(testDataDirInfo.Exists);

            var testDirInfo = new DirectoryInfo(FileUtils.CreateTempDirectory());
            Assert.That(testDirInfo.Exists);
            string testDirPath = testDirInfo.FullName;

            var projectName = "TestProject";
            var modelName = "TestModel";

            string projectDirName = projectName + ProjectDirExtension;
            string projectFileName = projectName + ProjectFileExtension;
            string modelDirName = modelName;
            string outputWAQDirName = $"DFM_DELWAQ_{modelName}";
            string mduFileName = modelName + ".mdu";
            string explicitWorkingDirName = $"{modelName}_output";

            // Set expected paths
            string projectFilePath = Path.Combine(testDirPath, projectFileName);
            string projectDirPath = Path.Combine(testDirPath, projectDirName);
            string modelDirPath = Path.Combine(projectDirPath, modelDirName);
            string outputDirPath = Path.Combine(modelDirPath, OutputDirName);
            string outputWAQDirPath = Path.Combine(outputDirPath, outputWAQDirName);

            // To be removed paths
            string explicitWorkingDirPath = Path.Combine(projectDirPath, explicitWorkingDirName);
            string oldOutputFMDirPath = Path.Combine(modelDirPath, outputFMDirName);
            string oldOutputWAQDirPath = Path.Combine(modelDirPath, outputWAQDirName);

            var filtersOutput = new List<string>
            {
                ".out",
                ".dia",
                "_timings.txt",
                ".tek",
                "_rst.nc",
                "_his.nc",
                "_map.nc",
                "_clm.nc",
                "_numlimdth.xyz"
            };

            var filtersOutputWAQ = new List<string>
            {
                ".are",
                ".atr",
                ".bnd",
                ".flo",
                ".hyd",
                ".len",
                ".poi",
                ".srf",
                ".srfold",
                ".tau",
                ".vol",
                "_waqgeom.nc",
                ".sal",
                ".tem",
                ".vdf"
            };

            List<string> filters = filtersOutput
                                   .Union(filtersOutputWAQ)
                                   .ToList();

            try
            {
                FileUtils.CopyAll(testDataDirInfo, testDirInfo, string.Empty);

                Assert.That(Directory.Exists(explicitWorkingDirPath));
                Assert.That(Directory.Exists(oldOutputFMDirPath));
                Assert.That(Directory.Exists(oldOutputWAQDirPath));

                // Get model for test
                string mduFilePath = Path.Combine(modelDirPath, mduFileName);
                Assert.That(File.Exists(mduFilePath));

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                // Get all output files in the model directory before migration
                List<string> outputFilesBeforeMigration = GetAllFilesByFilter(filters, modelDirPath);

                // When there is no FM output folder, output is placed directly under "output" 

                // Select for each folder all the files that should be there after migration
                List<string> filesForOutput = GetAllFilesByFilter(filtersOutput, modelDirPath);
                List<string> filesForOutputWAQ = GetAllFilesByFilter(filtersOutputWAQ, modelDirPath);

                // Perform migration
                TypeUtils.CallPrivateStaticMethod(typeof(WaterFlowFMModel110LegacyLoader),
                                                  "PerformDirectoryRestructuring",
                                                  model, explicitWorkingDirPath);

                // Assert every expected (output) folder exists
                Assert.That(File.Exists(projectFilePath));
                Assert.That(Directory.Exists(projectDirPath));
                Assert.That(Directory.Exists(modelDirPath));
                Assert.That(Directory.Exists(outputDirPath));
                Assert.That(Directory.Exists(outputWAQDirPath));
                if (oldOutputFMDirPath != modelDirPath)
                {
                    Assert.That(!Directory.Exists(oldOutputFMDirPath));
                }

                // Get all output files in the model directory after migration
                List<string> outputFilesAfterMigration = filters
                                                         .SelectMany(filter => Directory.GetFiles(outputDirPath, "*.*", SearchOption.AllDirectories)
                                                                                        .Where(filePath => filePath.EndsWith(filter))
                                                                                        .Select(filePath => Path.GetFileName(filePath)))
                                                         .ToList();

                // Assert there are no duplicate output files in output folder
                Assert.IsTrue(outputFilesAfterMigration.HasUniqueValues());

                // Assert that no extra files are created in the output folder after migration
                List<string> extraFiles = outputFilesAfterMigration.Except(outputFilesBeforeMigration).ToList();
                Assert.IsEmpty(extraFiles,
                               $"The following files were added after migration: {string.Join(", ", extraFiles)}");

                // Assert that no files are removed from the output folder after migration 
                List<string> missingFiles = outputFilesBeforeMigration.Except(outputFilesAfterMigration).ToList();
                Assert.IsEmpty(missingFiles,
                               $"The following files are missing after migration: {string.Join(", ", missingFiles)}");

                // Assert every expected file is in destined directory
                AssertNoMissingFilesAndDirectoryFilesInDirectory(outputDirPath, filesForOutput, SnappedDirectoryName);
                AssertNoMissingFilesAndDirectoryFilesInDirectory(outputWAQDirPath, filesForOutputWAQ);
            }
            finally
            {
                FileUtils.DeleteIfExists(testDirPath);
            }
        }

        [TestCase("run_with_save_and_extra_output", "DFM_OUTPUT_TestModel")]
        [Category(TestCategory.Jira)] // D3DFMIQ-1344
        public void TestDirectoryRestructuring_OutputSubfoldersAreRemained(string testCaseDir, string outputFMDirName)
        {
            var testDataDirInfo = new DirectoryInfo(TestHelper.GetTestFilePath(
                                                        Path.Combine(@"LegacyLoaderOutput", testCaseDir)));
            Assert.That(testDataDirInfo.Exists);

            var testDirInfo = new DirectoryInfo(FileUtils.CreateTempDirectory());
            Assert.That(testDirInfo.Exists);
            string testDirPath = testDirInfo.FullName;

            var projectName = "TestProject";
            var modelName = "TestModel";

            string projectDirName = projectName + ProjectDirExtension;
            string projectFileName = projectName + ProjectFileExtension;
            string modelDirName = modelName;
            string outputWAQDirName = $"DFM_DELWAQ_{modelName}";
            string mduFileName = modelName + ".mdu";
            string explicitWorkingDirName = $"{modelName}_output";

            // Set expected default paths
            string projectFilePath = Path.Combine(testDirPath, projectFileName);
            string projectDirPath = Path.Combine(testDirPath, projectDirName);
            string modelDirPath = Path.Combine(projectDirPath, modelDirName);
            string outputDirPath = Path.Combine(modelDirPath, OutputDirName);
            string outputWAQDirPath = Path.Combine(outputDirPath, outputWAQDirName);

            // Set expected extra output file/folder paths
            string extraFolderB2_Path = Path.Combine(outputDirPath, "ExtraFolderB2");
            string extraFileB2_Path = Path.Combine(outputDirPath, "ExtraFileB2.txt");
            string extraFolderC2_Path = Path.Combine(outputWAQDirPath, "ExtraFolderC2");
            string extraFileC2_Path = Path.Combine(outputWAQDirPath, "ExtraFileC2.txt");
            string snappedDirectoryPath = Path.Combine(outputDirPath, SnappedDirectoryName);
            string explicitWorkingDirPath = Path.Combine(projectDirPath, explicitWorkingDirName);

            try
            {
                FileUtils.CopyAll(testDataDirInfo, testDirInfo, string.Empty);
                Assert.That(!FileUtils.IsDirectoryEmpty(testDirPath));

                // Get model for test
                string mduFilePath = Path.Combine(modelDirPath, mduFileName);
                Assert.That(File.Exists(mduFilePath));

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                // Perform migration
                TypeUtils.CallPrivateStaticMethod(typeof(WaterFlowFMModel110LegacyLoader),
                                                  "PerformDirectoryRestructuring",
                                                  model, explicitWorkingDirPath);

                // Assert every expected (output) folder exists
                Assert.That(File.Exists(projectFilePath));
                Assert.That(Directory.Exists(projectDirPath));
                Assert.That(Directory.Exists(modelDirPath));
                Assert.That(Directory.Exists(outputDirPath));
                Assert.That(Directory.Exists(outputWAQDirPath));
                Assert.That(!Directory.Exists(Path.Combine(modelDirPath, outputFMDirName)));
                Assert.That(Directory.Exists(extraFolderB2_Path));
                Assert.That(File.Exists(extraFileB2_Path));
                Assert.That(Directory.Exists(extraFolderC2_Path));
                Assert.That(File.Exists(extraFileC2_Path));
                Assert.That(Directory.Exists(snappedDirectoryPath));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDirPath);
            }
        }

        private static List<string> GetAllFilesByFilter(List<string> filters, string dirPath, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return filters
                   .SelectMany(filter => Directory.GetFiles(dirPath, "*.*", searchOption)
                                                  .Where(filePath => filePath.EndsWith(filter))
                                                  .Select(filePath => Path.GetFileName(filePath)))
                   .Distinct()
                   .ToList();
        }

        private static void AssertNoMissingFilesAndDirectoryFilesInDirectory(string targetDirPath, List<string> filesForFolder, string directoryName = null)
        {
            List<string> missingFiles = filesForFolder
                                        .Except(Directory.GetFiles(targetDirPath)
                                                         .Select(filePath => Path.GetFileName(filePath))
                                                         .ToList())
                                        .ToList();

            Assert.IsEmpty(missingFiles,
                           $"The following files are missing in '{new DirectoryInfo(targetDirPath).Name}': " +
                           $"{string.Join(", ", missingFiles)}");

            if (directoryName == null)
            {
                return;
            }

            string targetDirectory = Directory.GetDirectories(targetDirPath)
                                              .FirstOrDefault(d => Path.GetFileName(d) == directoryName);
            Assert.NotNull(targetDirectory, $"Directory '{directoryName}' does not exist in directory '{targetDirPath}', but was expected.");
        }
    }
}