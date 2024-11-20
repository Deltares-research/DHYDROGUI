using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    /// <summary>
    /// Property descriptor for a <see cref="KeyValuePair{TKey,TValue}"/>
    /// </summary>
    /// <typeparam name="T"> Type of key value pair value </typeparam>
    /// <seealso cref="PropertyDescriptor"/>
    public class KeyValuePairPropertyDescriptor<T> : PropertyDescriptor
    {
        private readonly string key;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairPropertyDescriptor{T}"/> class.
        /// </summary>
        /// <param name="name"> The key. </param>
        /// <param name="attrs"> The attributes. </param>
        /// <param name="isReadOnly"> Whether or not this property should be read-only. </param>
        public KeyValuePairPropertyDescriptor(string name, Attribute[] attrs, bool isReadOnly) : base(name, attrs)
        {
            key = name;
            IsReadOnly = isReadOnly;
        }

        /// <inheritdoc/>
        public override bool IsReadOnly { get; }

        /// <inheritdoc/>
        public override Type ComponentType => typeof(KeyValuePair<string, T>[]);

        /// <inheritdoc/>
        public override Type PropertyType => typeof(T);

        /// <summary>
        /// Gets the value of the key value pair.
        /// </summary>
        /// <param name="component"> The key value pair array. </param>
        /// <returns> The value of corresponding key value pair of this descriptor. </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="component"/> is not an array of type <see cref="KeyValuePair{String, T}"/>
        /// </exception>
        public override object GetValue(object component)
        {
            if (!(component is KeyValuePair<string, T>[] keyValuePairs))
            {
                throw new ArgumentException($@"Must be of type {ComponentType}.", nameof(component));
            }

            return keyValuePairs.FirstOrDefault(p => p.Key == key).Value;
        }

        /// <summary>
        /// Sets the value of the key value pair.
        /// </summary>
        /// <param name="component"> The key value pair array. </param>
        /// <param name="value"> The new value. </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the property is read-only.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="component"/> is not an array of type <see cref="KeyValuePair{String, T}"/>
        /// </exception>
        public override void SetValue(object component, object value)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Property is read-only.");
            }

            if (!(component is KeyValuePair<string, T>[] keyValuePairs))
            {
                throw new ArgumentException($@"Must be of type {ComponentType}.", nameof(component));
            }

            KeyValuePair<string, T> kvp = keyValuePairs
                .FirstOrDefault(p => p.Key == key);

            int index = Array.IndexOf(keyValuePairs, kvp);
            keyValuePairs[index] = new KeyValuePair<string, T>(kvp.Key, (T) value);
        }

        /// <inheritdoc/>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// Throws an <exception cref="NotSupportedException"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown when this method is called.
        /// </exception>
        public override void ResetValue(object component)
        {
            throw new NotSupportedException("Resetting property value is not supported.");
        }

        /// <inheritdoc/>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}