using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils.AssertConstraints;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.IO
{
    [TestFixture]
    public class FilesManagerTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var filesManager = new FilesManager();

            // Assert
            Assert.That(filesManager, Is.InstanceOf<IFilesManager>());
        }

        [Test]
        public void Add_FilePathNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new FilesManager().Add(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("filePath"));
        }

        [Test]
        public void CopyTo_TargetPathNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new FilesManager().CopyTo(null, Substitute.For<ILogHandler>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("targetPath"));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Add_And_CopyTo_SourceFileDoesNotExist_LogsWarning()
        {
            // Given
            var filesManager = new FilesManager();
            var logHandler = Substitute.For<ILogHandler>();

            using (var tempDir = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDir.Path, "file.txt");

                // When
                filesManager.Add(filePath);
                filesManager.CopyTo(tempDir.Path, logHandler);

                // Then
                Assert.That(logHandler.ReceivedCalls().Count(), Is.EqualTo(1));
                logHandler.Received(1).ReportWarning($"Could not find file at '{filePath}'.");
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Add_And_CopyTo_FileExistsAtTarget_LogsWarning()
        {
            // Given
            var filesManager = new FilesManager();
            var logHandler = Substitute.For<ILogHandler>();

            const string fileName = "file_1.txt";

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDir1 = tempDir.CreateDirectory("source1");
                string sourceDir2 = tempDir.CreateDirectory("source2");
                string sourceFilePath1 = CreateFile(sourceDir1, fileName);
                string sourceFilePath2 = CreateFile(sourceDir2, fileName);
                string targetDir = tempDir.CreateDirectory("target");

                // When
                filesManager.Add(sourceFilePath1);
                filesManager.Add(sourceFilePath1);
                filesManager.Add(sourceFilePath2);
                filesManager.CopyTo(targetDir, logHandler);

                // Then
                string expectedTargetFilePath = Path.Combine(targetDir, fileName);
                Assert.That(sourceDir1, Does.Exist, "File was not copied.");
                Assert.That(logHandler.ReceivedCalls().Count(), Is.EqualTo(1));
                logHandler.Received(1).ReportWarning($"File already exists at '{expectedTargetFilePath}'.");
            }
        }

        [TestCaseSource(nameof(GetLogHandlers))]
        [Category(TestCategory.Integration)]
        public void Add_And_CopyTo_CopiesAddedFilesToTarget(ILogHandler logHandler)
        {
            // Given
            var filesManager = new FilesManager();

            const string fileName1 = "file_1.txt";
            const string fileName2 = "file_2.txt";

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDir = tempDir.CreateDirectory("source");
                string sourceFilePath1 = CreateFile(sourceDir, fileName1);
                string sourceFilePath2 = CreateFile(sourceDir, fileName2);
                string targetDir = tempDir.CreateDirectory("target");

                // When
                filesManager.Add(sourceFilePath1);
                filesManager.Add(sourceFilePath2);
                filesManager.CopyTo(targetDir, logHandler);

                // Then
                Assert.That(Path.Combine(targetDir, fileName1), Does.Exist,
                            "File was not copied.");
                Assert.That(Path.Combine(targetDir, fileName2), Does.Exist,
                            "File was not copied.");

                Assert.That(logHandler?.ReceivedCalls().Any(), Is.Not.True);
            }
        }

        private IEnumerable<ILogHandler> GetLogHandlers()
        {
            yield return Substitute.For<ILogHandler>();
            yield return null;
        }

        private static string CreateFile(string dirPath, string fileName)
        {
            string filePath = Path.Combine(dirPath, fileName);
            File.WriteAllText(filePath, "test_file");

            return filePath;
        }
    }
}