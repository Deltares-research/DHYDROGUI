using System;
using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
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
        private readonly IBoundaryContainer boundaryContainer;

        /// <summary>
        /// Creates a new <see cref="BoundaryContainerSyncService"/>.
        /// </summary>
        /// <param name="model">The model to observe.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public BoundaryContainerSyncService(WaveModel model)
        {
            // The dependency on WaveModel is an unfortunate result of the lack of well designed
            // interface, such that the properties can be separated correctly from their implementation.
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            boundaryContainer = model.BoundaryContainer;

            ((INotifyPropertyChange) model.OuterDomain).PropertyChanged += OnOuterGridChanged;
            ((INotifyPropertyChange) model).PropertyChanging += OnOuterDomainChanging;
            ((INotifyPropertyChange) model).PropertyChanged += OnOuterDomainChanged;
        }

        private void OnOuterGridChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is WaveDomainData outerDomain && 
                  e?.PropertyName == nameof(WaveDomainData.Grid)))
            {
                return;
            }

            HandleGridChanged(outerDomain.Grid);
        }

        private void OnOuterDomainChanging(object sender, PropertyChangingEventArgs e)
        {
            if (!(sender is WaveModel model &&
                  e?.PropertyName == nameof(WaveModel.OuterDomain)))
            {
                return;
            }

            ((INotifyPropertyChange) model.OuterDomain).PropertyChanged -= OnOuterGridChanged;
        }

        private void OnOuterDomainChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is WaveModel model &&
                  e?.PropertyName == nameof(WaveModel.OuterDomain)))
            {
                return;
            }

            ((INotifyPropertyChange) model.OuterDomain).PropertyChanged += OnOuterGridChanged;
            HandleGridChanged(model.OuterDomain.Grid);
        }

        private void HandleGridChanged(IDiscreteGridPointCoverage outerDomainGrid)
        {
            boundaryContainer.Boundaries.Clear();
            boundaryContainer.UpdateSnappingCalculator(CreateSnappingCalculator(outerDomainGrid));
        }

        private static IBoundarySnappingCalculator CreateSnappingCalculator(IDiscreteGridPointCoverage outerDomainGrid)
        {
            if (outerDomainGrid == null   || 
                outerDomainGrid.Size1 < 2 ||
                outerDomainGrid.Size2 < 2)
            {
                return null;
            }

            var boundary = new GridBoundary(outerDomainGrid);
            return new BoundarySnappingCalculator(boundary);
        }
    }
}