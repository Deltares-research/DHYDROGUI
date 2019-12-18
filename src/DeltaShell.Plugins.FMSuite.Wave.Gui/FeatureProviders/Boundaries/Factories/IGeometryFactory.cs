using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories
{
    /// <summary>
    /// <see cref="IGeometryFactory"/> provides the methods to construct
    /// geometry from a <see cref="IWaveBoundary"/>.
    /// </summary>
    public interface IGeometryFactory
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> is <c>null</c>.
        /// </exception>
        ILineString ConstructBoundaryLineGeometry(IWaveBoundary waveBoundary);

        /// <summary>
        /// Constructs the <see cref="IPoint"/> corresponding with the given
        /// <paramref name="waveBoundary"/>.
        /// </summary>
        /// <param name="waveBoundary">The wave boundary.</param>
        /// <returns>
        /// The sequence of <see cref="IPoint"/> corresponding with the endpoints
        /// of <paramref name="waveBoundary"/>. If no end points could be generated
        /// than an empty <see cref="IEnumerable{IPoint}"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> is <c>null</c>.
        /// </exception>
        IEnumerable<IPoint> ConstructBoundaryEndPoints(IWaveBoundary waveBoundary);
    }
}