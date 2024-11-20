using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="IBoundaryProvider"/> defines the interface to retrieve a
    /// collection of <see cref="IWaveBoundary"/>.
    /// </summary>
    public interface IBoundaryProvider
    {
        /// <summary>
        /// Gets the boundaries defined on the current outer grid.
        /// </summary>
        /// <value>
        /// The boundaries defined on the current outer grid.
        /// </value>
        IEventedList<IWaveBoundary> Boundaries { get; }
    }
}