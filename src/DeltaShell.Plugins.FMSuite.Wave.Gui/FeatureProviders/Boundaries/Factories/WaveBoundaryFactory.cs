using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
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
    /// <seealso cref="IWaveBoundaryFactory"/>
    public sealed class WaveBoundaryFactory : IWaveBoundaryFactory
    {
        private readonly IBoundarySnappingCalculatorProvider snappingCalculatorProvider;
        private readonly IWaveBoundaryFactoryHelper factoryHelper;
        private readonly IUniqueBoundaryNameProvider nameProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="WaveBoundaryFactory"/>.
        /// </summary>
        /// <param name="snappingCalculatorProvider">The snapping calculator provider.</param>
        /// <param name="factoryHelper">The factory helper.</param>
        /// <param name="nameProvider">The unique boundary name provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        public WaveBoundaryFactory(IBoundarySnappingCalculatorProvider snappingCalculatorProvider,
                                   IWaveBoundaryFactoryHelper factoryHelper,
                                   IUniqueBoundaryNameProvider nameProvider)
        {
            Ensure.NotNull(snappingCalculatorProvider, nameof(snappingCalculatorProvider));
            Ensure.NotNull(factoryHelper, nameof(factoryHelper));
            Ensure.NotNull(nameProvider, nameof(nameProvider));

            this.snappingCalculatorProvider = snappingCalculatorProvider;
            this.factoryHelper = factoryHelper;
            this.nameProvider = nameProvider;
        }

        public IWaveBoundary ConstructWaveBoundary(ILineString geometry)
        {
            Ensure.NotNull(geometry, nameof(geometry));

            IBoundarySnappingCalculator calculator = snappingCalculatorProvider.GetBoundarySnappingCalculator();
            if (calculator == null)
            {
                return null;
            }

            IEnumerable<GridBoundaryCoordinate> snappedCoordinates =
                factoryHelper.GetSnappedEndPoints(calculator,
                                                  geometry.Coordinates);

            IWaveBoundaryGeometricDefinition geometricDefinition =
                factoryHelper.GetGeometricDefinition(snappedCoordinates, calculator);

            if (geometricDefinition == null)
            {
                return null;
            }

            IWaveBoundaryConditionDefinition conditionDefinition =
                factoryHelper.GetConditionDefinition();

            string newName = nameProvider.GetUniqueName();

            return new WaveBoundary(newName, geometricDefinition, conditionDefinition);
        }
    }
}