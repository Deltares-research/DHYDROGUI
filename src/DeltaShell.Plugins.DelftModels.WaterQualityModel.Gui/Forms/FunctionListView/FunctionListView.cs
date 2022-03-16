using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Functions;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView
{
    public partial class FunctionListView : UserControl, IView
    {
        private readonly IEventedList<IFunctionTypeCreator> functionCreators;
        private readonly DelayedEventHandler<EventArgs> functionCollectionChangedDelayedEventHandler;
        private IGui gui;
        private IEventedList<IFunction> functions;
        private IEditableObject dataOwner;

        private ITableViewColumn defaultValueColumn;
        private ITableViewColumn segmentFunctionColumn;

        public FunctionListView()
        {
            InitializeComponent();
            InitializeTableView();

            functionCreators = new EventedList<IFunctionTypeCreator>();
            functionCreators.CollectionChanged += FunctionCreatorsCollectionChanged;

            functionCollectionChangedDelayedEventHandler =
                new DelayedEventHandler<EventArgs>(delegate
                {
                    UpdateTableView();
                    UpdateFunctionViewPanel();
                })
                { SynchronizingObject = this };

            splitContainer1.Panel2Collapsed = true;

            ExcludeList = new HashSet<string>();
        }

        /// <summary>
        /// By setting this property we change the name of the fourth column (Default value) in the Table.
        /// NOTE: Set this value only during initialization (WaterQualityModelGuiPlugin.cs)
        /// </summary>
        public bool UseInitialValueColumn
        {
            get => useInitialValueColumn;
            set
            {
                if (useInitialValueColumn == value)
                {
                    return;
                }

                useInitialValueColumn = value;
                defaultValueColumn.Caption = GetDefaultValueColumnName();
            }
        }

        public IGui Gui
        {
            get => gui;
            set
            {
                gui = value;

                UpdateFunctionViewPanel();
            }
        }

        /// <summary>
        /// Collection of function creators (<see cref="IFunctionTypeCreator"/>) to identify and change functions
        /// </summary>
        public ICollection<IFunctionTypeCreator> FunctionCreators => functionCreators;

        /// <summary>
        /// Whether arguments are shown in the table view or not
        /// </summary>
        public bool ShowArguments
        {
            get => tableView.GetColumnByName(argumentPropertyName).Visible;
            set => tableView.GetColumnByName(argumentPropertyName).Visible = value;
        }

        /// <summary>
        /// Whether components are shown in the table view or not
        /// </summary>
        public bool ShowComponents
        {
            get => tableView.GetColumnByName(componentsPropertyName).Visible;
            set => tableView.GetColumnByName(componentsPropertyName).Visible = value;
        }

        /// <summary>
        /// Whether names are shown read only in the table view or not
        /// </summary>
        public bool ShowNamesReadOnly
        {
            get => tableView.GetColumnByName(namePropertyName).ReadOnly;
            set => tableView.GetColumnByName(namePropertyName).ReadOnly = value;
        }

        public bool ShowDescriptionsReadOnly
        {
            get => tableView.GetColumnByName(descriptionPropertyName).ReadOnly;
            set => tableView.GetColumnByName(descriptionPropertyName).ReadOnly = value;
        }

        /// <summary>
        /// Whether units are shown read only in the table view or not
        /// </summary>
        public bool ShowUnitsReadOnly
        {
            get => tableView.GetColumnByName(unitPropertyName).ReadOnly;
            set => tableView.GetColumnByName(unitPropertyName).ReadOnly = value;
        }

        /// <summary>
        /// Whether edit buttons are shown in the table view or not
        /// </summary>
        public bool ShowEditButtons
        {
            get => tableView.GetColumnByName(editPropertyName).Visible;
            set => tableView.GetColumnByName(editPropertyName).Visible = value;
        }

        /// <summary>
        /// Gets or sets the function that determines if the default value should be read-only
        /// (returning true) or not (false) for a given function.
        /// </summary>
        public Func<IFunction, bool> IsDefaultValueCellReadOnly { get; set; }

        public IEditableObject DataOwner
        {
            get => dataOwner;
            set
            {
                dataOwner = value;

                // Update FunctionOwner of FunctionWrapper objects
                var functionWrappers = (IEnumerable<FunctionWrapper>)tableView.Data;
                foreach (FunctionWrapper functionWrapper in functionWrappers)
                {
                    functionWrapper.FunctionOwner = dataOwner;
                }
            }
        }

        public ISet<string> ExcludeList { get; private set; }

        public object Data
        {
            get => functions;
            set
            {
                if (functions != null)
                {
                    functions.CollectionChanged -= functionCollectionChangedDelayedEventHandler;
                    ((INotifyPropertyChange)functions).PropertyChanged -= OnPropertyChanged;
                }

                functions = (IEventedList<IFunction>)value;

                if (functions != null)
                {
                    functions.CollectionChanged += functionCollectionChangedDelayedEventHandler;
                    ((INotifyPropertyChange)functions).PropertyChanged += OnPropertyChanged;
                }

                UpdateTableView();
                UpdateFunctionViewPanel();
                tableView.BestFitColumns();
            }
        }

        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        public void UpdateTableView()
        {
            if (functions != null)
            {
                var wrappers = new List<FunctionWrapper>();
                foreach (IFunction function in functions)
                {
                    // check toLower, because the parameter names are case insensitive.
                    if (!ExcludeList.Contains(function.Name.ToLowerInvariant()))
                    {
                        wrappers.Add(new FunctionWrapper(function, functions, DataOwner, functionCreators));
                    }
                }

                tableView.Data = wrappers;
            }
            else
            {
                tableView.Data = null;
            }
        }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

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

        /// <summary>
        /// Initializes the table view.
        /// </summary>
        private void InitializeTableView()
        {
            tableView.AutoGenerateColumns = false;
            tableView.AllowAddNewRow = false;
            tableView.AllowDeleteRow = false;
            tableView.EditButtons = false;
            tableView.RowHeight += 2;

            tableView.AddColumn(namePropertyName, Resources.FunctionListView_InitializeTableView_Name);
            tableView.AddColumn(descriptionPropertyName, Resources.FunctionListView_InitializeTableView_Description);
            tableView.AddColumn(functionTypePropertyName, Resources.FunctionListView_InitializeTableView_Function_type);

            defaultValueColumn = tableView.AddColumn(defaultValuePropertyName, GetDefaultValueColumnName());
            tableView.AddColumn(unitPropertyName, Resources.FunctionListView_InitializeTableView_Unit);
            segmentFunctionColumn = tableView.AddColumn(urlPropertyName,
                                                        Resources
                                                            .FunctionListView_InitializeTableView_SegmentFunctionFilePath);
            tableView.AddColumn(argumentPropertyName, Resources.FunctionListView_InitializeTableView_Arguments, true,
                                100);
            tableView.AddColumn(componentsPropertyName, Resources.FunctionListView_InitializeTableView_Components, true,
                                100);
            tableView.AddColumn(editPropertyName, Resources.FunctionListView_InitializeTableView_Edit);

            tableView.GetColumnByName(editPropertyName).Editor =
                new ButtonTypeEditor { ButtonClickAction = OpenViewForFunction };

            //Execute BestFitColumns to make the columns always readable and add a scrollbar if not.
            tableView.BestFitColumns();

            tableView.ReadOnlyCellFilter = ReadOnlyCellFilter;

            ShowArguments = false;
            ShowComponents = false;
            ShowNamesReadOnly = true;
            ShowDescriptionsReadOnly = true;
            ShowUnitsReadOnly = true;
        }

        private string GetDefaultValueColumnName()
        {
            return UseInitialValueColumn
                       ? Resources.FunctionListView_GetDefaultValueColumnName_Initial_value
                       : Resources.FunctionListView_InitializeTableView_Default_value;
        }

        private bool ReadOnlyCellFilter(TableViewCell tableViewCell)
        {
            // Default value should be readonly when data coming from external source.
            if (IsDefaultValueCellReadOnly == null
                || tableViewCell.Column != defaultValueColumn && tableViewCell.Column != segmentFunctionColumn)
            {
                return false;
            }

            IFunction functionCorrespondingToCell = GetFunctionForCell(tableViewCell.RowIndex);
            return functionCorrespondingToCell == null
                   || IsDefaultValueCellReadOnly(functionCorrespondingToCell)
                   || tableViewCell.Column == segmentFunctionColumn && !functionCorrespondingToCell.IsSegmentFile();
        }

        private IFunction GetFunctionForCell(int rowIndex)
        {
            var functionWrapper = tableView.GetRowObjectAt(rowIndex) as FunctionWrapper;
            return functionWrapper?.Function;
        }

        private void OpenViewForFunction()
        {
            var functionWrapper = tableView.CurrentFocusedRowObject as FunctionWrapper;

            // Function should not be constant, but either dependent on time or space in order to be active. 
            // Constant values can be adapted in the FunctionListView itself. 
            if (functionWrapper == null || Gui == null || functionWrapper.Function.IsConst())
            {
                return;
            }

            object viewData = functionWrapper.Function;

            if (viewData is ICoverage)
            {
                viewData = ((WaterQualityModel)DataOwner).GetDataItemByValue(viewData);
            }

            if (functionWrapper.Function.IsSegmentFile())
            {
                //Create dialog asking for the file location.
                string filePath = new FileDialogService().SelectFile("");
                if (filePath == null)
                {
                    return;
                }

                var fileFunction = functionWrapper.Function as SegmentFileFunction;
                if (fileFunction != null)
                {
                    fileFunction.UrlPath = filePath;
                }

                UpdateTableView();
                return;
            }

            if (viewData == null)
            {
                return;
            }

            Gui.DocumentViewsResolver.OpenViewForData(viewData);
        }

        private void UpdateFunctionViewPanel()
        {
            var functionWrapper = tableView.CurrentFocusedRowObject as FunctionWrapper;

            panel1.Controls.Clear();

            if (functionWrapper == null || Gui == null || functionWrapper.Function.IsConst())
            {
                return;
            }

            var view = Gui.DocumentViewsResolver.CreateViewForData(functionWrapper.Function,
                                                                   info => info.ViewType == typeof(CoverageTableView))
                           as Control;
            if (view == null)
            {
                return;
            }

            view.Dock = DockStyle.Fill;
            panel1.Controls.Add(view);
        }

        private void FunctionCreatorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateFunctionTypeEditor();
        }

        private void TableViewFocusedRowChanged(object sender, EventArgs e)
        {
            UpdateFunctionViewPanel();
            UpdateFunctionTypeEditor();
        }

        private void UpdateFunctionTypeEditor()
        {
            IFunction function = GetFunctionForCell(tableView.FocusedRowIndex);
            if (function == null)
            {
                return;
            }

            // HACK : Need to set entire editor because items are only set when setting editor
            tableView.GetColumnByName(functionTypePropertyName).Editor = new ComboBoxTypeEditor
            {
                Items = functionCreators.Where(fc => fc.IsAllowed(function)).Select(fc => fc.FunctionTypeName).ToList(),
                ItemsMandatory =
                    false // Show items that might have been filtered out for this row but have been selected for a different row
            };
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // Filter for performance:
            if (propertyChangedEventArgs.PropertyName == defaultValuePropertyName)
            {
                PerformTableViewDataRefresh();
            }
        }

        [InvokeRequired]
        private void PerformTableViewDataRefresh()
        {
            tableView.RefreshData();
        }

        #region Column property names

        private readonly string namePropertyName = nameof(FunctionWrapper.Name);
        private readonly string descriptionPropertyName = nameof(FunctionWrapper.Description);
        private readonly string functionTypePropertyName = nameof(FunctionWrapper.FunctionType);
        private readonly string defaultValuePropertyName = nameof(FunctionWrapper.DefaultValue);
        private readonly string unitPropertyName = nameof(FunctionWrapper.Unit);
        private readonly string urlPropertyName = nameof(FunctionWrapper.Url);
        private readonly string argumentPropertyName = nameof(FunctionWrapper.Arguments);
        private readonly string componentsPropertyName = nameof(FunctionWrapper.Components);
        private readonly string editPropertyName = nameof(FunctionWrapper.Edit);
        private bool useInitialValueColumn;

        #endregion
    }
}