using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="IBoundaryContainer"/> is responsible for managing a set of
    /// boundaries linked to the current outer grid.
    /// </summary>
    public interface IBoundaryContainer : IBoundarySnappingCalculatorProvider,
                                          IGridBoundaryProvider,
                                          IBoundaryProvider,
                                          IBoundariesPerFile
    {
        /// <summary>
        /// Updates the current <see cref="IGridBoundary"/> to
        /// <paramref name="gridBoundary"/>.
        /// </summary>
        /// <param name="gridBoundary">The grid boundary.</param>
        void UpdateGridBoundary(IGridBoundary gridBoundary);
    }
}