using Deltares.Infrastructure.API.Guards;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// Simple struct that holds an <see cref="IWaveBoundary"/> and the Begin and ending
    /// <see cref="GeometricDefinitions.SupportPoint"/> coordinates
    /// in WorldSpace.
    /// </summary>
    internal class CachedBoundary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedBoundary"/> class.
        /// </summary>
        /// <param name="startingPointWorldCoordinate">World coordinate for the <see cref="IWaveBoundary"/> begin point</param>
        /// <param name="endingPointWorldCoordinate">World coordinate for the <see cref="IWaveBoundary"/> end point</param>
        /// <param name="waveBoundary">the <see cref="IWaveBoundary"/></param>
        /// <exception cref="System.ArgumentNullException">Is thrown is any of the parameters are null.</exception>
        public CachedBoundary(Coordinate startingPointWorldCoordinate, Coordinate endingPointWorldCoordinate, IWaveBoundary waveBoundary)
        {
            Ensure.NotNull(startingPointWorldCoordinate, nameof(startingPointWorldCoordinate));
            Ensure.NotNull(endingPointWorldCoordinate, nameof(endingPointWorldCoordinate));
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));

            StartingPointWorldCoordinate = startingPointWorldCoordinate;
            EndingPointWorldCoordinate = endingPointWorldCoordinate;
            WaveBoundary = waveBoundary;
        }

        /// <summary>
        /// Gets the <see cref="Coordinate"/> in worldspace of the <see cref="IWaveBoundary"/> begin point
        /// </summary>
        public Coordinate StartingPointWorldCoordinate { get; }

        /// <summary>
        /// Gets the <see cref="Coordinate"/> in worldspace of the <see cref="IWaveBoundary"/> end point
        /// </summary>
        public Coordinate EndingPointWorldCoordinate { get; }

        /// <summary>
        /// The <see cref="IWaveBoundary"/>
        /// </summary>
        public IWaveBoundary WaveBoundary { get; }
    }
}