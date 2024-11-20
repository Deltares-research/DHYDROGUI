using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters
{
    /// <summary>
    /// <see cref="SpiderWebPathVisibilityConverter"/> computes the visibility of the spider web
    /// path control based on whether use spider web is active or spider web grid is selected as
    /// input.
    /// </summary>
    /// <seealso cref="IMultiValueConverter" />
    /// <remarks>
    /// The use spider web and wind input type bindings can be set in any order.
    /// </remarks>
    public sealed class SpiderWebPathVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null &&
                values.Length == 2 &&
                TryGetFromValues(values, out bool useSpiderWeb) &&
                TryGetFromValues(values, out WindInputType windInputType) &&
                targetType == typeof(Visibility))
            {
                return useSpiderWeb || (windInputType == WindInputType.SpiderWebGrid)
                           ? Visibility.Visible
                           : Visibility.Collapsed;
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
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Converting Visibilities back is currently not supported.");
        }
    }
}