using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.IO.Restart
{
    [TestFixture]
    public class RestartFileTest
    {
        [Test]
        public void Constructor_Default_InitializesInstanceCorrectly()
        {
            // Call
            var restartFile = new RestartFile();

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
            var restartFile = new RestartFile(argPath);

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
            void Call() => new RestartFile(path);

            // Assert
            Assert.Throws<ArgumentException>(Call);
        }

        [Test]
        public void Constructor_ContainsColon_ThrowsNotSupportedException()
        {
            // Setup
            string path = "c:/:folder_path";

            // Call
            void Call() => new RestartFile(path);

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void Constructor_EmptyString_ThrowsArgumentException()
        {
            // Call
            void Call() => new RestartFile(string.Empty);

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
                var restartFile = new RestartFile(origFile);

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
            var restartFile = new RestartFile(path);

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

                var restartFile = new RestartFile(origFile);

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

                var restartFile = new RestartFile(origFile);

                string targetDir = tempDir.CreateDirectory("target_dir");

                // Call
                restartFile.CopyToDirectory(targetDir, switchTo);

                // Assert
                string targetFile = Path.Combine(targetDir, restartFile.Name);
                Assert.That(targetFile, Does.Exist);
                Assert.That(restartFile.Path, switchTo ? Is.EqualTo(targetFile) : Is.EqualTo(origFile));
            }
        }

        [TestCase(null)]
        [TestCase("path/to/the.file")]
        public void Clone_ReturnsCorrectClone(string path)
        {
            // Setup
            var restartFile = new RestartFile(path);

            // Call
            RestartFile clone = restartFile.Clone();

            // Assert
            Assert.That(clone, Is.Not.SameAs(restartFile));
            Assert.That(clone.Path, Is.EqualTo(path));
        }

        [Test]
        public void Exists_ShouldReturnFalseIfFileDoesNotExistCurrently()
        {
            // Setup
            var restartFile = new RestartFile("NotExistingRestartFile_rst.nc");

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

                var restartFile = new RestartFile(restartFilePath);

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
                
                var restartFile = new RestartFile(restartFilePath);
                
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
                
                var restartFile = new RestartFile(restartFilePath);

                // Call
                File.WriteAllText(restartFilePath, "test");
                bool fileExists = restartFile.Exists;

                // Assert
                Assert.IsTrue(fileExists);
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