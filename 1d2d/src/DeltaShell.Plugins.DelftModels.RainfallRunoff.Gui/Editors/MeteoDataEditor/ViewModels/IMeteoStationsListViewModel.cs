using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels
{
    /// <summary>
    /// <see cref="IMeteoStationsListViewModel"/> defines the properties used in the
    /// <see cref="Views.MeteoStationsListView"/>.
    /// </summary>
    public interface IMeteoStationsListViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets the <see cref="ICommand"/> to add a new station
        /// with the <see cref="NewStationName"/> as name.
        /// </summary>
        ICommand AddStationCommand { get; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to remove the currently selected
        /// stations from this <see cref="IMeteoStationsListViewModel"/>.
        /// </summary>
        ICommand RemoveStationsCommand { get; }

        /// <summary>
        /// Gets the Stations currently in the model.
        /// </summary>
        ObservableCollection<MeteoStationViewModel> Stations { get; }

        /// <summary>
        /// Gets the currently selected stations.
        /// </summary>
        ObservableCollection<MeteoStationViewModel> SelectedStations { get; }

        /// <summary>
        /// Gets the currently written new station name used to construct stations.
        /// </summary>
        string NewStationName { get; set; }

        /// <summary>
        /// Set the current selection to the stations with the names in <paramref name="selectedStations"/>.
        /// </summary>
        /// <param name="selectedStations"></param>
        void SetSelection(ISet<string> selectedStations);
    }
}