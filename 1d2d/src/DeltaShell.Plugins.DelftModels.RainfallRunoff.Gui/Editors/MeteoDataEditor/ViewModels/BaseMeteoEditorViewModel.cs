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
using Deltares.Infrastructure.API.Guards;
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
    public class BaseMeteoEditorViewModel<T> : IMeteoEditorViewModel where T : class, IMeteoData
    {
        protected readonly T meteoData;
        private readonly ITableViewMeteoStationSelectionAdapter tableSelectionAdapter;
        private readonly ITimeDependentFunctionSplitter functionSplitter;

        private bool isSynchronizingTableSelection = false;
        private bool isSynchronizingStationsViewModel = false;

        private readonly DateTime modelStartTime;
        private readonly DateTime modelEndTime;

        protected IFunction[] timeSeries;
        private const string catchment = "Catchment";

        /// <summary>
        /// Creates a new <see cref="MeteoEditorViewModel"/> with the given data.
        /// </summary>
        /// <param name="meteoData">The meteo data to visualize and edit.</param>
        /// <param name="meteoStationsListViewModel">The view model for the stations list.</param>
        /// <param name="tableSelectionAdapter">The table selection adapter.</param>
        /// <param name="functionSplitter">The function splitter used to split a <see cref="IMeteoData.Data"/></param>
        /// <param name="modelStartTime">The model start time.</param>
        /// <param name="modelEndTime">The model end time.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        protected BaseMeteoEditorViewModel(T meteoData,
                                    IMeteoStationsListViewModel meteoStationsListViewModel,
                                    ITableViewMeteoStationSelectionAdapter tableSelectionAdapter,
                                    ITimeDependentFunctionSplitter functionSplitter,
                                    DateTime modelStartTime,
                                    DateTime modelEndTime)
        {
            Ensure.NotNull(meteoData, nameof(meteoData));
            Ensure.NotNull(meteoStationsListViewModel, nameof(meteoStationsListViewModel));
            Ensure.NotNull(tableSelectionAdapter, nameof(tableSelectionAdapter));
            Ensure.NotNull(functionSplitter, nameof(functionSplitter));

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

        public virtual MeteoDataSource ActiveMeteoDataSource
        {
            get => MeteoDataSource.UserDefined;
            set => throw new NotSupportedException();
        }

        public virtual MeteoDataSource[] PossibleMeteoDataSources
        {
            get
            {
                return new []
                {
                    MeteoDataSource.UserDefined
                };
            }
        }

        public virtual bool CanEditActiveMeteoDataSource => false;

        public virtual MeteoDataDistributionType MeteoDataDistributionType
        {
            get => meteoData.DataDistributionType;
            set
            {
                if (value == meteoData.DataDistributionType) return;

                meteoData.DataDistributionType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEditActiveMeteoDataSource));
            }
        }

        public virtual IFunction[] TimeSeries
        {
            get => timeSeries;
            protected set
            {
                if (TimeSeries == value) return;

                timeSeries = value;
                OnPropertyChanged();
            }
        }

        protected void RefreshTimeSeries()
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
            ResetTimeSeriesWhenEmpty();
        }
        
        private void ResetTimeSeriesWhenEmpty()
        {
            if (TimeSeries.Length == 0)
            {
                TimeSeries = new[] { meteoData.Data };
            }
        }

        public MultipleFunctionView.CreateBindingListDelegate CreateBindingList => CreateBindingListDelegate;

        public bool ShowNoStationsWarning =>
            MeteoDataDistributionType == MeteoDataDistributionType.PerStation &&
            StationsViewModel.Stations.Count == 0;

        public bool ShowNoFeaturesWarning =>
            MeteoDataDistributionType == MeteoDataDistributionType.PerFeature &&
            NoCatchmentAvailable();

        private bool NoCatchmentAvailable()
        {
            return meteoData.Data.Arguments.First(variable => variable.Name.Equals(catchment)).Values.Count == 0;
        }

        public EventHandler<TableSelectionChangedEventArgs> TableSelectionChangedEventHandler => 
            OnTableSelectionChanged;

        public virtual bool ShowYears => true;

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

            if (ShowNoStationsWarning)
            {
                MessageBox.Show(Resources.BaseMeteoEditorViewModel_OnGenerateTimeSeries_Please_add_a_station_before_generating_time_series);
                return;
            }
            
            if (ShowNoFeaturesWarning)
            {
                MessageBox.Show(Resources.BaseMeteoEditorViewModel_OnGenerateTimeSeries_Please_add_a_feature_before_generating_time_series);
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region IDisposable

        private bool disposed = false;
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                StationsViewModel.SelectedStations.CollectionChanged -= OnSelectedStationsChanged;
                StationsViewModel.Stations.CollectionChanged -= OnStationsChanged;
                StationsViewModel.Dispose();

                meteoData.PropertyChanged -= OnMeteoDataChanged;
                meteoData.CatchmentsChanged -= OnCatchmentsChanged;
            }

            disposed = true;
        }
        
        #endregion
    }
}