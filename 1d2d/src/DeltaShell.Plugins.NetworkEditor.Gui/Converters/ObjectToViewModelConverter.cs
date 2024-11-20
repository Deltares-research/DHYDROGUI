using System;
using System.Globalization;
using System.Windows.Data;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Converters
{
    /// <summary>
    /// Class for converting objects to their corresponding view model.
    /// </summary>
    public class ObjectToViewModelConverter : IValueConverter
    {
        /// <inheritdoc cref="IValueConverter"/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case IOrifice orifice:
                    return new OrificeShapeEditViewModel(orifice);
                case IPump pump:
                    return new PumpShapeEditViewModel(pump);
                case IWeir weir:
                    return new WeirShapeEditViewModel(weir);
                default:
                    return value;
            }
        }

        /// <summary>Method is not implemented.</summary>
        /// <exception cref="NotSupportedException">Thrown when this method is called.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}