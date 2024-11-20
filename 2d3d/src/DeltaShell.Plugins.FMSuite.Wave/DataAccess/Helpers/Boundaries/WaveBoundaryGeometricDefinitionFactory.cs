using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Provides the method to create a <see cref="IWaveBoundaryGeometricDefinition"/>.
    /// </summary>
    public class WaveBoundaryGeometricDefinitionFactory : IWaveBoundaryGeometricDefinitionFactory
    {
        private readonly IBoundarySnappingCalculatorProvider snappingCalculatorProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveBoundaryGeometricDefinitionFactory"/> class.
        /// </summary>
        /// <param name="snappingCalculatorProvider"> The snapping calculator provider. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="snappingCalculatorProvider"/> is <c>null</c>.
        /// </exception>
        public WaveBoundaryGeometricDefinitionFactory(IBoundarySnappingCalculatorProvider snappingCalculatorProvider)
        {
            Ensure.NotNull(snappingCalculatorProvider, nameof(snappingCalculatorProvider));

            this.snappingCalculatorProvider = snappingCalculatorProvider;
        }

        public IWaveBoundaryGeometricDefinition ConstructWaveBoundaryGeometricDefinition(Coordinate startCoordinate,
                                                                                         Coordinate endCoordinate)
        {
            Ensure.NotNull(startCoordinate, nameof(startCoordinate));
            Ensure.NotNull(endCoordinate, nameof(endCoordinate));

            Coordinate[] coordinates =
            {
                startCoordinate,
                endCoordinate
            };

            IBoundarySnappingCalculator calculator = snappingCalculatorProvider.GetBoundarySnappingCalculator();
            if (calculator == null)
            {
                return null;
            }

            IEnumerable<GridBoundaryCoordinate> snappedEndPoints =
                WaveBoundaryGeometricDefinitionFactoryHelper.GetSnappedEndPoints(calculator, coordinates);

            return WaveBoundaryGeometricDefinitionFactoryHelper.GetGeometricDefinition(snappedEndPoints, calculator);
        }

        public IWaveBoundaryGeometricDefinition ConstructWaveBoundaryGeometricDefinition(BoundaryOrientationType orientation)
        {
            Ensure.IsDefined(orientation, nameof(orientation));

            IBoundarySnappingCalculator calculator = snappingCalculatorProvider.GetBoundarySnappingCalculator();
            if (calculator == null)
            {
                return null;
            }

            return WaveBoundaryGeometricDefinitionFactoryHelper.GetGeometricDefinition(orientation, calculator);
        }

        public bool HasInvertedOrderingCoordinates(IWaveBoundaryGeometricDefinition geometricDefinition,
                                                   Coordinate startCoordinate)
        {
            Ensure.NotNull(geometricDefinition, nameof(geometricDefinition));
            Ensure.NotNull(startCoordinate, nameof(startCoordinate));

            IBoundarySnappingCalculator calculator = snappingCalculatorProvider.GetBoundarySnappingCalculator();
            if (calculator == null)
            {
                return false;
            }

            var startGridBoundaryCoordinate =
                new GridBoundaryCoordinate(geometricDefinition.GridSide,
                                           geometricDefinition.StartingIndex);

            Coordinate currentStartCoordinate =
                calculator.GridBoundary.GetWorldCoordinateFromBoundaryCoordinate(startGridBoundaryCoordinate);

            return !currentStartCoordinate.Equals2D(startCoordinate, 0.00001);
        }
    }
}