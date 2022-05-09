using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.IO.Helpers
{
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
}
