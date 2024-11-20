using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories
{
    /// <summary>
    /// <see cref="IWaveBoundaryFactory"/> provides the method to construct
    /// <see cref="IWaveBoundary"/> from view data.
    /// </summary>
    public interface IWaveBoundaryFactory
    {
        /// <summary>
        /// Constructs a <see cref="IWaveBoundary"/> from the given <paramref name="geometry"/>.
        /// </summary>
        /// <param name="geometry">The geometry from which to construct the <see cref="IWaveBoundary"/>.</param>
        /// <returns>
        /// The <see cref="IWaveBoundary"/> corresponding with the specified
        /// geometry. If no <see cref="IWaveBoundary"/> cannot be constructed,
        /// then <c>null</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="geometry"/> is <c>null</c>.
        /// </exception>
        IWaveBoundary ConstructWaveBoundary(ILineString geometry);
    }
}