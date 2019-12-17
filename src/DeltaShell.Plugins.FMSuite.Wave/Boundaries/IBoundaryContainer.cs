using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="IBoundaryContainer"/> is responsible for managing a set of
    /// boundaries linked to the current outer grid.
    /// </summary>
    public interface IBoundaryContainer : IBoundarySnappingCalculatorProvider
    {
        /// <summary>
        /// Gets the boundaries defined on the current outer grid.
        /// </summary>
        /// <value>
        /// The boundaries defined on the current outer grid.
        /// </value>
        IEventedList<IWaveBoundary> Boundaries { get; }

        /// <summary>
        /// Updates the current <see cref="IBoundarySnappingCalculator"/> to
        /// <paramref name="newSnappingCalculator"/>.
        /// </summary>
        /// <param name="newSnappingCalculator">The new snapping calculator.</param>
        void UpdateSnappingCalculator(IBoundarySnappingCalculator newSnappingCalculator);
    }
}