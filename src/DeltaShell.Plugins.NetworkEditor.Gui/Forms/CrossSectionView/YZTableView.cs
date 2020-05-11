using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Threading;
using GeoAPI.Geometries;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public partial class YZTableView : UserControl, IView, INotifyCollectionChanged, INotifyPropertyChange
    {
        private const int YZTableYColumnIndex = 0;
        private const int YZTableZColumnIndex = 1;

        /// <summary>
        /// Allow adding of new coordinates
        /// GeometryBased
        /// Do not allow adding or removing in the table
        /// Only allow change of the Z column
        /// YZ
        /// Allow adding / removing
        /// Allow change of Y and Z
        /// ZW
        /// Table is readonly
        /// </summary>
        private bool allowAddRemove;

        /// <summary>
        /// Do not allow editing; for example for ZW cross sections
        /// </summary>
        private bool readOnly;

        private BindingList<SimplifiedCoordinate> bindingList;

        private IEventedList<SimplifiedCoordinate> data;

        private bool readOnlyYColumn;

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event NotifyCollectionChangingEventHandler CollectionChanging;

        public YZTableView()
        {
            InitializeComponent();
        }

        public bool AllowAddRemove
        {
            get
            {
                return allowAddRemove;
            }
            set
            {
                allowAddRemove = value;
                tableViewYZ.AllowAddNewRow = allowAddRemove;
                tableViewYZ.AllowDeleteRow = allowAddRemove;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return readOnly;
            }
            set
            {
                if (value == readOnly)
                {
                    return;
                }

                readOnly = value;
                tableViewYZ.Enabled = !readOnly;
            }
        }

        public bool ReadOnlyYColumn
        {
            get
            {
                return readOnlyYColumn;
            }
            set
            {
                if (value == readOnlyYColumn)
                {
                    return;
                }

                readOnlyYColumn = value;
                if (tableViewYZ != null && tableViewYZ.Columns.Count > 1)
                {
                    tableViewYZ.Columns[YZTableYColumnIndex].ReadOnly = readOnlyYColumn;
                }
            }
        }

        public IEditableObject EditableObject
        {
            get
            {
                return tableViewYZ.EditableObject;
            }
            set
            {
                tableViewYZ.EditableObject = value;
            }
        }

        public bool SkipChildItemEventBubbling { get; set; }

        bool INotifyPropertyChange.HasParent { get; set; }

        public object Data
        {
            get
            {
                return data;
            }
            set
            {
                UnSubscribeToData();
                var coordata = (IEventedList<Coordinate>) value;
                if (coordata == null)
                {
                    return;
                }

                data = new EventedList<SimplifiedCoordinate>();
                foreach (Coordinate coordinate in coordata)
                {
                    data.Add(new SimplifiedCoordinate()
                    {
                        X = coordinate.X,
                        Y = coordinate.Y
                    });
                }

                tableViewYZ.Data = null;
                SetupTableView();
                SubscribeToData();
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item) {}

        private void SetupTableView()
        {
            bindingList = new ThreadsafeBindingList<SimplifiedCoordinate>(SynchronizationContext.Current, data)
            {
                AllowNew = AllowAddRemove,
                AllowRemove = AllowAddRemove
            };

            tableViewYZ.AutoGenerateColumns = false;
            tableViewYZ.AddColumn("X", "Y'");
            tableViewYZ.AddColumn("Y", "Z");
            tableViewYZ.SelectionChanged += TableViewYZSelectionChanged;
            tableViewYZ.CellChanged += TableViewYZChanged;
            tableViewYZ.Columns[YZTableYColumnIndex].DisplayFormat = "0.00";
            tableViewYZ.Columns[YZTableZColumnIndex].DisplayFormat = "0.00";
            tableViewYZ.Columns[YZTableYColumnIndex].ReadOnly = readOnlyYColumn;

            tableViewYZ.PasteController = new TableViewArgumentBasedPasteController(tableViewYZ, new List<int> {0}) {DataIsSorted = false};
            tableViewYZ.Data = bindingList;
        }

        private void UnSubscribeToData()
        {
            if (data != null)
            {
                data.CollectionChanged -= DataCollectionChanged;
            }
        }

        private void SubscribeToData()
        {
            if (data != null)
            {
                data.CollectionChanged += DataCollectionChanged;
            }
        }

        private void DataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!Equals(sender, data))
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (data.Count > 1)
                {
                    var coordinate = (Coordinate) e.GetRemovedOrAddedItem();
                    UnSubscribeToData();
                    // item is already added to internal list
                    coordinate.X = data[data.Count - 2].X;
                    coordinate.Y = data[data.Count - 2].Y;
                    SubscribeToData();
                }
            }

            CollectionChanged?.Invoke(sender, e);
        }

        private void TableViewYZSelectionChanged(object sender, TableSelectionChangedEventArgs e)
        {
            if (!AllowAddRemove)
            {
                return;
            }

            if (e.Cells.Count > 0)
            {
                int lastRow = tableViewYZ.RowCount - 1;
                if (e.Cells.Where(c => c.RowIndex == 0 || c.RowIndex == lastRow).Count() >= 1)
                {
                    // do not allow removal of the first or last row
                    tableViewYZ.AllowDeleteRow = false;
                }
                else
                {
                    tableViewYZ.AllowDeleteRow = true;
                }
            }
            else
            {
                if (tableViewYZ.CurrentFocusedRowObject != null)
                {
                    int index = data.IndexOf((SimplifiedCoordinate) tableViewYZ.CurrentFocusedRowObject);
                    if (index == 0 || index == data.Count - 1)
                    {
                        // do not allow removal of the first or last row
                        tableViewYZ.AllowDeleteRow = false;
                    }
                    else
                    {
                        tableViewYZ.AllowDeleteRow = true;
                    }
                }
            }
        }

        /// <summary>
        /// User has edited the value in the cell of the yz table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableViewYZChanged(object sender, EventArgs<TableViewCell> e)
        {
            switch (e.Value.Column.AbsoluteIndex)
            {
                case YZTableZColumnIndex:
                    if (null != PropertyChanged)
                    {
                        int rowIndex = e.Value.RowIndex < 0 ? data.Count - 1 : e.Value.RowIndex;
                        PropertyChanged(data[rowIndex], new PropertyChangedEventArgs("Y"));
                    }

                    break;
                case YZTableYColumnIndex:
                    // e.Value.RowIndex < 0 when editing a newly added row
                    //if (e.Value.RowIndex < 0)
                    //{
                    //    return;
                    //}
                    if (null != PropertyChanged)
                    {
                        //var value = tableViewYZ.GetCellValue(e.Value.RowIndex, e.Value.ColumnIndex);
                        int rowIndex = e.Value.RowIndex < 0 ? data.Count - 1 : e.Value.RowIndex;
                        PropertyChanged(data[rowIndex], new PropertyChangedEventArgs("X"));
                    }

                    break;
            }
        }
    }
}