using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Controls.Wpf.Services;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using ICommand = System.Windows.Input.ICommand;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="FileBasedParametersViewModel"/> defines the view model for the FileBasedParametersView.
    /// </summary>
    public class FileBasedParametersViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="FileBasedParametersViewModel"/>.
        /// </summary>
        /// <param name="parameters">The observed file based parameters.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>.
        /// </exception>
        public FileBasedParametersViewModel(FileBasedParameters parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ObservedParameters = parameters;

            SelectFileCommand = new RelayCommand(SelectFile);
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public FileBasedParameters ObservedParameters { get; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        public string FilePath
        {
            get => ObservedParameters.FilePath;
            set
            {
                ObservedParameters.FilePath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the command to execute when selecting a file.
        /// </summary>
        public ICommand SelectFileCommand { get; }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SelectFile(object obj)
        {
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions { FileFilter = Resources.Spectrum_Files_Filter };
            
            string selectedFilePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);
            if (selectedFilePath != null)
            {
                FilePath = selectedFilePath;
            }
        }
    }
}