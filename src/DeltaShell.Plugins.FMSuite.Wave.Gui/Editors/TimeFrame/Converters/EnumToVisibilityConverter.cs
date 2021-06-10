using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters
{
    /// <summary>
    /// <see cref="EnumToVisibilityConverter{TEnum}"/> provides a base class for enum to visibility
    /// converters.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <seealso cref="IValueConverter" />
    public abstract class EnumToVisibilityConverter<TEnum> : IValueConverter where TEnum : struct
    {
        /// <summary>
        /// Gets or sets a value indicating whether to provide <see cref="Visibility.Hidden"/> or
        /// <see cref="Visibility.Collapsed"/>.
        /// </summary>
        protected bool CollapseHidden { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to flip the visibility.
        /// </summary>
        protected bool InvertVisibility { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TEnum currentEnum && targetType == typeof(Visibility) && parameter is TEnum expectedEnum)
            {
                return IsVisible(currentEnum, expectedEnum)
                           ? Visibility.Visible
                           : CollapseHidden ? Visibility.Collapsed : Visibility.Hidden;
            }

            return DependencyProperty.UnsetValue;
        }

        private bool IsVisible(TEnum currentValue, TEnum expectedEnumValue) =>
            Equals(currentValue, expectedEnumValue) ^ InvertVisibility;

        /// <summary>
        /// Converting Visibilities back is currently not supported.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Converting Visibilities to Enums is currently not supported.");
        }
    }
}