using System;
using System.Globalization;
using System.Windows.Data;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Converters
{
    public class ObjectNotNullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // not required
            return null;
        }
    }
}