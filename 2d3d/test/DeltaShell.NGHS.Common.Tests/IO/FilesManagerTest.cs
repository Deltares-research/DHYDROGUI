using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.Common.IO;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.IO
{
    [TestFixture]
    public class FilesManagerTest
    {
        private SwitchToActionHelper helper;

        [SetUp]
        public void Setup()
        {
            helper = new SwitchToActionHelper();
        }

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
            void Call() => new FilesManager().Add(null, helper.Action);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("filePath"));
        }

        [Test]
        [TestCaseSource(nameof(CopyToArgumentNullCases))]
        public void CopyTo_ArgumentNull_ThrowsArgumentNullException(string targetPath, ILogHandler logHandler, bool switchTo,
                                                                    string expectedParamName)
        {
            // Call
            void Call() => new FilesManager().CopyTo(targetPath, logHandler, switchTo);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCase(true)]
        [TestCase(false)]
        public void Add_And_CopyTo_SourceFileDoesNotExist_LogsError(bool switchTo)
        {
            // Given
            var filesManager = new FilesManager();
            var logHandler = Substitute.For<ILogHandler>();

            using (var tempDir = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDir.Path, "file.txt");

                // When
                filesManager.Add(filePath, helper.Action);
                filesManager.CopyTo(tempDir.Path, logHandler, switchTo);

                // Then
                Assert.That(logHandler.ReceivedCalls().Count(), Is.EqualTo(1));
                logHandler.Received(1).ReportError($"Could not find file at '{filePath}'.");
                Assert.That(helper.TimesInvoked, Is.EqualTo(0));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Add_And_CopyTo_SwitchToFalse_FileExistsAtTarget_OverwritesFile()
        {
            // Given
            var filesManager = new FilesManager();
            var logHandler = Substitute.For<ILogHandler>();

            const string fileName = "file.txt";

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDir = tempDir.CreateDirectory("source");
                string sourceFilePath = CreateFile(sourceDir, fileName);
                string targetDir = tempDir.CreateDirectory("target");
                string targetFilePath = CreateFile(targetDir, fileName);

                // Precondition
                string sourceFileContent = File.ReadAllText(sourceFilePath);
                string targetFileContent = File.ReadAllText(targetFilePath);
                Assert.That(targetFileContent, Is.Not.EqualTo(sourceFileContent));

                // When
                filesManager.Add(sourceFilePath, helper.Action);
                filesManager.CopyTo(targetDir, logHandler, false);

                // Then
                Assert.That(targetFilePath, Does.Exist);
                Assert.That(File.ReadAllText(targetFilePath), Is.EqualTo(sourceFileContent));

                Assert.That(logHandler.ReceivedCalls().Count(), Is.EqualTo(0));
                Assert.That(helper.TimesInvoked, Is.EqualTo(0));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Add_And_CopyTo_SwitchToTrue_FileExistsAtTarget_OverwritesFileAndInvokesSwitchToAction()
        {
            // Given
            var filesManager = new FilesManager();
            var logHandler = Substitute.For<ILogHandler>();

            const string fileName = "file.txt";

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDir = tempDir.CreateDirectory("source");
                string sourceFilePath = CreateFile(sourceDir, fileName);
                string targetDir = tempDir.CreateDirectory("target");
                string targetFilePath = CreateFile(targetDir, fileName);

                // Precondition
                string sourceFileContent = File.ReadAllText(sourceFilePath);
                string targetFileContent = File.ReadAllText(targetFilePath);
                Assert.That(targetFileContent, Is.Not.EqualTo(sourceFileContent));

                // When
                filesManager.Add(sourceFilePath, helper.Action);
                filesManager.CopyTo(targetDir, logHandler, true);

                // Then
                Assert.That(targetFilePath, Does.Exist);
                Assert.That(File.ReadAllText(targetFilePath), Is.EqualTo(sourceFileContent));

                Assert.That(logHandler.ReceivedCalls().Count(), Is.EqualTo(0));
                Assert.That(helper.TimesInvoked, Is.EqualTo(1));
                Assert.That(helper.InvokeParameters[0], Is.EqualTo(targetFilePath));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Add_And_CopyTo_SwitchToFalse_CopiesAddedFilesToTarget()
        {
            // Given
            var filesManager = new FilesManager();
            var logHandler = Substitute.For<ILogHandler>();

            const string fileName1 = "file_1.txt";
            const string fileName2 = "file_2.txt";

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDir = tempDir.CreateDirectory("source");
                string sourceFilePath1 = CreateFile(sourceDir, fileName1);
                string sourceFilePath2 = CreateFile(sourceDir, fileName2);
                string targetDir = tempDir.CreateDirectory("target");

                // When
                filesManager.Add(sourceFilePath1, helper.Action);
                filesManager.Add(sourceFilePath2, helper.Action);
                filesManager.CopyTo(targetDir, logHandler, false);

                // Then
                Assert.That(Path.Combine(targetDir, fileName1), Does.Exist,
                            "File was not copied.");
                Assert.That(Path.Combine(targetDir, fileName2), Does.Exist,
                            "File was not copied.");

                Assert.That(logHandler?.ReceivedCalls().Any(), Is.Not.True);
                Assert.That(helper.TimesInvoked, Is.EqualTo(0));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Add_And_CopyTo_SwitchToTrue_CopiesAddedFilesToTargetAndInvokesSwitchToAction()
        {
            // Given
            var filesManager = new FilesManager();
            var logHandler = Substitute.For<ILogHandler>();

            const string fileName1 = "file_1.txt";
            const string fileName2 = "file_2.txt";

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceDir = tempDir.CreateDirectory("source");
                string sourceFilePath1 = CreateFile(sourceDir, fileName1);
                string sourceFilePath2 = CreateFile(sourceDir, fileName2);
                string targetDir = tempDir.CreateDirectory("target");

                // When
                filesManager.Add(sourceFilePath1, helper.Action);
                filesManager.Add(sourceFilePath2, helper.Action);
                filesManager.CopyTo(targetDir, logHandler, true);

                // Then
                string targetFilePath1 = Path.Combine(targetDir, fileName1);
                Assert.That(targetFilePath1, Does.Exist,
                            "File was not copied.");
                string targetFilePath2 = Path.Combine(targetDir, fileName2);
                Assert.That(targetFilePath2, Does.Exist,
                            "File was not copied.");

                Assert.That(logHandler?.ReceivedCalls().Any(), Is.Not.True);
                Assert.That(helper.TimesInvoked, Is.EqualTo(2));
                Assert.That(helper.InvokeParameters[0], Is.EqualTo(targetFilePath1));
                Assert.That(helper.InvokeParameters[1], Is.EqualTo(targetFilePath2));
            }
        }

        private static IEnumerable<TestCaseData> CopyToArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<ILogHandler>(), false, "targetPath");
            yield return new TestCaseData(null, Substitute.For<ILogHandler>(), true, "targetPath");
            yield return new TestCaseData("some_value", null, false, "logHandler");
            yield return new TestCaseData("some_value", null, true, "logHandler");
        }

        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.Integration)]
        public void Add_And_CopyTo_SameLocation_SwitchToActionIsNotInvoked(bool switchTo)
        {
            // Given
            var filesManager = new FilesManager();
            var logHandler = Substitute.For<ILogHandler>();

            using (var tempDir = new TemporaryDirectory())
            {
                string filePath = CreateFile(tempDir.Path, "file.txt");

                // When
                filesManager.Add(filePath, helper.Action);
                filesManager.CopyTo(tempDir.Path, logHandler, switchTo);

                // Then
                Assert.That(filePath, Does.Exist);
                Assert.That(logHandler.ReceivedCalls().Any(), Is.False);
                Assert.That(helper.TimesInvoked, Is.EqualTo(0));
            }
        }

        private static string CreateFile(string dirPath, string fileName)
        {
            string filePath = Path.Combine(dirPath, fileName);
            File.WriteAllText(filePath, filePath);

            return filePath;
        }

        private class SwitchToActionHelper
        {
            public readonly Action<string> Action;

            public SwitchToActionHelper()
            {
                Action = s =>
                {
                    InvokeParameters.Add(s);
                    TimesInvoked++;
                };
            }

            public int TimesInvoked { get; private set; }

            public IList<string> InvokeParameters { get; } = new List<string>();
        }
    }
}