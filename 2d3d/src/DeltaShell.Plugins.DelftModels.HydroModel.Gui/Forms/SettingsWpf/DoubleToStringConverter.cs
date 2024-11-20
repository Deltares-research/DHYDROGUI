using System;
using System.Globalization;
using System.Windows.Data;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <inheritdoc/>
    /// <summary>
    /// Converter for showing doubles with the right culture settings (. and ,)
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as double?)?.ToString() ?? value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue;
            if (!(value is string) || !double.TryParse((string) value, out doubleValue))
            {
                return value;
            }

            return doubleValue;
        }
    }
}