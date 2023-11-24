using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DHYDRO.Common.IO.Ini;
using log4net;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class IniSectionExtensions
    {
        private static readonly char[] StandardSeperators = new[] { ' ', '\t' };
        private static readonly ILog log = LogManager.GetLogger(typeof(IniSectionExtensions));
        
        public static T ReadProperty<T>(this IniSection iniSection, string key, bool isOptional = false, T defaultValue = default(T), bool logError = true)
        {
            var iniProperty = iniSection.FindProperty(key);

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (iniProperty != null && CanConvertFromString(typeConverter, iniProperty.Value))
            {
                return (T)typeConverter.ConvertFromInvariantString(iniProperty.Value);
            }

            if (!isOptional)
            {
                string message = $"Property '{key}' is not found in the file for section '{iniSection.Name}' on line {iniSection.LineNumber}";
                if (!logError)
                    throw new PropertyNotFoundInFileException(message);
                
                log.Error(message);
            }
            return defaultValue;

        }

        public static T ReadProperty<T>(this IniSection iniSection, ConfigurationSetting setting, bool isOptional = false, T defaultValue = default(T), bool logError = true)
        {
            return iniSection.ReadProperty(setting.Key, isOptional, defaultValue, logError);

        }

        public static IList<T> ReadPropertiesToListOfType<T>(this IniSection iniSection, string key, bool isOptional = false, char customSeparator = '\0', IList<T> defaultValue = default(IList<T>), bool useStandardSeparators = true, bool logError = true)
        {
            var iniProperty = iniSection.FindProperty(key);

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
                string message = string.Format(Properties.Resources.IniProperty_NotFound, key, iniSection.Name, iniSection.LineNumber);
                if (!logError)
                    throw new PropertyNotFoundInFileException(message);

                log.Error(message);
            }
            return defaultValue;
        }

        public static IList<T> ReadPropertiesToListOfType<T>(this IniSection iniSection, ConfigurationSetting setting, bool isOptional = false, char customSeparator = '\0', IList<T> defaultValue = default(IList<T>), bool useStandardSeparators = true, bool logError = true)
        {
            return iniSection.ReadPropertiesToListOfType(setting.Key, isOptional, customSeparator, defaultValue, useStandardSeparators, logError);
        }

        private static bool CanConvertFromString(TypeConverter typeConverter, string value)
        {
            // skip IsValid check for EnumConverter because it is case sensitive while the ConvertFromInvariantString is not
            return typeConverter.CanConvertFrom(typeof(string)) 
                   && (typeConverter is EnumConverter 
                       || typeConverter.IsValid(value));
        }
        
                /// <summary>
        /// for unique property names, otherwise first!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetPropertyValueWithOptionalDefaultValue(this IniSection iniSection, string name, string defaultValue = null)
        {
            IniProperty property = iniSection.Properties.FirstOrDefault(p => p.Key.ToLower().Equals(name.ToLower()));
            return property != null ? property.Value : defaultValue;
        }

        /// <summary>
        /// returns all values, ordered, for a property with multiplicity > 1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetPropertyValuesByName(this IniSection iniSection, string name)
        {
            IList<string> foundProperties = new List<string>();
            foreach (var property in iniSection.Properties.Where(p => p.Key.ToLower().Equals(name.ToLower())))
            {
                foundProperties.Add(property.Value);
            }
            return foundProperties;
        }

        public static void AddPropertyWithOptionalComment<T>(this IniSection iniSection, string name, T value, string comment = null) where T: IConvertible
        {
            var property = iniSection.AddProperty(name, value);
            property.Comment = comment;
        }

        public static void AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(this IniSection iniSection, string name, IEnumerable<double> values, string comment = null, string format = "e7")
        {
            var valuesString = string.Join(" ", values.Select(value => value.ToString(format, CultureInfo.InvariantCulture)));
            iniSection.AddPropertyWithOptionalComment(name, valuesString, comment);
        }

        public static void AddPropertyWithOptionalCommentAndFormat(this IniSection iniSection, string name, double value,  string comment = null, string format = "e7")
        {
            iniSection.AddPropertyWithOptionalComment(name, value.ToString(format, CultureInfo.InvariantCulture), comment);
        }

        public static void AddPropertyWithMultipleValuesWithOptionalComment<T>(this IniSection iniSection, string name, IEnumerable<T> values, string comment = null) where T : IConvertible
        {
            var valuesString = string.Join(" ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
            iniSection.AddPropertyWithOptionalComment(name, valuesString, comment);
        }

        public static void AddProperty(this IniSection iniSection, string name, int value, string comment = null)
        {
            iniSection.AddPropertyWithOptionalComment(name, value.ToString(CultureInfo.InvariantCulture), comment);
        }

        public static void AddProperty(this IniSection iniSection, string name, bool value, string comment = null)
        {
            iniSection.AddPropertyWithOptionalComment(name, value.ToString(CultureInfo.InvariantCulture), comment);
        }

        public static void AddPropertyFromConfiguration(this IniSection iniSection, ConfigurationSetting propertyConfiguration, string value)
        {
            iniSection.AddPropertyWithOptionalComment(propertyConfiguration.Key, value, propertyConfiguration.Description);
        }

        public static void AddPropertyFromConfiguration(this IniSection iniSection, ConfigurationSetting propertyConfiguration, int value)
        {
            iniSection.AddProperty(propertyConfiguration.Key, value, propertyConfiguration.Description);
        }
        public static void AddPropertyFromConfiguration(this IniSection iniSection, ConfigurationSetting propertyConfiguration, bool value)
        {
            iniSection.AddProperty(propertyConfiguration.Key, value, propertyConfiguration.Description);
        }

        public static void AddPropertyFromConfiguration(this IniSection iniSection, ConfigurationSetting propertyConfiguration, double value)
        {
            iniSection.AddPropertyWithOptionalCommentAndFormat(propertyConfiguration.Key, value, propertyConfiguration.Description, propertyConfiguration.Format);
        }

        public static void AddPropertyFromConfigurationWithMultipleValues(this IniSection iniSection, ConfigurationSetting propertyConfiguration, IEnumerable<double> values)
        {
            iniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(propertyConfiguration.Key, values, propertyConfiguration.Description, propertyConfiguration.Format);
        }

        public static void SetPropertyWithOptionalComment(this IniSection iniSection, string name, string value, string comment = null)
        {
            var prop = iniSection.Properties.FirstOrDefault(p => p.Key == name);
            if (prop != null)
            {
                prop.Value = value;
                prop.Comment = comment;
            }
            else
            {
                iniSection.AddPropertyWithOptionalComment(name, value, comment);
            }
        }

        public static void SetPropertyWithOptionalCommentAndFormat(this IniSection iniSection, string name, double value, string comment = null, string format = "e7")
        {
            iniSection.SetPropertyWithOptionalComment(name, value.ToString(format, CultureInfo.InvariantCulture), comment);
        }
    }
}