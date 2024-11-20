using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Converters
{
    [ValueConversion(typeof(WindInputType), typeof(Visibility), ParameterType = typeof(WindInputType))]
    public class WindInputTypeToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="value"/> to a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">The wind input type.</param>
        /// <param name="targetType">Type of the target, which should be <see cref="Visibility"/>.</param>
        /// <param name="parameter">The wind input type.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns>
        /// <see cref="Visibility.Visible"/> if <paramref name="value"/> equals the <paramref name="parameter"/>;
        /// otherwise, false;
        /// </returns>
        /// <remarks>
        /// Returns <see cref="DependencyProperty.UnsetValue"/> if
        /// the <paramref name="value"/> is not a <see cref="WindInputType"/> or
        /// the <paramref name="targetType"/> is not a <see cref="Visibility"/> or
        /// the <paramref name="parameter"/> is not a <see cref="WindInputType"/>.
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindInputType windInputType && targetType == typeof(Visibility) && parameter is WindInputType fieldInputType)
            {
                return new BooleanToVisibilityConverter().Convert(windInputType == fieldInputType, targetType, parameter, culture);
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