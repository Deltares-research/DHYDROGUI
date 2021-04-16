using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons
{
    /// <summary>
    /// Class that holds button functionality for selecting communication
    /// files (*.com).
    /// </summary>
    // We will not cover the code in this class with tests, as we cannot
    // automate the behavior in this class.
    public static class SelectComFileButton
    {
        /// <summary>
        /// Opens a file selection dialog and sets the selected file location, if any,
        /// on the COMFile-property of the input wave model.
        /// </summary>
        /// <param name="input"> The input for this action. </param>
        /// <remarks>
        /// The selected file location value will be put on the COMFile-property with
        /// forward slashes as file separators.
        /// We will not cover the code in this class with tests, as we cannot
        /// automate the behavior in this method, due to the OpenFileDialog.
        /// </remarks>
        public static void ButtonAction(object input)
        {
            var waveModel = input as WaveModel;
            if (waveModel == null)
            {
                return;
            }

            string selectedFilePath = new FileDialogService().SelectFile(string.Format(Resources.SelectComFileButton_ButtonAction_Communication_files___0_, FileConstants.ComFileExtension));
            if (selectedFilePath == null)
            {
                return;
            }

            string fileLocation = selectedFilePath.Replace('\\', '/');
            waveModel.ModelDefinition
                     .GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.COMFile)
                     .SetValueAsString(fileLocation);
        }
    }
}