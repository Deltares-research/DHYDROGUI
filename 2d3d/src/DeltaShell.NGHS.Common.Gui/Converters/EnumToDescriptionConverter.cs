using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DelftTools.Utils.Reflection;

namespace DeltaShell.NGHS.Common.Gui.Converters
{
    /// <summary>
    /// Value converter for converting an <see cref="Enum"/> to its description.
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumToDescriptionConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="value"/> as an <see cref="Enum"/> to its description.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <param name="targetType">Type of the target, which should be <see cref="string"/>.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns> The enum description. </returns>
        /// <remarks>
        /// <see cref="DependencyProperty.UnsetValue"/> is returned if
        /// the <paramref name="value"/> is not an <see cref="Enum"/> or
        /// the <paramref name="targetType"/> is not a <see cref="string"/>.
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue && targetType == typeof(string))
            {
                return enumValue.GetDescription();
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>Method is not implemented.</summary>
        /// <exception cref="NotSupportedException">Thrown when this method is called.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}