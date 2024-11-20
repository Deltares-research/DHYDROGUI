using System;
using System.ComponentModel;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Adapters;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels
{
    /// <summary>
    /// A view model for the evaporation meteo data editor.
    /// </summary>
    public class MeteoEditorEvaporationViewModel : BaseMeteoEditorViewModel <IEvaporationMeteoData> 
    {
        /// <summary>
        /// Creates a new <see cref="MeteoEditorEvaporationViewModel"/> with the given data.
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
        public MeteoEditorEvaporationViewModel(IEvaporationMeteoData meteoData,
                                               IMeteoStationsListViewModel meteoStationsListViewModel,
                                               ITableViewMeteoStationSelectionAdapter tableSelectionAdapter,
                                               ITimeDependentFunctionSplitter functionSplitter,
                                               DateTime modelStartTime,
                                               DateTime modelEndTime) : base( meteoData,
                                                                              meteoStationsListViewModel,
                                                                              tableSelectionAdapter,
                                                                              functionSplitter,
                                                                              modelStartTime,
                                                                              modelEndTime)
        {
            meteoData.PropertyChanged += MeteoDataPropertyChanged;
            RefreshTimeSeries();
        }
        
        public override bool CanEditActiveMeteoDataSource => PossibleMeteoDataSources.Length > 1 && MeteoDataDistributionType == MeteoDataDistributionType.Global;
        public override bool ShowYears => ActiveMeteoDataSource == MeteoDataSource.UserDefined;

        public override MeteoDataSource ActiveMeteoDataSource
        {
            get => meteoData.SelectedMeteoDataSource;
            set
            {
                if (meteoData.SelectedMeteoDataSource == value)
                {
                    return;
                }
                meteoData.SelectedMeteoDataSource = value;
                OnSelectedMeteoDataSourceChanged();
            }
        }
        
        public override MeteoDataSource[] PossibleMeteoDataSources
        {
            get
            {
                return new []
                { 
                    MeteoDataSource.UserDefined,
                    MeteoDataSource.GuidelineSewerSystems, 
                    MeteoDataSource.LongTermAverage
                };
            }
        }
        
        public override MeteoDataDistributionType MeteoDataDistributionType
        {
            get => meteoData.DataDistributionType;
            set
            {
                if (value == meteoData.DataDistributionType)
                {
                    return;
                }
                if (value != MeteoDataDistributionType.Global)
                {
                    ActiveMeteoDataSource = MeteoDataSource.UserDefined;
                }

                meteoData.DataDistributionType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEditActiveMeteoDataSource));
            }
        }

        public override IFunction[] TimeSeries
        {
            get => timeSeries;
            protected set
            {
                if (TimeSeries == value)
                {
                    return;
                }

                timeSeries = value;
                OnPropertyChanged();
            }
        }

        private void MeteoDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IEvaporationMeteoData.SelectedMeteoDataSource))
            {
                OnSelectedMeteoDataSourceChanged();
            }
        }

        private void OnSelectedMeteoDataSourceChanged()
        {
            RefreshTimeSeries();

            OnPropertyChanged(nameof(ActiveMeteoDataSource));
            OnPropertyChanged(nameof(ShowYears));
        }

        #region IDisposable
        private bool disposed = false;
        
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                meteoData.PropertyChanged -= MeteoDataPropertyChanged;
            }

            base.Dispose(disposing);

            disposed = true;
        }
        #endregion
    }
}