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

        private const int columnBranchIndex = 0;
        private const int columnRoughnessValueIndex = 2;
        private const int columnRoughnessTypeIndex = 3;
        private const int columnRoughnessFunctionIndex = 4;
        private const int columnRoughnessUnitIndex = 5;
        private const int columnRoughnessButtonIndex = 6;

        private RoughnessSection data;

        private static readonly ILog log = LogManager.GetLogger(typeof(RoughnessSectionCoverageTableView));

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
                return Data != null && Data.Reversed
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
            tableview.UnboundColumnData = RoughnessSectionCoverageViewUnboundColumnData;
            tableview.CellChanged += RoughnessSectionCoverageViewCellChanged;
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
            var roughnessTypeColumn = tableview.Columns[columnRoughnessTypeIndex];
            roughnessTypeColumn.Caption = "Roughness type";
            roughnessTypeColumn.CustomFormatter = new EnumFormatter<RoughnessType>();
            roughnessTypeColumn.Editor = roughnessTypeEditor;
            roughnessTypeColumn.DisplayIndex = 2;

            tableview.Columns[columnRoughnessValueIndex].Caption = "value";

            // Add unbound columns
            var columnFunctionType = tableview.AddUnboundColumn("Function type", typeof(RoughnessFunction), 2, functionTypeEditor);
            tableview.AddUnboundColumn("Unit", typeof(string));
            tableview.AddUnboundColumn(" ", typeof(string), -1, buttonEditor);

            tableview.Columns[columnFunctionType].CustomFormatter = new EnumFormatter();

            tableview.AutoGenerateColumns = false;
        }

        private object RoughnessSectionCoverageViewUnboundColumnData(int column, int dataSourceIndex, bool isGetData, bool isSetData, object value)
        {
            var location = GetLocation(dataSourceIndex);
            if (location == null) return null;
            
            var branch = location.Branch;

            if (column == columnRoughnessFunctionIndex)
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
            else if (column == columnRoughnessTypeIndex)
            {
                return "";
            }
            else if (column == columnRoughnessUnitIndex)
            {
                return RoughnessHelper.GetUnit(GetRoughnessType(branch));
            }

            return null;
        }

        private void RoughnessSectionCoverageViewCellChanged(object sender, EventArgs<TableViewCell> e)
        {
            if (data == null)
            {
                return;
            }

            int columnIndex = e.Value.Column.AbsoluteIndex;
            var rowIndex = e.Value.RowIndex;

            if (columnIndex == columnBranchIndex)
            {
                // commit the current row to the roughness section data
                // required for other columns to retrieve the selected branch
                coverageTableView.TableView.ValidateAndCommitRow(rowIndex);
            }
            else if (columnIndex == columnRoughnessTypeIndex)
            {
                var location = GetLocation(rowIndex);
                var oldRoughnessType = data.EvaluateRoughnessType(location);
                var tableView = (TableView) coverageTableView.TableView;
                var newRoughnessType = (RoughnessType)tableView.GetCellValue(e.Value);

                if (oldRoughnessType == newRoughnessType)
                {
                    return;
                }

                var defaultValue = RoughnessHelper.GetDefault(newRoughnessType);
                data.BeginEdit($"Changing roughness type {oldRoughnessType} -> {newRoughnessType}");
                tableView.SetCellValue(rowIndex, e.Value.Column.AbsoluteIndex + 1, defaultValue.ToString("N3"));

                while (true)
                {
                    // set all locations to the same roughness type 
                    var nextLocation = GetLocation(rowIndex);
                    if (nextLocation != null && nextLocation.Branch == location.Branch)
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
                log.Warn("No location (branch + chainage) to edit.");
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
            GetCellProperties(tableViewCellStyle.RowIndex, tableViewCellStyle.Column.AbsoluteIndex, out bool visible, out bool _);
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
        /// column is roughness type and not the first row (network location) of the branch when
        ///    branch is RoughnessFunctionOfH or RoughnessFunctionOfQ.
        /// </summary>
        /// <param name="arg">
        /// arg ColumnIndex is index of visible column. For TableViewCell varies per usage.
        /// </param>
        /// <returns></returns>
        private bool RoughnessReadOnlyCellFilter(TableViewCell arg)
        {
            GetCellProperties(arg.RowIndex, arg.Column.AbsoluteIndex, out bool _, out bool editable);
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
                case columnRoughnessFunctionIndex:
                    visible = firstRowOfBranch;
                    editable = firstRowOfBranch;
                    return;
                case columnRoughnessTypeIndex:
                    visible = firstRowOfBranch;
                    editable = firstRowOfBranch && !data.Reversed;
                    return;
                case columnRoughnessValueIndex:
                    visible = constantFunctionType;
                    editable = constantFunctionType;
                    return;
                case columnRoughnessUnitIndex:
                    visible = firstRowOfBranch;
                    editable = false;
                    return;
                case columnRoughnessButtonIndex:
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

            if (functionBindingList[sortedIndex] is NetworkCoverageBindingListRow functionBindingListRow)
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

                return value.GetType().IsEnum
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
