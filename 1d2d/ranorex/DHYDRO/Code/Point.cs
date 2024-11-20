namespace DHYDRO.Code
{
    /// <summary>
    ///     Represents an ordered pair of double x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    public class Point
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> class.
        /// </summary>
        /// <param name="x">The horizontal position of the point. </param>
        /// <param name="y">The vertical position of the point. </param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///     The horizontal position of this point.
        /// </summary>
        public double X { get; }

        /// <summary>
        ///     The vertical position of this point.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Gets a value indicating whether this Point is empty.
        /// </summary>
        public bool IsEmpty => X == 0 && Y == 0;

        /// <summary>
        /// Represents a Point that has X and Y values set to zero.
        /// </summary>
        public static Point Empty { get; } = new Point(0, 0);
    }
}