using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="BoundaryContainer"/> is responsible for managing a set of
    /// boundaries linked to the current outer grid.
    /// </summary>
    public class BoundaryContainer
    {
        /// <summary>
        /// Get the boundaries.
        /// </summary>
        /// <value>
        /// The boundaries.
        /// </value>
        /// <remarks>
        /// This class is the logical owner of the <see cref="IWaveBoundary"/>
        /// within the model it is part of.
        /// </remarks>
        public IEventedList<IWaveBoundary> Boundaries { get; } = new EventedList<IWaveBoundary>();
    }
}