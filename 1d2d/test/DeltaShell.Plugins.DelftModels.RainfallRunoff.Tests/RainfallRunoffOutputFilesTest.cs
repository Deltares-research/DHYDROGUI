using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    public class RainfallRunoffOutputFilesTest
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(@"D:\test|")]
        public void SetDirectory_DirectoryPathNullOrEmptyOrInvalid_ThrowsArgumentException(string directoryPath)
        {
            // Setup
            var outputFiles = new RainfallRunoffOutputFiles();

            // Call
            void Call() => outputFiles.SetDirectory(directoryPath);

            // Assert
            Assert.Throws<ArgumentException>(Call);
        }

        [Test]
        public void Constructor_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            // Setup
            var outputFiles = new RainfallRunoffOutputFiles();
            string directoryPath = Path.GetTempFileName();

            // Call
            void Call() => outputFiles.SetDirectory(directoryPath);

            // Assert
            Assert.Throws<DirectoryNotFoundException>(Call);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(@"D:\test|")]
        public void CopyTo_DirectoryPathNullOrEmptyOrInvalid_ThrowsArgumentException(string directoryPath)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var outputFiles = new RainfallRunoffOutputFiles();
                outputFiles.SetDirectory(temp.Path);

                // Call
                void Call() => outputFiles.CopyTo(directoryPath);

                // Assert
                Assert.Throws<ArgumentException>(Call);
            }
        }

        [Test]
        public void CopyTo_CopiesTheCorrectFilesToTheDirectory()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string copyDir = temp.CreateDirectory("copies");
                var expectedFiles = new List<string>();

                expectedFiles.AddRange(AddFilesIncludedByExtension(temp));
                expectedFiles.AddRange(AddFilesIncludedByName(temp));
                AddFilesExcludedByName(temp);
                AddRandomExcludedFiles(temp);

                var outputFiles = new RainfallRunoffOutputFiles();
                outputFiles.SetDirectory(temp.Path);

                // Call
                outputFiles.CopyTo(copyDir);

                // Assert
                IEnumerable<string> copiedFiles = new DirectoryInfo(copyDir).GetFiles().Select(f => f.Name);
                Assert.That(copiedFiles, Is.EquivalentTo(expectedFiles));
            }
        }

        [Test]
        public void Clear_AndThenCopy_DoesNotCopyAnything()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string copyDir = temp.CreateDirectory("copies");

                AddFilesIncludedByExtension(temp);
                AddFilesIncludedByName(temp);

                var outputFiles = new RainfallRunoffOutputFiles();
                outputFiles.SetDirectory(temp.Path);

                // Calls
                outputFiles.Clear();
                outputFiles.CopyTo(copyDir);

                // Assert
                FileInfo[] copiedFiles = new DirectoryInfo(copyDir).GetFiles();
                Assert.That(copiedFiles, Is.Empty);
            }
        }

        [Test]
        [TestCase(RainfallRunoffOutputFiles.LogFileName, "sobek_3b.log")]
        [TestCase(RainfallRunoffOutputFiles.RunReportFilename, "3b_bal.out")]
        public void FileNames_AreCorrect(string result, string expResult)
        {
            Assert.That(result, Is.EqualTo(expResult));
        }
        
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(@"D:\test|")]
        public void DeleteOutputFiles_DirectoryPathNullOrEmptyOrInvalid_ThrowsArgumentException(string directoryPath)
        {
            // Setup
            var outputFiles = new RainfallRunoffOutputFiles();

            // Call
            TestDelegate call = () => outputFiles.DeleteOutputFiles(directoryPath);

            // Assert
            Assert.That(call, Throws.ArgumentException);
        }

        [Test]
        public void DeleteOutputFiles_DeletesOutput()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                AddFilesIncludedByExtension(temp);
                AddFilesIncludedByName(temp);

                // Precondition
                var tempDir = new DirectoryInfo(temp.Path);
                FileInfo[] files = tempDir.GetFiles();
                Assert.That(files, Is.Not.Empty);

                var outputFiles = new RainfallRunoffOutputFiles();
                outputFiles.SetDirectory(temp.Path);

                // Call
                outputFiles.DeleteOutputFiles(temp.Path);

                // Assert
                files = tempDir.GetFiles();
                Assert.That(files, Is.Empty);
            }
        }

        [Test]
        public void DeleteOutputFiles_OutputFileIsLocked_AddsErrorToLog()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                AddFilesIncludedByExtension(temp);
                AddFilesIncludedByName(temp);
                
                var outputFiles = new RainfallRunoffOutputFiles();
                outputFiles.SetDirectory(temp.Path);

                // Precondition
                var tempDir = new DirectoryInfo(temp.Path);
                FileInfo[] files = tempDir.GetFiles();
                Assert.That(files, Is.Not.Empty);
                
                FileInfo fileToLock = files[0];
                using (FileStream lockedFile = File.Open(fileToLock.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // Call
                    Action action = () => outputFiles.DeleteOutputFiles(temp.Path);

                    // Assert
                    string expectedMessage = string.Format(Resources.RainfallRunoffOutputFiles_Could_not_delete_file,
                                                           fileToLock.FullName);
                    TestHelper.AssertAtLeastOneLogMessagesContains(action, expectedMessage);
                    
                    files = tempDir.GetFiles();
                    Assert.That(files, Has.Length.EqualTo(1));
                }
            }
        }

        private static void AddRandomExcludedFiles(TemporaryDirectory temp)
        {
            for (var i = 0; i < 3; i++)
            {
                temp.CreateFile(Path.GetTempFileName());
            }
        }

        private static void AddFilesExcludedByName(TemporaryDirectory temp)
        {
            foreach (string excludeFile in outputFileExclusions)
            {
                temp.CreateFile(excludeFile);
            }
        }

        private static IList<string> AddFilesIncludedByName(TemporaryDirectory temp)
        {
            var includedFiles = new List<string>();
            foreach (string includeFile in outputFileInclusions)
            {
                string file = temp.CreateFile(includeFile);
                includedFiles.Add(Path.GetFileName(file));
            }

            return includedFiles;
        }

        private static IList<string> AddFilesIncludedByExtension(TemporaryDirectory temp)
        {
            var includedFiles = new List<string>();
            foreach (string extension in outputFileExtensions)
            {
                string file = temp.CreateFile($"test{extension}");
                includedFiles.Add(Path.GetFileName(file));
            }

            return includedFiles;
        }

        private static readonly string[] outputFileExtensions =
        {
            ".hia",
            ".his",
            ".nc",
            ".out",
            ".txt",
            ".dbg",
            ".log",
            ".rtn",
        };

        private static readonly string[] outputFileInclusions =
        {
            "RR-ready",
            "RSRR_OUT"
        };

        private static readonly string[] outputFileExclusions =
        {
            "runoff.out",
            "sobek_3b.dbg"
        };
    }
}