using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Converters
{
    /// <summary>
    /// <see cref="TypeToVisibilityConverter"/> defines the <see cref="IValueConverter"/>
    /// to convert a <see cref="Type"/> and an additional parameter to a <see cref="Visibility"/> value.
    /// </summary>
    /// <seealso cref="IValueConverter"/>
    [ValueConversion(typeof(Type), typeof(Visibility), ParameterType = typeof(Type))]
    public class TypeToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts <paramref name="value"/> to a visibility, if it is a Type, and the <paramref name="parameter"/>
        /// is specified.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// Visible if <paramref name="value"/> matches <paramref name="parameter"/>,
        /// Collapsed if <paramref name="value"/> does not match <paramref name="parameter"/>.
        /// If the method returns <see langword="null"/>, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type currentType && targetType == typeof(Visibility) && parameter is Type expectedType)
            {
                return new BooleanToVisibilityConverter().Convert(currentType == expectedType, targetType, parameter, culture);
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Converting Visibilities to Types is currently not supported.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Converting Visibilities to Types is currently not supported.");
        }
    }
}