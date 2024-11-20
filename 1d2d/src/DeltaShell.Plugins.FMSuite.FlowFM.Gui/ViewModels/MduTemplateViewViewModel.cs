using DelftTools.Controls.Wpf.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Views;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.ViewModels
{
    /// <summary>
    /// ViewModel for the <seealso cref="MduTemplateView"/>. Will set the FilePath where to retrieve the mdu file from and invoke the set commands Browse, Cancel and Import.
    /// </summary>
    public class MduTemplateViewViewModel : INotifyPropertyChanged
    {
        private string filePath;

        /// <summary>
        /// Constructor sets the the set commands as <seealso cref="RelayCommand"/> Browse, Cancel and Import.
        /// </summary>
        public MduTemplateViewViewModel()
        {
            BrowseCommand = new RelayCommand(o =>
            {
                FilePath = GetFilePath?.Invoke();
                if (!string.IsNullOrWhiteSpace(FilePath))
                {
                    ExecuteProjectTemplate?.Invoke(FilePath);
                }
            });
            CancelCommand = new RelayCommand(o => Cancel?.Invoke());
            ImportCommand = new RelayCommand(o => ExecuteProjectTemplate?.Invoke(FilePath), o => !string.IsNullOrWhiteSpace(FilePath));
        }
        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
                OnPropertyChanged();
            }
        }

        public ICommand BrowseCommand { get; }

        public ICommand ImportCommand { get; }

        public ICommand CancelCommand { get; }

        public Action<object> ExecuteProjectTemplate { get; set; }

        public Action Cancel { get; set; }

        public Func<string> GetFilePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}