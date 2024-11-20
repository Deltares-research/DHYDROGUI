using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Converters
{
    /// <summary>
    /// <see cref="MeteoDataDistributionTypeToIsEnabledConverter"/> provides the conversion from
    /// a <see cref="MeteoDataSource"/> to whether columns can be sorted in the multiple function view.
    /// </summary>
    [ValueConversion(typeof(MeteoDataSource), typeof(bool))]
    public class MeteoDataDistributionTypeToIsEnabledConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MeteoDataSource distributionType))
            {
                return DependencyProperty.UnsetValue;
            }

            return distributionType == MeteoDataSource.UserDefined;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(string.Format(Resources.MeteoDataDistributionTypeConverter_ConvertBack_Converting_from__0___1__back_to_a__2__is_not_supported_, 
                                                          "bool", 
                                                          value,
                                                          nameof(MeteoDataSource)));
        }
    }
}