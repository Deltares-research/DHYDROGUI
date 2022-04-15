using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.IO;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class FileBasedFolderTest
    {
        [Test]
        public void CopyTo_WhenSourceFolderIsSubfolderOfTheDestinationFolder_ThenInvalidOperationExceptionIsThrown()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string destinationDirectory = Path.Combine(tempDirectory.Path, "destination");
                string sourceDirectory = Path.Combine(destinationDirectory, "source");

                Directory.CreateDirectory(sourceDirectory);

                var fileBasedFolder = new FileBasedFolder(sourceDirectory);

                // Call
                void Call() => fileBasedFolder.CopyTo(destinationDirectory);

                // Assert
                Assert.That(Call, Throws.TypeOf<InvalidOperationException>()
                                        .With.Message.EqualTo("Cannot delete destination folder when source folder is a subfolder of the destination folder."));
            }
        }

        [Test]
        public void CopyTo_WhenPathDoesNotExist_ThenMethodReturnsAndNoExceptionIsThrown()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string destinationPathArg = tempDirectory.Path;
                const string originalPath = "does_not_exist";

                // Setup
                var fileBasedFolder = new FileBasedFolder(originalPath);

                // Call
                void Call() => fileBasedFolder.CopyTo(destinationPathArg);

                // Assert
                Assert.DoesNotThrow(Call, "No exception should be thrown when path does not exist.");
            }
        }

        [Test]
        public void CopyTo_WhenPathAndDestinationPathAreEqual_ThenMethodReturnsAndNoExceptionIsThrown()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string destinationPathArg = tempDirectory.Path;

                // Setup
                var fileBasedFolder = new FileBasedFolder(destinationPathArg);

                // Precondition
                Assert.That(Directory.Exists(destinationPathArg), "Precondition violation: source directory should exist.");

                // Call
                void Call() => fileBasedFolder.CopyTo(destinationPathArg);

                // Assert
                Assert.DoesNotThrow(Call, "No exception should be thrown when destination is equal to source path.");
                Assert.That(Directory.Exists(destinationPathArg), "When folder should not be moved, folder at original path should not be deleted");
            }
        }

        [Test]
        public void CopyTo_WhenRelativeDestinationPathRefersToSamePath_ThenPathIsNotSwitchedAndNoExceptionIsThrown()
        {
            // Setup
            string previousCurrentDirectory = Directory.GetCurrentDirectory();

            using (var tempDirectory = new TemporaryDirectory())
            {
                string currentDirectory = Path.Combine(tempDirectory.Path, "this", "is");
                string relativePath = Path.Combine("a", "folder");
                string fullPath = Path.Combine(currentDirectory, relativePath);

                Directory.CreateDirectory(fullPath);
                Directory.SetCurrentDirectory(currentDirectory);

                try
                {
                    // Setup
                    var fileBasedFolder = new FileBasedFolder(fullPath);

                    // Call
                    void Call() => fileBasedFolder.CopyTo(relativePath);

                    // Assert
                    Assert.DoesNotThrow(Call, "No exception should be thrown when destination is refers to same path as source path.");
                    Assert.That(Directory.Exists(fullPath), "Source directory should still exist when folder should not be moved.");
                }
                finally
                {
                    Directory.SetCurrentDirectory(previousCurrentDirectory);
                }
            }
        }

        [Test]
        public void SwitchTo_ThenPathIsSwitchedToNewPath()
        {
            // Setup
            var fileBasedFolder = new FileBasedFolder();
            const string path = "folder_path";

            // Call
            fileBasedFolder.SwitchTo(path);

            // Assert
            Assert.That(fileBasedFolder.Path, Is.EqualTo(path),
                        "After switching, the path should be set to the new path.");
        }

        [Test]
        public void Delete_ThenFolderIsDeleted()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string folderPath = Path.Combine(tempDirectory.Path, "folder");

                Directory.CreateDirectory(folderPath);

                var fileBasedFolder = new FileBasedFolder(folderPath);

                // Precondition
                Assert.That(Directory.Exists(folderPath),
                            "This test is unreliable when the folder path does not exist.");

                // Call
                fileBasedFolder.Delete();

                // Assert
                Assert.That(!Directory.Exists(folderPath),
                            "After deleting, the folder should be deleted.");
            }
        }

        [Test]
        public void SetPath_ThenOnPropertyChangedIsFiredOnce()
        {
            // Setup
            var fileBasedFolder = new FileBasedFolder();

            // Call
            void Call() => fileBasedFolder.Path = "folder_path";

            // Assert
            TestHelper.AssertPropertyChangedIsFired(fileBasedFolder, 1, Call);
        }

        [Test]
        public void SetPath_WhenValueIsARelativePathThatRefersToTheSamePath_ThenOnPropertyChangedIsNotFired()
        {
            // Setup
            string previousCurrentDirectory = Directory.GetCurrentDirectory();

            using (var tempDirectory = new TemporaryDirectory())
            {
                string currentDirectory = Path.Combine(tempDirectory.Path, "this", "is");
                string relativePath = Path.Combine("a", "folder");
                string fullPath = Path.Combine(currentDirectory, relativePath);

                Directory.CreateDirectory(currentDirectory);
                Directory.SetCurrentDirectory(currentDirectory);

                try
                {
                    var fileBasedFolder = new FileBasedFolder {Path = fullPath};

                    // Call
                    void Call() => fileBasedFolder.Path = relativePath;

                    // Assert
                    TestHelper.AssertPropertyChangedIsFired(fileBasedFolder, 0, Call);
                }
                finally
                {
                    Directory.SetCurrentDirectory(previousCurrentDirectory);
                }
            }
        }

        [Test]
        public void SetPath_WhenValueIsEmpty_ThenArgumentExceptionIsThrown()
        {
            // Call
            void Call() => new FileBasedFolder().Path = string.Empty;

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Exists_WhenFolderExists_ThenTrueIsReturned()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string folderPath = tempDirectory.Path;
                var fileBasedFolder = new FileBasedFolder {Path = folderPath};

                // Precondition
                Assert.That(Directory.Exists(folderPath),
                            "This test is unreliable when the folder path does not exist.");

                // Call
                bool result = fileBasedFolder.Exists;

                // Assert
                Assert.That(result, Is.True,
                            $"When folder exists, the property {fileBasedFolder.Exists} should return true;");
            }
        }

        [Test]
        public void Exists_WhenFolderDoesNotExist_ThenFalseIsReturned()
        {
            // Setup
            var fileBasedFolder = new FileBasedFolder();

            // Precondition
            Assert.That(!Directory.Exists(fileBasedFolder.Path),
                        "This test is unreliable when the folder path does exist.");

            // Call
            bool result = fileBasedFolder.Exists;

            // Assert
            Assert.That(result, Is.False,
                        $"When folder does not exist, the property {fileBasedFolder.Exists} should return false;");
        }

        [Test]
        public void ContainsFile_WhenPathDoesNotExist_ThenFalseIsReturnedAndFilePathIsEmpty()
        {
            // Setup
            var fileBasedFolder = new FileBasedFolder {Path = "folder_path"};
            const string fileName = "file_name";

            // Call
            bool result = fileBasedFolder.ContainsFile(fileName, out string filePath);

            // Assert
            Assert.That(result, Is.False,
                        "When folder path does not exist, false should be returned.");
            Assert.That(filePath, Is.EqualTo(string.Empty),
                        "When folder path does not exist, an empty path should be returned.");
        }

        [Test]
        public void ContainsFile_WhenFileExistsInFolder_ThenTrueIsReturnedAndFilePathIsCorrect()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string folderPath = tempDirectory.Path;
                var fileBasedFolder = new FileBasedFolder {Path = folderPath};
                const string fileName = "file_name";
                string expectedFilePath = Path.Combine(folderPath, fileName);

                File.WriteAllText(expectedFilePath, "");

                // Precondition
                Assert.That(File.Exists(expectedFilePath),
                            "This test is unreliable when the file does not exist.");

                // Call
                bool result = fileBasedFolder.ContainsFile(fileName, out string resultedFilePath);

                // Assert
                Assert.That(result, Is.True,
                            "When file exists, true should be returned.");
                Assert.That(resultedFilePath, Is.EqualTo(expectedFilePath),
                            "When file exists, the correct path should be returned.");
            }
        }

        [Test]
        public void ContainsFile_WhenFileDoesNotExistInFolder_ThenFalseIsReturnedAndFilePathIsEmpty()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string folderPath = tempDirectory.Path;
                var fileBasedFolder = new FileBasedFolder(folderPath);
                File.WriteAllText(Path.Combine(folderPath, "irrelevant_file"), string.Empty);

                // Call
                bool result = fileBasedFolder.ContainsFile("file_name", out string resultedFilePath);

                // Assert
                Assert.That(result, Is.False,
                            "When file does not exist, false should be returned.");
                Assert.That(resultedFilePath, Is.EqualTo(string.Empty),
                            "When file does not exist, an empty path should be returned.");
            }
        }

        [Test]
        public void Paths_Get_ReturnsPath()
        {
            // Setup
            const string folderPath = "folder_path";
            var fileBasedFolder = new FileBasedFolder(folderPath);

            // Call
            string[] paths = fileBasedFolder.Paths.ToArray();

            // Assert
            Assert.That(paths.Single(), Is.EqualTo(folderPath),
                        "The path should be returned.");
        }

        [Test]
        public void IsOpen_Get_ReturnsFalse()
        {
            // Setup
            var fileBasedFolder = new FileBasedFolder();

            // Call
            bool result = fileBasedFolder.IsOpen;

            // Assert
            Assert.That(result, Is.False,
                        $"{nameof(fileBasedFolder.IsOpen)} should be false.");
        }

        [Test]
        public void IsFileCritical_Get_ReturnsFalse()
        {
            // Setup
            var fileBasedFolder = new FileBasedFolder();

            // Call
            bool result = fileBasedFolder.IsFileCritical;

            // Assert
            Assert.That(result, Is.False,
                        $"{nameof(fileBasedFolder.IsFileCritical)} should be false.");
        }

        [TestCase("")]
        [TestCase(null)]
        public void CopyTo_WhenArgumentNullOrEmpty_ThenMethodReturns(string destinationPathArg)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string originalPath = tempDirectory.Path;

                // Setup
                var fileBasedFolder = new FileBasedFolder(originalPath);

                // Call
                fileBasedFolder.CopyTo(destinationPathArg);

                // Assert
                Assert.That(fileBasedFolder.Path, Is.EqualTo(originalPath), "When folder cannot be moved, path should never be switched.");
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CopyTo_WithValidPathAndDestinationPathAndGivenArguments_WhenDestinationFolderDoesOrDoesNotExist_ThenFolderIsCorrectlyCopied(bool destinationAlreadyExists)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string tempDirPath = tempDirectory.Path;
                string sourcePath = Path.Combine(tempDirPath, "source");
                string destinationPath = Path.Combine(tempDirPath, "destination");

                string[] expectedFilesInDestinationFolder = ExpectedFilesInDestinationFolder(sourcePath,
                                                                                             destinationPath,
                                                                                             destinationAlreadyExists,
                                                                                             true).ToArray();

                var fileBasedFolder = new FileBasedFolder(sourcePath);

                // Call
                fileBasedFolder.CopyTo(destinationPath);

                // Assert
                AssertThatFolderWasCopiedCorrectly(destinationPath, expectedFilesInDestinationFolder);
            }
        }

        [TestCase("", false, true)]
        [TestCase(null, false, true)]
        [TestCase("", false, false)]
        [TestCase(null, false, false)]
        [TestCase("", true, true)]
        [TestCase(null, true, true)]
        [TestCase("", true, false)]
        [TestCase(null, true, false)]
        public void MoveTo_WhenArgumentNullOrEmpty_ThenMethodReturnsAndPathIsNotSwitched(string destinationPathArg, bool deleteIfExistsArg, bool switchToArg)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string originalPath = tempDirectory.Path;

                // Setup
                var fileBasedFolder = new FileBasedFolder(originalPath);

                // Call
                fileBasedFolder.MoveTo(destinationPathArg, deleteIfExistsArg, switchToArg);

                // Assert
                Assert.That(fileBasedFolder.Path, Is.EqualTo(originalPath), "When folder cannot be moved, path should never be switched.");
                Assert.That(Directory.Exists(originalPath), "When folder should not be moved, folder at original path should not be deleted");
            }
        }

        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public void MoveTo_WhenDestinationFolderIsSubfolderOfTheSourceFolder_ThenInvalidOperationExceptionIsThrown(bool deleteIfExistsArg, bool switchToArg)
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string sourceDirectory = Path.Combine(tempDirectory.Path, "source");
                string destinationDirectory = Path.Combine(sourceDirectory, "destination");

                Directory.CreateDirectory(sourceDirectory);

                // Call
                void Call() => new FileBasedFolder(sourceDirectory).MoveTo(destinationDirectory, deleteIfExistsArg, switchToArg);

                // Assert
                Assert.That(Call, Throws.TypeOf<InvalidOperationException>()
                                        .With.Message.EqualTo("Cannot move source folder when destination folder is a subfolder of the source folder."));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MoveTo_WhenSourceFolderIsSubfolderOfTheDestinationFolderAndDeleteIfExistsIsTrue_ThenInvalidOperationExceptionIsThrown(bool switchToArg)
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string destinationDirectory = Path.Combine(tempDirectory.Path, "destination");
                string sourceDirectory = Path.Combine(destinationDirectory, "source");

                Directory.CreateDirectory(sourceDirectory);

                var fileBasedFolder = new FileBasedFolder(sourceDirectory);

                // Call
                void Call() => fileBasedFolder.MoveTo(destinationDirectory, true, switchToArg);

                // Assert
                Assert.That(Call, Throws.TypeOf<InvalidOperationException>()
                                        .With.Message.EqualTo("Cannot delete destination folder when source folder is a subfolder of the destination folder."));
            }
        }

        [TestCase(false, true)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(true, false)]
        public void MoveTo_WhenSourceFolderIsSubfolderOfTheDestinationFolderAndDeleteIfExistsIsFalse_ThenFolderIsCorrectlyMoved(bool destinationAlreadyExists, bool switchToArg)
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string destinationDirectory = Path.Combine(tempDirectory.Path, "destination");
                string sourceDirectory = Path.Combine(destinationDirectory, "source");
                string[] expectedFilesInDestinationFolder = ExpectedFilesInDestinationFolder(sourceDirectory,
                                                                                             destinationDirectory,
                                                                                             destinationAlreadyExists,
                                                                                             false).ToArray();

                var fileBasedFolder = new FileBasedFolder(sourceDirectory);

                // Call
                fileBasedFolder.MoveTo(destinationDirectory, false, switchToArg);

                // Assert
                AssertThatFolderWasMovedCorrectly(sourceDirectory, destinationDirectory, expectedFilesInDestinationFolder);
                AssertThatCurrentPathIsCorrect(switchToArg, fileBasedFolder, sourceDirectory, destinationDirectory);
            }
        }

        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public void MoveTo_WhenPathDoesNotExist_ThenPathIsNotSwitchedAndNoExceptionIsThrown(bool deleteIfExistsArg, bool switchToArg)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string destinationPathArg = tempDirectory.Path;
                const string originalPath = "does_not_exist";

                // Setup
                var fileBasedFolder = new FileBasedFolder(originalPath);

                // Call
                void Call() => fileBasedFolder.MoveTo(destinationPathArg, deleteIfExistsArg, switchToArg);

                // Assert
                Assert.DoesNotThrow(Call, "No exception should be thrown when path does not exist.");
                Assert.That(fileBasedFolder.Path, Is.EqualTo(originalPath), "When folder cannot be moved, path should never be switched.");
            }
        }

        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public void MoveTo_WhenPathAndDestinationPathAreEqual_ThenPathIsNotSwitchedAndNoExceptionIsThrown(bool deleteIfExistsArg, bool switchToArg)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string destinationPathArg = tempDirectory.Path;

                // Setup
                var fileBasedFolder = new FileBasedFolder(destinationPathArg);

                // Precondition
                Assert.That(Directory.Exists(destinationPathArg), "Precondition violation: source directory should exist.");

                // Call
                void Call() => fileBasedFolder.MoveTo(destinationPathArg, deleteIfExistsArg, switchToArg);

                // Assert
                Assert.DoesNotThrow(Call, "No exception should be thrown when destination is equal to source path.");
                Assert.That(Directory.Exists(destinationPathArg), "When folder should not be moved, folder at original path should not be deleted");
            }
        }

        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public void MoveTo_WhenRelativeDestinationPathRefersToSamePath_ThenPathIsNotSwitchedAndNoExceptionIsThrown(bool deleteIfExistsArg, bool switchToArg)
        {
            // Setup
            string previousCurrentDirectory = Directory.GetCurrentDirectory();

            using (var tempDirectory = new TemporaryDirectory())
            {
                string currentDirectory = Path.Combine(tempDirectory.Path, "this", "is");
                string relativePath = Path.Combine("a", "folder");
                string fullPath = Path.Combine(currentDirectory, relativePath);

                Directory.CreateDirectory(fullPath);
                Directory.SetCurrentDirectory(currentDirectory);

                try
                {
                    // Setup
                    var fileBasedFolder = new FileBasedFolder(fullPath);

                    // Call
                    void Call() => fileBasedFolder.MoveTo(relativePath, deleteIfExistsArg, switchToArg);

                    // Assert
                    Assert.DoesNotThrow(Call, "No exception should be thrown when destination is refers to same path as source path.");
                    Assert.That(Directory.Exists(fullPath), "Source directory should still exist when folder should not be moved.");
                }
                finally
                {
                    Directory.SetCurrentDirectory(previousCurrentDirectory);
                }
            }
        }

        [TestCase(false, false, true)]
        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, true)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(true, true, false)]
        public void MoveTo_WithValidPathAndDestinationPathAndGivenArguments_WhenDestinationFolderDoesOrDoesNotExist_ThenFolderIsCorrectlyMoved(bool destinationAlreadyExists, bool deleteIfExistsArg, bool switchToArg)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string tempDirPath = tempDirectory.Path;
                string sourcePath = Path.Combine(tempDirPath, "source");
                string destinationPath = Path.Combine(tempDirPath, "destination");

                string[] expectedFilesInDestinationFolder = ExpectedFilesInDestinationFolder(sourcePath,
                                                                                             destinationPath,
                                                                                             destinationAlreadyExists,
                                                                                             deleteIfExistsArg).ToArray();

                var fileBasedFolder = new FileBasedFolder(sourcePath);

                // Call
                fileBasedFolder.MoveTo(destinationPath, deleteIfExistsArg, switchToArg);

                // Assert
                AssertThatFolderWasMovedCorrectly(sourcePath, destinationPath, expectedFilesInDestinationFolder);
                AssertThatCurrentPathIsCorrect(switchToArg, fileBasedFolder, sourcePath, destinationPath);
            }
        }

        [TestCase(false, false, true)]
        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, true)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(true, true, false)]
        [Explicit] // Likely to fail on build server, because we have no permission for other drives.
        public void MoveTo_WithValidPathAndDestinationPathAndGivenArguments_WhenDestinationIsOnOtherVolumeAndDoesOrDoesNotExist_ThenFolderIsMovedToDestinationAndPathSwitchedAccordingly(bool destinationAlreadyExists, bool deleteIfExistsArg, bool switchToArg)
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string tempDirPath = tempDirectory.Path;
                string otherDrive = Environment.GetLogicalDrives()
                                               .First(d => Directory.GetDirectoryRoot(tempDirPath) != d);
                string sourcePath = Path.Combine(tempDirPath, "source");
                string destinationPath = Path.Combine(otherDrive, "destination");

                string[] expectedFilesInDestinationFolder = ExpectedFilesInDestinationFolder(sourcePath,
                                                                                             destinationPath,
                                                                                             destinationAlreadyExists,
                                                                                             deleteIfExistsArg).ToArray();

                var fileBasedFolder = new FileBasedFolder(sourcePath);

                try
                {
                    Assert.That(Directory.GetDirectoryRoot(destinationPath),
                                Is.Not.EqualTo(Directory.GetDirectoryRoot(sourcePath)),
                                "This test is unreliable when destination and source directory are on the same volume.");

                    // Call
                    fileBasedFolder.MoveTo(destinationPath, deleteIfExistsArg, switchToArg);

                    // Assert
                    AssertThatFolderWasMovedCorrectly(sourcePath, destinationPath, expectedFilesInDestinationFolder);
                    AssertThatCurrentPathIsCorrect(switchToArg, fileBasedFolder, sourcePath, destinationPath);
                }
                finally
                {
                    // Cleanup
                    FileUtils.DeleteIfExists(destinationPath);
                }
            }
        }

        [TestCase("c:\\folder\\path")]
        [TestCase("c:/folder\\path")]
        [TestCase("c:\\folder/path")]
        [TestCase("c:\\folder/path")]
        [TestCase("c:/folder/./path")]
        public void SetPath_WhenValueAndPathAreEqual_ThenOnPropertyChangedIsNotFired(string newPath)
        {
            // Setup
            var fileBasedFolder = new FileBasedFolder {Path = "c:/folder/path"};

            // Call
            void Call() => fileBasedFolder.Path = newPath;

            // Assert
            TestHelper.AssertPropertyChangedIsFired(fileBasedFolder, 0, Call);
        }

        [TestCase(null, null)]
        [TestCase("c:/path", "c:\\path")]
        [TestCase("c:/folder/path", "c:\\folder\\path")]
        [TestCase("c:/folder/./path", "c:\\folder\\path")]
        public void GetFullPath_ThenCorrectPathIsReturned(string setPath, string expectedFullPath)
        {
            // Setup
            var fileBasedFolder = new FileBasedFolder(setPath);

            // Call
            string result = fileBasedFolder.FullPath;

            // Assert
            Assert.That(result, Is.EqualTo(expectedFullPath),
                        $"{nameof(fileBasedFolder.FullPath)} should have returned the correct full path.");
        }

        [TestCaseSource(nameof(InvalidChars))]
        public void SetPath_WhenValueContainsInvalidChars_ThenArgumentExceptionIsThrown(char invalidCharacter)
        {
            // Setup
            var path = $"c:/{invalidCharacter}folder_path";

            // Call
            void Call() => new FileBasedFolder().Path = path;

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [TestCase(null)]
        [TestCase("path")]
        [TestCase("c:/path")]
        public void SetPath_WhenValueIsValid_ThenPathIsSetCorrectly(string path)
        {
            // Setup
            var fileBasedFolder = new FileBasedFolder();

            // Call
            fileBasedFolder.Path = path;

            // Assert
            Assert.That(fileBasedFolder.Path, Is.EqualTo(path),
                        $"{nameof(fileBasedFolder.Path)} should be set correctly.");
        }

        [TestCase("")]
        [TestCase(null)]
        public void ContainsFile_WhenFileNameArgNull_ThenFalseIsReturnedAndFilePathIsEmpty(string fileNameArg)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string folderPath = tempDirectory.Path;
                var fileBasedFolder = new FileBasedFolder {Path = folderPath};

                // Precondition
                Assert.That(Directory.Exists(folderPath),
                            "This test is unreliable when the folder path does not exist.");

                // Call
                bool result = fileBasedFolder.ContainsFile(fileNameArg, out string filePath);

                // Assert
                Assert.That(result, Is.False,
                            "When argument is null, false should be returned.");
                Assert.That(filePath, Is.EqualTo(string.Empty),
                            "When argument is null, an empty path should be returned.");
            }
        }

        private static IEnumerable<char> InvalidChars()
        {
            return Path.GetInvalidPathChars();
        }

        private static IEnumerable<string> ExpectedFilesInDestinationFolder(string sourcePath,
                                                                            string destinationPath,
                                                                            bool destinationAlreadyExists,
                                                                            bool deleteIfExistsArg)
        {
            var expectedFilesInDestinationFolder = new List<string>();

            const string sourceFileName = "source.txt";
            const string destinationFileName = "destination.txt";

            IEnumerable<string> sourceFilePaths = PrepareDirectory(sourcePath, sourceFileName);

            expectedFilesInDestinationFolder.AddRange(sourceFilePaths
                                                          .Select(sfp => sfp.Replace(sourcePath, destinationPath)));

            if (destinationAlreadyExists)
            {
                IEnumerable<string> destinationFilePaths = PrepareDirectory(destinationPath, destinationFileName);

                if (!deleteIfExistsArg)
                {
                    expectedFilesInDestinationFolder.AddRange(destinationFilePaths);
                }
            }

            return expectedFilesInDestinationFolder.Distinct();
        }

        private static IEnumerable<string> PrepareDirectory(string mainFolderPath, string fileName)
        {
            const string sharedFileName = "file.txt";
            string subFolderPath = Path.Combine(mainFolderPath, "folder");

            Directory.CreateDirectory(mainFolderPath);
            Directory.CreateDirectory(subFolderPath);

            yield return CreateFile(mainFolderPath, fileName);
            yield return CreateFile(mainFolderPath, sharedFileName);
            yield return CreateFile(subFolderPath, fileName);
            yield return CreateFile(subFolderPath, sharedFileName);
        }

        private static string CreateFile(string folderPath, string fileName)
        {
            string sharedFileInSubFolderPath = Path.Combine(folderPath, fileName);
            File.WriteAllText(sharedFileInSubFolderPath, "");
            return sharedFileInSubFolderPath;
        }

        private static void AssertThatCurrentPathIsCorrect(bool switchToArg, IFileBasedFolder fileBasedFolder, string sourcePath,
                                                           string destinationPath)
        {
            Assert.That(fileBasedFolder.Path.Equals(sourcePath), Is.EqualTo(!switchToArg),
                        $"After moving and switchTo is {switchToArg}, then folder path is unchanged should be {!switchToArg}.");
            Assert.That(fileBasedFolder.Path.Equals(destinationPath), Is.EqualTo(switchToArg),
                        $"After moving and switchTo is {switchToArg}, then folder path is target directory path should be {switchToArg}.");
        }

        private static void AssertThatFolderWasMovedCorrectly(string sourcePath, string destinationPath, ICollection<string> expectedFilePaths)
        {
            AssertThatFolderWasCopiedCorrectly(destinationPath, expectedFilePaths);

            Assert.That(!Directory.Exists(sourcePath),
                        "After moving, source directory should not exist anymore.");
        }

        private static void AssertThatFolderWasCopiedCorrectly(string destinationPath, ICollection<string> expectedFilePaths)
        {
            Assert.That(Directory.Exists(destinationPath),
                        "After moving or copying, destination directory should exist.");

            IEnumerable<string> filesInDestination =
                Directory.GetFiles(destinationPath, "*", SearchOption.AllDirectories);

            Assert.That(filesInDestination.Count(), Is.EqualTo(expectedFilePaths.Count),
                        "Number of files in destination folder is incorrect.");

            foreach (string filePath in expectedFilePaths)
            {
                Assert.That(File.Exists(filePath),
                            "After moving or copying, this file should exist in destination directory.");
            }
        }
    }
}