using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WaveOutputDataHarvesterTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var harvester = new WaveOutputDataHarvester();

            // Assert
            Assert.That(harvester, Is.InstanceOf<IWaveOutputDataHarvester>());
        }

        [Test]
        public void HarvestDiagnosticFiles_DirectoryInfoNull_ThrowsArgumentNullException()
        {
            // Setup
            var harvester = new WaveOutputDataHarvester();

            // Call | Assert
            void Call() => harvester.HarvestDiagnosticFiles(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("outputDataDirectory"));
        }

        public static IEnumerable<TestCaseData> GetHarvestDiagnosticFilesValidData()
        {
            const string logName = "swan_bat.log";
            const string logContent = "Some log data.";
            var logFile = new ReadOnlyTextFileData(logName, logContent);

            const string diagName = "swn-diag.Waves";
            const string diagContent = "Some diagnostic data.";
            var diagFile = new ReadOnlyTextFileData(diagName, diagContent);

            const string otherFileName = "waves.mdw";
            const string otherFileContent = "swoosh swash";
            var otherFile = new ReadOnlyTextFileData(otherFileName, otherFileContent);

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
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetHarvestDiagnosticFilesValidData))]
        public void HarvestDiagnosticFiles_ExpectedResults(IList<ReadOnlyTextFileData> inputFiles, 
                                                           IList<ReadOnlyTextFileData> expectedDiagnosticFiles)
        {
            // Setup
            var harvester = new WaveOutputDataHarvester();
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
            var harvester = new WaveOutputDataHarvester();

            // Call | Assert
            void Call() => harvester.HarvestSpectraFiles(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("outputDataDirectory"));
        }

        public static IEnumerable<TestCaseData> GetHarvestSpectraFilesValidData()
        {
            List<ReadOnlyTextFileData> sp1Files =
                Enumerable.Range(0, 3)
                          .Select(i => new ReadOnlyTextFileData($"spectra{i}.sp1", $"some content {i}"))
                          .ToList();
            List<ReadOnlyTextFileData> sp2Files =
                Enumerable.Range(0, 3)
                          .Select(i => new ReadOnlyTextFileData($"spectra{i}.sp2", $"some content {i}"))
                          .ToList();

            ReadOnlyTextFileData[] otherFiles = new[]
            {
                new ReadOnlyTextFileData("swan_bat.log", "Some log data."),
                new ReadOnlyTextFileData("swn-diag.Waves", "Some diagnostic data"),
                new ReadOnlyTextFileData("waves.mdw", "Swoosh swash"),
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
            var harvester = new WaveOutputDataHarvester();
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

                return x.DocumentName == y.DocumentName && x.Content == y.Content;
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