using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Controls.Swf.Table;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.Roughness
{
    public partial class RoughnessSectionCoverageTableView : UserControl, ILayerEditorView
    {
        private readonly CoverageTableView coverageTableView;

        private const int ColumnRoughnessValueIndex = 2;
        private const int ColumnRoughnessTypeIndex = 3;
        private const int ColumnRoughnessFunctionIndex = 4;
        private const int ColumnRoughnessUnitIndex = 5;
        private const int ColumnRoughnessButtonIndex = 6;

        private RoughnessSection data;

        private static readonly ILog Log = LogManager.GetLogger(typeof(RoughnessSectionCoverageTableView));

        public RoughnessSectionCoverageTableView()
        {
            InitializeComponent();
            
            coverageTableView = new CoverageTableView { Dock = DockStyle.Fill };

            coverageTableView.SelectedFeaturesChanged += OnSelectedFeaturesChanged;
            
            Controls.Add(coverageTableView);

            SubscribeTableViewEvents();
        }

        private void OnSelectedFeaturesChanged(object s, EventArgs a)
        {
            if (SelectedFeaturesChanged != null)
            {
                SelectedFeaturesChanged(s, a);
            }
        }

        object IView.Data
        {
            get { return Data; }
            set { Data = (RoughnessSection)value; }
        }

        public RoughnessSection Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChange)data).PropertyChanged -= OnRoughnessSectionPropertyChanged;
                }
                data = value;
                coverageTableView.Data = data != null ? data.RoughnessNetworkCoverage : null;
                
                if (data == null) return;

                ((INotifyPropertyChange)data).PropertyChanged += OnRoughnessSectionPropertyChanged;
                SetTableViewColumns();
            }
        }

        private void OnRoughnessSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEditing" && !data.IsEditing)
            {
                UpdateTableView();
            }
        }

        public Image Image
        {
            get
            {
                return (Data != null && Data.Reversed)
                           ? NetworkEditor.Properties.Resources.ReverseRoughnessSection
                           : NetworkEditor.Properties.Resources.RoughnessSection;
            }
            set { }
        }

        public void EnsureVisible(object item)
        {
            coverageTableView.EnsureVisible(item);
        }

        public ViewInfo ViewInfo { get; set; }

        private void SubscribeTableViewEvents()
        {
            var tableview = (TableView) coverageTableView.TableView;

            tableview.ReadOnlyCellFilter = RoughnessReadOnlyCellFilter;
            tableview.DisplayCellFilter = RoughnessDisplayCellFilter;
            tableview.UnboundColumnData = RougnessSectionCoverageViewUnboundColumnData;
            tableview.CellChanged += RougnessSectionCoverageViewCellChanged;
        }

        private void SetTableViewColumns()
        {
            var tableview = (TableView) coverageTableView.TableView;

            // Create editors
            var roughnessTypeEditor = new ComboBoxTypeEditor
                {
                    Items = Enum.GetValues(typeof (RoughnessType)).Cast<int>().ToList(),
                    CustomFormatter = new EnumFormatter<RoughnessType>()
                };

            var functionTypeEditor = new ComboBoxTypeEditor {Items = Enum.GetValues(typeof (RoughnessFunction))};
            var buttonEditor = new ButtonTypeEditor {ButtonClickAction = DoEditFunction, HideOnReadOnly = true};

            // Update coverageView columns
            var roughnessTypeColumn = tableview.Columns[ColumnRoughnessTypeIndex];
            roughnessTypeColumn.Caption = "Roughness type";
            roughnessTypeColumn.CustomFormatter = new EnumFormatter<RoughnessType>();
            roughnessTypeColumn.Editor = roughnessTypeEditor;
            roughnessTypeColumn.DisplayIndex = 2;

            tableview.Columns[ColumnRoughnessValueIndex].Caption = "value";

            // Add unbound columns
            var columnFunctionType = tableview.AddUnboundColumn("Function type", typeof(RoughnessFunction), 2, functionTypeEditor);
            tableview.AddUnboundColumn("Unit", typeof(string));
            tableview.AddUnboundColumn(" ", typeof(string), -1, buttonEditor);

            tableview.Columns[columnFunctionType].CustomFormatter = new EnumFormatter();

            tableview.AutoGenerateColumns = false;
        }

        private object RougnessSectionCoverageViewUnboundColumnData(int column, int dataSourceIndex, bool isGetData, bool isSetData, object value)
        {
            var location = GetLocation(dataSourceIndex);
            if (location == null) return null;
            
            var branch = location.Branch;

            if (column == ColumnRoughnessFunctionIndex)
            {
                if (isGetData)
                {
                    return data.GetRoughnessFunctionType(branch);
                }
                if (isSetData)
                {
                    ChangeToFunction((RoughnessFunction) value);
                }
            }
            else if (column == ColumnRoughnessTypeIndex)
            {
                return "";
            }
            else if (column == ColumnRoughnessUnitIndex)
            {
                return RoughnessHelper.GetUnit(GetRoughnessType(branch));
            }

            return null;
        }

        private void RougnessSectionCoverageViewCellChanged(object sender, EventArgs<TableViewCell> e)
        {
            if (data == null || e.Value.Column.AbsoluteIndex != ColumnRoughnessTypeIndex) return;

            var rowIndex = e.Value.RowIndex;
            var location = GetLocation(rowIndex);
            var oldRoughnessType = data.EvaluateRoughnessType(location);
            var tableView = (TableView) coverageTableView.TableView;
            var newRoughnessType = (RoughnessType)tableView.GetCellValue(e.Value);

            if (oldRoughnessType == newRoughnessType)
            {
                return;
            }

            var defaultValue = RoughnessHelper.GetDefault(newRoughnessType);
            data.BeginEdit(String.Format("Changing roughness type {0} -> {1}", oldRoughnessType, newRoughnessType));
            tableView.SetCellValue(rowIndex, e.Value.Column.AbsoluteIndex + 1, defaultValue.ToString("N3"));

            while (true)
            {
                // set all locations to the same roughnesstype 
                var nextLocation = GetLocation(rowIndex);
                if ((nextLocation != null) && (nextLocation.Branch == location.Branch))
                {
                    data.RoughnessNetworkCoverage[nextLocation] = new object[] { defaultValue, newRoughnessType };
                }
                else
                {
                    break;
                }
                rowIndex++;
            }
            data.EndEdit();
            UpdateTableView();
        }

        private void UpdateTableView()
        {
            if (coverageTableView.TableView == null) return;
            
            coverageTableView.TableView.ScheduleRefresh();
        }

        private void ChangeToFunction(RoughnessFunction roughnessFunction)
        {
            var location = GetLocation(((TableView)coverageTableView.TableView).SelectedCells[0].RowIndex);
            data.ChangeBranchFunction(location.Branch, roughnessFunction);

            UpdateTableView();
        }

        private void DoEditFunction()
        {
            var selectedCells = ((TableView) coverageTableView.TableView).SelectedCells;

            if (selectedCells.Count == 0)
            {
                return;
            }

            var location = GetLocation(selectedCells[0].RowIndex);
            if (location == null)
            {
                Log.Warn("No location (branch + chainage) to edit.");
                return;
            }

            var roughnessFunctionType = data.GetRoughnessFunctionType(location.Branch);
            IFunction function;
            string variableName;

            switch (roughnessFunctionType)
            {
                case RoughnessFunction.FunctionOfQ:
                    function = data.FunctionOfQ(location.Branch);
                    variableName = "Q";
                    break;
                case RoughnessFunction.FunctionOfH:
                    function = data.FunctionOfH(location.Branch);
                    variableName = "H";
                    break;
                case RoughnessFunction.Constant:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var form = new RoughnessAsFunctionOfView(variableName, location.Branch.Name,
                                                     data.EvaluateRoughnessType(location),
                                                     RoughnessHelper.GetUnit(GetRoughnessType(location.Branch)))
                {
                    Data = function
                };

            form.ShowDialog();
        }

        private bool RoughnessDisplayCellFilter(TableViewCellStyle tableViewCellStyle)
        {
            bool visible;
            GetCellProperties(tableViewCellStyle.RowIndex, tableViewCellStyle.Column.AbsoluteIndex, out visible, out bool _);
            if (!visible)
            {
                var color = ((TableView) coverageTableView.TableView).ReadOnlyCellBackColor;
                tableViewCellStyle.BackColor = color;
                tableViewCellStyle.ForeColor = color;
            }
            return true;
        }

        /// <summary>
        /// Cells should be readonly when
        /// column is roughness value and branch is RoughnessFunctionOfH or RoughnessFunctionOfQ.
        /// column is roughness type and not the first row (networklocation) of the branch when
        ///    branch is RoughnessFunctionOfH or RoughnessFunctionOfQ.
        /// </summary>
        /// <param name="arg">
        /// arg ColumnIndex is index of visible column. For TableViewCell varies per usage.
        /// </param>
        /// <returns></returns>
        private bool RoughnessReadOnlyCellFilter(TableViewCell arg)
        {
            bool editable;
            GetCellProperties(arg.RowIndex, arg.Column.AbsoluteIndex, out bool _, out editable);
            return !editable;
        }

        private void GetCellProperties(int rowIndex, int columnIndex, out bool visible, out bool editable)
        {
            if (null == data || rowIndex < 0 || columnIndex < 0)
            {
                visible = true;
                editable = true;
                return;
            }

            var location = GetLocation(rowIndex);
            if (location == null)
            {
                visible = false;
                editable = false;
                return;
            }

            var constantFunctionType = data.GetRoughnessFunctionType(location.Branch) == RoughnessFunction.Constant;
            var firstRowOfBranch = rowIndex == 0 || !Equals(GetLocation(rowIndex - 1).Branch, location.Branch);
            
            switch (columnIndex)
            {
                case ColumnRoughnessFunctionIndex:
                    visible = firstRowOfBranch;
                    editable = firstRowOfBranch;
                    return;
                case ColumnRoughnessTypeIndex:
                    visible = firstRowOfBranch;
                    editable = firstRowOfBranch && !data.Reversed;
                    return;
                case ColumnRoughnessValueIndex:
                    visible = constantFunctionType;
                    editable = constantFunctionType;
                    return;
                case ColumnRoughnessUnitIndex:
                    visible = firstRowOfBranch;
                    editable = false;
                    return;
                case ColumnRoughnessButtonIndex:
                    visible = firstRowOfBranch && !constantFunctionType;
                    editable = firstRowOfBranch && !constantFunctionType;
                    return;
            }
            visible = true;
            editable = true;
        }

        private INetworkLocation GetLocation(int rowIndex)
        {
            var tableView = (TableView) coverageTableView.TableView;
            var functionBindingList = tableView.Data as FunctionBindingList;
            
            if (functionBindingList == null || rowIndex >= functionBindingList.Count)
            {
                return null;
            }

            if (rowIndex < 0 )
            {
                if (tableView.OnNewRow())
                {
                    rowIndex = functionBindingList.Count - 1;
                }
                else
                {
                    return null;
                }
            }

            var sortedIndex = tableView.GetDataSourceIndexByRowIndex(rowIndex);
            if (sortedIndex < 0)
            {
                return null; 
            }
            var functionBindingListRow = functionBindingList[sortedIndex] as NetworkCoverageBindingListRow;
            
            if (functionBindingListRow != null)
            {
                return functionBindingListRow.GetNetworkLocation();
            }
            
            throw new NotSupportedException("The function binding list is not for network spatial data, unable to retrieve location");
        }

        private RoughnessType GetRoughnessType(IBranch branch)
        {
            return data.EvaluateRoughnessType(new NetworkLocation(branch, 0.0)); 
        }

        private class EnumFormatter<T> : ICustomFormatter where T : struct, IConvertible
        {
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                var value = arg;

                if (arg is int)
                {
                    value = (T) arg;
                }

                return (value.GetType().IsEnum)
                           ? ((Enum)value).GetDescription()
                           : value.ToString();
            }
        }

        public IEnumerable<IFeature> SelectedFeatures
        {
            get { return coverageTableView.SelectedFeatures; }
            set { coverageTableView.SelectedFeatures = value; }
        }

        public event EventHandler SelectedFeaturesChanged;

        public ILayer Layer
        {
            get { return coverageTableView.Layer; }
            set { coverageTableView.Layer = value; }
        }

        public void OnActivated()
        {
            coverageTableView.OnActivated();
        }

        public void OnDeactivated()
        {
            coverageTableView.OnDeactivated();
        }
    }
}
