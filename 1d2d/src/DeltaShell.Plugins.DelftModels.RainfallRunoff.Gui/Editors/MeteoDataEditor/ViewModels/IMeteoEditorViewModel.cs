using System;
using System.ComponentModel;
using DelftTools.Controls;
using DelftTools.Functions;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using ICommand = System.Windows.Input.ICommand;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels
{
    /// <summary>
    /// <see cref="IMeteoEditorViewModel"/> defines the available properties and
    /// methods of the view model associated with the <see cref="Views.MeteoEditorView"/>.
    /// </summary>
    public interface IMeteoEditorViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets the <see cref="System.Windows.Input.ICommand"/> to generate a time series with user input.
        /// </summary>
        ICommand GenerateTimeSeriesCommand { get; }

        /// <summary>
        /// Gets the <see cref="IMeteoStationsListViewModel"/> used to visualize the
        /// <see cref="Views.MeteoStationsListView"/>.
        /// </summary>
        IMeteoStationsListViewModel StationsViewModel { get; }

        /// <summary>
        /// Gets the <see cref="MeteoDataSource"/> that is currently active.
        /// </summary>
        MeteoDataSource ActiveMeteoDataSource { get; set; }

        /// <summary>
        /// Gets the array of possible <see cref="MeteoDataSource"/>.
        /// </summary>
        MeteoDataSource[] PossibleMeteoDataSources { get; }

        /// <summary>
        /// Gets whether the <see cref="ActiveMeteoDataSource"/> can be edited.
        /// </summary>
        bool CanEditActiveMeteoDataSource { get; }

        /// <summary>
        /// Gets the current <see cref="MeteoDataDistributionType"/>.
        /// </summary>
        MeteoDataDistributionType MeteoDataDistributionType { get; set; }

        /// <summary>
        /// Gets the array of <see cref="IFunction"/> objects which make up the time series
        /// of the <see cref="MeteoData"/>
        /// </summary>
        IFunction[] TimeSeries { get; }

        /// <summary>
        /// Gets the <see cref="CreateBindingList"/> used to visualize the <see cref="TimeSeries"/>.
        /// </summary>
        MultipleFunctionView.CreateBindingListDelegate CreateBindingList { get; }

        /// <summary>
        /// Gets whether to show the no stations warning.
        /// </summary>
        bool ShowNoStationsWarning { get; }

        /// <summary>
        /// Gets whether to show the no catchments warning.
        /// </summary>
        bool ShowNoFeaturesWarning { get; } 
        
        /// <summary>
        /// Gets the <see cref="EventHandler"/> used to handle changes in the table selection.
        /// </summary>
        EventHandler<TableSelectionChangedEventArgs> TableSelectionChangedEventHandler { get; }
    }
}