using System.Collections.Generic;
using System.IO;
using DelftTools.Controls;
using DeltaShell.NGHS.Common.Gui.Modals.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.Helpers
{
    [TestFixture]
    public class TimeFrameEditorFileImportHelperTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var dialogService = Substitute.For<IFileDialogService>();
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            // Call
            var helper = new TimeFrameEditorFileImportHelper(dialogService,
                                                             importerService,
                                                             userInputService);

            // Assert
            Assert.That(helper, Is.InstanceOf<ITimeFrameEditorFileImportHelper>());
        }

        private static IEnumerable<TestCaseData> GetConstructorArgumentNullData()
        {
            var dialogService = Substitute.For<IFileDialogService>();
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            yield return new TestCaseData(null, importerService, userInputService, "dialogService");
            yield return new TestCaseData(dialogService, null, userInputService, "inputFileImporterService");
            yield return new TestCaseData(dialogService, importerService, null, "userInputService");
        }

        [Test]
        [TestCaseSource(nameof(GetConstructorArgumentNullData))]
        public void Constructor_ArgumentNullException(IFileDialogService dialogService,
                                                      IInputFileImporterService importerService,
                                                      IRequestUserInputService<FileAlreadyExistsChoice> userInputService,
                                                      string expectedParamName)
        {
            void Call() => new TimeFrameEditorFileImportHelper(dialogService,
                                                               importerService,
                                                               userInputService);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        [Test]
        public void HandleInputFileImport_SelectedFileNull_ReturnsNull()
        {
            // Setup
            var dialogService = Substitute.For<IFileDialogService>();
            const string fileFilter = "imagine this to be a completely valid file filter";
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            dialogService.SelectFile(null).ReturnsForAnyArgs((string)null);

            var helper = new TimeFrameEditorFileImportHelper(dialogService,
                                                             importerService,
                                                             userInputService);

            // Call
            string result = helper.HandleInputFileImport(fileFilter);

            // Assert
            Assert.That(result, Is.Null);
            dialogService.Received(1).SelectFile(fileFilter, "", false);
            importerService.DidNotReceiveWithAnyArgs().CopyFile(null);
        }

        [Test]
        public void HandleInputFileImport_SelectedFileInInputFolder_ReturnsRelativePath()
        {
            // Setup
            var dialogService = Substitute.For<IFileDialogService>();
            const string fileFilter = "imagine this to be a completely valid file filter";
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            const string selectedFilePath = "someSelectedFilePath.txt";
            const string relativeSelectedFilePath = "someRelativePath.txt";

            importerService.IsInInputFolder(selectedFilePath).Returns(true);
            importerService.GetRelativePath(selectedFilePath).Returns(relativeSelectedFilePath);

            dialogService.SelectFile(null).ReturnsForAnyArgs(selectedFilePath);

            var helper = new TimeFrameEditorFileImportHelper(dialogService,
                                                             importerService,
                                                             userInputService);
            // Call
            string result = helper.HandleInputFileImport(fileFilter);

            // Assert
            Assert.That(result, Is.EqualTo(relativeSelectedFilePath));
            importerService.DidNotReceiveWithAnyArgs().CopyFile(null);
        }

        [Test]
        public void HandleInputFileImport_FileExistsInInput_Null_ReturnsNull()
        {
            // Setup
            var dialogService = Substitute.For<IFileDialogService>();
            const string fileFilter = "imagine this to be a completely valid file filter";
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            const string selectedFilePath = "someSelectedFilePath.txt";

            dialogService.SelectFile(null).ReturnsForAnyArgs(selectedFilePath);
            importerService.IsInInputFolder(selectedFilePath).Returns(false);
            importerService.HasFile(selectedFilePath).Returns(true);
            userInputService.RequestUserInput(null, null).ReturnsForAnyArgs((FileAlreadyExistsChoice?)null);

            var helper = new TimeFrameEditorFileImportHelper(dialogService,
                                                             importerService,
                                                             userInputService);
            // Call
            string result = helper.HandleInputFileImport(fileFilter);

            // Assert
            Assert.That(result, Is.Null);
            importerService.DidNotReceiveWithAnyArgs().CopyFile(null);
        }

        [Test]
        public void HandleInputFileImport_FileExistsInInput_Overwrite_CopiesCorrectly()
        {
            // Setup
            var dialogService = Substitute.For<IFileDialogService>();
            const string fileFilter = "imagine this to be a completely valid file filter";
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            const string selectedFilePath = "someSelectedFilePath.txt";

            dialogService.SelectFile(null).ReturnsForAnyArgs(selectedFilePath);
            importerService.IsInInputFolder(selectedFilePath).Returns(false);
            importerService.HasFile(selectedFilePath).Returns(true);
            userInputService.RequestUserInput(null, null).ReturnsForAnyArgs(FileAlreadyExistsChoice.Overwrite);

            var helper = new TimeFrameEditorFileImportHelper(dialogService,
                                                             importerService,
                                                             userInputService);

            // Call
            string result = helper.HandleInputFileImport(fileFilter);

            // Assert
            Assert.That(result, Is.EqualTo(selectedFilePath));
            importerService.CopyFile(selectedFilePath, selectedFilePath);
        }

        [Test]
        public void HandleInputFileImport_FileExistsInInput_Add_CopiesCorrectlyWithNewName()
        {
            // Setup
            var dialogService = Substitute.For<IFileDialogService>();
            const string fileFilter = "imagine this to be a completely valid file filter";
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            const string selectedFilePath = "someSelectedFilePath.txt";
            const string selectedFilePath2 = "someSelectedFilePath_0.txt";
            string absPath = Path.GetFullPath(Path.Combine(".", selectedFilePath));

            dialogService.SelectFile(null).ReturnsForAnyArgs(absPath);
            importerService.IsInInputFolder(selectedFilePath).Returns(false);
            importerService.HasFile(selectedFilePath).Returns(true);
            importerService.HasFile(selectedFilePath2).Returns(true);
            userInputService.RequestUserInput(null, null).ReturnsForAnyArgs(FileAlreadyExistsChoice.Add);

            var helper = new TimeFrameEditorFileImportHelper(dialogService,
                                                             importerService,
                                                             userInputService);
            // Call
            string result = helper.HandleInputFileImport(fileFilter);

            // Assert
            const string expectedSelectedFilePath = "someSelectedFilePath_1.txt";
            Assert.That(result, Is.EqualTo(expectedSelectedFilePath));
            importerService.Received(1).CopyFile(absPath, expectedSelectedFilePath);
        }

        [Test]
        public void HandleInputFileImport_FileExistsInInput_UseExisting_ReturnsRelativePath()
        {
            // Setup
            var dialogService = Substitute.For<IFileDialogService>();
            const string fileFilter = "imagine this to be a completely valid file filter";
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            const string selectedFilePath = "someSelectedFilePath.txt";
            string absPath = Path.GetFullPath(Path.Combine(".", selectedFilePath));

            dialogService.SelectFile(null).ReturnsForAnyArgs(absPath);
            importerService.IsInInputFolder(selectedFilePath).Returns(false);
            importerService.HasFile(selectedFilePath).Returns(true);
            userInputService.RequestUserInput(null, null).ReturnsForAnyArgs(FileAlreadyExistsChoice.UseExisting);

            var helper = new TimeFrameEditorFileImportHelper(dialogService,
                                                             importerService,
                                                             userInputService);
            // Call
            string result = helper.HandleInputFileImport(fileFilter);

            // Assert
            Assert.That(result, Is.EqualTo(selectedFilePath));
            importerService.DidNotReceiveWithAnyArgs().CopyFile(null);
        }

        [Test]
        public void HandleInputFileImport_FileExistsInInput_Cancel_ReturnsNull()
        {
            // Setup
            var dialogService = Substitute.For<IFileDialogService>();
            const string fileFilter = "imagine this to be a completely valid file filter";
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            const string selectedFilePath = "someSelectedFilePath.txt";

            dialogService.SelectFile(null).ReturnsForAnyArgs(selectedFilePath);
            importerService.IsInInputFolder(selectedFilePath).Returns(false);
            importerService.HasFile(selectedFilePath).Returns(true);
            userInputService.RequestUserInput(null, null).ReturnsForAnyArgs(FileAlreadyExistsChoice.Cancel);

            var helper = new TimeFrameEditorFileImportHelper(dialogService,
                                                             importerService,
                                                             userInputService);
            // Call
            string result = helper.HandleInputFileImport(fileFilter);

            // Assert
            Assert.That(result, Is.Null);
            importerService.DidNotReceiveWithAnyArgs().CopyFile(null);
        }

        [Test]
        public void HandleInputFileImport_FileNotExistsAndNotInput_CopiesCorrectlyAndReturnsRelativePath()
        {
            // Setup
            var dialogService = Substitute.For<IFileDialogService>();
            const string fileFilter = "imagine this to be a completely valid file filter";
            var importerService = Substitute.For<IInputFileImporterService>();
            var userInputService = Substitute.For<IRequestUserInputService<FileAlreadyExistsChoice>>();

            const string selectedFilePath = "someSelectedFilePath.txt";
            string absPath = Path.GetFullPath(Path.Combine(".", selectedFilePath));

            dialogService.SelectFile(null).ReturnsForAnyArgs(absPath);
            importerService.IsInInputFolder(selectedFilePath).Returns(false);
            importerService.HasFile(selectedFilePath).Returns(false);

            var helper = new TimeFrameEditorFileImportHelper(dialogService,
                                                             importerService,
                                                             userInputService);
            // Call
            string result = helper.HandleInputFileImport(fileFilter);

            // Assert
            Assert.That(result, Is.EqualTo(selectedFilePath));
            importerService.Received(1).CopyFile(absPath, selectedFilePath);
        }
    }
}