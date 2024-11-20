using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public partial class CatchmentMeteoStationSelection : UserControl, IRRMeteoStationAwareView
    {
        private IEventedList<string> meteoStations;
        private CatchmentModelData catchmentModelData;
        private bool useMeteoStations;

        public CatchmentMeteoStationSelection()
        {
            InitializeComponent();
            stationComboBox.SelectionChangeCommitted += UserChangedStationSelection;
        }

        void UserChangedStationSelection(object sender, System.EventArgs e)
        {
            if (stationComboBox.SelectedIndex != -1)
            {
                CatchmentModelData.MeteoStationName = (string) stationComboBox.SelectedItem;
            }
        }
        
        private void TxtAreaAdjustmentFactorValidated(object sender, System.EventArgs e)
        {
            double aaf;
            if (double.TryParse(txtAreaAdjustmentFactor.Text, out aaf))
            {
                catchmentModelData.AreaAdjustmentFactor = aaf;
            }
        }

        public CatchmentModelData CatchmentModelData
        {
            get { return catchmentModelData; }
            set
            {
                if (catchmentModelData != null)
                {
                    ((INotifyPropertyChanged)catchmentModelData).PropertyChanged -= CatchmentPropertyChanged;
                }
                catchmentModelData = value;
                if (catchmentModelData != null)
                {
                    ((INotifyPropertyChanged)catchmentModelData).PropertyChanged += CatchmentPropertyChanged;
                }
            }
        }

        public bool UseMeteoStations
        {
            get { return useMeteoStations; }
            set
            {
                useMeteoStations = value;
                stationComboBox.Enabled = useMeteoStations;
                txtAreaAdjustmentFactor.Enabled = useMeteoStations;
            }
        }

        public IEventedList<string> MeteoStations
        {
            get { return meteoStations; }
            set
            {
                if (meteoStations != null)
                {
                    meteoStations.CollectionChanged -= MeteoStationsCollectionChanged;
                }
                meteoStations = value;
                if (meteoStations != null)
                {
                    meteoStations.CollectionChanged += MeteoStationsCollectionChanged;
                    ResetStationComboBox();
                }
            }
        }

        void CatchmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Equals(sender, catchmentModelData))
                return;

            if (e.PropertyName == nameof(catchmentModelData.AreaAdjustmentFactor) ||
                e.PropertyName == nameof(catchmentModelData.MeteoStationName))
            {
                FillControls();
            }
        }

        void MeteoStationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetStationComboBox();
        }

        private void ResetStationComboBox()
        {
            stationComboBox.Items.Clear();
            stationComboBox.Items.AddRange(meteoStations.Cast<object>().ToArray());
            FillControls();
        }

        private void FillControls()
        {
            txtAreaAdjustmentFactor.Text = string.Format("{0:0.000}", catchmentModelData.AreaAdjustmentFactor);
            stationComboBox.SelectedIndex = stationComboBox.Items.IndexOf(CatchmentModelData.MeteoStationName);
        }
    }
}
