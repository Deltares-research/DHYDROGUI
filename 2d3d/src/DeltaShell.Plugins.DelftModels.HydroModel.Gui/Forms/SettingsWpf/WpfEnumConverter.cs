using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    [ValueConversion(typeof(Type), typeof(Enum[]))]
    public class WpfEnumConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// Helper to retrieve list of options for a given Enum Type.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Type type))
            {
                return DependencyProperty.UnsetValue;
            }

            //Get the enum values.
            Enum[] enumValues = Enum.GetValues(type).OfType<Enum>().ToArray();
            return enumValues;
        }

        /// <summary>
        /// Converts a value.
        /// Not really needed, yet it needs to be implemented.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}