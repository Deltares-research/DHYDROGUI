using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    public class KeyValuePairPropertyDescriptor<T> : PropertyDescriptor
    {
        public KeyValuePairPropertyDescriptor(string name, Attribute[] attrs, bool isReadOnly) : base(name, attrs)
        {
            IsReadOnly = isReadOnly;
        }

        public override object GetValue(object component)
        {
            if (component is KeyValuePair<string, T>[] keyValuePairs)
            {
                return keyValuePairs.FirstOrDefault(p => p.Key == Name).Value;
            }

            return null;
        }

        public override void SetValue(object component, object value)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Property is read-only.");
            }

            if (component is KeyValuePair<string, T>[] keyValuePairs)
            {
                KeyValuePair<string, T> kvp = keyValuePairs
                    .FirstOrDefault(p => p.Key == Name);

                int index = Array.IndexOf(keyValuePairs, kvp);
                keyValuePairs[index] = new KeyValuePair<string, T>(kvp.Key, (T)value);
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
            throw new NotSupportedException();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override bool IsReadOnly { get; }

        public override Type ComponentType => typeof(KeyValuePair<string, T>[]);

        public override Type PropertyType => typeof(T);
    }
}