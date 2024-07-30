using System.IO;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

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
                PrepareFileSystem(tempDir.Path, 
                                  out string mduFilePath);

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
        [TestCase("")]
        [TestCase(@"computations\test\JAMM\D2776")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenRun_ThenTheCacheFileIsCorrect(string mduDirectory)
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir.Path, 
                                  mduDirectory,
                                  out string mduFilePath,
                                  out string _);

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
        [TestCase("")]
        [TestCase(@"computations\test\JAMM\D2776")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenExported_ThenTheCacheFileIsCorrect(string mduDirectory)
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir.Path,
                                  mduDirectory,
                                  out string inputMduFilePath,
                                  out string outputMduFilePath);

                using (var model = new WaterFlowFMModel(inputMduFilePath))
                {
                    // When
                    Export(model, outputMduFilePath);

                    // Then
                    Assert.That(model.CacheFile, Is.Not.Null, "Expected a .cache file to not be null.");
                    Assert.That(model.CacheFile.Exists, Is.False, "Expected a .cache file not to be generated.");

                    // After running and saving the .cache file should be in the save directory.
                    string expectedPath = Path.ChangeExtension(model.MduFilePath, FileConstants.CachingFileExtension);
                    Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
                }
            }
        }

        [Test]
        [TestCase("")]
        [TestCase(@"computations\hist\stormperiode_2002")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenAWaterFlowFMModel_WhenRunAndExported_ThenTheCacheFileIsCorrect(string mduDirectory)
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir.Path,
                                  mduDirectory,
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
        [TestCase("")]
        [TestCase(@"computations\hist\stormperiode_2002")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenExportedAndRun_ThenTheCacheFileIsCorrect(string mduDirectory)
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir.Path,
                                  mduDirectory,
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
        [TestCase("")]
        [TestCase(@"computations\test\S_2000")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenExportedAndRunAndExported_ThenTheCacheFileIsCorrect(string mduDirectory)
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir.Path,
                                  mduDirectory,
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
        [TestCase("")]
        [TestCase(@"computations\hist\2007_metRD_zonderMD")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModelWithoutUseCaching_WhenRunAndExported_ThenTheCacheFileIsCorrect(string mduDirectory)
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir.Path,
                                  mduDirectory,
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
                    Assert.That(model.CacheFile.Exists, Is.False, "Expected a .cache file not to be generated.");

                    // After running and saving the .cache file should be in the save directory.
                    string expectedPath = Path.ChangeExtension(model.MduFilePath, FileConstants.CachingFileExtension);
                    Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
                }
            }
        }

        [Test]
        [TestCase("")]
        [TestCase(@"computations\hist\2007_metRD_zonderMD")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAWaterFlowFMModel_WhenRunAndExportedWithoutSwitchTo_ThenTheCacheFileIsCorrect(string mduDirectory)
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir.Path,
                                  mduDirectory,
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
        [TestCase("")]
        [TestCase(@"computations\test\JAMM\D2776")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void GivenAnExportedWaterFlowFMModelWithACacheFile_WhenOpenedAndExported_ThenTheCacheFileIsCorrectlyCopied(string mduDirectory)
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                PrepareFileSystem(tempDir.Path,
                                  mduDirectory,
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
        [TestCase("")]
        [TestCase(@"computations\test\JAMM\D2776")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenAHydroModelWithFMModelAndCacheFile_WhenInitializeIsCalled_ThenTheCacheFileShouldBeCopiedToWorkingDirectory(string mduDirectory)
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                string tempPath = tempDir.Path;
                
                PrepareFileSystem(tempPath,
                                  mduDirectory,
                                  out string mduFilePath,
                                  out string _);
                CreateDummyInputCacheFileForMdu(mduFilePath);
                
                // When
                using (var model = new WaterFlowFMModel(mduFilePath))
                {
                    model.WorkingDirectoryPathFunc = () => tempPath;

                    model.Initialize();

                    // Then
                    string expectedPath = Path.Combine(model.WorkingDirectory, model.DirectoryName, model.Name + FileConstants.CachingFileExtension);
                    Assert.That(File.Exists(expectedPath), "Expected a different path:");
                }
            }
        }

        private static void PrepareFileSystem(string baseDirectory, out string inputMduFilePath)
        {
            PrepareFileSystem(baseDirectory, string.Empty, out inputMduFilePath, out _);
        }
        
        private static void PrepareFileSystem(string baseDirectory, string mduDirectory, out string inputMduFilePath, out string outputMduFilePath)
        {
            string modelName = Path.GetFileNameWithoutExtension(mduFileName);
            
            string inputPath = Path.Combine(baseDirectory, input, modelName, mduDirectory);
            FileUtils.CreateDirectoryIfNotExists(inputPath);
            inputMduFilePath = Path.Combine(inputPath, mduFileName);

            CopyInputData(inputPath);

            string outputPath = Path.Combine(baseDirectory, output, modelName, mduDirectory);
            FileUtils.CreateDirectoryIfNotExists(outputPath);
            outputMduFilePath = Path.Combine(outputPath, mduFileName);
        }
        
        private static void CopyInputData(string inputFolder)
        {
            string sourceFolder = Path.Combine(TestHelper.GetTestDataDirectory(), sourceDirectoryName);

            var inputPathInfo = new DirectoryInfo(inputFolder);
            var sourcePathInfo = new DirectoryInfo(sourceFolder);

            FileUtils.CopyAll(sourcePathInfo, inputPathInfo, null);
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