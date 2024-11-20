using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Converters
{
    /// <summary>
    /// <see cref="NotNullToBooleanConverter"/> defines the <see cref="IValueConverter"/>
    /// to check whether a provided value is not <c>null</c>, and map this to a boolean.
    /// </summary>
    /// <seealso cref="IValueConverter"/>
    [ValueConversion(typeof(object), typeof(bool))]
    public class NotNullToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts <paramref name="value"/> to <c>true</c> if <paramref name="value"/>
        /// is not <c>null</c>; <c>false</c> otherwise.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null"/>, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {
                return value != null;
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Converting booleans to objects is not possible.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Converting booleans to objects is not possible.");
        }
    }
}