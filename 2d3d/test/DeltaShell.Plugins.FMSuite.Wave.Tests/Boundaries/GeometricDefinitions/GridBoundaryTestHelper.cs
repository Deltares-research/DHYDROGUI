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
        /// Gets a valid grid mock with the specified dimensions.
        /// </summary>
        /// <param name="sizeX">The size x.</param>
        /// <param name="sizeY">The size y.</param>
        /// <returns>A valid grid mock with the specified dimensions.</returns>
        public static IDiscreteGridPointCoverage GetValidGridMock(int sizeX, int sizeY)
        {
            var result = Substitute.For<IDiscreteGridPointCoverage>();
            result.Size1.Returns(sizeX);
            result.Size2.Returns(sizeY);

            for (var j = 0; j < sizeY; j++)
            {
                for (var i = 0; i < sizeX; i++)
                {
                    result.X.Values[i, j] = i;
                    result.Y.Values[i, j] = j;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a <see cref="GridBoundary"/> with a mocked grid backing it.
        /// </summary>
        /// <param name="x">The x dimension.</param>
        /// <param name="y">The y dimension.</param>
        /// <param name="grid">The mocked grid.</param>
        /// <returns>
        /// A <see cref="GridBoundary"/> with <paramref name="grid"/> backing it.
        /// </returns>
        public static GridBoundary GetGridBoundaryWithMockedGrid(int x, int y, out IDiscreteGridPointCoverage grid)
        {
            grid = GetValidGridMock(x, y);
            return new GridBoundary(grid);
        }

        /// <summary>
        /// Gets a <see cref="GridBoundary"/> with a mocked grid backing it.
        /// </summary>
        /// <param name="x">The x dimension.</param>
        /// <param name="y">The y dimension.</param>
        /// <returns>
        /// A <see cref="GridBoundary"/> with a mocked grid.
        /// </returns>
        public static GridBoundary GetGridBoundaryWithMockedGrid(int x, int y)
        {
            return GetGridBoundaryWithMockedGrid(x, y, out IDiscreteGridPointCoverage _);
        }
    }
}