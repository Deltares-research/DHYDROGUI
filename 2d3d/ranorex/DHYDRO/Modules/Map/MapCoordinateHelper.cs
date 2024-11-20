using DHYDRO.Code;
using Ranorex;
using System.Globalization;

namespace DHYDRO.Modules.Map
{
    /// <summary>
    ///     A helper for dealing with map coordinates.
    /// </summary>
    public static class MapCoordinateHelper
    {
        /// <summary>
        ///     Converts the specified <paramref name="worldCoordinatesValue" /> location string to the
        ///     corresponding pixel coordinate location string.
        /// </summary>
        /// <param name="worldCoordinatesValue"></param>
        /// <returns> A pixel coordinate location string </returns>
        /// <example>
        ///     Example of a location string:
        ///     "123;456"
        /// 
        ///     The first part defines the x-coordinate and the last part the y-coordinate, separated by ';'.
        /// </example>
        public static string GetPixelCoordinatesValue(string worldCoordinatesValue)
        {
            var split = worldCoordinatesValue.Split(';');
            var x = split[0];
            var y = split[1];

            var worldCoordinate = new Point(double.Parse(x, CultureInfo.InvariantCulture), double.Parse(y, CultureInfo.InvariantCulture));

            Point pixelCoordinate;
            try
            {
            	pixelCoordinate = Current.MapTransformation.Execute(worldCoordinate);
            }
            catch
            {
            	Report.Error("Map must first be calibrated.");
            	throw new System.InvalidOperationException("Map must first be calibrated.");
            }
            return $"{(int) pixelCoordinate.X};{(int) pixelCoordinate.Y}";
        }
    }
}