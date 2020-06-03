using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Nwrw
{
    public partial class NwrwDefinitionView : UserControl, IView
    {
        private const int NameColumnIndex = 0;

        private readonly TableView tableView;
        private IEventedList<NwrwDefinition> data;
        private readonly DelayedEventHandler<EventArgs> delayedEventHandlerDefinitionsCollectionChanged;


        public NwrwDefinitionView()
        {
            InitializeComponent();
            

            delayedEventHandlerDefinitionsCollectionChanged =
                new DelayedEventHandler<EventArgs>(OnCollectionChanged)
                {
                    FireLastEventOnly = true,
                    Delay = 500,
                    SynchronizingObject = this
                };

            tableView = new TableView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowDeleteRow = false,
                AllowAddNewRow = false,
            };
            Controls.Add(tableView);

            SubscribeTableViewEvents();
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

                data = value as IEventedList<NwrwDefinition>;

                if (data != null)
                {
                    tableView.Data = new BindingList<NwrwDefinition>(data);

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

        private void UpdateTableView()
        {
            if (data == null) return;

            SetTableViewColumns();

            tableView.BestFitColumns();
        }

        private void SetTableViewColumns()
        {
            tableView.Columns.Clear();

            AddNameColumn();
            AddSurfaceStorageColumn();
            AddMaximumInfiltrationCapacityColumn();
            AddMinimumInfiltrationCapacityColumn();
            AddInfiltrationCapacityReductionColumn();
            AddInfiltrationCapacityRecoveryColumn();
            AddRunoffDelayColumn();
        }

        private void AddNameColumn()
        {
            tableView.AddColumn(nameof(NwrwDefinition.Name), "Surface type");
        }

        private void AddSurfaceStorageColumn()
        {
            tableView.AddColumn(nameof(NwrwDefinition.SurfaceStorage), "Surface storage (mm)");
        }

        private void AddMaximumInfiltrationCapacityColumn()
        {
            tableView.AddColumn(nameof(NwrwDefinition.InfiltrationCapacityMax), "Maximum infiltration capacity (mm/h)");
        }

        private void AddMinimumInfiltrationCapacityColumn()
        {
            tableView.AddColumn(nameof(NwrwDefinition.InfiltrationCapacityMin), "Minimum infiltration capacity (mm/h)");
        }

        private void AddInfiltrationCapacityReductionColumn()
        {
            tableView.AddColumn(nameof(NwrwDefinition.InfiltrationCapacityReduction), "Infiltration capacity reduction (1/h)");
        }

        private void AddInfiltrationCapacityRecoveryColumn()
        {
            tableView.AddColumn(nameof(NwrwDefinition.InfiltrationCapacityRecovery), "Infiltration capacity recovery (1/h)");
        }

        private void AddRunoffDelayColumn()
        {
            tableView.AddColumn(nameof(NwrwDefinition.RunoffDelay), "Runoff delay (1/min)");
        }

        private void SubscribeTableViewEvents()
        {
            tableView.ReadOnlyCellFilter = ReadOnlyCellFilter;
        }

        private bool ReadOnlyCellFilter(TableViewCell arg)
        {
            if (data == null || arg.Column.AbsoluteIndex < 0)
            {
                return false;
            }
            
            if (arg.Column.AbsoluteIndex == NameColumnIndex) return true;

            return false;
        }

        public void EnsureVisible(object item) { }

        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            tableView.RefreshData();
            tableView.BestFitColumns();
        }

        private void OnCollectionChanged(object sender, EventArgs e)
        {
            tableView.RefreshData();
            tableView.BestFitColumns();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                delayedEventHandlerDefinitionsCollectionChanged.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
