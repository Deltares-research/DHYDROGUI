using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    public class KeyValuePairArrayConverter<T> : ArrayConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                         Type destinationType)
        {
            if (destinationType == typeof(string) &&
                value is KeyValuePair<string, T>[] keyValuePairs)
            {
                return $"({keyValuePairs.Length})";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value,
                                                                   Attribute[] attributes)
        {
            if (!(value is KeyValuePair<string, T>[] keyValuePairs))
            {
                return PropertyDescriptorCollection.Empty;
            }

            PropertyDescriptor[] descriptors = keyValuePairs
                                               .Select(p => new KeyValuePairPropertyDescriptor<T>(p.Key, attributes, true))
                                               .Cast<PropertyDescriptor>()
                                               .ToArray();
            return new PropertyDescriptorCollection(descriptors);
        }
    }
}