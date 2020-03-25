using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers
{
    /// <summary>
    /// Provides the method to create a <see cref="IWaveBoundaryGeometricDefinition" />.
    /// </summary>
    public interface IWaveBoundaryGeometricDefinitionFactory
    {
        /// <summary>
        /// Constructs the wave boundary geometric definition from the specified
        /// <paramref name="startCoordinate" /> and <paramref name="endCoordinate" />.
        /// </summary>
        /// <param name="startCoordinate"> The start coordinate. </param>
        /// <param name="endCoordinate"> The end coordinate. </param>
        /// <returns> The constructed wave boundary geometric definition. </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="startCoordinate" /> or <paramref name="endCoordinate" /> is<c> ull </c>.
        /// </exception>
        IWaveBoundaryGeometricDefinition ConstructWaveBoundaryGeometricDefinition(Coordinate startCoordinate,
                                                                                  Coordinate endCoordinate);
    }
}