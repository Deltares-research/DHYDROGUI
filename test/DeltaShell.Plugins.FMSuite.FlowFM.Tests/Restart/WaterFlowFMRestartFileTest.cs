using System;
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
        public void CopyToDirectory_DirectoryPathNullOrEmpty_Returns(bool switchTo, string targetDir)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = tempDir.CreateFile("the.file");
                var restartFile = new WaterFlowFMRestartFile(origFile);

                // Call
                restartFile.CopyToDirectory(targetDir, switchTo);

                // Assert
                Assert.That(origFile, Does.Exist);
                Assert.That(restartFile.Path, Is.EqualTo(origFile));
            }
        }

        [TestCase("path/to/the.file", true)]
        [TestCase("path/to/the.file", false)]
        [TestCase(null, true)]
        [TestCase(null, false)]
        public void CopyToDirectory_FileDoesNotExist_Returns(string path, bool switchTo)
        {
            // Setup
            var restartFile = new WaterFlowFMRestartFile(path);

            // Call
            restartFile.CopyToDirectory("some/folder", switchTo);

            // Assert
            Assert.That(restartFile.Path, Is.EqualTo(path));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CopyToDirectory_TargetDirDoesNotExist_CreatesTargetDir(bool switchTo)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = tempDir.CreateFile("the.file");

                var restartFile = new WaterFlowFMRestartFile(origFile);

                string targetDir = Path.Combine(tempDir.Path, "target_dir");

                // Call
                restartFile.CopyToDirectory(targetDir, switchTo);

                // Assert
                string targetFile = Path.Combine(targetDir, restartFile.Name);
                Assert.That(targetFile, Does.Exist);
                Assert.That(restartFile.Path, switchTo ? Is.EqualTo(targetFile) : Is.EqualTo(origFile));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CopyToDirectory_CopiesFileIntoDirectory(bool switchTo)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = tempDir.CreateFile("the.file");

                var restartFile = new WaterFlowFMRestartFile(origFile);

                string targetDir = tempDir.CreateDirectory("target_dir");

                // Call
                restartFile.CopyToDirectory(targetDir, switchTo);

                // Assert
                string targetFile = Path.Combine(targetDir, restartFile.Name);
                Assert.That(targetFile, Does.Exist);
                Assert.That(restartFile.Path, switchTo ? Is.EqualTo(targetFile) : Is.EqualTo(origFile));
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
                var source = new WaterFlowFMRestartFile(path);

                // Call
                var copy = new WaterFlowFMRestartFile(source);

                // Assert
                Assert.That(copy.Name, Is.EqualTo(source.Name));
                Assert.That(copy.IsEmpty, Is.EqualTo(source.IsEmpty));
                Assert.That(copy.Exists, Is.EqualTo(source.Exists));
                Assert.That(copy.Path, Is.EqualTo(source.Path));
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