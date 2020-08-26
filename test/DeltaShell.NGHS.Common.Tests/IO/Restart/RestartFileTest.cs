using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.IO.TestUtils;
using NUnit.Framework;
using Does = DeltaShell.NGHS.TestUtils.AssertConstraints.Does;

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
        public void Constructor_EmptyString_ThrowsArgumentException()
        {
            // Call
            void Call() => new RestartFile(string.Empty);

            // Assert
            Assert.Throws<ArgumentException>(Call);
        }

        [Test]
        public void Constructor_PathTooLong_ThrowsPathTooLongException()
        {
            // Setup
            var path = new StringBuilder().Append('p', 248).ToString();

            // Call
            void Call() => new RestartFile(path);

            // Assert
            Assert.Throws<PathTooLongException>(Call);
        }

        [Test]
        [TestCaseSource(nameof(GetPathTestCases))]
        public void SetPath_SetsPathCorrectly(string value, string expPath, string expName, bool expIsEmpty)
        {
            // Setup
            var restartFile = new RestartFile();

            // Call
            restartFile.Path = value;

            // Assert
            Assert.That(restartFile.Path, Is.EqualTo(expPath));
            Assert.That(restartFile.Name, Is.EqualTo(expName));
            Assert.That(restartFile.IsEmpty, Is.EqualTo(expIsEmpty));
        }

        [TestCaseSource(nameof(InvalidChars))]
        public void SetPath_ContainsInvalidChars_ThrowsArgumentException(char invalidCharacter)
        {
            // Setup
            string path = $"c:/{invalidCharacter}folder_path";

            // Call
            void Call() => new RestartFile().Path = path;

            // Assert
            Assert.Throws<ArgumentException>(Call);
        }

        [Test]
        public void SetPath_WithEmptyString_ThrowsArgumentException()
        {
            // Call
            void Call() => new RestartFile().Path = string.Empty;

            // Assert
            Assert.Throws<ArgumentException>(Call);
        }

        [Test]
        public void SetPath_PathTooLong_ThrowsPathTooLongException()
        {
            // Setup
            var path = new StringBuilder().Append('p', 248).ToString();

            // Call
            void Call() => new RestartFile().Path = path;

            // Assert
            Assert.Throws<PathTooLongException>(Call);
        }

        [Test]
        [TestCaseSource(nameof(GetPathTestCases))]
        public void SwitchTo_SetsPathCorrectly(string value, string expPath, string expName, bool expIsEmpty)
        {
            // Setup
            var restartFile = new RestartFile();

            // Call
            restartFile.SwitchTo(value);

            // Assert
            Assert.That(restartFile.Path, Is.EqualTo(expPath));
            Assert.That(restartFile.Name, Is.EqualTo(expName));
            Assert.That(restartFile.IsEmpty, Is.EqualTo(expIsEmpty));
        }

        [TestCaseSource(nameof(InvalidChars))]
        public void SwitchTo_ContainsInvalidChars_ThrowsArgumentException(char invalidCharacter)
        {
            // Setup
            string path = $"c:/{invalidCharacter}folder_path";

            // Call
            void Call() => new RestartFile().SwitchTo(path);

            // Assert
            Assert.Throws<ArgumentException>(Call);
        }

        [Test]
        public void SwitchTo_WithEmptyString_ThrowsArgumentException()
        {
            // Call
            void Call() => new RestartFile().SwitchTo(string.Empty);

            // Assert
            Assert.Throws<ArgumentException>(Call);
        }

        [Test]
        public void SwitchTo_PathTooLong_ThrowsPathTooLongException()
        {
            // Setup
            var path = new StringBuilder().Append('p', 248).ToString();

            // Call
            void Call() => new RestartFile().SwitchTo(path);

            // Assert
            Assert.Throws<PathTooLongException>(Call);
        }

        [TestCase(true, null)]
        [TestCase(true, "")]
        [TestCase(false, null)]
        [TestCase(false, "")]
        public void CopyInto_DirectoryPathNullOrEmpty_Returns(bool switchTo, string targetDir)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = Path.Combine(tempDir.Path, "the.file");
                File.WriteAllText(origFile, "");

                var restartFile = new RestartFile(origFile);

                // Call
                restartFile.CopyInto(targetDir, switchTo);

                // Assert
                Assert.That(origFile, Does.Exist);
                Assert.That(restartFile.Path, Is.EqualTo(origFile));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CopyInto_FileDoesNotExist_Returns(bool switchTo)
        {
            // Setup
            var restartFile = new RestartFile("path/to/the.file");

            // Call
            restartFile.CopyInto("some/folder", switchTo);

            // Assert
            Assert.That(restartFile.Path, Is.EqualTo("path/to/the.file"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CopyInto_TargetDirDoesNotExist_CreatesTargetDir(bool switchTo)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = Path.Combine(tempDir.Path, "the.file");
                File.WriteAllText(origFile, "");

                var restartFile = new RestartFile(origFile);

                string targetDir = Path.Combine(tempDir.Path, "target_dir");

                // Call
                restartFile.CopyInto(targetDir, switchTo);

                // Assert
                string targetFile = Path.Combine(targetDir, restartFile.Name);
                Assert.That(targetFile, Does.Exist);
                Assert.That(restartFile.Path, switchTo ? Is.EqualTo(targetFile) : Is.EqualTo(origFile));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CopyInto_CopiesFileIntoDirectory(bool switchTo)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = Path.Combine(tempDir.Path, "the.file");
                File.WriteAllText(origFile, "");

                var restartFile = new RestartFile(origFile);

                string targetDir = tempDir.CreateDirectory("target_dir");

                // Call
                restartFile.CopyInto(targetDir, switchTo);

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

        private IEnumerable<TestCaseData> GetPathTestCases()
        {
            yield return new TestCaseData(null, null, string.Empty, true);
            yield return new TestCaseData("path/to/the.file", "path/to/the.file", "the.file", false);
            yield return new TestCaseData("path\\to\\the.file", "path\\to\\the.file", "the.file", false);
            yield return new TestCaseData("the.file", "the.file", "the.file", false);
        }

        private static IEnumerable<char> InvalidChars() => Path.GetInvalidPathChars();
    }
}