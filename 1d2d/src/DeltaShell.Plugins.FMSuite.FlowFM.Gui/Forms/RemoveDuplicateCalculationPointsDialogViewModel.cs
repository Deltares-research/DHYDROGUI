using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class RemoveDuplicateCalculationPointsDialogViewModel : INotifyPropertyChanged
    {
        private IEnumerable<IGrouping<Coordinate, INetworkLocation>> duplicateNetworkLocationsByCoordinate;
        private ICollection<DuplicateCalculationPointsViewModel> duplicateCalculationPointsViewModels;
        private IDiscretization networkDiscretization;
        public ICollection<DuplicateCalculationPointsViewModel> DuplicateCalculationPointsViewModels
        {
            get { return duplicateCalculationPointsViewModels; }
            set
            {
                duplicateCalculationPointsViewModels = value;
                OnPropertyChanged();
            }
        }
        public IDiscretization NetworkDiscretization
        {
            get { return networkDiscretization; }
            set
            {
                networkDiscretization = value;
                OnPropertyChanged();
            }
        }
        public RemoveDuplicateCalculationPointsDialogViewModel()
        {
            RemoveDuplicateCalculationPointsCommand = new RelayCommand(RemoveDuplicateCalculationPoints);
        }

        public ICommand RemoveDuplicateCalculationPointsCommand { get; set; }

        public Action AfterFix { get; set; }

        public IEnumerable<IGrouping<Coordinate, INetworkLocation>> DuplicateLocationsByCoordinate
        {
            get { return duplicateNetworkLocationsByCoordinate; }
            set
            {
                duplicateNetworkLocationsByCoordinate = value;
                DuplicateCalculationPointsViewModels = duplicateNetworkLocationsByCoordinate?.Select(dnlbc => new DuplicateCalculationPointsViewModel(dnlbc.Key, dnlbc.First(), dnlbc.Skip(1).ToList())).ToList();
            }
        }
        
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RemoveDuplicateCalculationPoints(object o)
        {
            DuplicateCalculationPointsViewModels.ForEach(vm =>
            {
                foreach (var location in vm.DuplicateNetworkLocations)
                {
                    NetworkDiscretization.Locations.Values.Remove(location);
                }
            });
            AfterFix?.Invoke();
        }
    }
}