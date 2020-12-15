using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.IO;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.IO
{
    [TestFixture]
    public class CommonFileAndDirectoryActionsTest
    {
        [Test]
        public void ClearFolderWithFileExceptions_WhenThereAreNoExceptions_ShouldClearMainFolder()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string testFolder = Path.Combine(tempDirectory.Path, "test");
                Directory.CreateDirectory(testFolder);
                File.WriteAllText(Path.Combine(testFolder, "test.txt"), "test");
                CommonFileAndDirectoryActions.ClearFolderWithFileExceptions(testFolder, new List<string>());

                var dirInfoTestFolder = new DirectoryInfo(testFolder);
                Assert.AreEqual(dirInfoTestFolder.GetFiles().Length, 0);
            }
        }

        [Test]
        public void ClearFolderWithFileExceptions_WhenThereAreExceptionsInMainFolder_ShouldNotClearMainFolder()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string testFolder = Path.Combine(tempDirectory.Path, "test");
                Directory.CreateDirectory(testFolder);
                string filePath = Path.Combine(testFolder, "test.txt");
                File.WriteAllText(filePath, "test");
                string filePath2 = Path.Combine(testFolder, "test2.txt");
                File.WriteAllText(filePath2, "test");
                CommonFileAndDirectoryActions.ClearFolderWithFileExceptions(testFolder, new List<string>{ filePath });

                var dirInfoTestFolder = new DirectoryInfo(testFolder);
                FileInfo[] retrievedFilesAfterCleanup = dirInfoTestFolder.GetFiles();
                Assert.AreEqual(retrievedFilesAfterCleanup.Length, 1);
                Assert.AreEqual(filePath, retrievedFilesAfterCleanup.First().FullName);
            }
        }

        [Test]
        public void ClearFolderWithFileExceptions_WhenThereAreNoExceptionsInSubFolder_ShouldClearMainIncludingRemoveSubFolder()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string testFolderPath = Path.Combine(tempDirectory.Path, "test");
                Directory.CreateDirectory(testFolderPath);
                string subTestFolderPath = Path.Combine(testFolderPath, "test");
                Directory.CreateDirectory(subTestFolderPath);

                string filePath = Path.Combine(subTestFolderPath, "test.txt");
                File.WriteAllText(filePath, "test");
                string filePath2 = Path.Combine(subTestFolderPath, "test2.txt");
                File.WriteAllText(filePath2, "test");
                CommonFileAndDirectoryActions.ClearFolderWithFileExceptions(testFolderPath, new List<string>());

                Assert.IsFalse(Directory.Exists(subTestFolderPath));
                var dirInfoSubTestFolder = new DirectoryInfo(testFolderPath);
                FileInfo[] retrievedFilesAfterCleanup = dirInfoSubTestFolder.GetFiles();
                DirectoryInfo[] retrievedDirectoriesAfterCleanup = dirInfoSubTestFolder.GetDirectories();
                Assert.AreEqual(retrievedFilesAfterCleanup.Length, 0);
                Assert.AreEqual(retrievedDirectoriesAfterCleanup.Length, 0);
            }
        }

        [Test]
        public void ClearFolderWithFileExceptions_WhenThereAreExceptionsInSubFolder_ShouldNotClearMainAndSubFolder()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string testFolderPath = Path.Combine(tempDirectory.Path, "test");
                Directory.CreateDirectory(testFolderPath);
                string subTestFolderPath = Path.Combine(testFolderPath, "test");
                Directory.CreateDirectory(subTestFolderPath);

                string filePath = Path.Combine(subTestFolderPath, "test.txt");
                File.WriteAllText(filePath, "test");
                string filePath2 = Path.Combine(subTestFolderPath, "test2.txt");
                File.WriteAllText(filePath2, "test");
                CommonFileAndDirectoryActions.ClearFolderWithFileExceptions(testFolderPath, new List<string> { filePath });

                Assert.IsTrue(Directory.Exists(subTestFolderPath));
                var dirInfoSubTestFolder = new DirectoryInfo(subTestFolderPath);
                FileInfo[] retrievedFilesAfterCleanup = dirInfoSubTestFolder.GetFiles();
                Assert.AreEqual(retrievedFilesAfterCleanup.Length, 1);
                Assert.AreEqual(filePath, retrievedFilesAfterCleanup.First().FullName);
            }
        }
    }
}