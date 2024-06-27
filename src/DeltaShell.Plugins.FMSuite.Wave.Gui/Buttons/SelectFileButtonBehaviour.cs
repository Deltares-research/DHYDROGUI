using DelftTools.Controls;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using System;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons
{
    /// <summary>
    /// An abstract class defining the behaviour for a select file button.
    /// </summary>
    public abstract class SelectFileButtonBehaviour : IButtonBehaviour
    {
        private readonly IFileDialogService fileDialogService;

        /// <summary>
        /// Initialize a new instance of the <see cref="SelectFileButtonBehaviour"/> class.
        /// </summary>
        /// <param name="fileDialogService"> The file dialog service to select the file with. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="fileDialogService"/> is <c>null</c>.
        /// </exception>
        protected SelectFileButtonBehaviour(IFileDialogService fileDialogService)
        {
            Ensure.NotNull(fileDialogService, nameof(fileDialogService));

            this.fileDialogService = fileDialogService;
        }

        /// <summary>
        /// The file filter for the selection dialog.
        /// </summary>
        protected abstract string FileFilter { get; }

        /// <summary>
        /// Execute the button behaviour.
        /// </summary>
        /// <param name="inputObject"> The input object. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="inputObject"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="inputObject"/> is not a <see cref="WaveModel"/>.
        /// </exception>
        public void Execute(object inputObject)
        {
            Ensure.NotNull(inputObject, nameof(inputObject));

            var waveModel = inputObject as WaveModel;
            if (waveModel == null)
            {
                throw new ArgumentException(string.Format(Resources.Expected_argument_0_to_be_of_type_1_, nameof(inputObject), typeof(WaveModel)));
            }

            var fileDialogOptions = new FileDialogOptions { FileFilter = FileFilter };
            
            string selectedFilePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);
            if (selectedFilePath == null)
            {
                return;
            }

            string fileLocation = selectedFilePath.Replace('\\', '/');

            SetFileLocation(fileLocation, waveModel);
        }

        /// <summary>
        /// Set the file location on the wave model.
        /// </summary>
        /// <param name="fileLocation"> The file location. </param>
        /// <param name="waveModel">The wave model. </param>
        protected abstract void SetFileLocation(string fileLocation, WaveModel waveModel);
    }
}
