using System;
using System.Globalization;
using System.Windows.Data;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    /* This is a dummy converter that only returns the value it gets. 
     * This class is required for temporary bypassing an exception in the DateTimePicker 
     * Can be removed after the update to Framework 1.4 with Xceed.Wpf.Toolkit 3.3.0
    */
    public class DummyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}