using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Controls.Swf.Table;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.DataSetManager
{
    public partial class DataTableManagerView : UserControl, IView
    {
        private DataTableManager dataTableManager;
        private int dataFileColumnIndex;
        private int substanceColumnIndex;
        private int upButtonColumnIndex;
        private int downButtonColumnIndex;

        public DataTableManagerView()
        {
            InitializeComponent();
            InitTableView();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTableManager DataTableManager
        {
            get => dataTableManager;
            set
            {
                tableView1.Data = null;

                dataTableManager = value;

                if (dataTableManager != null)
                {
                    tableView1.Data = dataTableManager.DataTables;
                    tableView1.BestFitColumns();
                    tableView1.Columns[upButtonColumnIndex].Width = 50;
                    tableView1.Columns[downButtonColumnIndex].Width = 50;
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Data
        {
            get => DataTableManager;
            set => DataTableManager = (DataTableManager) value;
        }

        public Image Image { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        private void InitTableView()
        {
            tableView1.AddColumn("Name", "Name", false, 100, typeof(string));
            tableView1.AddColumn("IsEnabled", "Enabled", false, 50, typeof(bool));

            dataFileColumnIndex = tableView1.AddUnboundColumn("Data file", typeof(string));
            substanceColumnIndex = tableView1.AddUnboundColumn("Substance use for file", typeof(string));

            var upButtonEditor = new ButtonTypeEditor
            {
                Tooltip = "Move up",
                Image = Resources.arrow_090_medium,
                ButtonClickAction = () => MoveDataTable(true),
                HideOnReadOnly = true
            };

            var downButtonEditor = new ButtonTypeEditor
            {
                Tooltip = "Move down",
                Image = Resources.arrow_270_medium,
                ButtonClickAction = () => MoveDataTable(false),
                HideOnReadOnly = true
            };

            upButtonColumnIndex = tableView1.AddUnboundColumn(" ", typeof(string), -1, upButtonEditor);
            downButtonColumnIndex = tableView1.AddUnboundColumn(" ", typeof(string), -1, downButtonEditor);

            tableView1.UnboundColumnData += UnboundColumnData;
            tableView1.ReadOnlyCellFilter += ReadOnlyCellFilter;
            tableView1.DisplayCellFilter += DisplayCellFilter;
        }

        private bool DisplayCellFilter(TableViewCellStyle tableViewCellStyle)
        {
            var dataTable = tableView1.GetRowObjectAt(tableViewCellStyle.RowIndex) as DataTable;
            if (dataTable == null)
            {
                return false;
            }

            if (dataTable.IsEnabled)
            {
                return false;
            }

            Color disabledBackGroundColor = Color.Wheat;

            tableViewCellStyle.BackColor = IsReadOnlyColumn(tableViewCellStyle.Column.AbsoluteIndex)
                                               ? disabledBackGroundColor
                                               : Color.FromArgb(125, disabledBackGroundColor);

            return true;
        }

        private bool ReadOnlyCellFilter(TableViewCell tableViewCell)
        {
            int rowIndex = tableViewCell.RowIndex;
            int absoluteIndex = tableViewCell.Column.AbsoluteIndex;

            if (absoluteIndex == upButtonColumnIndex && rowIndex == 0 ||
                absoluteIndex == downButtonColumnIndex && rowIndex == tableView1.RowCount - 1)
            {
                return true;
            }

            return IsReadOnlyColumn(absoluteIndex);
        }

        private bool IsReadOnlyColumn(int columnIndex)
        {
            int[] readOnlyColumns = new[]
            {
                dataFileColumnIndex,
                substanceColumnIndex
            };
            return readOnlyColumns.Contains(columnIndex);
        }

        private object UnboundColumnData(int columnIndex, int rowIndex, bool isGet, bool isSet, object value)
        {
            int[] unboundColumns = new[]
            {
                dataFileColumnIndex,
                substanceColumnIndex
            };
            if (!unboundColumns.Contains(columnIndex) || isSet)
            {
                return null;
            }

            var dataTable = tableView1.GetRowObjectAt(rowIndex) as DataTable;
            if (dataTable == null)
            {
                return null;
            }

            if (columnIndex == substanceColumnIndex)
            {
                return GetPath(dataTable.SubstanceUseforFile);
            }

            if (columnIndex == dataFileColumnIndex)
            {
                return GetPath(dataTable.DataFile);
            }

            return null;
        }

        private static string GetPath(TextDocumentFromFile textDocument)
        {
            return textDocument != null ? Path.GetFileName(textDocument.Path) : "";
        }

        private static string GetContent(TextDocumentFromFile textDocument)
        {
            return textDocument != null ? textDocument.Content : "";
        }

        private void TableView1OnFocusedRowChanged(object sender, EventArgs eventArgs)
        {
            UpdateFileContentViews(tableView1.CurrentFocusedRowObject as DataTable);
        }

        private void TableView1OnColumnFilterChanged(object sender, EventArgs<ITableViewColumn> eventArgs)
        {
            UpdateFileContentViews(tableView1.CurrentFocusedRowObject as DataTable);
        }

        private bool RowDeleteHandler()
        {
            UpdateFileContentViews(tableView1.GetRowObjectAt(tableView1.FocusedRowIndex + 1) as DataTable);

            return false;
        }

        private void UpdateFileContentViews(DataTable dataTable)
        {
            if (dataTable == null)
            {
                textBoxSubstanceUseFor.Text = string.Empty;
                textBoxSubstanceUseFor.ReadOnly = true;

                textBoxDataFile.Text = string.Empty;
                textBoxDataFile.ReadOnly = true;
            }
            else
            {
                textBoxSubstanceUseFor.Text = GetContent(dataTable.SubstanceUseforFile);
                textBoxSubstanceUseFor.ReadOnly = false;

                textBoxDataFile.Text = GetContent(dataTable.DataFile);
                textBoxDataFile.ReadOnly = false;
            }
        }

        private void MoveDataTable(bool up)
        {
            var dataTable = tableView1.CurrentFocusedRowObject as DataTable;
            if (dataTable == null)
            {
                return;
            }

            int newIndex = tableView1.GetRowIndexByDataSourceIndex(dataTableManager.MoveDataTable(dataTable, up));

            tableView1.SelectRow(newIndex);
            tableView1.FocusedRowIndex = newIndex;
        }

        private void TextBoxSubstanceUseForValidating(object sender, CancelEventArgs e)
        {
            var dataTable = tableView1.CurrentFocusedRowObject as DataTable;
            if (dataTable == null || dataTable.SubstanceUseforFile.Content == textBoxSubstanceUseFor.Text)
            {
                return;
            }

            dataTable.SubstanceUseforFile.Content = textBoxSubstanceUseFor.Text;
        }

        private void TextBoxDataFileValidating(object sender, CancelEventArgs e)
        {
            var dataTable = tableView1.CurrentFocusedRowObject as DataTable;
            if (dataTable == null || dataTable.DataFile.Content == textBoxDataFile.Text)
            {
                return;
            }

            dataTable.DataFile.Content = textBoxDataFile.Text;
        }
    }
}