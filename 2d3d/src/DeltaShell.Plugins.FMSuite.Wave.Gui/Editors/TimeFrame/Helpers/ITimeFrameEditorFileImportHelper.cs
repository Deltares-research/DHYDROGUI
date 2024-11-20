namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Helpers
{
    /// <summary>
    /// <see cref="ITimeFrameEditorFileImportHelper"/> defines the methods for handling the
    /// importing of files related to the time frame editor.
    /// </summary>
    public interface ITimeFrameEditorFileImportHelper
    {
        /// <summary>
        /// Handles the input file import.
        /// </summary>
        /// <param name="fileFilter">A WPF file filter specifying the type of files presented to the user.</param>
        /// <returns>
        /// If a file was imported or an existing input file is selected, the path relative to the input folder,
        /// else <c>null</c>.
        /// </returns>
        string HandleInputFileImport(string fileFilter);
    }
}