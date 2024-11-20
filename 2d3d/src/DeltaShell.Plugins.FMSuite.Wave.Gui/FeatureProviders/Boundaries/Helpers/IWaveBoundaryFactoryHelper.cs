using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers
{
    /// <summary>
    /// <see cref="IWaveBoundaryFactoryHelper"/> defines the set of methods used
    /// by the <see cref="Factories.IWaveBoundaryFactory"/> to obtain the correct wave
    /// boundary data from view data.
    /// </summary>
    public interface IWaveBoundaryFactoryHelper
    {
        /// <summary>
        /// Gets the snapped endpoints from the provided <paramref name="coordinates"/>
        /// and the <paramref name="boundarySnappingCalculator"/>.
        /// </summary>
        /// <param name="boundarySnappingCalculator">The boundary snapping calculator.</param>
        /// <param name="coordinates">The coordinates.</param>
        /// <returns>
        /// The set of <see cref="GridBoundaryCoordinate"/> corresponding with the endpoints
        /// contained within <paramref name="coordinates"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when there are less than 2 distinct coordinates provided.
        /// </exception>
        /// <remarks>
        /// <paramref name="boundarySnappingCalculator"/> is assumed to not be <c>null</c>.
        /// <paramref name="coordinates"/> is assumed to be ordered from smallest to highest.
        /// </remarks>
        IEnumerable<GridBoundaryCoordinate> GetSnappedEndPoints(IBoundarySnappingCalculator boundarySnappingCalculator,
                                                                IEnumerable<Coordinate> coordinates);

        /// <summary>
        /// Gets the geometric definition from the provided <paramref name="snappedCoordinates"/>.
        /// </summary>
        /// <param name="snappedCoordinates">The snapped coordinates.</param>
        /// <param name="calculator">The boundary snapping calculator.</param>
        /// <returns>
        /// A <see cref="IWaveBoundaryGeometricDefinition"/> describing the
        /// <paramref name="snappedCoordinates"/>. If no
        /// <see cref="IWaveBoundaryGeometricDefinition"/> could be constructed,
        /// then <c>null</c> is returned.
        /// </returns>
        /// <remarks>
        /// <paramref name="calculator"/> is assumed to not be <c>null</c>.
        /// </remarks>
        IWaveBoundaryGeometricDefinition GetGeometricDefinition(IEnumerable<GridBoundaryCoordinate> snappedCoordinates,
                                                                IBoundarySnappingCalculator calculator);

        /// <summary>
        /// Gets the default condition definition.
        /// </summary>
        /// <returns>
        /// The default <see cref="IWaveBoundaryConditionDefinition"/>.
        /// </returns>
        IWaveBoundaryConditionDefinition GetConditionDefinition();
    }
}