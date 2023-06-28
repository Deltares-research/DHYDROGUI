using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WaveOutputDataHarvesterTest
    {
        [Test]
        public void Constructor_FeatureProviderNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WaveOutputDataHarvester(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("featureContainer"));
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());

            // Assert
            Assert.That(harvester, Is.InstanceOf<IWaveOutputDataHarvester>());
        }

        [Test]
        public void HarvestDiagnosticFiles_DirectoryInfoNull_ThrowsArgumentNullException()
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());

            // Call | Assert
            void Call() => harvester.HarvestDiagnosticFiles(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("outputDataDirectory"));
        }

        public static IEnumerable<TestCaseData> GetHarvestDiagnosticFilesValidData()
        {
            const string logName = "swan_bat.log";
            const string logContent = "Some log data.";
            var logFile = new ReadOnlyTextFileData(logName, logContent, ReadOnlyTextFileDataType.Default);

            const string diagName = "swn-diag.Waves";
            const string diagContent = "Some diagnostic data.";
            var diagFile = new ReadOnlyTextFileData(diagName, diagContent, ReadOnlyTextFileDataType.Default);

            const string otherFileName = "waves.mdw";
            const string otherFileContent = "swoosh swash";
            var otherFile = new ReadOnlyTextFileData(otherFileName, otherFileContent, ReadOnlyTextFileDataType.Default);

            const string diagAlternativeName = "swn-diag.dwaves";
            const string diagAlternativeContent = "Some diagnostic data.";
            var diagAlternativeFile = 
                new ReadOnlyTextFileData(diagAlternativeName, diagAlternativeContent, ReadOnlyTextFileDataType.Default);

            yield return new TestCaseData(new List<ReadOnlyTextFileData>(), 
                                          new List<ReadOnlyTextFileData>());
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              otherFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>());
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                              otherFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                              otherFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                              logFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                              logFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                              diagFile,
                                              otherFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                              diagFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              diagAlternativeFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              diagAlternativeFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                              diagAlternativeFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                              diagAlternativeFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                              diagAlternativeFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                              diagAlternativeFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              otherFile,
                                              diagAlternativeFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              diagAlternativeFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                              otherFile,
                                              diagAlternativeFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                              diagAlternativeFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                              otherFile,
                                              diagAlternativeFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                              diagAlternativeFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                              logFile,
                                              diagAlternativeFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              diagFile,
                                              logFile,
                                              diagAlternativeFile,
                                          });
            yield return new TestCaseData(new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                              diagFile,
                                              otherFile,
                                              diagAlternativeFile,
                                          }, 
                                          new List<ReadOnlyTextFileData>
                                          {
                                              logFile,
                                              diagFile,
                                              diagAlternativeFile,
                                          });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetHarvestDiagnosticFilesValidData))]
        public void HarvestDiagnosticFiles_ExpectedResults(IList<ReadOnlyTextFileData> inputFiles, 
                                                           IList<ReadOnlyTextFileData> expectedDiagnosticFiles)
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());
            using (var tempDir = new TemporaryDirectory())
            {
                BuildFiles(tempDir, inputFiles);
                var outputDir = new DirectoryInfo(tempDir.Path);

                // Call
                IReadOnlyList<ReadOnlyTextFileData> result = harvester.HarvestDiagnosticFiles(outputDir);

                // Assert
                Assert.That(result, Is.EquivalentTo(expectedDiagnosticFiles).Using(new ReadOnlyTextFileDataEqualityComparer()));
            }
        }

        [Test]
        public void HarvestSpectraFiles_DirectoryInfoNull_ThrowsArgumentNullException()
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());

            // Call | Assert
            void Call() => harvester.HarvestSpectraFiles(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("outputDataDirectory"));
        }

        public static IEnumerable<TestCaseData> GetHarvestSpectraFilesValidData()
        {
            List<ReadOnlyTextFileData> sp1Files =
                Enumerable.Range(0, 3)
                          .Select(i => new ReadOnlyTextFileData($"spectra{i}.sp1", $"some content {i}", ReadOnlyTextFileDataType.Default))
                          .ToList();
            List<ReadOnlyTextFileData> sp2Files =
                Enumerable.Range(0, 3)
                          .Select(i => new ReadOnlyTextFileData($"spectra{i}.sp2", $"some content {i}", ReadOnlyTextFileDataType.Default))
                          .ToList();

            ReadOnlyTextFileData[] otherFiles = new[]
            {
                new ReadOnlyTextFileData("swan_bat.log", "Some log data.", ReadOnlyTextFileDataType.Default),
                new ReadOnlyTextFileData("swn-diag.Waves", "Some diagnostic data", ReadOnlyTextFileDataType.Default),
                new ReadOnlyTextFileData("waves.mdw", "Swoosh swash", ReadOnlyTextFileDataType.Default),
            };
            
            yield return new TestCaseData(new List<ReadOnlyTextFileData>(), 
                                          new List<ReadOnlyTextFileData>());
            yield return new TestCaseData(otherFiles, 
                                          new List<ReadOnlyTextFileData>());
            yield return new TestCaseData(sp1Files,
                                          sp1Files);
            yield return new TestCaseData(sp2Files,
                                          sp2Files);
            yield return new TestCaseData(sp1Files.Concat(sp2Files).ToList(),
                                          sp1Files.Concat(sp2Files).ToList());
            yield return new TestCaseData(sp1Files.Concat(sp2Files)
                                                  .Concat(otherFiles)
                                                  .ToList(),
                                          sp1Files.Concat(sp2Files).ToList());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetHarvestSpectraFilesValidData))]
        public void HarvestSpectraFiles_ExpectedResults(IList<ReadOnlyTextFileData> inputFiles, 
                                                        IList<ReadOnlyTextFileData> expectedDiagnosticFiles)
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());
            using (var tempDir = new TemporaryDirectory())
            {
                BuildFiles(tempDir, inputFiles);
                var outputDir = new DirectoryInfo(tempDir.Path);

                // Call
                IReadOnlyList<ReadOnlyTextFileData> result = harvester.HarvestSpectraFiles(outputDir);

                // Assert
                Assert.That(result, Is.EquivalentTo(expectedDiagnosticFiles).Using(new ReadOnlyTextFileDataEqualityComparer()));
            }
        }
        
        [Test]
        public void HarvestSwanFiles_DirectoryInfoNull_ThrowsArgumentNullException()
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());

            // Call | Assert
            void Call() => harvester.HarvestSwanFiles(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("outputDataDirectory"));
        }

        public static IEnumerable<TestCaseData> GetHarvestSwanFilesData()
        {
            List<ReadOnlyTextFileData> swanFiles1 =
                Enumerable.Range(0, 3)
                          .Select(i => new ReadOnlyTextFileData($"INPUT_1_20060105_0{i}0000", $"PROJECT {i}", ReadOnlyTextFileDataType.Default))
                          .ToList();
            List<ReadOnlyTextFileData> swanFiles2 =
                Enumerable.Range(0, 3)
                          .Select(i => new ReadOnlyTextFileData($"INPUT_2_20060105_0{i}0000", $"PROJECT {i}", ReadOnlyTextFileDataType.Default))
                          .ToList();
            
            var invalidFiles = new[]
            {
                new ReadOnlyTextFileData("INPUT_1_20060105_000000.swan", "PROJECT", ReadOnlyTextFileDataType.Default),
                new ReadOnlyTextFileData("INPUT1_20060105_000000", "PROJECT", ReadOnlyTextFileDataType.Default),
                new ReadOnlyTextFileData("input_1_20060105_000000", "PROJECT", ReadOnlyTextFileDataType.Default)
            };

            var otherFiles = new[]
            {
                new ReadOnlyTextFileData("swan_bat.log", "Some log data.", ReadOnlyTextFileDataType.Default),
                new ReadOnlyTextFileData("swn-diag.Waves", "Some diagnostic data", ReadOnlyTextFileDataType.Default),
                new ReadOnlyTextFileData("waves.mdw", "Swoosh swash", ReadOnlyTextFileDataType.Default),
            };
            
            yield return new TestCaseData(new List<ReadOnlyTextFileData>(), 
                                          new List<ReadOnlyTextFileData>());
            yield return new TestCaseData(invalidFiles, 
                                          new List<ReadOnlyTextFileData>());
            yield return new TestCaseData(otherFiles, 
                                          new List<ReadOnlyTextFileData>());
            yield return new TestCaseData(swanFiles1,
                                          swanFiles1);
            yield return new TestCaseData(swanFiles2,
                                          swanFiles2);
            yield return new TestCaseData(swanFiles1.Concat(swanFiles2).ToList(),
                                          swanFiles1.Concat(swanFiles2).ToList());
            yield return new TestCaseData(swanFiles1.Concat(swanFiles2)
                                                  .Concat(otherFiles)
                                                  .ToList(),
                                          swanFiles1.Concat(swanFiles2).ToList());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetHarvestSwanFilesData))]
        public void HarvestSwanFiles_ExpectedResults(IList<ReadOnlyTextFileData> inputFiles, 
                                                     IList<ReadOnlyTextFileData> expectedFiles)
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());
            using (var tempDir = new TemporaryDirectory())
            {
                BuildFiles(tempDir, inputFiles);
                var outputDir = new DirectoryInfo(tempDir.Path);

                // Call
                IReadOnlyList<ReadOnlyTextFileData> result = harvester.HarvestSwanFiles(outputDir);

                // Assert
                Assert.That(result, Is.EquivalentTo(expectedFiles).Using(new ReadOnlyTextFileDataEqualityComparer()));
            }
        }

        [Test]
        public void HarvestWavmFileFunctionStores_DirectoryInfoNull_ThrowsArgumentNullException()
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());

            // Call | Assert
            void Call() => harvester.HarvestWavmFileFunctionStores(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("outputDataDirectory"));
        }

        public static IEnumerable<TestCaseData> GetHarvestWavmFileFunctionStoresValidData()
        {
            IList<string> sp1Files =
                Enumerable.Range(0, 3)
                          .Select(i => $"spectra{i}.sp1")
                          .ToList();
            IList<string> sp2Files =
                Enumerable.Range(0, 3)
                          .Select(i => $"spectra{i}.sp2")
                          .ToList();

            IList<string> wavhFiles =
                Enumerable.Range(0, 3)
                          .Select(i => $"wavh-Waves{i}.nc")
                          .ToList();

            IList<string> otherFiles =
                new [] {
                        "swan_bat.log",
                        "swn-diag.Waves",
                        "waves.mdw",}
                    .Concat(sp1Files)
                    .Concat(sp2Files)
                    .ToList();

            IList<string> wavmFiles =
                Enumerable.Range(0, 3)
                          .Select(i => $"wavm-Waves{i}.nc")
                          .ToList();
            
            yield return new TestCaseData(new List<string>(), 
                                          new List<string>());
            yield return new TestCaseData(otherFiles, 
                                          new List<string>());
            yield return new TestCaseData(wavhFiles, 
                                          new List<string>());
            yield return new TestCaseData(otherFiles.Concat(wavhFiles).ToList(), 
                                          new List<string>());
            yield return new TestCaseData(new List<string>(),
                                          wavmFiles);
            yield return new TestCaseData(otherFiles,
                                          wavmFiles);
            yield return new TestCaseData(wavhFiles,
                                          wavmFiles);
            yield return new TestCaseData(otherFiles.Concat(wavhFiles).ToList(),
                                          wavmFiles);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetHarvestWavmFileFunctionStoresValidData))]
        public void HarvestWavmFileFunctionStores_ExpectedResults(IList<string> inputTextFiles, 
                                                                  IList<string> wavmFiles)
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());
            using (var tempDir = new TemporaryDirectory())
            {
                foreach (string inputFileName in inputTextFiles)
                {
                    tempDir.CreateFile(inputFileName);
                }

                var expectedPaths = new List<string>();
                foreach (string wavmFileName in wavmFiles)
                {
                    string testPath = tempDir.CopyTestDataFileToTempDirectory("./WaveOutputDataHarvesterTest/wavm-Waves.nc");
                    string storePath = Path.Combine(Path.GetDirectoryName(testPath), wavmFileName);
                    File.Move(testPath, storePath);

                    expectedPaths.Add(storePath);
                }

                var outputDir = new DirectoryInfo(tempDir.Path);

                // Call
                IReadOnlyList<IWavmFileFunctionStore> result = 
                    harvester.HarvestWavmFileFunctionStores(outputDir);

                // Assert
                Assert.That(result.Count, Is.EqualTo(expectedPaths.Count));
                List<string> resultPaths = result.Select(x => x.Path).ToList();
                Assert.That(resultPaths, Is.EquivalentTo(expectedPaths));
            }
        }

        [Test]
        public void HarvestWavhFileFunctionStores_DirectoryInfoNull_ThrowsArgumentNullException()
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());

            // Call | Assert
            void Call() => harvester.HarvestWavhFileFunctionStores(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("outputDataDirectory"));
        }

        public static IEnumerable<TestCaseData> GetHarvestWavhFileFunctionStoresValidData()
        {
            IList<string> sp1Files =
                Enumerable.Range(0, 3)
                          .Select(i => $"spectra{i}.sp1")
                          .ToList();
            IList<string> sp2Files =
                Enumerable.Range(0, 3)
                          .Select(i => $"spectra{i}.sp2")
                          .ToList();

            IList<string> wavhFiles =
                Enumerable.Range(0, 3)
                          .Select(i => $"wavh-Waves{i}.nc")
                          .ToList();

            IList<string> wavmFiles =
                Enumerable.Range(0, 3)
                          .Select(i => $"wavm-Waves{i}.nc")
                          .ToList();
            

            IList<string> otherFiles =
                new [] {
                        "swan_bat.log",
                        "swn-diag.Waves",
                        "waves.mdw",}
                    .Concat(sp1Files)
                    .Concat(sp2Files)
                    .ToList();

            yield return new TestCaseData(new List<string>(), 
                                          new List<string>());
            yield return new TestCaseData(otherFiles, 
                                          new List<string>());
            yield return new TestCaseData(wavmFiles, 
                                          new List<string>());
            yield return new TestCaseData(otherFiles.Concat(wavmFiles).ToList(), 
                                          new List<string>());
            yield return new TestCaseData(new List<string>(),
                                          wavhFiles);
            yield return new TestCaseData(otherFiles,
                                          wavhFiles);
            yield return new TestCaseData(wavmFiles,
                                          wavhFiles);
            yield return new TestCaseData(otherFiles.Concat(wavmFiles).ToList(),
                                          wavhFiles);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetHarvestWavhFileFunctionStoresValidData))]
        public void HarvestWavhFileFunctionStores_ExpectedResults(IList<string> inputTextFiles, 
                                                                  IList<string> wavhFiles)
        {
            // Setup
            var harvester = new WaveOutputDataHarvester(Substitute.For<IWaveFeatureContainer>());
            using (var tempDir = new TemporaryDirectory())
            {
                foreach (string inputFileName in inputTextFiles)
                {
                    tempDir.CreateFile(inputFileName);
                }

                var expectedPaths = new List<string>();
                foreach (string wavhFileName in wavhFiles)
                {
                    string testPath = tempDir.CopyTestDataFileToTempDirectory("./WaveOutputDataHarvesterTest/wavh-Waves.nc");
                    string storePath = Path.Combine(Path.GetDirectoryName(testPath), wavhFileName);
                    File.Move(testPath, storePath);

                    expectedPaths.Add(storePath);
                }

                var outputDir = new DirectoryInfo(tempDir.Path);

                // Call
                IReadOnlyList<IWavhFileFunctionStore> result = 
                    harvester.HarvestWavhFileFunctionStores(outputDir);

                // Assert
                Assert.That(result.Count, Is.EqualTo(expectedPaths.Count));
                List<string> resultPaths = result.Select(x => x.Path).ToList();
                Assert.That(resultPaths, Is.EquivalentTo(expectedPaths));
            }
        }

        private static void BuildFiles(TemporaryDirectory tempDir, IEnumerable<ReadOnlyTextFileData> files)
        {
            foreach (ReadOnlyTextFileData readOnlyTextFileData in files)
            {
                tempDir.CreateFile(readOnlyTextFileData.DocumentName, readOnlyTextFileData.Content);
            }
        }

        private class ReadOnlyTextFileDataEqualityComparer : IEqualityComparer<ReadOnlyTextFileData>
        {
            public bool Equals(ReadOnlyTextFileData x, ReadOnlyTextFileData y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.DocumentName == y.DocumentName && x.Content == y.Content && x.Type == y.Type;
            }

            public int GetHashCode(ReadOnlyTextFileData obj)
            {
                unchecked
                {
                    return ((obj.DocumentName != null ? obj.DocumentName.GetHashCode() : 0) * 397) ^ (obj.Content != null ? obj.Content.GetHashCode() : 0);
                }
            }
        }
    }
}