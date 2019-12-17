using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="BoundaryContainer"/> is responsible for managing a set of
    /// boundaries linked to the current outer grid.
    /// </summary>
    public class BoundaryContainer : IBoundaryContainer
    {
        /// <summary>
        /// Get the boundaries defined on the current outer grid.
        /// </summary>
        /// <value>
        /// The boundaries defined on the current outer grid.
        /// </value>
        /// <remarks>
        /// This class is the logical owner of the <see cref="IWaveBoundary"/>
        /// within the model it is part of.
        /// </remarks>
        public IEventedList<IWaveBoundary> Boundaries { get; } = new EventedList<IWaveBoundary>();

        public void UpdateSnappingCalculator(IBoundarySnappingCalculator newSnappingCalculator)
        {
            snappingCalculator = newSnappingCalculator;
        }

        public IBoundarySnappingCalculator GetBoundarySnappingCalculator()
        {
            return snappingCalculator;
        }

        private IBoundarySnappingCalculator snappingCalculator = null;
    }
}