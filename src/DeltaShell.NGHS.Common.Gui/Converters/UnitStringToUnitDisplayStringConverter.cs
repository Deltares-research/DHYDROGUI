using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeltaShell.NGHS.Common.Gui.Converters
{
    /// <summary>
    /// <see cref="UnitStringToUnitDisplayStringConverter"/> converts a Unit
    /// string, e.g. 'm' or 'Kg', to its display equivalent, respectively
    /// '[m]' and '[Kg]'.
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class UnitStringToUnitDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string unit && targetType == typeof(string))
            {
                return $"[{unit}]";
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string unit && targetType == typeof(string))
            {
                return unit.Substring(1, unit.Length - 2);
            }

            return DependencyProperty.UnsetValue;
        }
    }
}