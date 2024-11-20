namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="IGridBoundaryProvider"/> defines the method required to obtain a
    /// <see cref="IGridBoundary"/>. This <see cref="IGridBoundary"/> should not be
    /// cached, instead this method should be used to retrieve the latest version of
    /// the <see cref="IGridBoundary"/>.
    /// </summary>
    public interface IGridBoundaryProvider
    {
        /// <summary>
        /// Gets the grid boundary.
        /// </summary>
        /// <returns>
        /// The latest version of the <see cref="IGridBoundary"/>, if it exists;
        /// Otherwise <c>null</c>.
        /// </returns>
        IGridBoundary GetGridBoundary();
    }
}