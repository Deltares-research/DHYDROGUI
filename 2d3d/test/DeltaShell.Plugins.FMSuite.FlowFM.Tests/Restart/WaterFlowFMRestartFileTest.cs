﻿using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Restart
{
    [TestFixture]
    public class WaterFlowFMRestartFileTest
    {
        [Test]
        public void Constructor_Default_InitializesInstanceCorrectly()
        {
            // Call
            var restartFile = new WaterFlowFMRestartFile();

            // Assert
            Assert.That(restartFile.Path, Is.EqualTo(null));
            Assert.That(restartFile.Name, Is.EqualTo(string.Empty));
            Assert.That(restartFile.IsEmpty, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(GetPathTestCases))]
        public void Constructor_InitializesInstanceCorrectly(string argPath, string expPath, string expName, bool expIsEmpty)
        {
            // Call
            var restartFile = new WaterFlowFMRestartFile(argPath);

            // Assert
            Assert.That(restartFile.Path, Is.EqualTo(expPath));
            Assert.That(restartFile.Name, Is.EqualTo(expName));
            Assert.That(restartFile.IsEmpty, Is.EqualTo(expIsEmpty));
        }

        [TestCaseSource(nameof(InvalidChars))]
        public void Constructor_InvalidChars_ThrowsArgumentException(char invalidCharacter)
        {
            // Setup
            string path = $"c:/{invalidCharacter}folder_path";

            // Call
            void Call() => new WaterFlowFMRestartFile(path);

            // Assert
            Assert.Throws<ArgumentException>(Call);
        }

        [Test]
        public void Constructor_ContainsColon_ThrowsNotSupportedException()
        {
            // Setup
            string path = "c:/:folder_path";

            // Call
            void Call() => new WaterFlowFMRestartFile(path);

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void Constructor_EmptyString_ThrowsArgumentException()
        {
            // Call
            void Call() => new WaterFlowFMRestartFile(string.Empty);

            // Assert
            Assert.Throws<ArgumentException>(Call);
        }

        [TestCase(true, null)]
        [TestCase(true, "")]
        [TestCase(false, null)]
        [TestCase(false, "")]
        public void CopyTo_DestinationPathNullOrEmpty_Returns(bool switchTo, string destinationPath)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = tempDir.CreateFile("the.file");
                var restartFile = new WaterFlowFMRestartFile(origFile);

                // Call
                restartFile.CopyTo(destinationPath, switchTo);

                // Assert
                Assert.That(origFile, Does.Exist);
                Assert.That(restartFile.Path, Is.EqualTo(origFile));
            }
        }

        [TestCase("path/to/the.file", true)]
        [TestCase("path/to/the.file", false)]
        [TestCase(null, true)]
        [TestCase(null, false)]
        public void CopyTo_FileDoesNotExist_Returns(string path, bool switchTo)
        {
            // Setup
            var restartFile = new WaterFlowFMRestartFile(path);

            // Call
            restartFile.CopyTo("some.file", switchTo);

            // Assert
            Assert.That(restartFile.Path, Is.EqualTo(path));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CopyTo_TargetDirDoesNotExist_CreatesTargetDir(bool switchTo)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = tempDir.CreateFile("the.file");

                var restartFile = new WaterFlowFMRestartFile(origFile);

                string destinationPath = Path.Combine(tempDir.Path, "target_dir", restartFile.Name);

                // Call
                restartFile.CopyTo(destinationPath, switchTo);

                // Assert
                Assert.That(destinationPath, Does.Exist);
                Assert.That(restartFile.Path, switchTo ? Is.EqualTo(destinationPath) : Is.EqualTo(origFile));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CopyTo_CopiesFileToDestinationPath(bool switchTo)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = tempDir.CreateFile("the.file");

                var restartFile = new WaterFlowFMRestartFile(origFile);

                string targetDir = tempDir.CreateDirectory("target_dir");
                string destinationPath = Path.Combine(targetDir, restartFile.Name);

                // Call
                restartFile.CopyTo(destinationPath, switchTo);

                // Assert
                Assert.That(destinationPath, Does.Exist);
                Assert.That(restartFile.Path, switchTo ? Is.EqualTo(destinationPath) : Is.EqualTo(origFile));
            }
        }

        [Test]
        public void Exists_ShouldReturnFalseIfFileDoesNotExistCurrently()
        {
            // Setup
            var restartFile = new WaterFlowFMRestartFile("NotExistingRestartFile_rst.nc");

            // Call
            bool fileExists = restartFile.Exists;

            // Assert
            Assert.IsFalse(fileExists);
        }

        [Test]
        public void Exists_ShouldReturnTrueIfFileExistsCurrently()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string restartFilePath = Path.Combine(tempDirectory.Path, "test_rst.nc");
                File.WriteAllText(restartFilePath, "test");

                var restartFile = new WaterFlowFMRestartFile(restartFilePath);

                // Call
                bool fileExists = restartFile.Exists;

                // Assert
                Assert.IsTrue(fileExists);
            }
        }

        [Test]
        public void Exists_ShouldReturnFalseIfFileHasBeenDeletedAfterInitializingRestartFile()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string restartFilePath = Path.Combine(tempDirectory.Path, "test_rst.nc");
                File.WriteAllText(restartFilePath, "test");

                var restartFile = new WaterFlowFMRestartFile(restartFilePath);

                // Call
                File.Delete(restartFilePath);
                bool fileExists = restartFile.Exists;

                // Assert
                Assert.IsFalse(fileExists);
            }
        }

        [Test]
        public void Exists_ShouldReturnTrueIfFileHasBeenAddedAfterInitializingRestartFile()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string restartFilePath = Path.Combine(tempDirectory.Path, "test_rst.nc");

                var restartFile = new WaterFlowFMRestartFile(restartFilePath);

                // Call
                File.WriteAllText(restartFilePath, "test");
                bool fileExists = restartFile.Exists;

                // Assert
                Assert.IsTrue(fileExists);
            }
        }
        
        [TestCaseSource(nameof(GetIsMapFileTestCases))]
        public void IsMapFile_ReturnsExpectedResults(string filepath, bool expectedResult)
        {
            // Setup
            var restartFile = new WaterFlowFMRestartFile(filepath);

            // Call
            bool isMapFile = restartFile.IsMapFile;

            // Assert
            Assert.That(isMapFile, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> GetIsMapFileTestCases()
        {
            yield return new TestCaseData("_map.nc", true);
            yield return new TestCaseData("map.nc", false);
            yield return new TestCaseData(".nc", false);
            yield return new TestCaseData("_rst.nc", false);
            yield return new TestCaseData(null, false);
        }

        [Test]
        public void CopyConstructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WaterFlowFMRestartFile((WaterFlowFMRestartFile)null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CopyConstructor_FromInstance_CreatesCopy()
        {
            using (var temp = new TemporaryDirectory())
            {
                string path = Path.Combine(temp.Path, "some_file_rst.nc");

                var startTime = new DateTime(1990, 07, 18, 12, 34, 56);
                var source = new WaterFlowFMRestartFile(path) { StartTime = startTime };

                // Call
                var copy = new WaterFlowFMRestartFile(source);

                // Assert
                Assert.That(copy.Name, Is.EqualTo(source.Name));
                Assert.That(copy.IsEmpty, Is.EqualTo(source.IsEmpty));
                Assert.That(copy.Exists, Is.EqualTo(source.Exists));
                Assert.That(copy.Path, Is.EqualTo(source.Path));
                Assert.That(copy.StartTime, Is.EqualTo(source.StartTime));
            }
        }
        
        private static IEnumerable<TestCaseData> GetPathTestCases()
        {
            yield return new TestCaseData(null, null, string.Empty, true);
            yield return new TestCaseData("path/to/the.file", "path/to/the.file", "the.file", false);
            yield return new TestCaseData("path\\to\\the.file", "path\\to\\the.file", "the.file", false);
            yield return new TestCaseData("the.file", "the.file", "the.file", false);
        }

        private static IEnumerable<char> InvalidChars() => Path.GetInvalidPathChars();
    }
}