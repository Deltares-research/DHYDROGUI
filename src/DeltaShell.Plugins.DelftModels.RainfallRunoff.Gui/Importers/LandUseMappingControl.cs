using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Editors;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Importers
{
    public partial class LandUseMappingControl : UserControl
    {
        private DataTable dataTable;
        private IEnumerable<object> landUseCategories;

        public LandUseMappingControl()
        {
            InitializeComponent();
        }

        public IEnumerable<object> LandUseCategories
        {
            get { return landUseCategories; }
            set
            {
                landUseCategories = value;
                SetDataToTable();
            }
        }

        private void SetDataToTable()
        {
            dataTable = new DataTable();
            tableView.Data = dataTable;

            DataColumn landUseColumn = dataTable.Columns.Add("Land use category", typeof (object));
            dataTable.Columns.Add("Polder subtype", typeof (object));
            landUseColumn.ReadOnly = false;

            if (LandUseCategories == null)
                return;

            SetComboBoxEditor();

            foreach (object category in LandUseCategories)
            {
                dataTable.Rows.Add(new[] {category, PolderSubTypes.None});
            }

            tableView.AllowAddNewRow = false;
            tableView.AllowDeleteRow = false;
            tableView.AllowColumnFiltering = false;
            tableView.BestFitColumns();
        }

        private void SetComboBoxEditor()
        {
            tableView.Columns[1].Editor = new ComboBoxTypeEditor {Items = Enum.GetValues(typeof (PolderSubTypes))};
        }

        public IDictionary<object, PolderSubTypes> GetMappingDictionary()
        {
            return dataTable.Rows.OfType<DataRow>().ToDictionary(row => row[0], row => (PolderSubTypes) row[1]);
        }
    }
}