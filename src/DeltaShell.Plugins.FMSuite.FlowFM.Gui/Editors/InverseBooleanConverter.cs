using System;
using System.Globalization;
using System.Windows.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    // Temporary duplicate class, eventually we want to use the converters in the framework
    // TODO: this class should be removed and replaced when we upgrade to Framework 1.3
    // Currently, (Framework\Common\src\DelftTools.Controls.Wpf\ValueConverters\InverseBooleanConverter.cs)
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        #endregion
    }
}
