using System.Collections.Generic;
using System.IO;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class WaveDirectoryStructureMigrationHelperTest
    {
        [Test]
        public void MigrateFileStructure_MdwPathNull_ThrowsArgumentNullException()
        {
            void Call() => WaveDirectoryStructureMigrationHelper.MigrateFileStructure(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("mdwPath"));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCase("obw.zip")]
        [TestCase("waddenzee.zip")]
        [TestCase("westerscheldt.zip")]
        public void MigrateFileStructure_ExpectedResults(string testFileName)
        {
            // Note, we assume that the migration of the content of the files
            // is correct, as we test that in the MigratorFactoryTest. As such,
            // we only focus on the overall file directory structure and files.

            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                // Prepare input data
                string inputDataPath = TestHelper.GetTestFilePath(Path.Combine("Migrations", "1.1.0.0", nameof(WaveDirectoryStructureMigrationHelperTest), testFileName));
                ZipFileUtils.Extract(inputDataPath, tempDir.Path);

                // Paths
                string sourceDirectoryPath = Path.Combine(tempDir.Path, "source_data_directory");
                string sourceMdwPath = Path.Combine(sourceDirectoryPath, "waves", "waves.mdw");

                // Call
                WaveDirectoryStructureMigrationHelper.MigrateFileStructure(sourceMdwPath);

                // Assert
                Assert.That(Directory.Exists(sourceDirectoryPath), Is.True);
                Assert.That(Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.TopDirectoryOnly), Is.Empty);

                string[] sourceDirectoriesChildren = Directory.GetDirectories(sourceDirectoryPath);
                Assert.That(sourceDirectoriesChildren.Length, Is.EqualTo(1));

                string sourceModelDirectory = sourceDirectoriesChildren[0];
                Assert.That(Path.GetFileName(sourceModelDirectory), Is.EqualTo("waves"));
                Assert.That(Directory.GetFiles(sourceModelDirectory, "*", SearchOption.TopDirectoryOnly), Is.Empty);

                string[] modelSubFolders = Directory.GetDirectories(sourceModelDirectory);
                Assert.That(modelSubFolders.Length, Is.EqualTo(2));

                string inputSourceFolder = modelSubFolders[0];
                Assert.That(Path.GetFileName(inputSourceFolder), Is.EqualTo("input"));
                string outputSourceFolder = modelSubFolders[1];
                Assert.That(Path.GetFileName(outputSourceFolder), Is.EqualTo("output"));

                string referenceModelFolder = Path.Combine(tempDir.Path, "expected_data_directory", "waves");

                var inputSourceInfo = new DirectoryInfo(inputSourceFolder);
                var outputSourceInfo = new DirectoryInfo(outputSourceFolder);

                var inputRefInfo = new DirectoryInfo(Path.Combine(referenceModelFolder, "input"));
                var outputRefInfo = new DirectoryInfo(Path.Combine(referenceModelFolder, "output"));

                AssertSameFiles(inputSourceInfo, inputRefInfo);
                AssertSameFiles(outputSourceInfo, outputRefInfo);
            }
        }

        private static void AssertSameFiles(DirectoryInfo sourceDirectoryInfo, DirectoryInfo referenceDirectoryInfo)
        {
            FileInfo[] sourceFiles = sourceDirectoryInfo.GetFiles();
            FileInfo[] referenceFiles = referenceDirectoryInfo.GetFiles();

            Assert.That(sourceFiles.Length, Is.EqualTo(referenceFiles.Length));

            for (var i = 0; i < sourceFiles.Length; i++)
            {
                Assert.That(sourceFiles[i].Name, Is.EqualTo(referenceFiles[i].Name));
            }

            DirectoryInfo[] sourceSubDirectories = sourceDirectoryInfo.GetDirectories();
            DirectoryInfo[] referenceSubDirectories = referenceDirectoryInfo.GetDirectories();

            Assert.That(sourceSubDirectories.Length, Is.EqualTo(referenceSubDirectories.Length));

            for (var i = 0; i < sourceSubDirectories.Length; i++)
            {
                AssertSameFiles(sourceSubDirectories[i], 
                                referenceSubDirectories[i]);
            }
        }

        [Test]
        public void GetTemporaryMigrationDirectoryName_SrcDirectoryNull_ThrowsArgumentNullException()
        {
            void Call() => WaveDirectoryStructureMigrationHelper.GetTemporaryMigrationDirectoryName(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("srcDirectory"));
        }

        [Test]
        public void GetTemporaryMigrationDirectoryName_SrcDirectoryParentNull_ThrowsArgumentException()
        {
            var dirInfo = new DirectoryInfo("C:\\");
            void Call() => WaveDirectoryStructureMigrationHelper.GetTemporaryMigrationDirectoryName(dirInfo);

            Assert.Throws<System.ArgumentException>(Call);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetTemporaryMigrationDirectoryName_NoNameClashes_ExpectedNameIsReturned()
        {
            // Setup
            const string srcDirName = "srcDirName";

            using (var tempDir = new TemporaryDirectory())
            {
                var parentDirInfo = new DirectoryInfo(tempDir.Path);
                DirectoryInfo srcDirInfo = parentDirInfo.CreateSubdirectory(srcDirName);

                // Call
                string migrationDirectoryName =
                    WaveDirectoryStructureMigrationHelper.GetTemporaryMigrationDirectoryName(srcDirInfo);

                // Assert
                Assert.That(migrationDirectoryName, Is.EqualTo(srcDirName + "_tmp.1"));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetTemporaryMigrationDirectoryName_NameClashes_ExpectedNameIsReturned()
        {
            // Setup
            const string srcDirName = "srcDirName";

            using (var tempDir = new TemporaryDirectory())
            {
                var parentDirInfo = new DirectoryInfo(tempDir.Path);
                DirectoryInfo srcDirInfo = parentDirInfo.CreateSubdirectory(srcDirName);
                parentDirInfo.CreateSubdirectory(srcDirName + "_tmp.1");
                parentDirInfo.CreateSubdirectory(srcDirName + "_tmp.2");
                parentDirInfo.CreateSubdirectory(srcDirName + "_tmp.3");

                // Call
                string migrationDirectoryName =
                    WaveDirectoryStructureMigrationHelper.GetTemporaryMigrationDirectoryName(srcDirInfo);

                // Assert
                Assert.That(migrationDirectoryName, Is.EqualTo(srcDirName + "_tmp.4"));
            }
        }

        [Test]
        [TestCase(null, null, false)]
        [TestCase("", null, false)]
        [TestCase("someKey WithSpaces=AValue;someOtherKeyWithoutSpaces=AnotherValue", null, false)]
        [TestCase("Data Source=/a/path/to/a/database", "/a/path/to/a/database", true)]
        [TestCase(@"someKey WithSpaces=AValue;Data Source=\a\path\to\a\database;someOtherKeyWithoutSpaces=AnotherValue", @"\a\path\to\a\database", true)]
        [TestCase(@"someKey WithSpaces=AValue;Data Source=C:\a\path\to\a\database;someOtherKeyWithoutSpaces=AnotherValue", @"C:\a\path\to\a\database", true)]
        public void TryParseDatabasePath_ExpectedResults(string connectionString, string expectedDatabasePath, bool couldParse)
        {
            // Call
            bool hasParsed = 
                WaveDirectoryStructureMigrationHelper.TryParseDatabasePath(connectionString, out string databasePath);

            // Assert
            Assert.That(hasParsed, Is.EqualTo(couldParse));
            Assert.That(databasePath, Is.EqualTo(expectedDatabasePath));
        }
        
        private IEnumerable<TestCaseData> UpdateWavmFileFunctionStorePaths_ParameterNull_Data()
        {
            yield return new TestCaseData(null, new WaveModel(), "modelPath");
            yield return new TestCaseData("some/path", null, "model");
        }

        [Test]
        [TestCaseSource(nameof(UpdateWavmFileFunctionStorePaths_ParameterNull_Data))]
        public void UpdateWavmFileFunctionStorePath_ParameterNull_ThrowsArgumentNullException(string modelPath, WaveModel model, string expectedParameterName)
        {
            void Call() => WaveDirectoryStructureMigrationHelper.UpdateWavmFileFunctionStorePaths(modelPath, model);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));

            model.Dispose();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateWavmFileFunctionStorePaths_ExpectedResults()
        {
            // Setup
            string inputDataPath = TestHelper.GetTestFilePath(Path.Combine("Migrations", "1.1.0.0", nameof(WaveDirectoryStructureMigrationHelperTest), "wavm-wad.nc"));

            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string oldModelPath = tempDir.CreateDirectory("oldModel");
                string newModelPath = tempDir.CreateDirectory("newModel");
                string expectedOutputPath = tempDir.CreateDirectory(
                    Path.Combine("newModel", "output"));

                string ncOldPath = Path.Combine(oldModelPath, Path.GetFileName(inputDataPath));
                string ncNewPath = Path.Combine(expectedOutputPath, Path.GetFileName(inputDataPath));

                File.Copy(inputDataPath, ncOldPath);
                File.Copy(inputDataPath, ncNewPath);

                // Associate wavm file function store with the outer domain
                var functionStore = new WavmFileFunctionStore(ncOldPath);
                var dataItem = new DataItem(functionStore, DataItemRole.Output, WaveModel.WavmStoreDataItemTag + model.OuterDomain.Name);
                model.DataItems.Add(dataItem);

                // Call
                WaveDirectoryStructureMigrationHelper.UpdateWavmFileFunctionStorePaths(newModelPath, model);

                // Assert
                Assert.That(functionStore.Path, Is.EqualTo(ncNewPath));
            }
        }
    }
}