using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    public partial class InputSelectionDialog : Form
    {
        private List<IFeature> features;

        public InputSelectionDialog()
        {
            InitializeComponent();

            tableViewLocations.ColumnAutoWidth = true;
            tableViewLocations.MultiSelect = false;
            tableViewLocations.ReadOnlyCellBackColor = SystemColors.Window;
            tableViewLocations.AddUnboundColumn("Name", typeof(string), 0);
            tableViewLocations.AddUnboundColumn("Type", typeof(string), 1);
            tableViewLocations.UnboundColumnData = TableViewLocationsOnUnboundColumnData;
            tableViewLocations.FocusedRowChanged += TableViewLocationsSelectionChanged;

            tableViewDataItems.ColumnAutoWidth = true;
            tableViewDataItems.MultiSelect = false;
            tableViewDataItems.ReadOnlyCellBackColor = SystemColors.Window;
            tableViewDataItems.AddUnboundColumn("Parameter", typeof(string), 0);
            tableViewDataItems.UnboundColumnData = TableViewDataItemsUnboundColumnData;
            tableViewDataItems.FocusedRowChanged += TableViewDataItemsSelectionChanged;
        }

        public List<IFeature> Features
        {
            get
            {
                return features;
            }
            set
            {
                features = value;

                tableViewLocations.Data = features;
                tableViewLocations.BestFitColumns();
            }
        }

        public Func<IFeature, IList<IDataItem>> GetDataItemsForFeature { get; set; }

        public IDataItem SelectedDataItem
        {
            get
            {
                return tableViewDataItems.CurrentFocusedRowObject as IDataItem;
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void TableViewDataItemsSelectionChanged(object sender, EventArgs e)
        {
            buttonOk.Enabled = SelectedDataItem != null;
        }

        private void TableViewLocationsSelectionChanged(object sender, EventArgs e)
        {
            UpdateDataItemsTableForSelectedLocation();
        }

        private void UpdateDataItemsTableForSelectedLocation()
        {
            var feature = tableViewLocations.CurrentFocusedRowObject as IFeature;
            if (feature == null || GetDataItemsForFeature == null)
            {
                return;
            }

            tableViewDataItems.Data = GetDataItemsForFeature(feature);
            tableViewDataItems.BestFitColumns();
            buttonOk.Enabled = SelectedDataItem != null;
        }

        private object TableViewDataItemsUnboundColumnData(int column, int dataSourceIndex, bool isGetData, bool isSetData, object value)
        {
            if (dataSourceIndex < 0 || dataSourceIndex >= ((IList<IDataItem>) tableViewDataItems.Data).Count)
            {
                return null;
            }

            IDataItem dataItem = ((IList<IDataItem>) tableViewDataItems.Data)[dataSourceIndex];
            if (column != 0)
            {
                return null;
            }

            return dataItem.GetParameterName();
        }

        private object TableViewLocationsOnUnboundColumnData(int column, int dataSourceIndex, bool isGetData, bool isSetData, object value)
        {
            if (dataSourceIndex < 0 || dataSourceIndex >= Features.Count)
            {
                return null;
            }

            IFeature feature = Features[dataSourceIndex];

            if (column == 0)
            {
                return ((INameable) feature).Name;
            }

            if (column == 1)
            {
                return feature.GetEntityType().Name;
            }

            return null;
        }

        private void InputSelectionDialogLoad(object sender, EventArgs e)
        {
            UpdateDataItemsTableForSelectedLocation();
        }
    }
}