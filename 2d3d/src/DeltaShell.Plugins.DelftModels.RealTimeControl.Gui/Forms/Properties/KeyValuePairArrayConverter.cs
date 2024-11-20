using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    /// <summary>
    /// Provides a type converter to convert a <see cref="KeyValuePair{TKey,TValue}"/> array to another representation.
    /// </summary>
    /// <typeparam name="T"> The type of the key value pair value. </typeparam>
    /// <seealso cref="ArrayConverter"/>
    public class KeyValuePairArrayConverter<T> : ArrayConverter
    {
        /// <summary>
        /// Converts the specified key value pair in <paramref name="value"/> to a string.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <param name="culture"> The culture. </param>
        /// <param name="value"> The value. </param>
        /// <param name="destinationType"> Type of the destination. </param>
        /// <returns> The string representation of the specified key value pair in <paramref name="value"/>. </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> is not an array of type <see cref="KeyValuePair{String, T}"/>
        /// </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                         Type destinationType)
        {
            if (!(value is KeyValuePair<string, T>[] keyValuePairs))
            {
                throw new ArgumentException($@"Must be of type {typeof(KeyValuePair<string, T>[])}.",
                                            nameof(value));
            }

            return destinationType == typeof(string)
                       ? $"({keyValuePairs.Length})"
                       : base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Gets the property descriptor collection for the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="context"> This parameter is not used. </param>
        /// <param name="value"> The key value pairs. </param>
        /// <param name="attributes"> The attributes. </param>
        /// <returns> The property descriptor collection for the specified <paramref name="value"/>. </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> is not an array of type <see cref="KeyValuePair{String, T}"/>
        /// </exception>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value,
                                                                   Attribute[] attributes)
        {
            if (!(value is KeyValuePair<string, T>[] keyValuePairs))
            {
                throw new ArgumentException($@"Must be of type {typeof(KeyValuePair<string, T>[])}.",
                                            nameof(value));
            }

            PropertyDescriptor[] descriptors = keyValuePairs
                                               .Select(p => new KeyValuePairPropertyDescriptor<T>(p.Key, attributes, true))
                                               .Cast<PropertyDescriptor>()
                                               .ToArray();
            return new PropertyDescriptorCollection(descriptors);
        }
    }
}