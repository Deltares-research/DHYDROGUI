using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels
{
    /// <summary>
    /// <see cref="MeteoStationViewModel"/> implements the <see cref="IMeteoStationsListViewModel"/>.
    /// </summary>
    public sealed class MeteoStationsListViewModel : IMeteoStationsListViewModel
    {
        private readonly IEventedList<string> domainStations;
        private string newStationName;

        /// <summary>
        /// Creates a new <see cref="MeteoStationViewModel"/> with the given <paramref name="stations"/> source.
        /// </summary>
        /// <param name="stations"></param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="stations"/> is <c>null</c>.
        /// </exception>
        public MeteoStationsListViewModel(IEventedList<string> stations)
        {
            Ensure.NotNull(stations, nameof(stations));

            domainStations = stations;
            domainStations.CollectionChanged += OnStationsChanged;

            foreach (string domainStation in domainStations)
            {
                Stations.Add(CreateMeteoStationViewModel(domainStation));
            }

            NewStationName = string.Empty;

            AddStationCommand = new RelayCommand(OnAddStation, CanAddStation);
            RemoveStationsCommand = new RelayCommand(OnRemoveStation, CanRemoveStation);
        }

        private void OnStationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RemoveStations(e.OldItems);
            AddStations(e.NewItems);
        }

        private void RemoveStations(IList oldItems)
        {
            if (oldItems == null || oldItems.Count <= 0) return;

            // Iterate first to ensure no funky business when removing stations.
            MeteoStationViewModel[] stationsToRemove =
                Stations.Where(x => oldItems.Contains(x.Name))
                        .ToArray();

            foreach (MeteoStationViewModel station in stationsToRemove)
            {
                station.PropertyChanged -= OnMeteoStationPropertyChanged;

                if (station.IsSelected)
                    SelectedStations.Remove(station);

                Stations.Remove(station);
            }
        }

        private void AddStations(ICollection newItems)
        {
            if (newItems == null || newItems.Count <= 0) return;

            // Iterate first to ensure no funky business when adding stations.
            MeteoStationViewModel[] stationsToAdd =
                newItems.Cast<string>()
                        .Where(name => Stations.All(s => s.Name != name))
                        .Select(CreateMeteoStationViewModel)
                        .ToArray();
            foreach (MeteoStationViewModel station in stationsToAdd)
            {
                Stations.Add(station);
            }
        }

        private MeteoStationViewModel CreateMeteoStationViewModel(string name)
        {
            var meteoStation = new MeteoStationViewModel
            {
                Name = name,
                IsSelected = false,
            };
            meteoStation.PropertyChanged += OnMeteoStationPropertyChanged;
            return meteoStation;
        }

        private void OnMeteoStationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is MeteoStationViewModel meteoStationViewModel &&
                  e.PropertyName == nameof(MeteoStationViewModel.IsSelected))) return;

            if (meteoStationViewModel.IsSelected)
                SelectedStations.Add(meteoStationViewModel);
            else
                SelectedStations.Remove(meteoStationViewModel);
        }

        public ICommand AddStationCommand { get; }

        public ICommand RemoveStationsCommand { get; }

        public ObservableCollection<MeteoStationViewModel> Stations { get; } = 
            new ObservableCollection<MeteoStationViewModel>();

        public ObservableCollection<MeteoStationViewModel> SelectedStations { get; } =
            new ObservableCollection<MeteoStationViewModel>();

        public string NewStationName
        {
            get => newStationName;
            set
            {
                if (value == newStationName) return;
                newStationName = value;
                OnPropertyChanged();
            }
        }

        public void SetSelection(ISet<string> selectedStations)
        {
            foreach (MeteoStationViewModel station in Stations)
            {
                station.IsSelected = selectedStations.Contains(station.Name);
            }
        }

        private void OnAddStation(object _)
        {
            domainStations.Add(newStationName);
            // Add logic to update domain model.
            NewStationName = string.Empty;
        }

        // Currently, we do not use a parameter because the RelayCommand
        // will not work with null.
        private bool CanAddStation(object _)
        {
            return !string.IsNullOrWhiteSpace(NewStationName) &&
                   Stations.All(x => x.Name != NewStationName);
        }

        // Currently, we do not use a parameter because the RelayCommand
        // will not work with null and value types.
        private void OnRemoveStation(object _)
        {
            // Create a copy to ensure no funky enumeration business.
            MeteoStationViewModel[] selectedStations = SelectedStations.ToArray();
            foreach (MeteoStationViewModel selectedViewModel in selectedStations)
                domainStations.Remove(selectedViewModel.Name);
        }

        private bool CanRemoveStation(object _)
        {
            return Stations.Any(x => x.IsSelected);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            domainStations.CollectionChanged -= OnStationsChanged;
        }
    }
}