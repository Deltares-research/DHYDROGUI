using System;
using System.Globalization;
using System.Windows.Data;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ValueConverters
{
    [ValueConversion(typeof(DateOnly), typeof(DateTime))]
    public class DateOnlyToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateOnly dateOnly)
            {
                return dateOnly.ToDateTime(TimeOnly.MinValue);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                var date = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
                if (date.ToDateTime(TimeOnly.MinValue) != dateTime)
                {
                    throw new ArgumentException($"Cannot convert DateTime with non-zero time to DateOnly");
                }

                return date;
            }
            return value;
        }
    }
}