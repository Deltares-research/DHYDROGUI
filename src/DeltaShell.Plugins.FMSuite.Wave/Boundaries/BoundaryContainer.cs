using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

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

        public void UpdateGridBoundary(GridBoundary gridBoundary)
        {
            this.gridBoundary = gridBoundary;
            snappingCalculator = this.gridBoundary != null ? new BoundarySnappingCalculator(this.gridBoundary) : null;
        }

        public IBoundarySnappingCalculator GetBoundarySnappingCalculator() => snappingCalculator;
        private IBoundarySnappingCalculator snappingCalculator = null;

        public GridBoundary GetGridBoundary() => gridBoundary;
        private GridBoundary gridBoundary = null;
    }
}