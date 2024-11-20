using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Converters
{
    /// <summary>
    /// Converter for converting a file path to its file name
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter"/>
    [ValueConversion(typeof(string), typeof(string))]
    public class FilePathToFileNameConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="value"/> as a file path to the file name.
        /// </summary>
        /// <param name="value">The file path</param>
        /// <param name="targetType">Type of the target, which should be <see cref="string"/>.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns> The file name. </returns>
        /// <remarks>
        /// <see cref="DependencyProperty.UnsetValue"/> is returned if
        /// the <paramref name="value"/> is not a <see cref="string"/> or
        /// the <paramref name="targetType"/> is not a <see cref="string"/>.
        /// the <paramref name="value"/> is returned if the specified <paramref name="value"/> is not recognized as a valid path, .
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && targetType == typeof(string))
            {
                try
                {
                    return Path.GetFileName(stringValue);
                }
                catch
                {
                    return value;
                }
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