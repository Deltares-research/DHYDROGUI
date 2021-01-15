using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.IO;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class CommonFileSystemActionsTest
    {
        [Test]
        public void ClearFolderWithFileExceptions_WhenThereAreNoExceptions_ShouldClearMainFolder()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                File.WriteAllText(Path.Combine(tempDirectory.Path, "test.txt"), "test");

                // Call
                CommonFileSystemActions.ClearFolderWithFileExceptions(tempDirectory.Path, 
                                                                      new HashSet<string>());

                // Assert
                var dirInfoTestFolder = new DirectoryInfo(tempDirectory.Path);
                Assert.That(dirInfoTestFolder.GetFiles(), Is.Empty);
            }
        }

        [Test]
        public void ClearFolderWithFileExceptions_WhenThereAreExceptionsInMainFolder_ShouldNotClearMainFolder()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDirectory.Path, "test.txt");
                File.WriteAllText(filePath, "test");

                string filePath2 = Path.Combine(tempDirectory.Path, "test2.txt");
                File.WriteAllText(filePath2, "test");

                // Call
                CommonFileSystemActions.ClearFolderWithFileExceptions(
                    tempDirectory.Path, 
                    new HashSet<string>{ filePath });

                // Assert
                var dirInfoTestFolder = new DirectoryInfo(tempDirectory.Path);
                FileInfo[] retrievedFilesAfterCleanup = dirInfoTestFolder.GetFiles();

                Assert.That(retrievedFilesAfterCleanup.Length, Is.EqualTo(1));
                Assert.That(retrievedFilesAfterCleanup.Single().FullName, 
                            Is.EqualTo(filePath));
            }
        }

        [Test]
        public void ClearFolderWithFileExceptions_WhenThereAreNoExceptionsInSubFolder_ShouldClearMainIncludingRemoveSubFolder()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string subTestFolderPath = Path.Combine(tempDirectory.Path, "test");
                Directory.CreateDirectory(subTestFolderPath);

                string filePath = Path.Combine(subTestFolderPath, "test.txt");
                File.WriteAllText(filePath, "test");
                string filePath2 = Path.Combine(subTestFolderPath, "test2.txt");
                File.WriteAllText(filePath2, "test");

                // Call
                CommonFileSystemActions.ClearFolderWithFileExceptions(tempDirectory.Path, new HashSet<string>());

                // Assert
                Assert.That(Directory.Exists(subTestFolderPath), Is.False);

                var dirInfoTestFolder = new DirectoryInfo(tempDirectory.Path);

                Assert.That(dirInfoTestFolder.EnumerateFiles(), Is.Empty);
                Assert.That(dirInfoTestFolder.EnumerateDirectories(), Is.Empty);
            }
        }

        [Test]
        public void ClearFolderWithFileExceptions_WhenThereAreExceptionsInSubFolder_ShouldNotClearMainAndSubFolder()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string subTestFolderPath = Path.Combine(tempDirectory.Path, "test");
                Directory.CreateDirectory(subTestFolderPath);

                string filePath = Path.Combine(subTestFolderPath, "test.txt");
                File.WriteAllText(filePath, "test");
                string filePath2 = Path.Combine(subTestFolderPath, "test2.txt");
                File.WriteAllText(filePath2, "test");

                // Call
                CommonFileSystemActions.ClearFolderWithFileExceptions(tempDirectory.Path, new HashSet<string> { filePath });

                // Assert
                var dirInfoSubTestFolder = new DirectoryInfo(subTestFolderPath);

                Assert.That(dirInfoSubTestFolder.Exists, Is.True);

                FileInfo[] retrievedFilesAfterCleanup = dirInfoSubTestFolder.GetFiles();

                Assert.That(retrievedFilesAfterCleanup.Length, Is.EqualTo(1));
                Assert.That(retrievedFilesAfterCleanup.Single().FullName, Is.EqualTo(filePath));
            }
        }
    }
}