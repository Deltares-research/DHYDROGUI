using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public partial class MeteoStationsListEditor : UserControl
    {
        private IEventedList<string> data;
        private bool settingSectionFromOutside;

        public MeteoStationsListEditor()
        {
            InitializeComponent();
            if (stationsList != null)
            {
                stationsList.SelectionMode = SelectionMode.MultiExtended;
                stationsList.SelectedIndexChanged += stationsList_SelectedIndexChanged;
            }
        }

        public IEventedList<string> Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    data.CollectionChanged -= DataCollectionChanged;
                }
                data = value;
                if (data != null)
                {
                    data.CollectionChanged += DataCollectionChanged;
                    FillStationsList();
                }
            }
        }

        public event EventHandler<MeteoStationsSelectedEventArgs> MeteoStationsSelected;

        void DataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!Equals(sender, data))
                return;
            
            var oldSelection = stationsList.SelectedIndex;
            FillStationsList();
            stationsList.SelectedIndex = Math.Min(oldSelection, stationsList.Items.Count - 1);
        }

        private void FillStationsList()
        {
            stationsList.Items.Clear();
            stationsList.Items.AddRange(data.Cast<object>().ToArray());
        }

        private void BtnAddStationClick(object sender, System.EventArgs e)
        {
            var newStationName = txtNewStationName.Text;
            
            if (string.IsNullOrEmpty(newStationName)) 
                return;

            txtNewStationName.Text = "";

            if (!data.Contains(newStationName))
                data.Add(newStationName);
        }

        private void BtnRemoveStationClick(object sender, System.EventArgs e)
        {
            if (stationsList.SelectedIndex < 0) 
                return;

            var itemsToRemove = stationsList.SelectedItems.Cast<object>().Select(o => o.ToString()).ToArray();
            foreach (var itemToRemove in itemsToRemove)
            {
                data.Remove(itemToRemove);
            }
        }

        private void stationsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(settingSectionFromOutside || stationsList?.SelectedItems == null) return;
            var selectedMeteoStationEventArgs = new MeteoStationsSelectedEventArgs()
            {
                SelectedMeteoStations = stationsList.SelectedItems.Cast<object>().Select(o => o.ToString()).ToArray()
            };
            MeteoStationsSelected?.Invoke(sender, selectedMeteoStationEventArgs);
        }

        public void SetSelection(IEnumerable<string> selection)
        {
            if (selection != null && stationsList?.Items != null && data != null)
            {
                settingSectionFromOutside = true;
                stationsList.ClearSelected();
                selection.Select(s => stationsList.Items.IndexOf(data.First(d => d.Equals(s)))).Where(i => i >= 0)
                    .ToList().ForEach(i => stationsList.SetSelected(i, true));
                settingSectionFromOutside = false;
            }
        }


    }

    public class MeteoStationsSelectedEventArgs
    {
        public string[] SelectedMeteoStations { get; set; }
    }
}