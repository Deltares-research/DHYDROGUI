using System;
using System.Globalization;
using System.Windows.Data;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Converter for showing doubles with the right culture settings (. and ,)
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(value is double) ? value : ((double)value).ToString("F3", CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double doubleValue;
            if (!(value is string) || !double.TryParse((string)value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out doubleValue)) return value;

            return doubleValue;
        }
    }
}