using System;
using DelftTools.Controls;
using DelftTools.Utils.Guards;
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

            string selectedFilePath = new FileDialogService().SelectFile(string.Format(Resources.Sp2FileFilter_0_, FileConstants.SpectrumFileExtension));
            if (selectedFilePath != null)
            {
                string fileLocation = selectedFilePath.Replace('\\', '/');
                waveModel.ModelDefinition
                         .BoundaryContainer.FilePathForBoundariesPerFile = fileLocation;
            }
        }
    }
}