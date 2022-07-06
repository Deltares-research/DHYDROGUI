using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Adapters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;
using ICommand = System.Windows.Input.ICommand;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels
{
    /// <summary>
    /// <see cref="MeteoEditorViewModel"/> implements the <see cref="IMeteoEditorViewModel"/>.
    /// </summary>
    public sealed class MeteoEditorViewModel : IMeteoEditorViewModel
    {
        private readonly IMeteoData meteoData;
        private readonly ITableViewMeteoStationSelectionAdapter tableSelectionAdapter;
        private readonly ITimeDependentFunctionSplitter functionSplitter;

        private bool isSynchronizingTableSelection = false;
        private bool isSynchronizingStationsViewModel = false;

        private readonly DateTime modelStartTime;
        private readonly DateTime modelEndTime;
        private MeteoDataSource activeMeteoDataSource;

        private IFunction[] timeSeries;

        /// <summary>
        /// Creates a new <see cref="MeteoEditorViewModel"/> with the given data.
        /// </summary>
        /// <param name="meteoData">The meteo data to visualize and edit.</param>
        /// <param name="meteoStationsListViewModel">The view model for the stations list.</param>
        /// <param name="possibleSources">The possible meteo data sources.</param>
        /// <param name="tableSelectionAdapter">The table selection adapter.</param>
        /// <param name="functionSplitter">The function splitter used to split a <see cref="IMeteoData.Data"/></param>
        /// <param name="modelStartTime">The model start time.</param>
        /// <param name="modelEndTime">The model end time.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public MeteoEditorViewModel(IMeteoData meteoData,
                                    IMeteoStationsListViewModel meteoStationsListViewModel, 
                                    IEnumerable<MeteoDataSource> possibleSources, 
                                    ITableViewMeteoStationSelectionAdapter tableSelectionAdapter,
                                    ITimeDependentFunctionSplitter functionSplitter,
                                    DateTime modelStartTime,
                                    DateTime modelEndTime)
        {
            Ensure.NotNull(meteoData, nameof(meteoData));
            Ensure.NotNull(meteoStationsListViewModel, nameof(meteoStationsListViewModel));
            Ensure.NotNull(tableSelectionAdapter, nameof(tableSelectionAdapter));
            Ensure.NotNull(functionSplitter, nameof(functionSplitter));
            
            PossibleMeteoDataSources = possibleSources?.ToArray();
            Ensure.NotNull(PossibleMeteoDataSources, nameof(possibleSources));
            ActiveMeteoDataSource = PossibleMeteoDataSources.First();

            this.modelStartTime = modelStartTime;
            this.modelEndTime = modelEndTime;

            this.meteoData = meteoData;
            this.tableSelectionAdapter = tableSelectionAdapter;
            this.functionSplitter = functionSplitter;
            this.meteoData.PropertyChanged += OnMeteoDataChanged;
            this.meteoData.CatchmentsChanged += OnCatchmentsChanged;

            StationsViewModel = meteoStationsListViewModel;
            StationsViewModel.Stations.CollectionChanged += OnStationsChanged;
            StationsViewModel.SelectedStations.CollectionChanged += OnSelectedStationsChanged;

            GenerateTimeSeriesCommand = new RelayCommand(OnGenerateTimeSeries);

            RefreshTimeSeries();
        }

        private void OnStationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshTimeSeries();
            OnPropertyChanged(nameof(ShowNoStationsWarning));
        }

        private void OnSelectedStationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MeteoDataDistributionType != MeteoDataDistributionType.PerStation ||
                isSynchronizingStationsViewModel) return;

            isSynchronizingTableSelection = true;
            tableSelectionAdapter.SetSelection(StationsViewModel.SelectedStations.Select(x => x.Name));
            isSynchronizingTableSelection = false;
        }

        public ICommand GenerateTimeSeriesCommand { get; }
        public IMeteoStationsListViewModel StationsViewModel { get; }

        public MeteoDataSource ActiveMeteoDataSource
        {
            get => activeMeteoDataSource;
            set
            {
                if (activeMeteoDataSource == value) return;

                activeMeteoDataSource = value;
                OnPropertyChanged();
            }
        }

        public MeteoDataSource[] PossibleMeteoDataSources { get; }
        public bool CanEditActiveMeteoDataSource => PossibleMeteoDataSources.Length > 1;

        public MeteoDataDistributionType MeteoDataDistributionType
        {
            get => meteoData.DataDistributionType;
            set
            {
                if (value == meteoData.DataDistributionType) return;

                meteoData.DataDistributionType = value;
                OnPropertyChanged();
            }
        }

        public IFunction[] TimeSeries
        {
            get => timeSeries;
            private set
            {
                if (TimeSeries == value) return;

                timeSeries = value;
                OnPropertyChanged();
            }
        }

        private void RefreshTimeSeries()
        { 
            switch (MeteoDataDistributionType)
            {
                case MeteoDataDistributionType.Global:
                    TimeSeries = new[] { meteoData.Data };
                    break;
                case MeteoDataDistributionType.PerFeature:
                case MeteoDataDistributionType.PerStation:
                    TimeSeries = functionSplitter.SplitIntoFunctionsPerArgumentValue(meteoData.Data).ToArray();
                    break;
                default:
                    throw new InvalidOperationException($@"{MeteoDataDistributionType} is not a valid value for a {nameof(MeteoDataDistributionType)}");
            }
        }

        public MultipleFunctionView.CreateBindingListDelegate CreateBindingList => CreateBindingListDelegate;

        public bool ShowNoStationsWarning =>
            MeteoDataDistributionType == MeteoDataDistributionType.PerStation &&
            StationsViewModel.Stations.Count == 0;

        public bool ShowNoFeaturesWarning =>
            MeteoDataDistributionType == MeteoDataDistributionType.PerFeature &&
            TimeSeries.Length == 0;

        public EventHandler<TableSelectionChangedEventArgs> TableSelectionChangedEventHandler => 
            OnTableSelectionChanged;

        private void OnTableSelectionChanged(object sender, TableSelectionChangedEventArgs e)
        {
            if (MeteoDataDistributionType != MeteoDataDistributionType.PerStation ||
                isSynchronizingTableSelection) return;

            isSynchronizingStationsViewModel = true;
            HashSet<string> selectedStations = e.Cells
                                                .Select(c => c.Column)
                                                .Distinct()
                                                .Select(c => c.Name)
                                                .ToHashSet();
            StationsViewModel.SetSelection(selectedStations);
            isSynchronizingStationsViewModel = false;
        }

        private static IFunctionBindingList CreateBindingListDelegate(IEnumerable<IFunction> functions)
        {
            return new SplitFunctionsBindingList(functions);
        }

        private void OnCatchmentsChanged(object sender, EventArgs e)
        {
            if (!Equals(sender, meteoData) || MeteoDataDistributionType != MeteoDataDistributionType.PerFeature)
            {
                return;
            }

            RefreshTimeSeries();
            OnPropertyChanged(nameof(ShowNoFeaturesWarning));
        }

        private void OnMeteoDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Equals(sender, meteoData)) return;

            if (e.PropertyName == nameof(IMeteoData.DataDistributionType))
            {
                OnPropertyChanged(nameof(MeteoDataDistributionType)); 
                RefreshTimeSeries();
                OnPropertyChanged(nameof(CreateBindingList));
                OnPropertyChanged(nameof(ShowNoStationsWarning));
                OnPropertyChanged(nameof(ShowNoFeaturesWarning));
            }
            else if (e.PropertyName == nameof(IMeteoData.Data))
            {
                RefreshTimeSeries();
                OnPropertyChanged(nameof(CreateBindingList));
            }
        }

        private void OnGenerateTimeSeries(object _)
        {
            IVariable<DateTime> localTimeSeries = GetTimeVariable();

            if (localTimeSeries == null)
            {
                MessageBox.Show(Resources.MeteoEditorViewModel_OnGenerateTimeSeries_Unknown_error__no_time_series_found);
                return;
            }

            var generateDialog = new TimeSeriesGeneratorDialog();

            var hintTimeStep = meteoData.Name != null && meteoData.Name.Contains("Evap") ? TimeSpan.FromDays(1) : TimeSpan.FromHours(1);
            generateDialog.SetData(localTimeSeries, modelStartTime, modelEndTime, hintTimeStep);

            EditableObjectExtensions.BeginEdit(meteoData, "Generate/modify timeseries");

            generateDialog.ShowDialog();

            meteoData.EndEdit();
            RefreshTimeSeries();
        }

        private IVariable<DateTime> GetTimeVariable()
        {
            return meteoData.Data.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region IDisposable
        public void Dispose()
        {
            StationsViewModel.SelectedStations.CollectionChanged -= OnSelectedStationsChanged;
            StationsViewModel.Stations.CollectionChanged -= OnStationsChanged;
            StationsViewModel.Dispose();

            meteoData.PropertyChanged -= OnMeteoDataChanged;
            meteoData.CatchmentsChanged -= OnCatchmentsChanged;
        }
        #endregion
    }
}