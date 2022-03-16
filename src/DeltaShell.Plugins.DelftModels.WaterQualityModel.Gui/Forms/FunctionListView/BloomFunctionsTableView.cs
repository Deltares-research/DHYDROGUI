using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView
{
    public partial class BloomFunctionsTableView : UserControl, IView
    {
        private readonly DelayedEventHandler<EventArgs> functionCollectionChangedDelayedEventHandler;
        private BloomInfo info;
        private IEventedList<IFunction> functions;
        private Dictionary<int, string> columnIndices;
        private int nameColumnIndex;

        public BloomFunctionsTableView()
        {
            InitializeComponent();

            functionCollectionChangedDelayedEventHandler =
                new DelayedEventHandler<EventArgs>(delegate
                {
                    UpdateTableColumns();
                    UpdateTableView();
                }) {SynchronizingObject = this};
        }

        public IGui Gui { get; set; }

        public IEditableObject DataOwner { get; set; }

        public BloomInfo BloomInfo
        {
            set
            {
                info = value;
                InitializeTableView();
            }
        }

        public object Data
        {
            get => functions;
            set
            {
                if (functions != null)
                {
                    functions.CollectionChanged -= functionCollectionChangedDelayedEventHandler;
                    ((INotifyPropertyChange) functions).PropertyChanged -= OnPropertyChanged;
                }

                functions = (IEventedList<IFunction>) value;

                if (functions != null)
                {
                    functions.CollectionChanged += functionCollectionChangedDelayedEventHandler;
                    ((INotifyPropertyChange) functions).PropertyChanged += OnPropertyChanged;
                }

                UpdateTableView();
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"> true if managed resources should be disposed; otherwise, false. </param>
        protected override void Dispose(bool disposing)
        {
            functionCollectionChangedDelayedEventHandler.Enabled = false;
            functionCollectionChangedDelayedEventHandler.Dispose();

            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeTableView()
        {
            tableView.AutoGenerateColumns = false;
            tableView.AllowAddNewRow = false;
            tableView.AllowDeleteRow = false;
            tableView.EditButtons = false;
            tableView.AllowColumnSorting = false; // data is assumed to be in static order in UnboundColumnData

            UpdateTableColumns();

            tableView.UnboundColumnData = UnboundColumnData;

            UpdateTableView();
        }

        private void UpdateTableColumns()
        {
            columnIndices = new Dictionary<int, string>();

            tableView.Columns.Clear();

            // add the columns
            nameColumnIndex = tableView.AddUnboundColumn("BLOOM Algae Type", typeof(string));
            tableView.Columns[nameColumnIndex].ReadOnly = true;

            foreach (string header in GetHeadersThatAreInFunctions())
            {
                string prettyHeader = string.Format("{0}[{1}]", header.Substring(0, header.Length - 3),
                                                    header.Substring(header.Length - 3));
                int resultColumn = tableView.AddUnboundColumn(prettyHeader, typeof(double));
                columnIndices.Add(resultColumn, header);
            }
        }

        // Parameters : column index, datasource row index, is getter, is setter, value
        private object UnboundColumnData(int columnIndex, int rowIndex, bool isGetter, bool isSetter, object value)
        {
            if (info == null)
            {
                return null;
            }

            var kortName = (string) tableView.GetRowObjectAt(rowIndex);

            if (isGetter)
            {
                string columnHeader;

                if (columnIndex == nameColumnIndex)
                {
                    return string.Format("{0} ({1})", kortName, info.GetKortDescription(kortName));
                }

                if (columnIndices.TryGetValue(columnIndex, out columnHeader))
                {
                    string funcName = info.MakeParameter(columnHeader, kortName);

                    IFunction function =
                        functions.FirstOrDefault(
                            f => string.Equals(f.Name, funcName, StringComparison.InvariantCultureIgnoreCase));

                    if (function != null)
                    {
                        return function.Components[0].DefaultValue;
                    }
                }
            }

            if (isSetter)
            {
                string columnHeader;

                if (!columnIndices.TryGetValue(columnIndex, out columnHeader))
                {
                    return null;
                }

                string funcName = info.MakeParameter(columnHeader, kortName);
                IFunction function =
                    functions.FirstOrDefault(
                        f => string.Equals(f.Name, funcName, StringComparison.InvariantCultureIgnoreCase));

                if (function != null)
                {
                    return function.Components[0].DefaultValue = Convert.ToDouble(value);
                }
            }

            return null;
        }

        private void UpdateTableView()
        {
            if (functions != null && info != null)
            {
                tableView.Data = info.GetKortsPresentInFunctions(functions).ToList();
            }
            else
            {
                tableView.Data = null;
            }

            tableView.BestFitColumns();
        }

        private IEnumerable<string> GetHeadersThatAreInFunctions()
        {
            return info.GetHeadersPresentInFunctions(functions);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Filter for performance:
            if (e.PropertyName == "DefaultValue")
            {
                PerformTableViewDataRefresh();
            }
        }

        [InvokeRequired]
        private void PerformTableViewDataRefresh()
        {
            tableView.RefreshData();
        }
    }
}