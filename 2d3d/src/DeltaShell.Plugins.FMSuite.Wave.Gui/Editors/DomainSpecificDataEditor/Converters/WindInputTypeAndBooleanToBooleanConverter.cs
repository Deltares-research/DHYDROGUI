using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Converters
{
    /// <summary>
    /// Converter for converting a <see cref="WindInputType"/> and <see cref="bool"/> to a <see cref="bool"/>.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter"/>
    public class WindInputTypeAndBooleanToBooleanConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="values"/> to a <see cref="bool"/>.
        /// </summary>
        /// <param name="values">The values; the wind input type and the boolean value, respectively.</param>
        /// <param name="targetType">Type of the target, which should be <see cref="bool"/>.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns>
        /// <c>true</c> if the wind input type is <see cref="WindInputType.SpiderWebGrid"/>
        /// or the boolean value is <c>true</c>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Returns <see cref="DependencyProperty.UnsetValue"/> if
        /// the first value of <paramref name="values"/> is not a <see cref="WindInputType"/> or
        /// the second value of the <paramref name="values"/> is not a <see cref="bool"/> or
        /// the <paramref name="targetType"/> is not a <see cref="bool"/>.
        /// </remarks>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is WindInputType windInputType && values[1] is bool booleanValue && targetType == typeof(bool))
            {
                if (windInputType == WindInputType.SpiderWebGrid)
                {
                    return true;
                }

                return booleanValue;
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>Method is not implemented.</summary>
        /// <exception cref="NotSupportedException">Thrown when this method is called.</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}