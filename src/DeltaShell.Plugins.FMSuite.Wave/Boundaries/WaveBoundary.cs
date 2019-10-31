using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="WaveBoundary"/> implements a WaveBoundary.
    /// </summary>
    /// <seealso cref="IWaveBoundary" />
    public class WaveBoundary : IWaveBoundary
    {
        /// <summary>
        /// Creates a new instance of the <see cref="WaveBoundary"/>.
        /// </summary>
        /// <param name="geometricDefinition">The geometric definition.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        public WaveBoundary(IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            GeometricDefinition = geometricDefinition ?? throw new ArgumentNullException(nameof(geometricDefinition));
        }

        public IWaveBoundaryGeometricDefinition GeometricDefinition { get; }
    }
}