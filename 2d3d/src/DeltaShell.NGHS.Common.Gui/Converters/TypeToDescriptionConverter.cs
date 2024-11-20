using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using DeltaShell.NGHS.Common.Gui.Properties;

namespace DeltaShell.NGHS.Common.Gui.Converters
{
    /// <summary>
    /// <see cref="TypeToDescriptionConverter"/> defines the <see cref="IValueConverter"/>
    /// to convert a <see cref="Type"/> to its DescriptionAttribute value.
    /// </summary>
    /// <seealso cref="IValueConverter"/>
    [ValueConversion(typeof(Type), typeof(string))]
    public sealed class TypeToDescriptionConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="Type"/> to its DescriptionAttribute.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// The value of the DescriptionAttribute of <paramref name="value"/>.
        /// If the method returns <see langword="null"/>, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type typeValue && targetType == typeof(string))
            {
                return GetDescription(typeValue) ?? DependencyProperty.UnsetValue;
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Converting strings to Types is currently not supported.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(Resources.TypeToDescriptionConverter_ConvertBack_Converting_strings_to_Types_is_currently_not_supported_);
        }

        private static string GetDescription(Type type)
        {
            IEnumerable<DescriptionAttribute> descriptions = type.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>();
            return descriptions.FirstOrDefault()?.Description;
        }
    }
}