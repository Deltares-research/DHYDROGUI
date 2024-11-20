using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    /// <summary>
    /// Creator method to used to create instances of <see cref="LineString"/>.
    /// </summary>
    public static class LineStringCreator
    {
        /// <summary>
        /// Creates and returns a <see cref="LineString"/> object with the provided coordinates.
        /// </summary>
        /// <param name="coordinates">The collection of coordinates to put into the <see cref="LineString"/> object.</param>
        /// <returns>A <see cref="LineString"/> object with the provided collection of coordinates.</returns>
        /// <exception cref="ArgumentException">Thrown when the amount of coordinates is smaller than 2.</exception>
        public static LineString CreateLineString(IList<Coordinate> coordinates)
        {
            if (coordinates.Count < 2)
            {
                throw new ArgumentException("Cannot create poly line with less than 2 points.");
            }

            return new LineString(coordinates.ToArray());
        }
    }
}