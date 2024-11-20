using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Utils.Extensions;
using NUnit.Framework;

namespace DeltaShell.NGHS.Utils.Test.Extensions
{
    [TestFixture]
    public class FileInfoExtensionsTest
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CopyToDirectory_CopiesTheFileToTheDirectory(bool overwrite)
        {
            const string file = "test_file.txt";

            using (var source = new TemporaryDirectory())
            using (var target = new TemporaryDirectory())
            {
                // Setup
                var sourceFile = new FileInfo(source.CreateFile(file));
                var targetDirectory = new DirectoryInfo(target.Path);

                // Call
                FileInfo targetFile = sourceFile.CopyToDirectory(targetDirectory, overwrite);

                // Assert
                Assert.That(targetFile, Does.Exist);
                Assert.That(targetFile.FullName, Is.EqualTo(Path.Combine(target.Path, file)));
            }
        }

        [Test]
        public void CopyToDirectory_SameTargetDirectory_ReturnsOriginalFileInfo()
        {
            const string file = "test_file.txt";

            using (var source = new TemporaryDirectory())
            {
                // Setup
                var sourceFile = new FileInfo(source.CreateFile(file));
                var targetDirectory = new DirectoryInfo(source.Path);

                // Call
                FileInfo targetFile = sourceFile.CopyToDirectory(targetDirectory, false);

                // Assert
                Assert.That(targetFile, Is.SameAs(sourceFile));
            }
        }

        [Test]
        public void CopyToDirectory_OverwriteFalse_FileExists_ThrowsIOException()
        {
            const string file = "test_file.txt";

            using (var source = new TemporaryDirectory())
            using (var target = new TemporaryDirectory())
            {
                // Setup
                var sourceFile = new FileInfo(source.CreateFile(file));
                var targetDirectory = new DirectoryInfo(target.Path);

                // Create existing file
                target.CreateFile(file);

                // Call
                void Call() => sourceFile.CopyToDirectory(targetDirectory, false);

                // Assert
                Assert.Throws<IOException>(Call);
            }
        }

        [Test]
        public void CopyToDirectory_OverwriteTrue_FileExists_OverwritesFile()
        {
            const string file = "test_file.txt";
            const string sourceContent = "source_content";
            const string targetContent = "target_content";

            using (var source = new TemporaryDirectory())
            using (var target = new TemporaryDirectory())
            {
                // Setup
                var sourceFile = new FileInfo(source.CreateFile(file, sourceContent));
                var targetDirectory = new DirectoryInfo(target.Path);

                // Create existing file
                string targetFilePath = target.CreateFile(file, targetContent);

                // Call
                FileInfo targetFile = sourceFile.CopyToDirectory(targetDirectory, true);

                // Assert
                Assert.That(targetFile, Does.Exist);
                Assert.That(targetFile.FullName, Is.EqualTo(targetFilePath));
                Assert.That(File.ReadAllText(targetFilePath), Is.EqualTo(sourceContent));
            }
        }

        [Test]
        public void CopyToDirectory_DirectoryDoesNotExist_DirectoryIsCreatedAndFileIsCopied()
        {
            const string file = "test_file.txt";
            const string targetDirName = "target";

            using (var source = new TemporaryDirectory())
            {
                // Setup
                var sourceFile = new FileInfo(source.CreateFile(file));
                string targetDirectoryPath = Path.Combine(source.Path, targetDirName);
                var targetDirectory = new DirectoryInfo(targetDirectoryPath);

                // Call
                FileInfo targetFile = sourceFile.CopyToDirectory(targetDirectory, true);

                // Assert
                Assert.That(targetDirectory, Does.Exist);
                Assert.That(targetFile, Does.Exist);
                Assert.That(targetFile.FullName, Is.EqualTo(Path.Combine(targetDirectoryPath, file)));
            }
        }
    }
}