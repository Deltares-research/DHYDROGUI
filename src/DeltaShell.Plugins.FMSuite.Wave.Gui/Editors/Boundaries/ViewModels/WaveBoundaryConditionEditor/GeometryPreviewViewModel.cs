using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using SharpMap;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="GeometryPreviewViewModel"/> implements the view model for the geometry preview view.
    /// </summary>
    public sealed class GeometryPreviewViewModel : IRefreshGeometryView, INotifyPropertyChanged, IDisposable
    {
        private bool shouldRefreshMap;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="GeometryPreviewViewModel"/>.
        /// </summary>
        /// <param name="waveBoundary">The wave boundary.</param>
        /// <param name="supportPointDataComponentViewModel">The support point data component view model.</param>
        /// <param name="configurator">The map configurator used to configure this instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public GeometryPreviewViewModel(IWaveBoundary waveBoundary,
                                        SupportPointDataComponentViewModel supportPointDataComponentViewModel,
                                        IGeometryPreviewMapConfigurator configurator)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            Ensure.NotNull(supportPointDataComponentViewModel, nameof(supportPointDataComponentViewModel));
            Ensure.NotNull(configurator, nameof(configurator));

            var singleBoundaryProvider = new SimpleBoundaryProvider(waveBoundary);
            configurator.ConfigureMap(Map,
                                      singleBoundaryProvider,
                                      supportPointDataComponentViewModel,
                                      this);
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        public IMap Map { get; } = new Map();

        /// <summary>
        /// Boolean property used as a boolean flip. This value should be bound
        /// to the ShouldRefresh dependency property of the underlying MapView.
        /// When changed to value that it does not currently have, a refresh
        /// will be triggered in the underlying MapView.
        /// </summary>
        public bool ShouldRefreshMap
        {
            get => shouldRefreshMap;
            set
            {
                shouldRefreshMap = value;
                OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            (Map as IDisposable)?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Refreshes the geometry view.
        /// </summary>
        public void RefreshGeometryView() => ShouldRefreshMap = !ShouldRefreshMap;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}