using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Coverages;
using NSubstitute;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// A test helper for working with the <see cref="GridBoundary"/> class in tests.
    /// </summary>
    public static class GridBoundaryTestHelper
    {
        /// <summary>
        /// Gets a <see cref="GridBoundary"/> with a mocked grid backing it.
        /// </summary>
        /// <param name="x">The x dimension.</param>
        /// <param name="y">The y dimension.</param>
        /// <param name="grid">The mocked grid.</param>
        /// <returns>
        /// A <see cref="Gridboundary"/> with <paramref name="grid"/> backing it.
        /// </returns>
        public static GridBoundary GetGridBoundaryWithMockedGrid(int x, int y, out IDiscreteGridPointCoverage grid)
        {
            grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(x);
            grid.Size2.Returns(y);

           return new GridBoundary(grid);
        }

        /// <summary>
        /// Gets a <see cref="GridBoundary"/> with a mocked grid backing it.
        /// </summary>
        /// <param name="x">The x dimension.</param>
        /// <param name="y">The y dimension.</param>
        /// <returns>
        /// A <see cref="Gridboundary"/> with a mocked grid.
        /// </returns>
        public static GridBoundary GetGridBoundaryWithMockedGrid(int x, int y)
        {
            return GetGridBoundaryWithMockedGrid(x, y, out IDiscreteGridPointCoverage _);
        }
    }
}