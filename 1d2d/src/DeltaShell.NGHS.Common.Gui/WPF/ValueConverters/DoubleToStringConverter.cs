using System;
using System.Windows.Data;

namespace DeltaShell.NGHS.Common.Gui.WPF.ValueConverters
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
            return !(value is double) ? value : ((double)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double doubleValue;
            if (!(value is string) || !double.TryParse((string)value, out doubleValue)) return value;

            return doubleValue;
        }
    }
}