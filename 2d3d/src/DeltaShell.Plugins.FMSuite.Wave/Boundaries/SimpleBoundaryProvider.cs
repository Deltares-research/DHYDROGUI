using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="SimpleBoundaryProvider"/> implements the <see cref="IBoundaryProvider"/>
    /// interface containing the specified boundaries.
    /// </summary>
    /// <seealso cref="IBoundaryProvider"/>
    public class SimpleBoundaryProvider : IBoundaryProvider
    {
        /// <summary>
        /// Creates a new <see cref="SimpleBoundaryProvider"/>.
        /// </summary>
        /// <param name="waveBoundaries">The wave boundaries.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveBoundaries"/> is <c>null</c>.
        /// </exception>
        public SimpleBoundaryProvider(params IWaveBoundary[] waveBoundaries)
        {
            Ensure.NotNull(waveBoundaries, nameof(waveBoundaries));
            Boundaries = new EventedList<IWaveBoundary>(waveBoundaries);
        }

        public IEventedList<IWaveBoundary> Boundaries { get; }
    }
}