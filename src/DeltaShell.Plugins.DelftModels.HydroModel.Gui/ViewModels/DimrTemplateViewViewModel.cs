using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.ViewModels
{
    public class DimrTemplateViewViewModel : INotifyPropertyChanged
    {
        private string filePath;

        public DimrTemplateViewViewModel()
        {
            BrowseCommand = new RelayCommand(o => FilePath = GetFilePath());
            CancelCommand = new RelayCommand(o => Cancel());
            ImportCommand = new RelayCommand(o => ExecuteProjectTemplate(FilePath), o => !string.IsNullOrWhiteSpace(FilePath));
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

        public ICommand BrowseCommand { get; set; }

        public ICommand ImportCommand { get; set; }

        public ICommand CancelCommand { get; set; }

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