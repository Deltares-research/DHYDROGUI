using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons
{
    /// <summary>
    /// Class that holds button functionality for selecting spectrum
    /// files (*.sp2).
    /// </summary>
    public static class SelectSp2FileButton
    {
        /// <summary>
        /// Opens a file selection dialog and sets the selected file location, if any,
        /// in the boundary container.
        /// </summary>
        /// <param name="input"> The input for this action. </param>
        /// <remarks>
        /// The selected file location value will be set with
        /// forward slashes as file separators.
        /// </remarks>
        public static void ButtonAction(object input)
        {
            var waveModel = input as WaveModel;
            if (waveModel == null)
            {
                return;
            }

            string selectedFilePath = new FileDialogService().SelectFile(string.Format(Resources.SelectSp2FileButton_ButtonAction_Spectrum_Files___0_, FileConstants.SpectrumFileExtension));
            if (selectedFilePath != null)
            {
                string fileLocation = selectedFilePath.Replace('\\', '/');
                waveModel.ModelDefinition
                         .BoundaryContainer.FilePathForBoundariesPerFile = fileLocation;
            }
        }
    }
}