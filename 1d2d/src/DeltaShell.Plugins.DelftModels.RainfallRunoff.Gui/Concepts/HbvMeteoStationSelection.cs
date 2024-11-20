using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public partial class HbvMeteoStationSelection : UserControl, IRRMeteoStationAwareView, IRRTemperatureStationAwareView
    {
        private readonly CatchmentMeteoStationSelection meteoStationControl;
        private HbvData data;
        private bool useTemperatureStations;
        private IEventedList<string> temperatureStations;

        public HbvMeteoStationSelection()
        {
            meteoStationControl = new CatchmentMeteoStationSelection();
            InitializeComponent();
            splitContainer1.Panel1.Controls.Add(meteoStationControl);
            temperatureStationComboBox.SelectionChangeCommitted += UserChangedStationSelection;
        }

        private void UserChangedStationSelection(object sender, EventArgs e)
        {
            if (temperatureStationComboBox.SelectedIndex != -1)
            {
                data.TemperatureStationName = (string)temperatureStationComboBox.SelectedItem;
            }
        }

        public HbvData Data
        {
            get { return data; }
            set 
            {
                if (data != null)
                {
                    ((INotifyPropertyChanged)data).PropertyChanged -= HbvCatchmentPropertyChanged;
                }
                data = value;
                meteoStationControl.CatchmentModelData = data;
                if (data != null)
                {
                    ((INotifyPropertyChanged)data).PropertyChanged += HbvCatchmentPropertyChanged;
                }
            }
        }

        private void HbvCatchmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Equals(sender, data))
                return;

            if (e.PropertyName == nameof(data.TemperatureStationName))
            {
                FillTemperatureStationControls();
            }
        }

        private void FillTemperatureStationControls()
        {
            temperatureStationComboBox.SelectedIndex =
                temperatureStationComboBox.Items.IndexOf(data.TemperatureStationName);
        }

        public bool UseMeteoStations
        {
            get { return meteoStationControl.UseMeteoStations; }
            set { meteoStationControl.UseMeteoStations = value; }
        }

        public IEventedList<string> MeteoStations
        {
            get { return meteoStationControl.MeteoStations; }
            set { meteoStationControl.MeteoStations = value; }
        }

        public bool UseTemperatureStations
        {
            get { return useTemperatureStations; }
            set
            {
                useTemperatureStations = value;
                temperatureStationComboBox.Enabled = useTemperatureStations;
            }
        }

        public IEventedList<string> TemperatureStations
        { 
            get { return temperatureStations; }
            set
            {
                if (temperatureStations != null)
                {
                    temperatureStations.CollectionChanged -= TemperatureStationsCollectionChanged;
                }
                temperatureStations = value;
                if (temperatureStations != null)
                {
                    temperatureStations.CollectionChanged += TemperatureStationsCollectionChanged;
                    ResetTemperatureStationComboBox();
                }
            }
        }

        private void ResetTemperatureStationComboBox()
        {
            temperatureStationComboBox.Items.Clear();
            temperatureStationComboBox.Items.AddRange(temperatureStations.Cast<object>().ToArray());
            FillTemperatureStationControls();
        }

        private void TemperatureStationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetTemperatureStationComboBox();
        }
    }
}
