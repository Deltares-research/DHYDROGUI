using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WaveOutputDataCopyHandlerTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var copyHandler = new WaveOutputDataCopyHandler();

            // Assert
            Assert.That(copyHandler, Is.InstanceOf<IWaveOutputDataCopyHandler>());
        }

        public static IEnumerable<TestCaseData> GetCopyNullParameterData()
        {
            var dirInfo = new DirectoryInfo(".");
            yield return new TestCaseData(null, dirInfo, "sourceDirectoryInfo");
            yield return new TestCaseData(dirInfo, null, "targetDirectoryInfo");
        }

        [Test]
        [TestCaseSource(nameof(GetCopyNullParameterData))]
        public void CopyRunDataTo_DirectoryParameterNull_ThrowsArgumentNull(DirectoryInfo sourceDirectoryInfo,
                                                                            DirectoryInfo targetDirectoryInfo,
                                                                            string expectedParameterName)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var copyHandler = new WaveOutputDataCopyHandler();

            // Call | Assert
            void Call() => copyHandler.CopyRunDataTo(sourceDirectoryInfo, 
                                                     targetDirectoryInfo, 
                                                     logHandler);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        [TestCaseSource(nameof(GetCopyNullParameterData))]
        public void CopyOutputDataTo_DirectoryParameterNull_ThrowsArgumentNull(DirectoryInfo sourceDirectoryInfo,
                                                                               DirectoryInfo targetDirectoryInfo,
                                                                               string expectedParameterName)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var copyHandler = new WaveOutputDataCopyHandler();

            // Call | Assert
            void Call() => copyHandler.CopyOutputDataTo(sourceDirectoryInfo, 
                                                        targetDirectoryInfo, 
                                                        logHandler);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CopyRunData_CopiesAllOutputData()
        {
            // Setup
            const string relativeTestDataPath = "WaveModelTest\\alternative_output";
            string sourceTestDataPath = TestHelper.GetTestFilePath(relativeTestDataPath);

            const string relativeReferenceDataPath = "WaveModelTest\\alternative_output_reference";
            string referenceDataPath = TestHelper.GetTestFilePath(relativeReferenceDataPath);

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDataPath = tempDir.CopyDirectoryToTempDirectory(sourceTestDataPath);
                string targetDataPath = tempDir.CreateDirectory("goalPath");

                string referencePath = tempDir.CopyDirectoryToTempDirectory(referenceDataPath);

                var sourceInfo = new DirectoryInfo(sourceDataPath);
                var targetInfo = new DirectoryInfo(targetDataPath);
                var referenceInfo = new DirectoryInfo(referencePath);

                IReadOnlyList<FileCompareInfo> referenceFiles = 
                    CollectFileInformation(referenceInfo).ToList();

                var logHandler = Substitute.For<ILogHandler>();
                var copyHandler = new WaveOutputDataCopyHandler();

                // Call
                copyHandler.CopyRunDataTo(sourceInfo, targetInfo, logHandler);

                // Assert
                IReadOnlyList<FileCompareInfo> targetFiles = 
                    CollectFileInformation(targetInfo).ToList();
                AssertContainsSameFiles(referenceFiles, 
                                        targetFiles);

                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CopyRunData_SourceDoesNotExist_DoesNotCopyAndLogsWarning()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                var sourceInfo = new DirectoryInfo("should/not/exist");
                var targetInfo = new DirectoryInfo(tempDir.Path);

                var logHandler = Substitute.For<ILogHandler>();
                var copyHandler = new WaveOutputDataCopyHandler();

                // Call
                copyHandler.CopyRunDataTo(sourceInfo, targetInfo, logHandler);

                // Assert
                IReadOnlyList<FileCompareInfo> targetFiles = 
                    CollectFileInformation(targetInfo).ToList();

                Assert.That(targetFiles, Is.Empty);
                
                Assert.That(logHandler.ReceivedCalls(), Has.Exactly(1).Items);
                logHandler.Received(1)
                          .ReportWarningFormat("The output source path {0} does not exist, skipping copying output data.", 
                                               sourceInfo.FullName);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CopyRunData_SourceDoesNotContainMdw_DoesNotCopyAndLogsWarning()
        {
            // Setup
            const string relativeTestDataPath = @"WaveModelTest\Waves\output";
            string sourceTestDataPath = TestHelper.GetTestFilePath(relativeTestDataPath);

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDataPath = tempDir.CopyDirectoryToTempDirectory(sourceTestDataPath);
                string targetDataPath = tempDir.CreateDirectory("goalPath");

                var sourceInfo = new DirectoryInfo(sourceDataPath);
                var targetInfo = new DirectoryInfo(targetDataPath);

                var logHandler = Substitute.For<ILogHandler>();
                var copyHandler = new WaveOutputDataCopyHandler();

                // Call
                copyHandler.CopyRunDataTo(sourceInfo, targetInfo, logHandler);

                // Assert
                IReadOnlyList<FileCompareInfo> targetFiles = 
                    CollectFileInformation(targetInfo).ToList();
                Assert.That(targetFiles, Is.Empty);
                
                Assert.That(logHandler.ReceivedCalls(), Has.Exactly(1).Items);
                logHandler.Received(1)
                          .ReportWarningFormat("No .mdw path could be found in {0}, skipping copying output data.", 
                                               sourceInfo.FullName);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CopyOutputData_CopiesContentCorrectly()
        {
            // Setup
            const string relativeTestDataPath = @"WaveModelTest\Waves\output";
            string sourceTestDataPath = TestHelper.GetTestFilePath(relativeTestDataPath);

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDataPath = tempDir.CopyDirectoryToTempDirectory(sourceTestDataPath);
                string targetDataPath = tempDir.CreateDirectory("goalPath");

                var sourceInfo = new DirectoryInfo(sourceDataPath);
                var targetInfo = new DirectoryInfo(targetDataPath);

                IReadOnlyList<FileCompareInfo> sourceFiles = 
                    CollectFileInformation(sourceInfo).ToList();

                var logHandler = Substitute.For<ILogHandler>();
                var copyHandler = new WaveOutputDataCopyHandler();

                // Call
                copyHandler.CopyOutputDataTo(sourceInfo, targetInfo, logHandler);

                // Assert
                IReadOnlyList<FileCompareInfo> targetFiles = 
                    CollectFileInformation(targetInfo).ToList();
                AssertContainsSameFiles(sourceFiles, 
                                        targetFiles);

                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CopyOutputDataTo_SourceDoesNotExist_DoesNotCopyAndLogsWarning()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                var sourceInfo = new DirectoryInfo("should/not/exist");
                var targetInfo = new DirectoryInfo(tempDir.Path);

                var logHandler = Substitute.For<ILogHandler>();
                var copyHandler = new WaveOutputDataCopyHandler();

                // Call
                copyHandler.CopyOutputDataTo(sourceInfo, targetInfo, logHandler);

                // Assert
                IReadOnlyList<FileCompareInfo> targetFiles = 
                    CollectFileInformation(targetInfo).ToList();

                Assert.That(targetFiles, Is.Empty);
                logHandler.Received(1)
                          .ReportWarningFormat("The output source path {0} does not exist, skipping copying output data.", 
                                               sourceInfo.FullName);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CopyOutputDataTo_SourceEqualsTarget_DoesNotChangeContent()
        {
            // Setup
            const string relativeTestDataPath = @"WaveModelTest\Waves\output";
            string sourceTestDataPath = TestHelper.GetTestFilePath(relativeTestDataPath);

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDataPath = tempDir.CopyDirectoryToTempDirectory(sourceTestDataPath);

                var sourceInfo = new DirectoryInfo(sourceDataPath);
                var targetInfo = new DirectoryInfo(sourceDataPath);

                IReadOnlyList<FileCompareInfo> sourceFiles = 
                    CollectFileInformation(sourceInfo).ToList();

                var logHandler = Substitute.For<ILogHandler>();
                var copyHandler = new WaveOutputDataCopyHandler();

                // Call
                copyHandler.CopyOutputDataTo(sourceInfo, targetInfo, logHandler);

                // Assert
                IReadOnlyList<FileCompareInfo> targetFiles = 
                    CollectFileInformation(targetInfo).ToList();
                
                AssertContainsSameFiles(sourceFiles, 
                                        targetFiles);

                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        private static void AssertContainsSameFiles(IReadOnlyList<FileCompareInfo> originalFileData,
                                                    IReadOnlyList<FileCompareInfo> savedFileData)
        {
            Assert.That(savedFileData.Count, Is.EqualTo(originalFileData.Count));

            for (var i = 0; i < savedFileData.Count; i++)
            {
                Assert.That(savedFileData[i].Name, Is.EqualTo(originalFileData[i].Name));
                Assert.That(savedFileData[i].Hash, Is.EqualTo(originalFileData[i].Hash));
            }
        }

        private class FileCompareInfo
        {
            public FileCompareInfo(string name, string hash)
            {
                Name = name;
                Hash = hash;
            }

            public string Name { get; }
            public string Hash { get; }
        }

        private static IEnumerable<FileCompareInfo> CollectFileInformation(DirectoryInfo directoryInfo) =>
            directoryInfo.EnumerateFiles().Select(fi => new FileCompareInfo(fi.Name, FileUtils.GetChecksum(fi.FullName)));
    }
}