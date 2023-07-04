using System;
using DelftTools.Controls;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons
{
    /// <summary>
    /// Class that holds button functionality for selecting the template SWAN input file.
    /// </summary>
    // We will not cover the code in this class with tests, as we cannot
    // automate the behavior in this class.
    public static class SelectInputTemplateFileButton
    {
        /// <summary>
        /// Opens a file selection dialog and sets the selected file location, if any,
        /// on the COMFile-property of the input wave model.
        /// </summary>
        /// <param name="input"> The input for this action. </param>
        /// <remarks>
        /// The selected file location value will be put on the INPUTTemplateFile-property with
        /// forward slashes as file separators.
        /// We will not cover the code in this class with tests, as we cannot
        /// automate the behavior in this method, due to the OpenFileDialog.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="input"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="input"/> is not a <see cref="WaveModel"/>.
        /// </exception>
        public static void ButtonAction(object input)
        {
            Ensure.NotNull(input, nameof(input));
            
            var waveModel = input as WaveModel;
            if (waveModel == null)
            {
                throw new ArgumentException(string.Format(Resources.Expected_argument_0_to_be_of_type_1_, nameof(input), typeof(WaveModel)));
            }

            string selectedFilePath = new FileDialogService().SelectFile(Resources.InputTemplateFileFilter);
            if (selectedFilePath == null)
            {
                return;
            }

            string fileLocation = selectedFilePath.Replace('\\', '/');
            waveModel.ModelDefinition
                     .GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.InputTemplateFile)
                     .SetValueAsString(fileLocation);
        }
    }
}