using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters
{
    /// <summary>
    /// <see cref="ColumnVisibilitiesConverter"/> is responsibility for converting the selected
    /// time frame data options into an array of visible columns.
    /// </summary>
    /// <seealso cref="IMultiValueConverter" />
    public sealed class ColumnVisibilitiesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null &&
                values.Length == 2 &&
                TryGetFromValues(values, out HydrodynamicsInputDataType hydrodynamicsInputDataType) &&
                TryGetFromValues(values, out WindInputDataType windInputDataType) &&
                targetType == typeof(IList<bool>))
            {
                bool isVisibleHydrodynamics = hydrodynamicsInputDataType == HydrodynamicsInputDataType.TimeVarying;
                bool isVisibleWind = windInputDataType == WindInputDataType.TimeVarying;

                // Unfortunately these are implicitly linked, but these values correspond with the specified
                // columns
                return new[]
                {
                    true,                   // Time           (should always be visible)
                    isVisibleHydrodynamics, // Water Level    (Hydrodynamics)
                    isVisibleHydrodynamics, // Velocity X     (Hydrodynamics)
                    isVisibleHydrodynamics, // Velocity Y     (Hydrodynamics)
                    isVisibleWind,          // Wind Speed     (Wind)
                    isVisibleWind,          // Wind Direction (Wind)
                };
            }

            return DependencyProperty.UnsetValue;
        }

        private static bool TryGetFromValues<T>(IEnumerable<object> objects, out T value) where T : new()
        {
            foreach (object o in objects)
            {
                if (o is T v)
                {
                    value = v;
                    return true;
                }
            }

            value = new T();
            return false;
        }

        /// <summary>
        /// Converting Visibilities back is currently not supported.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException("Converting Visibilities back is currently not supported.");
    }
}