using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="BoundaryContainerSyncService"/> ensures the <see cref="IBoundaryContainer"/>
    /// of a <see cref="WaveModel"/> is correctly synchronized with the grid of the
    /// <see cref="WaveModel.OuterDomain"/>.
    /// </summary>
    public class BoundaryContainerSyncService
    {
        private readonly WaveModel model;

        /// <summary>
        /// Creates a new <see cref="BoundaryContainerSyncService"/>.
        /// </summary>
        /// <param name="model">The model to observe.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public BoundaryContainerSyncService(WaveModel model)
        {
            // The dependency on WaveModel is an unfortunate result of the lack of well designed
            // interface, such that the properties can be separated correctly from their implementation.
            Ensure.NotNull(model, nameof(model));

            this.model = model;

            ((INotifyPropertyChange) model.OuterDomain).PropertyChanged += OnOuterGridChanged;
            ((INotifyPropertyChange) model).PropertyChanging += OnOuterDomainChanging;
            ((INotifyPropertyChange) model).PropertyChanged += OnOuterDomainChanged;
        }

        private void OnOuterGridChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is WaveDomainData outerDomainData &&
                  outerDomainData == model.OuterDomain &&
                  e?.PropertyName == nameof(WaveDomainData.Grid)))
            {
                return;
            }

            HandleGridChanged(((WaveDomainData) sender).Grid);
        }

        private void OnOuterDomainChanging(object sender, PropertyChangingEventArgs e)
        {
            if (!(sender is WaveModel waveModel &&
                  e?.PropertyName == nameof(WaveModel.OuterDomain)))
            {
                return;
            }

            ((INotifyPropertyChange) waveModel.OuterDomain).PropertyChanged -= OnOuterGridChanged;
        }

        private void OnOuterDomainChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is WaveModel waveModel &&
                  e?.PropertyName == nameof(WaveModel.OuterDomain)))
            {
                return;
            }

            ((INotifyPropertyChange) waveModel.OuterDomain).PropertyChanged += OnOuterGridChanged;
            HandleGridChanged(waveModel.OuterDomain.Grid);
        }

        private void HandleGridChanged(IDiscreteGridPointCoverage outerDomainGrid)
        {
            IEnumerable<CachedBoundary> cache = SnapBoundariesToNewGrid.CreateCachedBoundaries(
                model.BoundaryContainer.Boundaries.ToList(),
                model.BoundaryContainer.GetGridBoundary());

            model.BoundaryContainer.Boundaries.Clear();
            model.BoundaryContainer.UpdateGridBoundary(CreateGridBoundary(outerDomainGrid));

            var factory = new WaveBoundaryGeometricDefinitionFactory(model.BoundaryContainer);
            IEnumerable<IWaveBoundary> reSnappedBoundaries = SnapBoundariesToNewGrid.RestoreBoundariesIfPossible(cache, factory);

            foreach (IWaveBoundary bound in reSnappedBoundaries)
            {
                model.BoundaryContainer.Boundaries.Add(bound);
            }
        }

        private static IGridBoundary CreateGridBoundary(IDiscreteGridPointCoverage outerDomainGrid)
        {
            if (outerDomainGrid == null ||
                outerDomainGrid.Size1 < 2 ||
                outerDomainGrid.Size2 < 2)
            {
                return null;
            }

            return new GridBoundary(outerDomainGrid);
        }
    }
}