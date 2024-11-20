using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Controls.Swf.Table;
using DeltaShell.Plugins.NetworkEditor.Import;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    public partial class DefineMappingTableWizardPage : UserControl, IWizardPage
    {
        private const int IndexMappingColumn = 2;
        private const int IndexItems = 6;
        private const int IndexPropertyMapping = 7;
        
        private readonly DataTable dataTable;
        private readonly TableView tableView;

        public DefineMappingTableWizardPage()
        {
            InitializeComponent();
            
            dataTable = new DataTable();
            dataTable.RowChanged += dataTable_RowChanged;
            SetDataTableColumns();

            tableView = new TableView
                {
                    Dock = DockStyle.Fill, 
                    Data = dataTable
                };

            tableView.FocusedRowChanged += TableViewFocusedRowChanged;
            tableView.ReadOnlyCellFilter += ReadOnlyCellFilter;
            tableView.DisplayCellFilter += DisplayCellFilter;
			
            Controls.Add(tableView);

            SetTableViewProperties();

            VisibleChanged += DefineMappingTableWizardPageVisibleChanged;
        }

        private void TableViewFocusedRowChanged(object sender, EventArgs e)
        {
            var dataRowView = tableView.CurrentFocusedRowObject as DataRowView;
            if (dataRowView == null) return;

            var comboBoxEditor = new ComboBoxTypeEditor
                {
                    ItemsMandatory = false,
                    Items = (dataRowView.Row.ItemArray[IndexItems] as IEnumerable<MappingColumn>)?.Where(a=> a.Alias != null)
                };

            tableView.Columns[IndexMappingColumn].Editor = comboBoxEditor;
        }

        private bool DisplayCellFilter(TableViewCellStyle tableViewCellStyle)
        {
            if (tableViewCellStyle.RowIndex < 0)
            {
                return false;
            }

            var displayText = tableView.GetCellDisplayText(tableViewCellStyle.RowIndex,0);

            if (displayText != "")
            {
                tableViewCellStyle.ForeColor = Color.Black;
                tableViewCellStyle.BackColor = Color.FromArgb(235, 234, 219);
                return true;
            }

            return false;
        }

        private bool ReadOnlyCellFilter(TableViewCell tableViewCell)
        {
            return tableViewCell.Column.AbsoluteIndex != IndexMappingColumn;
        }

        public bool CanFinish()
        {
            return true;
        }

        public bool CanDoNext()
        {
            return true;
        }

        public bool CanDoPrevious()
        {
            return true;
        }

        public HydroRegionFromGisImporter HydroRegionFromGisImporter { get; set; }

        private void DefineMappingTableWizardPageVisibleChanged(object sender, EventArgs e)
        {
            if (!Visible) return;

            SetDataTableToTableView();
            SetTableViewProperties();
        }

        private void SetTableViewProperties()
        {
            tableView.AllowAddNewRow = false;
            tableView.AllowDeleteRow = false;
            tableView.AllowColumnFiltering = false;
            tableView.AllowColumnSorting = false;
            tableView.Columns[IndexItems].Visible = false;
            tableView.Columns[IndexPropertyMapping].Visible = false;
            tableView.ReadOnlyCellBackColor = Color.WhiteSmoke;
            tableView.ReadOnlyCellForeColor = Color.Black;

            tableView.BestFitColumns();
        }

        private void SetDataTableColumns()
        {
            dataTable.Columns.Add(new DataColumn("Network feature", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Property", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Mapping column", typeof(object)));
            dataTable.Columns.Add(new DataColumn("Required", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Unique", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Unit", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Items", typeof(object)));
            dataTable.Columns.Add(new DataColumn("PropertyMapping", typeof(object)));
        }

        private void SetDataTableToTableView()
        {
            if (DesignMode)
                return;

            DataRow row;
            bool IsFeatureRow;
            List<MappingColumn> lstMappingColumns;

            dataTable.Rows.Clear();

            foreach (var importer in HydroRegionFromGisImporter.FeatureFromGisImporters)
            {
                IsFeatureRow = true;
                foreach (var propertyMapping in importer.FeatureFromGisImporterSettings.PropertiesMapping)
                {
                    row = dataTable.NewRow();

                    if(IsFeatureRow)
                    {
                        IsFeatureRow = false;
                        row[0] = importer.Name;
                    }
                    else
                    {
                        row[0] = "";
                    }

                    row[1] = propertyMapping.PropertyName;

                    lstMappingColumns = new List<MappingColumn>();
                    lstMappingColumns.AddRange(importer.PossibleMappingColumns);
                    if (!propertyMapping.IsRequired)
                    {
                        lstMappingColumns.Insert(0, new MappingColumn(null,null));
                    }
                    if (propertyMapping.MappingColumn.Alias == null)
                    {
                        propertyMapping.MappingColumn = (lstMappingColumns.Count == 0)
                                                            ? new MappingColumn(null, null)
                                                            : lstMappingColumns.First();
                    }
                    row[2] = propertyMapping.MappingColumn;
                    row[3] = propertyMapping.IsRequired ? "Yes" : "No";
                    row[4] = propertyMapping.IsUnique ? "Yes" : "No";
                    row[5] = propertyMapping.PropertyUnit;
                    row[6] = lstMappingColumns;
                    row[7] = propertyMapping;

                    dataTable.Rows.Add(row);
                }
            }
        }

        private void dataTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action != DataRowAction.Change) return;

            var dataRowValues = e.Row.ItemArray;

            var mappingColumn = (MappingColumn)dataRowValues[IndexMappingColumn];
            var propertyMapping = (PropertyMapping) dataRowValues[IndexPropertyMapping];

            propertyMapping.MappingColumn = mappingColumn;
        }
    }
}
