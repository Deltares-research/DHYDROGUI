using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Controls.Swf.Table;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Nwrw
{
    public partial class NwrwDryWeatherFlowDefinitionView : UserControl, IView
    {
        private const int ButtonColumnIndex = 3;

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
                }

                data = value as IEventedList<NwrwDryWeatherFlowDefinition>;

                if (data != null)
                {
                    tableView.Data = new BindingList<NwrwDryWeatherFlowDefinition>(data);

                    ((INotifyPropertyChanged)data).PropertyChanged += OnPropertyChanged;
                    data.CollectionChanged += delayedEventHandlerDefinitionsCollectionChanged;
                }
                else
                {
                    tableView.Data = null;
                }

                UpdateTableView();
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
            AddVolumeColumn();
            AddButtonColumn();
        }

        private void AddIdColumn()
        {
            tableView.AddColumn(nameof(NwrwDryWeatherFlowDefinition.Name), "Name");
        }

        private void AddTypeColumn()
        {
            var column = tableView.AddColumn(nameof(NwrwDryWeatherFlowDefinition.DistributionType), "Type");

            column.Editor = new ComboBoxTypeEditor
            {
                Items = Enum.GetValues(typeof(DryweatherFlowDistributionType)).Except(new List<DryweatherFlowDistributionType> { DryweatherFlowDistributionType.Variable }),
                CustomFormatter = new EnumFormatter(typeof(DryweatherFlowDistributionType))
            };
        }

        private void AddVolumeColumn()
        {
            tableView.AddColumn(nameof(NwrwDryWeatherFlowDefinition.DailyVolumeVariable),
                                columnCaption: "Daily volume (dm³/day)",
                                readOnly: false,
                                width: 100,
                                displayFormat: "0.###");
        }

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
            if (data == null || arg.Column.AbsoluteIndex < 0)
            {
                return false;
            }

            if (arg.RowIndex == 0) return true;

            if (arg.RowIndex >= 1 && arg.RowIndex < tableView.RowCount)
            {
                var nwrwDryWeatherFlowDefinition = (NwrwDryWeatherFlowDefinition)tableView.GetRowObjectAt(arg.RowIndex);

                if (arg.Column.AbsoluteIndex == ButtonColumnIndex)
                {
                    return nwrwDryWeatherFlowDefinition.DistributionType != DryweatherFlowDistributionType.Daily;
                }

                return false;
            }
            else
            {
                if (arg.Column.AbsoluteIndex == ButtonColumnIndex)
                {
                    return true;
                }

                return false;
            }
        }

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
            tableView.RefreshData();
            tableView.BestFitColumns();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            tableView.RefreshData();
            tableView.BestFitColumns();
        }

        private DelftTools.Utils.Tuple<string, bool> ValidateInput(TableViewCell cell, object newValue)
        {
            if (!IsValidId(cell, newValue))
            {
                return new DelftTools.Utils.Tuple<string, bool>("Id cannot be empty", false);
            }

            return new DelftTools.Utils.Tuple<string, bool>(string.Empty, true);
        }

        private bool IsValidId(TableViewCell cell, object newValue) =>
            cell.Column != tableView.Columns.First() || // The cell is not an id, thus is valid
            newValue is string newId && !string.IsNullOrWhiteSpace(newId);
    }
}
