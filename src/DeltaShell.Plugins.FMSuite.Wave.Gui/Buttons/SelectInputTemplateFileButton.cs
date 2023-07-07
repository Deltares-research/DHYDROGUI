using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons
{
    /// <summary>
    /// Class that holds button functionality for selecting the template SWAN input file.
    /// </summary>
    // We will not cover the code in this class with tests, as we cannot
    // automate the behavior in this class.
    public class SelectInputTemplateFileButton : SelectFileButtonBehaviour
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="SelectInputTemplateFileButton"/> class.
        /// </summary>
        /// <param name="fileDialogService"> The file dialog service to select the file with. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fileDialogService"/> is <c>null</c>.
        /// </exception>
        public SelectInputTemplateFileButton(IFileDialogService fileDialogService) : base(fileDialogService) { }

        protected override string FileFilter => Resources.InputTemplateFileFilter;

        protected override void SetFileLocation(string fileLocation, WaveModel waveModel)
        {
            waveModel.ModelDefinition
                     .GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.InputTemplateFile)
                     .SetValueAsString(fileLocation);
        }
    }
}