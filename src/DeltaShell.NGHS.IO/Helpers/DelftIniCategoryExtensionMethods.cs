using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DeltaShell.NGHS.IO.Helpers {
    public static class DelftIniCategoryExtensionMethods
    {
        public static T ReadProperty<T>(this IDelftIniCategory category, string key, ref string errorMessage)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => PropertyEqualsKey(key, property));

            if (iniProperty != null)
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(iniProperty.Value);

            errorMessage += string.Format("Unable to parse {0} property: {1}{2}", category.Name, key, Environment.NewLine);
            return default(T);
        }

        public static T ReadProperty<T>(this IDelftIniCategory category, string key, bool isOptional = false)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => PropertyEqualsKey(key, property));

            if (iniProperty != null)
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(iniProperty.Value);
            
            if(!isOptional)
                throw new PropertyNotFoundInFileException(String.Format("Property {0} is not found in the file", key));
            return default(T);
        }
        public static IList<T> ReadPropertiesToListOfType<T>(this IDelftIniCategory category, string key, ref string errorMessage)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => PropertyEqualsKey(key, property));

            if (iniProperty != null)
            {
                return iniProperty.Value.Split(' ').Select(elementValue => (T) TypeDescriptor.GetConverter(typeof (T)).ConvertFromInvariantString(elementValue.Split(',')[0])).ToList();
            }

            errorMessage += string.Format("Unable to parse {0} property: {1}{2}", category.Name, key, Environment.NewLine);
            return default(IList<T>);
        }
        public static IList<T> ReadPropertiesToListOfType<T>(this IDelftIniCategory category, string key, bool isOptional = false, char separator = ' ')
        {
            var iniProperty = category.Properties.FirstOrDefault(property => PropertyEqualsKey(key, property));

            if (iniProperty != null)
            {
                return iniProperty.Value.Split(separator).Select(elementValue => (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(elementValue)).ToList();
            }

            if (!isOptional)
                throw new PropertyNotFoundInFileException(String.Format("Property {0} is not found in the file", key));
            
            return default(IList<T>);
        }
        private static bool PropertyEqualsKey(string key, DelftIniProperty property)
        {
            return property.Name.Equals(key, StringComparison.OrdinalIgnoreCase);
        }
    }
}