using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap;

namespace DeltaShell.NGHS.Common.Gui
{
    public sealed class CoordinateSystemPickerViewModel : INotifyPropertyChanged
    {
        private ICoordinateSystem selectedCoordinateSystem;
        private string filterText;

        public CoordinateSystemPickerViewModel()
        {
            try
            {
                CoordinateSystems = (CollectionView)CollectionViewSource.GetDefaultView(Map.CoordinateSystemFactory.SupportedCoordinateSystems);
                CoordinateSystems.Filter = null;
            }
            catch
            {
                // ignored -> used for designer
            }
        }

        internal Action<ICoordinateSystem> UpdateCoordinateSystemAction { get; set; }

        public ICoordinateSystem SelectedCoordinateSystem
        {
            get { return selectedCoordinateSystem; }
            set
            {
                selectedCoordinateSystem = value;
                OnPropertyChanged();
                UpdateCoordinateSystemAction?.Invoke(selectedCoordinateSystem);
            }
        }

        public CollectionView CoordinateSystems { get; }

        public string FilterText
        {
            get { return filterText; }
            set
            {
                filterText = value?.ToLower();
                CoordinateSystems.Filter = o =>
                    ((ICoordinateSystem)o).Name.ToLower().Contains(filterText) ||
                    ((ICoordinateSystem)o).AuthorityCode.ToString().Contains(filterText);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}