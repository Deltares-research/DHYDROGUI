using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.NGHS.IO.Helpers
{
    #region DelftIniCategory

    public class DelftIniCategory : IDelftIniCategory
    {
        public string Name { get; private set; }
        public IList<DelftIniProperty> Properties { get; set; }
        
        /// <summary>
        /// The line number where this category was read in the file.
        /// </summary>
        public int LineNumber { get; set; }

        public DelftIniCategory(string categoryName)
        {
            Name = categoryName;
            Properties = new List<DelftIniProperty>();
        }

        /// <summary>
        /// for unique property names, otherwise first!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetPropertyValue(string name, string defaultValue = null)
        {
            foreach (var property in Properties)
            {
                if (property.Name.ToLower().Equals(name.ToLower()))
                {
                    return property.Value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// returns all values, ordered, for a property with multiplicity > 1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<string> GetPropertyValues(string name)
        {
            IList<string> foundProperties = new List<string>();
            foreach (var property in Properties)
            {
                if (property.Name.ToLower().Equals(name.ToLower()))
                {
                    foundProperties.Add(property.Value);
                }
            }
            return foundProperties;
        }

        public void RemoveProperty(DelftIniProperty property)
        {
            Properties.Remove(property);
        }

        public void AddProperty(string name, string value, string comment = null)
        {
            Properties.Add(new DelftIniProperty { Name = name, Value = value, Comment = comment ?? "" });
        }

        public void AddProperty(DelftIniProperty property)
        {
            Properties.Add(new DelftIniProperty { Name = property.Name, Value = property.Value, Comment = property.Comment });
        }

        public void AddProperty(string name, DateTime time, string comment = null, string format = "yyyy-MM-dd HH:mm:ss")
        {
            AddProperty(name, time.ToString(format, CultureInfo.InvariantCulture), comment);
        }

        public void AddProperty(string name, IEnumerable<double> values, string comment = null, string format = "e7")
        {
            var valuesString = string.Join(" ", values.Select(value => value.ToString(format, CultureInfo.InvariantCulture)));
            AddProperty(name, valuesString, comment);
        }

        public void AddProperty(string name, double value,  string comment = null, string format = "e7")
        {
                AddProperty(name, value.ToString(format, CultureInfo.InvariantCulture), comment);
        }

        public void AddProperty(string name, IEnumerable<int> values, string comment = null)
        {
            var valuesString = string.Join(" ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
            AddProperty(name, valuesString, comment);
        }

        public void AddProperty(string name, IEnumerable<string> values, string comment = null)
        {
            var valuesString = string.Join(" ", values);
            AddProperty(name, valuesString, comment);
        }

        public void AddProperty(string name, int value, string comment = null)
        {
            AddProperty(name, value.ToString(CultureInfo.InvariantCulture), comment);
        }

        public void AddProperty(string name, bool value, string comment = null)
        {
            AddProperty(name, value.ToString(CultureInfo.InvariantCulture), comment);
        }

        public void AddProperty(string name, ICoordinate coordinate, string comment = null, string format = "F4")
        {
            var coordinateAsIniValue = coordinate.X.ToString(format, CultureInfo.InvariantCulture) + " " + coordinate.Y.ToString(format, CultureInfo.InvariantCulture);
            Properties.Add(new DelftIniProperty { Name = name, Value = coordinateAsIniValue, Comment = comment ?? "" });
        }

        public void AddProperty(ConfigurationSetting propertyConfiguration, string value)
        {
            AddProperty(propertyConfiguration.Key, value, propertyConfiguration.Description);
        }

        public void AddProperty(ConfigurationSetting propertyConfiguration, int value)
        {
            AddProperty(propertyConfiguration.Key, value, propertyConfiguration.Description);
        }
        public void AddProperty(ConfigurationSetting propertyConfiguration, bool value)
        {
            AddProperty(propertyConfiguration.Key, value, propertyConfiguration.Description);
        }

        public void AddProperty(ConfigurationSetting propertyConfiguration, double value)
        {
            AddProperty(propertyConfiguration.Key, value, propertyConfiguration.Description, propertyConfiguration.Format);
        }

        public void AddProperty(ConfigurationSetting propertyConfiguration, IEnumerable<double> values)
        {
            AddProperty(propertyConfiguration.Key, values, propertyConfiguration.Description, propertyConfiguration.Format);
        }

        public void SetProperty(string name, string value, string comment = null)
        {
            var prop = Properties.FirstOrDefault(p => p.Name == name);
            if (prop != null)
            {
                prop.Value = value;
                prop.Comment = comment;
            }
            else
            {
                AddProperty(name, value, comment);
            }
        }

        public void SetProperty(string name, double value, string comment = null, string format = "e7")
        {
            SetProperty(name, value.ToString(format, CultureInfo.InvariantCulture), comment);
        }
        
        /// <summary>
        /// Gets the property with the specified <paramref name="propertyName"/> from this category.
        /// </summary>
        /// <param name="propertyName"> The property name to search for. </param>
        /// <param name="stringComparison"> Optional parameter; the type of comparison used to compare the name strings. </param>
        /// <returns> If found, the property with the specified <paramref name="propertyName"/>; otherwise, <c>null</c>.</returns>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <paramref name="stringComparison"/> is not defined.
        /// </exception>
        public IDelftIniProperty GetProperty(string propertyName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            Ensure.IsDefined(stringComparison, nameof(stringComparison));
            
            return Properties.FirstOrDefault(p => string.Equals(p.Name, propertyName, stringComparison));
        }
    }

    public interface IDelftIniCategory
    {
        string Name { get; }
        IList<DelftIniProperty> Properties { get; set; }

        /// <summary>
        /// The line number where this category was read in the file.
        /// </summary>
        int LineNumber { get; set; }

        /// <summary>
        /// for unique property names, otherwise first!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string GetPropertyValue(string name, string defaultValue = null);
        
        /// <summary>
        /// returns all values, ordered, for a property with multiplicity > 1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEnumerable<string> GetPropertyValues(string name);

        void AddProperty(string name, string value, string comment = null);
        void AddProperty(string name, DateTime time, string comment = null, string format = "yyyy-MM-dd HH:mm:ss");
        void AddProperty(string name, IEnumerable<double> values, string comment = null, string format = "e7");
        void AddProperty(string name, double value,  string comment = null, string format = "e7");
        void AddProperty(string name, IEnumerable<int> values, string comment = null);
        void AddProperty(string name, int value, string comment = null);

        void SetProperty(string name, string value, string comment = null);
        void SetProperty(string name, double value, string comment = null, string format = null);
    }

    public class PropertyNotFoundInFileException : FileReadingException
    {
        public PropertyNotFoundInFileException(string message)
            : base(message)
        {
        }

        public PropertyNotFoundInFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public static class DelftIniCategoryExtensionMethods
    {
        private static readonly char[] StandardSeperators = new[] { ' ', '\t' };
        private static readonly ILog log = LogManager.GetLogger(typeof(DelftIniCategoryExtensionMethods));
        
        public static T ReadProperty<T>(this IDelftIniCategory category, string key, ref string errorMessage)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => property.Name.ToLowerInvariant() == key.ToLowerInvariant());

            if (iniProperty != null)
                return iniProperty.ReadValue<T>();

            errorMessage += $"Unable to parse {category.Name} property: {key}{Environment.NewLine}";
            return default(T);
        }
        public static T ReadProperty<T>(this IDelftIniCategory category, string key, bool isOptional = false, T defaultValue = default(T), bool logError = true)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => string.Equals(property.Name, key, StringComparison.InvariantCultureIgnoreCase));

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (iniProperty != null && typeConverter.CanConvertFrom(typeof(string)) && typeConverter.IsValid(iniProperty.Value))
            {
                return (T)typeConverter.ConvertFromInvariantString(iniProperty.Value);
            }

            if (!isOptional)
            {
                string message = $"Property {key} is not found in the file";
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
        public static IList<T> ReadPropertiesToListOfType<T>(this IDelftIniCategory category, string key, ref string errorMessage)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => property.Name.ToLowerInvariant() == key.ToLowerInvariant());

            if (iniProperty != null)
            {
                return iniProperty.Value.Split(new []{' '},StringSplitOptions.RemoveEmptyEntries).Select(elementValue => (T) TypeDescriptor.GetConverter(typeof (T)).ConvertFromInvariantString(elementValue.Split(',')[0])).ToList();
            }

            errorMessage += string.Format("Unable to parse {0} property: {1}{2}", category.Name, key, Environment.NewLine);
            return default(IList<T>);
        }
        public static IList<T> ReadPropertiesToListOfType<T>(this IDelftIniCategory category, string key, bool isOptional = false, char customSeparator = '\0', IList<T> defaultValue = default(IList<T>), bool useStandardSeparators = true, bool logError = true)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => string.Equals(property.Name, key, StringComparison.InvariantCultureIgnoreCase));

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
                string message = $"Property {key} is not found in the file";
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
    #endregion

    #region Nested Type: DelftIniProperty

    
    public class DelftIniProperty : IDelftIniProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }

        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        public int LineNumber { get; set; }

        public DelftIniProperty()
        {
            
        }

        public DelftIniProperty(string name, string value, string comment)
        {
            Name = name;
            Value = value;
            Comment = comment;
        }

        public override string ToString()
        {
            return $"Line {LineNumber}: {Name}={Value}";
        }
    }

    public interface IDelftIniProperty
    {
        string Name { get; set; }
        string Value { get; set; }
        string Comment { get; set; }

        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        int LineNumber { get; set; }
    }

    public static class DelftIniPropertyExtensionMethods
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DelftIniPropertyExtensionMethods));
        
        public static double[] ParseDoublesFromPropertyValue(this IDelftIniProperty property)
        {
            var propertyStringValues = property.Value.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var propertyDoubleValues = new List<double>();
            foreach (var propertyString in propertyStringValues)
            {
                double propertyDouble;
                if (double.TryParse(propertyString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out propertyDouble))
                {
                    propertyDoubleValues.Add(propertyDouble);
                }
            }
            return propertyDoubleValues.ToArray();
        }

        /// <summary>
        /// Reads the value of the property and converts it to type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property"> The property to read the value from. </param>
        /// <typeparam name="T"> The converted value type. </typeparam>
        /// <returns> If parsable, the converted value; otherwise, the default value of <typeparamref name="T"/>. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="property"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Logs an error when the property value cannot be parsed to type <typeparamref name="T"/>.
        /// </remarks>>
        public static T ReadValue<T>(this IDelftIniProperty property)
        {
            Ensure.NotNull(property, nameof(property));

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.IsValid(property.Value))
            {
                return (T) converter.ConvertFromInvariantString(property.Value);
            }

            log.Error(string.Format(Resources.DelftIniPropertyExtensionMethods_Cannot_parse_value_for_property, 
                                    property.Value, typeof(T).Name, property.Name, property.LineNumber));
            return default(T);

        }
        
        /// <summary>
        /// Reads the boolean value of the property.
        /// </summary>
        /// <param name="property"> The property to read the value from. </param>
        /// <returns> If convertible, the boolean value; otherwise, false. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="property"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Logs an error when the property value cannot be parsed to a boolean.
        /// </remarks>
        public static bool ReadBooleanValue(this IDelftIniProperty property)
        {
            Ensure.NotNull(property, nameof(property));

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(bool));
            if (converter.IsValid(property.Value))
            {
                return (bool) converter.ConvertFromInvariantString(property.Value);
            }

            converter = TypeDescriptor.GetConverter(typeof(int));
            if (converter.IsValid(property.Value))
            {
                return Convert.ToBoolean(converter.ConvertFromInvariantString(property.Value));
            }

            log.Error(string.Format(Resources.DelftIniPropertyExtensionMethods_Cannot_parse_value_for_property, 
                                    property.Value, nameof(Boolean), property.Name, property.LineNumber));
            return false;

        }
    }
    #endregion

    #region Nested Type: DelftBcCategory

    public class DelftBcCategory : DelftIniCategory, IDelftBcCategory
    {
        public IList<IDelftBcQuantityData> Table { get; set; }

        public DelftBcCategory(string categoryName) : base(categoryName)
        {
            Table = new List<IDelftBcQuantityData>();
        }
    }

    public interface IDelftBcCategory:IDelftIniCategory
    {
        IList<IDelftBcQuantityData> Table { get; set; }
    }

    #endregion
    
    #region Nested Type: DelftBcQuantityData

    public class DelftBcQuantityData : IDelftBcQuantityData
    {
        public DelftIniProperty Quantity { get; set; }
        public DelftIniProperty Unit { get; set; }
        
        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        public int LineNumber { get; set; }

        public IList<string> Values { get; set; }

        public DelftBcQuantityData(DelftIniProperty quantity)
        {
            Quantity = quantity;
            Values = new List<string>();
        }

        public DelftBcQuantityData(DelftIniProperty quantity, DelftIniProperty unit, IEnumerable<double> values)
        {
            Quantity = quantity;
            Unit = unit;
            Values = values.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToList();
        }
    }

    public interface IDelftBcQuantityData
    {
        DelftIniProperty Quantity { get; set; }
        DelftIniProperty Unit { get; set; }

        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        int LineNumber { get; set; }
        IList<string> Values { get; set; }
    }
    #endregion
}
