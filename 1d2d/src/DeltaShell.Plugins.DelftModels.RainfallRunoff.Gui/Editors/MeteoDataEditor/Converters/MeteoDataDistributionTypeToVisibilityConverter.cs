using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Converters
{
    /// <summary>
    /// <see cref="MeteoDataDistributionTypeToVisibilityConverter"/> provides the conversion from
    /// a <see cref="MeteoDataDistributionType"/> to the whether the <see cref="Views.MeteoStationsListView"/>
    /// is visible.
    /// </summary>
    [ValueConversion(typeof(MeteoDataDistributionType), typeof(Visibility))]
    public class MeteoDataDistributionTypeToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MeteoDataDistributionType distributionType))
            {
                return DependencyProperty.UnsetValue;
            }

            return distributionType == MeteoDataDistributionType.PerStation ? Visibility.Visible 
                                                                            : Visibility.Collapsed;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(string.Format(Resources.MeteoDataDistributionTypeConverter_ConvertBack_Converting_from__0___1__back_to_a__2__is_not_supported_,
                                                          nameof(Visibility), 
                                                          value,
                                                          nameof(MeteoDataDistributionType)));
        }
    }
}