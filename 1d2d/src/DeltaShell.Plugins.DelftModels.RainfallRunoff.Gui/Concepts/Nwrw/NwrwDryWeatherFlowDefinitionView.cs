using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Controls.Swf.Table;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Nwrw
{
    public partial class NwrwDryWeatherFlowDefinitionView : UserControl, IView
    {
        private readonly ILog Log = LogManager.GetLogger(typeof(NwrwDryWeatherFlowDefinitionView));
        private readonly DelayedEventHandler<EventArgs> delayedEventHandlerDefinitionsCollectionChanged;

        private IEventedList<NwrwDryWeatherFlowDefinition> data;

        public NwrwDryWeatherFlowDefinitionView()
        {
            InitializeComponent();
            SubscribeTableViewEvents();

            delayedEventHandlerDefinitionsCollectionChanged =
                new DelayedEventHandler<EventArgs>(OnCollectionChanged)
                {
                    FireLastEventOnly = true,
                    Delay = 500,
                    SynchronizingObject = this
                };

            tableView.InputValidator = ValidateInput;
        }

        public object Data
        {
            get => data;
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChanged)data).PropertyChanged -= OnPropertyChanged;
                    data.CollectionChanged -= delayedEventHandlerDefinitionsCollectionChanged;
                    data.CollectionChanging -= DataOnCollectionChanging;
                }

                data = value as IEventedList<NwrwDryWeatherFlowDefinition>;

                if (data != null)
                {
                    tableView.Data = new BindingList<NwrwDryWeatherFlowDefinition>(data);

                    ((INotifyPropertyChanged)data).PropertyChanged += OnPropertyChanged;
                    data.CollectionChanged += delayedEventHandlerDefinitionsCollectionChanged;
                    data.CollectionChanging += DataOnCollectionChanging;
                }
                else
                {
                    tableView.Data = null;
                }

                UpdateTableView();
            }
        }

        private void DataOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add
                && e.Item is NwrwDryWeatherFlowDefinition newDryWeatherFlowDefinition 
                && sender is IEventedList<NwrwDryWeatherFlowDefinition> dryWeatherFlowDefinitions)
            {
                newDryWeatherFlowDefinition.Name = NamingHelper.GetUniqueName("DefinitionName{0:D2}", dryWeatherFlowDefinitions);
            }
        }

        public void EnsureVisible(object item) {}

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                delayedEventHandlerDefinitionsCollectionChanged.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void UpdateTableView()
        {
            if (data == null) return;

            SetTableViewColumns();

            tableView.BestFitColumns();
        }

        private void SubscribeTableViewEvents()
        {
            tableView.ReadOnlyCellFilter = ReadOnlyCellFilter;
            tableView.CanDeleteCurrentSelection = CanDeleteCurrentSelection;
        }

        private void SetTableViewColumns()
        {
            tableView.Columns.Clear();

            AddIdColumn();
            AddTypeColumn();
            AddConstantVolumeColumn();
            AddDailyVolumeColumn();
            AddButtonColumn();
        }

        private void AddIdColumn() => tableView.AddColumn(nameof(NwrwDryWeatherFlowDefinition.Name), "Name");

        private void AddTypeColumn()
        {
            var column = tableView.AddColumn(nameof(NwrwDryWeatherFlowDefinition.DistributionType), "Type");

            column.Editor = new ComboBoxTypeEditor
            {
                Items = Enum.GetValues(typeof(DryweatherFlowDistributionType)).Except(new List<DryweatherFlowDistributionType> { DryweatherFlowDistributionType.Variable }),
                CustomFormatter = new EnumFormatter(typeof(DryweatherFlowDistributionType))
            };
        }

        private void AddDoubleFieldColumn(string bindingName, string caption) =>
            tableView.AddColumn(bindingName,
                                columnCaption: caption,
                                readOnly: false,
                                width: 100,
                                displayFormat: "0.###");

        private void AddConstantVolumeColumn() =>
            AddDoubleFieldColumn(nameof(NwrwDryWeatherFlowDefinition.DailyVolumeConstant), 
                                 Resources.NwrwDryWeatherFlowDefinitionView_ConstantVolumeColumnCaption);

        private void AddDailyVolumeColumn() =>
            AddDoubleFieldColumn(nameof(NwrwDryWeatherFlowDefinition.DailyVolumeVariable), 
                                 Resources.NwrwDryWeatherFlowDefinitionView_DailyVolumeColumnCaption);

        private void AddButtonColumn()
        {
            var buttonTypeEditor = new ButtonTypeEditor
            {
                ButtonClickAction = DoEditFunction,
                HideOnReadOnly = true
            };

            tableView.AddUnboundColumn(" ", typeof(string), -1, buttonTypeEditor);
        }

        private void DoEditFunction()
        {
            var nwrwDryweatherFlowDefinition = (NwrwDryWeatherFlowDefinition)tableView.CurrentFocusedRowObject;

            Form form = new NwrwDryWeatherFlowDefinitionDailyVolumePercentagesForm(
                nwrwDryweatherFlowDefinition.HourlyPercentageDailyVolume,
                nwrwDryweatherFlowDefinition.Name);

            form.ShowDialog();
        }

        private bool ReadOnlyCellFilter(TableViewCell arg)
        {
            int rowIndex = arg.RowIndex;
            int columnIndex = arg.Column.AbsoluteIndex;

            if (IsNewRow(rowIndex))
            {
                return IsButtonColumn(columnIndex);
            }

            if (IsConstantVolumeColumn(columnIndex))
            {
                return !HasDistributionType(rowIndex, DryweatherFlowDistributionType.Constant);
            }

            if (IsDailyVolumeColumn(columnIndex))
            {
                return !HasDistributionType(rowIndex, DryweatherFlowDistributionType.Daily);
            }

            return false;
        }

        private bool HasDistributionType(int rowIndex, DryweatherFlowDistributionType distributionType) => 
            (tableView.GetRowObjectAt(rowIndex) as NwrwDryWeatherFlowDefinition)?.DistributionType == distributionType;

        private static bool IsNewRow(int rowIndex) => rowIndex < 0;
        private static bool IsConstantVolumeColumn(int columnIndex) => columnIndex == 2;
        private static bool IsDailyVolumeColumn(int columnIndex) => columnIndex == 3 || IsButtonColumn(columnIndex);
        private static bool IsButtonColumn(int columnIndex) => columnIndex == 4;

        private bool CanDeleteCurrentSelection()
        {
            var selectionContainsFirstRow = tableView.SelectedCells
                                                     .Any(cell => cell.RowIndex == 0);
            if (selectionContainsFirstRow)
            {
                Log.ErrorFormat("Cannot delete the selected rows, as the selection contains a cell from the default definition row.");
                return false;
            }

            return true;
        }

        private void OnCollectionChanged(object sender, EventArgs e)
        {
            RefreshTable();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NwrwDryWeatherFlowDefinition.DistributionType) ||
                e.PropertyName == nameof(NwrwDryWeatherFlowDefinition.DailyVolumeConstant))
            {
                UpdateDailyVolumeVariable((NwrwDryWeatherFlowDefinition)sender);
            }
            
            RefreshTable();
        }

        private void RefreshTable()
        {
            tableView.RefreshData();
            tableView.BestFitColumns();
        }

        private void UpdateDailyVolumeVariable(NwrwDryWeatherFlowDefinition flowDefinition)
        {
            if (flowDefinition.DistributionType == DryweatherFlowDistributionType.Constant)
            {
                flowDefinition.DailyVolumeVariable = flowDefinition.DailyVolumeConstant;
            }
        }

        private DelftTools.Utils.Tuple<string, bool> ValidateInput(TableViewCell cell, object newValue)
        {
            if (!IsValidId(cell, newValue))
            {
                return new DelftTools.Utils.Tuple<string, bool>(Resources.NwrwDryWeatherFlowDefinitionView_ValidateInput_Id_cannot_be_empty_or_duplicate_name, false);
            }

            return new DelftTools.Utils.Tuple<string, bool>(string.Empty, true);
        }

        private bool IsValidId(TableViewCell cell, object newValue) =>
            IsCellNotInColumnIds(cell)
            || newValue is string newId 
            && !string.IsNullOrWhiteSpace(newId) 
            && !IsDuplicateValueInColumnIds(newId);

        private bool IsDuplicateValueInColumnIds(string newId)
        {
            return ((IList<NwrwDryWeatherFlowDefinition>)tableView.Data).Select(dwf => dwf.Name).Contains(newId);
        }

        private bool IsCellNotInColumnIds(TableViewCell cell)
        {
            return cell.Column != tableView.Columns[0];
        }
    }
}
