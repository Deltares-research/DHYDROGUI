using System;
using Ranorex;

namespace DHYDRO.Code
{
    /// <summary>
    ///     Transforms 2D points.
    /// </summary>
    public class Transformation
    {
        private readonly double scaleX;

        private readonly double scaleY;

        private readonly double translationX;

        private readonly double translationY;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Transformation" /> class.
        /// </summary>
        /// <param name="pixels1"> The coordinates in pixels of the first point used for the calibration. </param>
        /// <param name="coords1"> The world coordinates of the first point used for the calibration.  </param>
        /// <param name="pixels2"> The coordinates in pixels of the second point used for the calibration. </param>
        /// <param name="coords2"> The world coordinates of the second point used for the calibration.  </param>
        public Transformation(Point pixels1, Point coords1, Point pixels2, Point coords2)
        {
            this.scaleX = (pixels1.X - pixels2.X) / (coords1.X - coords2.X);
            this.scaleY = (pixels1.Y - pixels2.Y) / (coords1.Y - coords2.Y);
            this.translationX = pixels1.X - scaleX * coords1.X;
            this.translationY = pixels1.Y - scaleY * coords1.Y;
            Report.Info("scaleX= " + this.scaleX.ToString() + ", scaleY= " + this.scaleY.ToString() + ", translationX= " + this.translationX.ToString() + ", translationY= " + this.translationY.ToString() );
        }

        /// <summary>
        ///     Transforms the specified <paramref name="point" /> to another point.
        /// </summary>
        /// <param name="point"> The point to transform. </param>
        /// <returns>
        ///     The transformed point.
        /// </returns>
        public Point Execute(Point point)
        {
            var x = Math.Round(translationX + scaleX * point.X, 0);
            var y = Math.Round(translationY + scaleY * point.Y, 0);
            Report.Info("scaleX= " + this.scaleX.ToString() + ", scaleY= " + this.scaleY.ToString() + ", translationX= " + this.translationX.ToString() + ", translationY= " + this.translationY.ToString() );
            Report.Info("point: (" + point.X + "; " + point.Y + ") => X = " + x.ToString() + ";   Y = " + y.ToString());
            return new Point(x, y);
        }
    }
}