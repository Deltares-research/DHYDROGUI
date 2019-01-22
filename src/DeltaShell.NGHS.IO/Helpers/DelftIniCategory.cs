using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.IO.FileReaders;
using GeoAPI.Geometries;

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
            var prop = Properties.FirstOrDefault(p => p.Name == name);
            return prop != null ? prop.Value : defaultValue;
        }

        /// <summary>
        /// returns all values, ordered, for a property with multiplicity > 1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<string> GetPropertyValues(string name)
        {
            return Properties.Where(p => p.Name == name).Select(p => p.Value);
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

        /// <summary>
        /// Add a DateTime property with the specified ConfigurationSettings
        /// <paramref name="settings"/> and the specified value <paramref name="time"/>.
        /// </summary>
        /// <param name="settings">The ConfigurationSetting of the time.</param>
        /// <param name="time">The DateTime value.</param>
        /// <remarks>
        /// This uses the format specified in the <paramref name="settings"/>,
        /// and not the default "yyyy-MM-dd HH:mm:ss"
        /// </remarks>
        public void AddProperty(ConfigurationSetting settings, DateTime time)
        {
            AddProperty(settings.Key, time, settings.Description, settings.Format);
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

        /// <summary>
        /// Add a double property with the specified ConfigurationSettings.
        /// </summary>
        /// <param name="settings">The ConfigurationSetting of the value.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// This uses the format specified in <paramref name="settings"/> and not the default "e7".
        /// </remarks>
        public void AddProperty(ConfigurationSetting settings, double value)
        {
            AddProperty(settings.Key, value, settings.Description, settings.Format);
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

        public void AddProperty(string name, int value, string comment = null)
        {
            AddProperty(name, value.ToString(CultureInfo.InvariantCulture), comment);
        }

        public void AddProperty(string name, ICoordinate coordinate, string comment = null, string format = "F4")
        {
            var coordinateAsIniValue = coordinate.X.ToString(format, CultureInfo.InvariantCulture) + " " + coordinate.Y.ToString(format, CultureInfo.InvariantCulture);
            Properties.Add(new DelftIniProperty { Name = name, Value = coordinateAsIniValue, Comment = comment ?? "" });
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
        public static T ReadProperty<T>(this IDelftIniCategory category, string key, ref string errorMessage)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => property.Name == key);

            if (iniProperty != null)
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(iniProperty.Value);

            errorMessage += string.Format("Unable to parse {0} property: {1}{2}", category.Name, key, Environment.NewLine);
            return default(T);
        }
        public static T ReadProperty<T>(this IDelftIniCategory category, string key, bool isOptional = false)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => property.Name == key);

            if (iniProperty != null)
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(iniProperty.Value);
            
            if(!isOptional)
                throw new PropertyNotFoundInFileException(String.Format("Property {0} is not found in the file", key));
            return default(T);
        }
        public static IList<T> ReadPropertiesToListOfType<T>(this IDelftIniCategory category, string key, ref string errorMessage)
        {
            var iniProperty = category.Properties.FirstOrDefault(property => property.Name == key);

            if (iniProperty != null)
            {
                return iniProperty.Value.Split(' ').Select(elementValue => (T) TypeDescriptor.GetConverter(typeof (T)).ConvertFromInvariantString(elementValue.Split(',')[0])).ToList();
            }

            errorMessage += string.Format("Unable to parse {0} property: {1}{2}", category.Name, key, Environment.NewLine);
            return default(IList<T>);
        }
        public static IList<T> ReadPropertiesToListOfType<T>(this IDelftIniCategory category, string key, bool isOptional = false, char separator = ' ')
        {
            var iniProperty = category.Properties.FirstOrDefault(property => property.Name == key);

            if (iniProperty != null)
            {
                return iniProperty.Value.Split(separator).Select(elementValue => (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(elementValue)).ToList();
            }

            if (!isOptional)
                throw new PropertyNotFoundInFileException(String.Format("Property {0} is not found in the file", key));
            
            return default(IList<T>);
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
        public static double[] ParseDoublesFromPropertyValue(this IDelftIniProperty property)
        {
            var propertyStringValues = property.Value.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var propertyDoubleValues = new List<double>();
            foreach (var propertyString in propertyStringValues)
            {
                double propertyDouble;
                if (double.TryParse(propertyString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out propertyDouble))
                {
                    propertyDoubleValues.Add(propertyDouble);
                }
            }
            return propertyDoubleValues.ToArray();
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
