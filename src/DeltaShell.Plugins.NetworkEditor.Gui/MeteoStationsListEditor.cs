using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public partial class MeteoStationsListEditor : UserControl
    {
        private IEventedList<string> data;

        public MeteoStationsListEditor()
        {
            InitializeComponent();
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

        void DataCollectionChanged(object sender, DelftTools.Utils.Collections.NotifyCollectionChangingEventArgs e)
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

            var itemToRemove = stationsList.Items[stationsList.SelectedIndex];
            data.Remove((string) itemToRemove);
        }
    }
}
