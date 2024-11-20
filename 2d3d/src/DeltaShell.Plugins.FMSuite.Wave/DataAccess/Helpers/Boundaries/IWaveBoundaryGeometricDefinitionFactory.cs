using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Provides the method to create a <see cref="IWaveBoundaryGeometricDefinition"/>.
    /// </summary>
    public interface IWaveBoundaryGeometricDefinitionFactory
    {
        /// <summary>
        /// Constructs the wave boundary geometric definition from the specified
        /// <paramref name="startCoordinate"/> and <paramref name="endCoordinate"/>.
        /// </summary>
        /// <param name="startCoordinate"> The start coordinate. </param>
        /// <param name="endCoordinate"> The end coordinate. </param>
        /// <returns> The constructed wave boundary geometric definition. </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="startCoordinate"/> or <paramref name="endCoordinate"/> is<c> ull </c>.
        /// </exception>
        IWaveBoundaryGeometricDefinition ConstructWaveBoundaryGeometricDefinition(Coordinate startCoordinate,
                                                                                  Coordinate endCoordinate);

        /// <summary>
        /// Constructs the wave boundary geometric definition from the specified
        /// <paramref name="orientation"/>
        /// </summary>
        /// <param name="orientation"> The orientation . </param>
        /// <returns> The constructed wave boundary geometric definition. </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="orientation"/> is undefined.
        /// </exception>
        IWaveBoundaryGeometricDefinition ConstructWaveBoundaryGeometricDefinition(BoundaryOrientationType orientation);

        /// <summary>
        /// Determines whether the coordinates of the <paramref name="geometricDefinition"/> have been inverted,
        /// given the original <paramref name="startCoordinate"/>
        /// </summary>
        /// <param name="geometricDefinition">The geometric definition.</param>
        /// <param name="startCoordinate">The original start coordinate.</param>
        /// <returns>
        /// <c>true</c> if the ordering has been inverted; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// If no calculator exists, then <c>false</c> will be returned.
        /// </remarks>
        bool HasInvertedOrderingCoordinates(IWaveBoundaryGeometricDefinition geometricDefinition, Coordinate startCoordinate);
    }
}