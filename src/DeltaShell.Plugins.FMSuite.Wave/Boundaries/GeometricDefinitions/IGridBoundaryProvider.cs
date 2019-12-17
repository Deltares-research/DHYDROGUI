namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="IGridBoundaryProvider"/> defines the method required to obtain a
    /// <see cref="GridBoundary"/>. This <see cref="GridBoundary"/> should not be
    /// cached, instead this method should be used to retrieve the latest version of
    /// the <see cref="GridBoundary"/>.
    /// </summary>
    public interface IGridBoundaryProvider
    {
        /// <summary>
        /// Gets the grid boundary.
        /// </summary>
        /// <returns>
        /// The latest version of the <see cref="GridBoundary"/>, if it exists;
        /// Otherwise <c>null</c>.
        /// </returns>
        GridBoundary GetGridBoundary();
    }
}