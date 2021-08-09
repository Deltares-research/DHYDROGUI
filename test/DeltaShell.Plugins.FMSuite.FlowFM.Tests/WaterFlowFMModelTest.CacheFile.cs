using System.IO;
using System.Text;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    /// <summary>
    /// Integration tests for the <see cref="CacheFile"/>.
    /// </summary>
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        private const string input = "model_input";
        private const string output = "model_output";
        private const string sourceDirectoryName = "WaterFlowFMModel.CacheFile";
        private const string mduFileName = "CacheFileTest.mdu";

        [Test]
        [Category(TestCategory.Integration)]
        public void Constructor_CorrectCacheFile()
        {
            // Call
            using (var model = new WaterFlowFMModel())
            {
                // Assert
                Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                Assert.That(model.CacheFile.Exists, Is.False, "Expected no .cache file on disk to exist.");

                string expectedPath = Path.ChangeExtension(model.MduFilePath, FileConstants.CachingFileExtension);
                Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void Constructor_WithMduPath_CorrectCacheFile()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string inputPath = Path.Combine(tempDir.Path, input);
                FileUtils.CreateDirectoryIfNotExists(inputPath);

                CopyInputData(inputPath);
                string mduFilePath = Path.Combine(inputPath, mduFileName);

                // Call
                using (var model = new WaterFlowFMModel(mduFilePath))
                {
                    // Assert
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.False, "Expected no .cache file on disk to exist.");

                    string expectedPath = Path.ChangeExtension(model.MduFilePath, FileConstants.CachingFileExtension);
                    Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenRun_ThenTheCacheFileIsCorrect()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                string inputPath = Path.Combine(tempDir.Path, input);
                FileUtils.CreateDirectoryIfNotExists(inputPath);

                CopyInputData(inputPath);
                string mduFilePath = Path.Combine(inputPath, mduFileName);

                using (var model = new WaterFlowFMModel(mduFilePath))
                {
                    // When
                    Run(model);

                    // Then
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.True, "Expected a .cache file to be generated.");

                    // After running the .cache file should be generated in the working directory.
                    string fullPath = Path.GetFullPath(model.CacheFile.Path);
                    Assert.That(fullPath, Does.StartWith(model.WorkingDirectory));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenExported_ThenTheCacheFileIsCorrect()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir,
                                  out string inputMduFilePath,
                                  out string outputMduFilePath);

                using (var model = new WaterFlowFMModel(inputMduFilePath))
                {
                    // When
                    Export(model, outputMduFilePath);

                    // Then
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.False, "Expected a .cache file to be generated.");

                    // After running and saving the .cache file should be in the save directory.
                    string expectedPath = Path.ChangeExtension(model.MduFilePath, FileConstants.CachingFileExtension);
                    Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenAWaterFlowFMModel_WhenRunAndExported_ThenTheCacheFileIsCorrect()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir,
                                  out string inputMduFilePath,
                                  out string outputMduFilePath);

                using (var model = new WaterFlowFMModel(inputMduFilePath))
                {
                    // When
                    Run(model);
                    Export(model, outputMduFilePath);

                    // Then
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.True, "Expected a .cache file to be generated.");

                    // After running and saving the .cache file should be in the save directory.
                    string expectedPath = Path.ChangeExtension(model.MduFilePath, FileConstants.CachingFileExtension);
                    Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenExportedAndRun_ThenTheCacheFileIsCorrect()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir,
                                  out string inputMduFilePath,
                                  out string outputMduFilePath);

                using (var model = new WaterFlowFMModel(inputMduFilePath))
                {
                    // When
                    Export(model, outputMduFilePath);
                    Run(model);

                    // Then
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.True, "Expected a .cache file to be generated.");

                    // After running and saving the .cache file should be in the save directory.
                    string fullPath = Path.GetFullPath(model.CacheFile.Path);
                    Assert.That(fullPath, Does.StartWith(model.WorkingDirectory));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenExportedAndRunAndExported_ThenTheCacheFileIsCorrect()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir,
                                  out string inputMduFilePath,
                                  out string outputMduFilePath);

                using (var model = new WaterFlowFMModel(inputMduFilePath))
                {
                    // When
                    Export(model, outputMduFilePath);
                    Run(model);
                    Export(model, outputMduFilePath);

                    // Then
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.True, "Expected a .cache file to be generated.");

                    // After running and saving the .cache file should be in the save directory.
                    string expectedPath = Path.ChangeExtension(model.MduFilePath, FileConstants.CachingFileExtension);
                    Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModelWithoutUseCaching_WhenRunAndExported_ThenTheCacheFileIsCorrect()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir,
                                  out string inputMduFilePath,
                                  out string outputMduFilePath);

                using (var model = new WaterFlowFMModel(inputMduFilePath))
                {
                    model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).Value = false;

                    // When
                    Run(model);
                    Export(model, outputMduFilePath);

                    // Then
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.False, "Expected a .cache file to be generated.");

                    // After running and saving the .cache file should be in the save directory.
                    string expectedPath = Path.ChangeExtension(model.MduFilePath, FileConstants.CachingFileExtension);
                    Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenRunAndExportedWithoutSwitch_ThenTheCacheFileIsCorrect()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir,
                                  out string inputMduFilePath,
                                  out string outputMduFilePath);

                using (var model = new WaterFlowFMModel(inputMduFilePath))
                {
                    // When
                    Run(model);
                    Export(model, outputMduFilePath, false);

                    // Then
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.True, "Expected a .cache file to be generated.");

                    // After running and saving the .cache file should be in the save directory.
                    string fullPath = Path.GetFullPath(model.CacheFile.Path);
                    Assert.That(fullPath, Does.StartWith(model.WorkingDirectory));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAnExportedWaterFlowFMModelWithACacheFile_WhenOpenedAndExported_ThenTheCacheFileIsCorrectlyCopied()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir,
                                  out string inputMduFilePath,
                                  out string outputMduFilePath);
                CreateDummyInputCacheFileForMdu(inputMduFilePath);

                // When
                using (var model = new WaterFlowFMModel(inputMduFilePath))
                {
                    Export(model, outputMduFilePath);

                    // Then
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.True, "Expected a .cache file to be imported.");

                    string expectedPath = Path.ChangeExtension(model.MduFilePath, FileConstants.CachingFileExtension);
                    Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");

                    Assert.That(File.Exists(model.CacheFile.Path),
                                $"Expected the cache file to exist at {model.CacheFile.Path}");
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenAHydroModelWithFMModelAndCacheFile_WhenInitializeIsCalled_ThenTheCacheFileShouldBeCopiedToWorkingDirectory()
        {
            // Given
            using (var tempDirectory = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                string testTempDirectory = tempDirectory.Path;
                string saveFolderPath = Path.Combine(testTempDirectory, "SaveLocation");
                Directory.CreateDirectory(saveFolderPath);

                string saveFolderCacheFilePath = Path.Combine(saveFolderPath, "test.cache");
                string mduFilePath = Path.Combine(saveFolderPath, "test.mdu");

                using (FileStream fs = File.Create(saveFolderCacheFilePath))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes("test");
                    fs.Write(info, 0, info.Length);
                }

                model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                model.WorkingDirectoryPathFunc = () => testTempDirectory;
                model.CacheFile.UpdatePathToMduLocation(mduFilePath);
                model.ModelDefinition.SetModelProperty(KnownProperties.UseCaching, true);

                // When 
                model.Initialize();

                // Then
                Assert.AreEqual(saveFolderCacheFilePath, model.CacheFile.Path);
                Assert.IsTrue(File.Exists(Path.Combine(model.WorkingDirectory, model.DirectoryName, model.Name + ".cache")));
            }
        }
        [Test]
        public void OnFinishIntegratedModelRun_WhenUseCachingIsTrue_SetsCacheFileToTheCorrectWorkingDirectory()
        {
            string workingDirectoryIntegratedModel = Path.Combine(Path.GetTempPath(), "IntegratedModel");

            // Setup
            using (var model = new WaterFlowFMModel())
            {
                // Call
                model.ModelDefinition.SetModelProperty(KnownProperties.UseCaching, true);
                
                model.OnFinishIntegratedModelRun(workingDirectoryIntegratedModel);
                
                string expectedPath = Path.Combine(workingDirectoryIntegratedModel, model.DirectoryName,
                                                   Path.ChangeExtension(model.Name,
                                                                        FileConstants.CachingFileExtension));

                // Assert
                Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
            }
        }

        [Test]
        public void OnFinishIntegratedModelRun_WhenUseCachingIsFalse_SetsCacheFileToTheCorrectWorkingDirectory()
        {
            string workingDirectoryIntegratedModel = Path.Combine(Path.GetTempPath(), "IntegratedModel");

            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).SetValueAsString("false");
                // Call
                model.OnFinishIntegratedModelRun(workingDirectoryIntegratedModel);

                // Assert
                Assert.IsNull(model.CacheFile.Path);
            }
        }
        private static void CopyInputData(string inputFolder)
        {
            string sourceFolder = Path.Combine(TestHelper.GetTestDataDirectory(), sourceDirectoryName);

            var inputPathInfo = new DirectoryInfo(inputFolder);
            var sourcePathInfo = new DirectoryInfo(sourceFolder);

            FileUtils.CopyAll(sourcePathInfo, inputPathInfo, null);
        }

        private static void PrepareFileSystem(TemporaryDirectory tempDir,
                                              out string inputMduFilePath,
                                              out string outputMduFilePath)
        {
            string inputPath = Path.Combine(tempDir.Path, input);
            FileUtils.CreateDirectoryIfNotExists(inputPath);
            inputMduFilePath = Path.Combine(inputPath, mduFileName);

            CopyInputData(inputPath);

            string outputPath = Path.Combine(tempDir.Path, output);
            FileUtils.CreateDirectoryIfNotExists(outputPath);
            outputMduFilePath = Path.Combine(outputPath, mduFileName);
        }

        private static void CreateDummyInputCacheFileForMdu(string mduPath)
        {
            string dummyCacheFilePath = Path.ChangeExtension(mduPath, FileConstants.CachingFileExtension);
            using (FileStream _ = File.Create(dummyCacheFilePath)) {}
        }

        private static void Run(WaterFlowFMModel model)
        {
            ActivityRunner.RunActivity(model);
            Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned), "Model run has failed, status was:");
        }

        private static void Export(WaterFlowFMModel model, string exportPath, bool switchTo = true)
        {
            model.ExportTo(exportPath, switchTo);
        }
    }
}