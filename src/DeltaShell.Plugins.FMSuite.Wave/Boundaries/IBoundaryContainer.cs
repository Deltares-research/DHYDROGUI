using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="IBoundaryContainer"/> is responsible for managing a set of
    /// boundaries linked to the current outer grid.
    /// </summary>
    public interface IBoundaryContainer
    {
        /// <summary>
        /// Get the boundaries defined on the current outer grid.
        /// </summary>
        /// <value>
        /// The boundaries defined on the current outer grid.
        /// </value>
        IEventedList<IWaveBoundary> Boundaries { get; }
    }
}