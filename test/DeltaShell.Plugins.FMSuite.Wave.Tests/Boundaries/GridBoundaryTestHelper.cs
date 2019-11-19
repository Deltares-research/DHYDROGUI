using GeoAPI.Extensions.Coverages;
using NSubstitute;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.Calculators
{
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

            return result;
        }
    }
}