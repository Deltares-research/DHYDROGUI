using System;
using System.Globalization;
using System.Windows.Data;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Converters
{
    /// <summary>
    /// The EmptyDoubleValueConverter is responsible for transforming empty values to
    /// double.NaN and vice versa. This allows fields to represent the absence of a
    /// number to be represented as double.NaN.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter"/>
    /// <inheritdoc cref="IValueConverter"/>
    [ValueConversion(typeof(double), typeof(string))]
    public class EmptyDoubleValueConverter : IValueConverter
    {
        /// <inheritdoc cref="IValueConverter"/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && double.IsNaN(v))
            {
                return string.Empty;
            }

            return value;
        }

        /// <inheritdoc cref="IValueConverter"/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && string.IsNullOrEmpty(s))
            {
                return double.NaN;
            }

            return value;
        }
    }
}