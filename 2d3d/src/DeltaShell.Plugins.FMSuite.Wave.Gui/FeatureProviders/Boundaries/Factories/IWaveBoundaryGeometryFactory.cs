using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories
{
    /// <summary>
    /// <see cref="IWaveBoundaryGeometryFactory"/> provides the methods to construct
    /// geometry from a <see cref="IWaveBoundary"/>.
    /// </summary>
    public interface IWaveBoundaryGeometryFactory
    {
        /// <summary>
        /// Constructs a <see cref="ILineString"/> from the given
        /// <paramref name="waveBoundary"/>.
        /// </summary>
        /// <param name="waveBoundary">
        /// The wave boundary for which to create the corresponding
        /// <see cref="ILineString"/>.
        /// </param>
        /// <returns>
        /// The <see cref="ILineString"/> displaying the <see cref="IWaveBoundary"/>.
        /// <c>null</c> if no <see cref="ILineString"/> could be created.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> is <c>null</c>.
        /// </exception>
        ILineString ConstructBoundaryLineGeometry(IWaveBoundary waveBoundary);

        /// <summary>
        /// Constructs the <see cref="IPoint"/> corresponding with the given
        /// <paramref name="waveBoundary"/> start point.
        /// </summary>
        /// <param name="waveBoundary">The wave boundary.</param>
        /// <returns>
        /// The <see cref="IPoint"/> corresponding with the start point
        /// of <paramref name="waveBoundary"/>. If no start point could be generated
        /// then <c>null</c> is returned.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> is <c>null</c>.
        /// </exception>
        IPoint ConstructBoundaryStartPoint(IWaveBoundary waveBoundary);

        /// <summary>
        /// Constructs the <see cref="IPoint"/> corresponding with the given
        /// <paramref name="waveBoundary"/> end point.
        /// </summary>
        /// <param name="waveBoundary">The wave boundary.</param>
        /// <returns>
        /// The <see cref="IPoint"/> corresponding with the end point
        /// of <paramref name="waveBoundary"/>. If no end point could be generated
        /// then <c>null</c> is returned.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> is <c>null</c>.
        /// </exception>
        IPoint ConstructBoundaryEndPoint(IWaveBoundary waveBoundary);

        /// <summary>
        /// Constructs the boundary support point geometry from the given <paramref name="supportPoint"/>.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <returns>The point geometry of the support point.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPoint"/> is <c>null</c>.
        /// </exception>
        IPoint ConstructBoundarySupportPoint(SupportPoint supportPoint);
    }
}