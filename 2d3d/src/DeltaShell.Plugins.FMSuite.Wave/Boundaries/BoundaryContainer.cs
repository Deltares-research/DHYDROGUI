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
        private bool definitionPerFileUsed;
        private IBoundarySnappingCalculator snappingCalculator;
        private IGridBoundary currentGridBoundary;

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

        public bool DefinitionPerFileUsed
        {
            get => definitionPerFileUsed;
            set
            {
                if (value)
                {
                    Boundaries.Clear();
                }

                definitionPerFileUsed = value;
            }
        }

        public string FilePathForBoundariesPerFile { get; set; } = string.Empty;

        public void UpdateGridBoundary(IGridBoundary gridBoundary)
        {
            currentGridBoundary = gridBoundary;
            snappingCalculator = currentGridBoundary != null ? new BoundarySnappingCalculator(currentGridBoundary) : null;
        }

        public IBoundarySnappingCalculator GetBoundarySnappingCalculator() => snappingCalculator;

        public IGridBoundary GetGridBoundary() => currentGridBoundary;
    }
}