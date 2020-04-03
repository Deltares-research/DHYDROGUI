using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using Microsoft.Win32;

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
            var openFileDialog = new OpenFileDialog
            {
                Filter = Resources.Spectrum_Files_Filter,
                Title = Resources.Select_spectrum_file
            };
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
            }
        }
    }
}