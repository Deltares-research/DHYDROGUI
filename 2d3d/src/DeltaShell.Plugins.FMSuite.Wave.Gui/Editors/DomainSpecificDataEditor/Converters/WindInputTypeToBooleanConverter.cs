using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Converters
{
    [ValueConversion(typeof(WindInputType), typeof(bool))]
    public class WindInputTypeToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="value"/> to a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The wind input type.</param>
        /// <param name="targetType">Type of the target, which should be <see cref="bool"/>.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns>
        /// <c>false</c> if the wind input type is a <see cref="WindInputType.SpiderWebGrid"/>;
        /// otherwise, <c>true</c>.
        /// </returns>
        /// <remarks>
        /// Returns <see cref="DependencyProperty.UnsetValue"/> if
        /// the <paramref name="value"/> is not a <see cref="WindInputType"/> or
        /// the <paramref name="targetType"/> is not a <see cref="bool"/>.
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindInputType windInputType && targetType == typeof(bool))
            {
                return windInputType != WindInputType.SpiderWebGrid;
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