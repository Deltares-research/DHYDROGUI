using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using log4net;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class DelftIniCategoryExtensionMethods
    {
        private static readonly char[] StandardSeperators = new[] { ' ', '\t' };
        private static readonly ILog log = LogManager.GetLogger(typeof(DelftIniCategoryExtensionMethods));
        
        public static T ReadProperty<T>(this IDelftIniCategory category, string key, bool isOptional = false, T defaultValue = default(T), bool logError = true)
        {
            var iniProperty = category.GetProperty(key);

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (iniProperty != null && typeConverter.CanConvertFrom(typeof(string)) && typeConverter.IsValid(iniProperty.Value))
            {
                return (T)typeConverter.ConvertFromInvariantString(iniProperty.Value);
            }

            if (!isOptional)
            {
                string message = $"Property '{key}' is not found in the file for category '{category.Name}' on line {category.LineNumber}";
                if (!logError)
                    throw new PropertyNotFoundInFileException(message);
                
                log.Error(message);
            }
            return defaultValue;

        }
        public static bool ContainsProperty(this IDelftIniCategory category, string key)
        {
            return category.Properties.Any(property => string.Equals(property.Name, key, StringComparison.InvariantCultureIgnoreCase));
        }

        public static T ReadProperty<T>(this IDelftIniCategory category, ConfigurationSetting setting, bool isOptional = false, T defaultValue = default(T), bool logError = true)
        {
            return category.ReadProperty(setting.Key, isOptional, defaultValue, logError);

        }

        public static IList<T> ReadPropertiesToListOfType<T>(this IDelftIniCategory category, string key, bool isOptional = false, char customSeparator = '\0', IList<T> defaultValue = default(IList<T>), bool useStandardSeparators = true, bool logError = true)
        {
            var iniProperty = category.GetProperty(key);

            if (iniProperty != null)
            { 
                var separators = useStandardSeparators
                                     ? StandardSeperators.Concat(new[] { customSeparator }).ToArray()
                                     : new[] { customSeparator };

                return iniProperty.Value?
                                  .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(elementValue => (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(elementValue))
                                  .ToList();
            }

            if (!isOptional)
            {
                string message = $"Property '{key}' is not found in the file for category '{category.Name}' on line {category.LineNumber}";
                if (!logError)
                    throw new PropertyNotFoundInFileException(message);

                log.Error(message);
            }
            return defaultValue;
        }

        public static IList<T> ReadPropertiesToListOfType<T>(this IDelftIniCategory category, ConfigurationSetting setting, bool isOptional = false, char customSeparator = '\0', IList<T> defaultValue = default(IList<T>), bool useStandardSeparators = true, bool logError = true)
        {
            return category.ReadPropertiesToListOfType(setting.Key, isOptional, customSeparator, defaultValue, useStandardSeparators, logError);
        }
    }
}