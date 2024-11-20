using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace DeltaShell.NGHS.Common.Gui.Converters
{
    /// <summary>
    /// <see cref="EnumToVisibilityConverter"/> provides a base class for enum to visibility
    /// converters.
    /// </summary>
    /// <seealso cref="IValueConverter"/>
    public class EnumToVisibilityConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether to provide <see cref="Visibility.Hidden"/> or
        /// <see cref="Visibility.Collapsed"/>.
        /// </summary>
        public bool CollapseHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to flip the visibility.
        /// </summary>
        public bool InvertVisibility { get; set; }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        /// <summary>
        /// Converts the provided enum value to a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value"> The current enum value. </param>
        /// <param name="targetType"> The type of <see cref="Visibility"/>. </param>
        /// <param name="parameter"> The enum value for which the visibility should be <see cref="Visibility.Visible"/>.</param>
        /// <param name="culture"> This argument is unused. </param>
        /// <returns>
        /// - <see cref="Visibility.Visible"/> if <paramref name="value"/> equals <paramref name="parameter"/>.
        /// - <see cref="Visibility.Collapsed"/> if <paramref name="value"/> does not equal <paramref name="parameter"/> and
        /// <see cref="CollapseHidden"/> is <c>true.</c>.
        /// - <see cref="Visibility.Hidden"/> if <paramref name="value"/> does not equal <paramref name="parameter"/> and
        /// <see cref="CollapseHidden"/> is <c>false</c>.
        /// - <see cref="DependencyProperty.UnsetValue"/> if the argument types do not match the expectations.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum currentEnum && targetType == typeof(Visibility) && parameter is Enum expectedEnum)
            {
                return GetVisibility(currentEnum, expectedEnum);
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Converting Visibilities back is currently not supported.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Converting Visibilities to Enums is currently not supported.");
        }

        private object GetVisibility(Enum currentEnum, Enum expectedEnum)
        {
            if (IsVisible(currentEnum, expectedEnum))
            {
                return Visibility.Visible;
            }

            return CollapseHidden ? Visibility.Collapsed : Visibility.Hidden;
        }

        private bool IsVisible(Enum currentValue, Enum expectedEnumValue) =>
            Equals(currentValue, expectedEnumValue) ^ InvertVisibility;
    }
}