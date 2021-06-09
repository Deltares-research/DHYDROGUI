using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [ValueConversion(typeof(IEnumerable<Coordinate>), typeof(string))]
    public class ProfileToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var coordinates = value as IEnumerable<Coordinate>;
            if (coordinates == null)
            {
                return DependencyProperty.UnsetValue;
            }

            var result = "";
            CultureInfo info = CultureInfo.InvariantCulture;
            
            foreach (var coordinate in coordinates)
            {
                double coordinateY = 1 - coordinate.Y;

                if (result.Length == 0)
                {
                    result += $"M {coordinate.X.ToString(info)},{coordinateY.ToString(info)}";
                    continue;
                }

                result += $" L {coordinate.X.ToString(info)},{coordinateY.ToString(info)}";
            }

            result += " Z";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}