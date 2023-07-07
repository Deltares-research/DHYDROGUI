using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons
{
    /// <summary>
    /// Class that holds button functionality for selecting spectrum
    /// files (*.sp2).
    /// </summary>
    public class SelectSp2FileButton : SelectFileButtonBehaviour
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="SelectSp2FileButton"/> class.
        /// </summary>
        /// <param name="fileDialogService"> The file dialog service to select the file with. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fileDialogService"/> is <c>null</c>.
        /// </exception>
        public SelectSp2FileButton(IFileDialogService fileDialogService) : base(fileDialogService) { }

        protected override string FileFilter => string.Format(Resources.Sp2FileFilter_0_, FileConstants.SpectrumFileExtension);

        protected override void SetFileLocation(string fileLocation, WaveModel waveModel)
        {
            waveModel.ModelDefinition.BoundaryContainer.FilePathForBoundariesPerFile = fileLocation;
        }
    }
}