using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories
{
    /// <summary>
    /// <see cref="WaveBoundaryFactory"/> implements the method to construct
    /// <see cref="IWaveBoundary"/> from view data with the help of
    /// <see cref="IWaveBoundaryFactoryHelper"/>.
    /// </summary>
    /// <seealso cref="IWaveBoundaryFactory" />
    public class WaveBoundaryFactory : IWaveBoundaryFactory
    {
        private readonly IBoundarySnappingCalculatorProvider snappingCalculatorProvider;
        private readonly IWaveBoundaryFactoryHelper factoryHelper;

        /// <summary>
        /// Creates a new instance of the <see cref="WaveBoundaryFactory"/>.
        /// </summary>
        /// <param name="snappingCalculatorProvider">The snapping calculator provider.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
         public WaveBoundaryFactory(IBoundarySnappingCalculatorProvider snappingCalculatorProvider,
                                    IWaveBoundaryFactoryHelper factoryHelper)
        {
            this.snappingCalculatorProvider = snappingCalculatorProvider ??
                throw new ArgumentNullException(nameof(snappingCalculatorProvider));
            this.factoryHelper = factoryHelper ??
                throw new ArgumentNullException(nameof(factoryHelper));
        }

        public IWaveBoundary ConstructWaveBoundary(ILineString geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            IBoundarySnappingCalculator calculator = snappingCalculatorProvider.GetBoundarySnappingCalculator();
            if (calculator == null)
            {
                return null;
            }

            IEnumerable<GridBoundaryCoordinate> snappedCoordinates =
                factoryHelper.GetSnappedEndPoints(calculator, 
                                                  geometry.Coordinates);

            IWaveBoundaryGeometricDefinition geometricDefinition = 
                factoryHelper.GetGeometricDefinition(snappedCoordinates);

            if (geometricDefinition == null)
            {
                return null;
            }

            IWaveBoundaryConditionDefinition conditionDefinition =
                factoryHelper.GetConditionDefinition();

            return new WaveBoundary(geometricDefinition, conditionDefinition);
        }
    }
}