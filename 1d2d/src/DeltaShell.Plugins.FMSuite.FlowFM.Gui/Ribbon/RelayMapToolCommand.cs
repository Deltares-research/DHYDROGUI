using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    public class RelayMapToolCommand : ICommand, INotifyPropertyChanged
    {
        public MapToolCommand MapToolCommand { get; set; }

        public bool IsActive
        {
            get { return MapToolCommand.Checked; }
            set { MapToolCommand.Checked = value;}
        }

        public bool CanExecute(object parameter)
        {
            return MapToolCommand.Enabled;
        }

        public void Execute(object parameter)
        {
            MapToolCommand.Execute(parameter);
        }

        public void Refresh()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(IsActive));
        }

        public event EventHandler CanExecuteChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}