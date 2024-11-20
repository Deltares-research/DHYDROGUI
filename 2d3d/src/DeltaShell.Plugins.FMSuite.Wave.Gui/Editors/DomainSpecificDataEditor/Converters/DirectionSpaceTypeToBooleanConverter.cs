using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Converters
{
    /// <summary>
    /// Converter for converting a <see cref="DirectionalSpaceType"/> to a <see cref="bool"/>.
    /// </summary>
    [ValueConversion(typeof(DirectionalSpaceType), typeof(bool))]
    public class DirectionSpaceTypeToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="value"/> to a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The directional space type.</param>
        /// <param name="targetType">Type of the target, which should be <see cref="bool"/>.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns>
        /// <c>true</c> if the directional space type is a <see cref="DirectionalSpaceType.Sector"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Returns <see cref="DependencyProperty.UnsetValue"/> if
        /// the <paramref name="value"/> is not a <see cref="DirectionalSpaceType"/> or
        /// the <paramref name="targetType"/> is not a <see cref="bool"/>.
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectionalSpaceType directionalSpaceType && targetType == typeof(bool))
            {
                return directionalSpaceType == DirectionalSpaceType.Sector;
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