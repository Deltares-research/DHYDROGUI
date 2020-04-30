using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using Microsoft.Win32;

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

            var openFileDialog = new OpenFileDialog
            {
                Filter = string.Format(Resources.SelectSp2FileButton_ButtonAction_Spectrum_Files___0_, FileConstants.SpectrumFileExtension),
                Title = Resources.Select_spectrum_file
            };

            {
                bool? result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    string fileLocation = openFileDialog.FileName.Replace('\\', '/');
                    waveModel.ModelDefinition
                             .BoundaryContainer.FilePathForBoundariesPerFile = fileLocation;
                }
            }
        }
    }
}