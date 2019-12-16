using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries
{
    public class WaveBoundaryFactory : IWaveBoundaryFactory
    {
        // (MWT) TODO: this should be invalidated once the grid changes
        private readonly IBoundarySnappingCalculator snappingCalculator;
        private readonly IWaveBoundaryFactoryHelper factoryHelper;

        /// <summary>
        /// Creates a new instance of the <see cref="WaveBoundaryFactory"/>.
        /// </summary>
        /// <param name="snappingCalculator">The snapping calculator.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
         public WaveBoundaryFactory(IBoundarySnappingCalculator snappingCalculator,
                                    IWaveBoundaryFactoryHelper factoryHelper)
        {
            this.snappingCalculator = snappingCalculator ??
                throw new ArgumentNullException(nameof(snappingCalculator));
            this.factoryHelper = factoryHelper ??
                throw new ArgumentNullException(nameof(factoryHelper));
        }

        public IWaveBoundary ConstructWaveBoundary(ILineString geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            IEnumerable<GridBoundaryCoordinate> snappedCoordinates =
                factoryHelper.GetSnappedEndPoints(snappingCalculator, 
                                                  geometry.Coordinates);

            IWaveBoundaryGeometricDefinition geometricDefinition = 
                factoryHelper.GetGeometricDefinition(snappedCoordinates);

            return geometricDefinition != null ? new WaveBoundary(geometricDefinition) : null;
        }
    }
}