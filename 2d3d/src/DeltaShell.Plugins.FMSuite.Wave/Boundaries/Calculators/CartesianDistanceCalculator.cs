using System;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators
{
    /// <summary>
    /// <see cref="CartesianDistanceCalculator"/> implements <see cref="IDistanceCalculator"/>
    /// by calculating the distance assuming cartesian coordinates.
    /// </summary>
    /// <seealso cref="IDistanceCalculator"/>
    public class CartesianDistanceCalculator : IDistanceCalculator
    {
        public double CalculateDistanceSquared(Coordinate coordinateA, Coordinate coordinateB)
        {
            if (coordinateA == null)
            {
                throw new ArgumentNullException(nameof(coordinateA));
            }

            if (coordinateB == null)
            {
                throw new ArgumentNullException(nameof(coordinateB));
            }

            double xDiff = coordinateA.X - coordinateB.X;
            double yDiff = coordinateA.Y - coordinateB.Y;
            return (xDiff * xDiff) + (yDiff * yDiff);
        }

        public double CalculateDistance(Coordinate coordinateA, Coordinate coordinateB)
        {
            return Math.Sqrt(CalculateDistanceSquared(coordinateA, coordinateB));
        }
    }
}