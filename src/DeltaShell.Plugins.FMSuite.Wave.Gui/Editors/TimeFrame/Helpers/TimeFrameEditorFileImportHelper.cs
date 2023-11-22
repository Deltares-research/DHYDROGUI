using System.IO;
using DelftTools.Controls;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Modals.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Enums;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Helpers
{
    /// <summary>
    /// <see cref="TimeFrameEditorFileImportHelper"/> provides the methods for handling the
    /// importing of files related to the time frame editor.
    /// </summary>
    public class TimeFrameEditorFileImportHelper : ITimeFrameEditorFileImportHelper
    {
        private readonly IFileDialogService dialogService;
        private readonly IInputFileImporterService inputFileImporterService;
        private readonly IRequestUserInputService<FileAlreadyExistsChoice> userInputService;

        /// <summary>
        /// Creates a new <see cref="TimeFrameEditorFileImportHelper"/>.
        /// </summary>
        /// <param name="dialogService">The dialog service.</param>
        /// <param name="inputFileImporterService">The input file importer service.</param>
        /// <param name="userInputService">The user input service.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public TimeFrameEditorFileImportHelper(IFileDialogService dialogService,
                                               IInputFileImporterService inputFileImporterService,
                                               IRequestUserInputService<FileAlreadyExistsChoice> userInputService)
        {
            Ensure.NotNull(dialogService, nameof(dialogService));
            Ensure.NotNull(inputFileImporterService, nameof(inputFileImporterService));
            Ensure.NotNull(userInputService, nameof(userInputService));

            this.dialogService = dialogService;
            this.inputFileImporterService = inputFileImporterService;
            this.userInputService = userInputService;
        }

        public string HandleInputFileImport(string fileFilter)
        {
            var dialogOptions = new FileDialogOptions { FileFilter = fileFilter };
            
            string selectedFile = dialogService.ShowOpenFileDialog(dialogOptions);

            if (selectedFile == null)
            {
                return null;
            }

            if (inputFileImporterService.IsInInputFolder(selectedFile))
            {
                return inputFileImporterService.GetRelativePath(selectedFile);
            }

            string fileName = Path.GetFileName(selectedFile);
            if (inputFileImporterService.HasFile(fileName))
            {
                FileAlreadyExistsChoice? result = userInputService.RequestUserInput(
                    $"File '{fileName}' already exists",
                    $"The file '{fileName}' already exists, please choose how to continue:");

                if (!result.HasValue)
                {
                    return null;
                }

                switch (result.Value)
                {
                    case FileAlreadyExistsChoice.Overwrite:
                        break;
                    case FileAlreadyExistsChoice.Add:
                        fileName = GetUniqueFileName(fileName, inputFileImporterService);
                        break;
                    case FileAlreadyExistsChoice.UseExisting:
                        return fileName;
                    case FileAlreadyExistsChoice.Cancel:
                    default:
                        return null;
                }
            }

            inputFileImporterService.CopyFile(selectedFile, fileName);
            return fileName;
        }

        private static string GetUniqueFileName(string fileName,
                                                IInputFileImporterService fileImporterService)
        {
            var template = $"{Path.GetFileNameWithoutExtension(fileName)}_{{0}}{Path.GetExtension(fileName)}";

            string newFileName;
            var i = 0;
            do
            {
                newFileName = string.Format(template, i);
                i += 1;
            } while (fileImporterService.HasFile(newFileName));

            return newFileName;
        }
    }
}
