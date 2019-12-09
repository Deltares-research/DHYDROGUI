using System.Drawing;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons
{
    /// <summary>
    /// Class that holds button functionality for selecting communication
    /// files (*.com).
    /// </summary>
    /// <remarks>
    /// We will not cover the code in this class with tests, as we cannot
    /// automate the behavior in this class.
    /// </remarks>
    public static class SelectComFileButton
    {
        /// <summary>
        /// The image for the button.
        /// </summary>
        public static readonly Bitmap ButtonImage = Resources.folder;

        /// <summary>
        /// Opens a file selection dialog and sets the selected file location, if any,
        /// on the COMFile-property of the input wave model.
        /// </summary>
        /// <param name="input"> The input for this action. </param>
        /// /// <remarks>
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

            using (var fileDialog = new OpenFileDialog
            {
                Filter = Resources.SelectComFileButton_ButtonAction_FileDialogFilter
            })
            {
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    waveModel.ModelDefinition
                             .GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.COMFile)
                             .SetValueAsString(fileDialog.FileName);
                }
            }
        }
    }
}