namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators
{
    /// <summary>
    /// <see cref="IBoundarySnappingCalculatorProvider"/> defines the method required to obtain a
    /// <see cref="IBoundarySnappingCalculator"/>. This <see cref="IBoundarySnappingCalculator"/>
    /// should not be cached, instead this method should be used to retrieve the latest version of
    /// the <see cref="IBoundarySnappingCalculator"/>.
    /// </summary>
    public interface IBoundarySnappingCalculatorProvider
    {
        /// <summary>
        /// Gets the latest boundary snapping calculator.
        /// </summary>
        /// <returns>
        /// The latest version of the <see cref="IBoundarySnappingCalculator"/>,
        /// if it exists; otherwise <c>null</c>.
        /// </returns>
        IBoundarySnappingCalculator GetBoundarySnappingCalculator();
    }
}