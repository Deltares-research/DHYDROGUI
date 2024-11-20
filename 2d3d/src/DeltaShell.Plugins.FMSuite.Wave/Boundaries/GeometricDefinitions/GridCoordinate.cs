using System;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="GridCoordinate"/> defines a discrete point on a grid.
    /// </summary>
    public class GridCoordinate
    {
        /// <summary>
        /// Create a new instance of the <see cref="GridCoordinate"/>.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either argument is smaller than 0.
        /// </exception>
        public GridCoordinate(int x, int y)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the x-coordinate of this <see cref="GridCoordinate"/>.
        /// </summary>
        /// <value>
        /// The x-coordinate.
        /// </value>
        public int X { get; }

        /// <summary>
        /// Gets the y-coordinate of this <see cref="GridCoordinate"/>.
        /// </summary>
        /// <value>
        /// The y-coordinate.
        /// </value>
        public int Y { get; }
    }
}