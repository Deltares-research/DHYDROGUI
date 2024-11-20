using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels
{
    /// <summary>
    /// <see cref="MeteoStationViewModel"/> defines the view model for a single
    /// meteo station in the <see cref="Views.MeteoStationsListView"/>.
    /// </summary>
    public sealed class MeteoStationViewModel : INotifyPropertyChanged
    {
        private string name;
        private bool isSelected;

        /// <summary>
        /// Gets or sets the <see cref="Name"/> of this meteo station.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (Name == value) return;
                
                name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether this meteo station is currently select.d
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (IsSelected == value) return;

                isSelected = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}