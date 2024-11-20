using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers
{
    /// <summary>
    /// <see cref="WaveBoundaryFactoryHelper"/> provides the set of methods used
    /// by the <see cref="IWaveBoundaryFactoryHelper"/> to obtain the correct wave boundary
    /// data from view data.
    /// </summary>
    /// <seealso cref="IWaveBoundaryFactoryHelper"/>
    public class WaveBoundaryFactoryHelper : IWaveBoundaryFactoryHelper
    {
        private readonly ISpatiallyDefinedDataComponentFactory componentFactory;

        /// <summary>
        /// Creates a new <see cref="WaveBoundaryFactoryHelper"/>.
        /// </summary>
        /// <param name="componentFactory">The component factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="componentFactory"/> is <c>null</c>.
        /// </exception>
        public WaveBoundaryFactoryHelper(ISpatiallyDefinedDataComponentFactory componentFactory)
        {
            Ensure.NotNull(componentFactory, nameof(componentFactory));
            this.componentFactory = componentFactory;
        }

        public IEnumerable<GridBoundaryCoordinate> GetSnappedEndPoints(IBoundarySnappingCalculator boundarySnappingCalculator,
                                                                       IEnumerable<Coordinate> coordinates)
        {
            return WaveBoundaryGeometricDefinitionFactoryHelper.GetSnappedEndPoints(boundarySnappingCalculator,
                                                                                    coordinates);
        }

        public IWaveBoundaryGeometricDefinition GetGeometricDefinition(IEnumerable<GridBoundaryCoordinate> snappedCoordinates, IBoundarySnappingCalculator calculator)
        {
            return WaveBoundaryGeometricDefinitionFactoryHelper.GetGeometricDefinition(snappedCoordinates, calculator);
        }

        public IWaveBoundaryConditionDefinition GetConditionDefinition()
        {
            var shape = new JonswapShape {PeakEnhancementFactor = 3.3};
            const BoundaryConditionPeriodType periodType =
                BoundaryConditionPeriodType.Peak;

            var dataComponent =
                componentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>>();

            return new WaveBoundaryConditionDefinition(shape,
                                                       periodType,
                                                       dataComponent);
        }
    }
}