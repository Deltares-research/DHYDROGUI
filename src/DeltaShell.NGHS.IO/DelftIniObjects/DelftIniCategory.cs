using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Utils;

namespace DeltaShell.NGHS.IO.DelftIniObjects
{
    /// <summary>
    /// Representation of a category in a .ini file.
    /// </summary>
    public class DelftIniCategory : INameable
    {
        private readonly List<DelftIniProperty> delftIniProperties;

        /// <summary>
        /// Creates an instance of <see cref="DelftIniCategory"/>.
        /// </summary>
        /// <param name="categoryName"> The category name. </param>
        public DelftIniCategory(string categoryName)
        {
            Name = categoryName;
            delftIniProperties = new List<DelftIniProperty>();
        }

        public DelftIniCategory(string categoryName, int lineNumber)
            : this(categoryName)
        {
            LineNumber = lineNumber;
        }

        /// <summary>
        /// The properties that belong to the category.
        /// </summary>
        public IEnumerable<DelftIniProperty> Properties => delftIniProperties;

        /// <summary>
        /// The line number where this category was read in the file.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// The category name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the property value as a string.
        /// </summary>
        /// <param name="name"> The name of the requested property. </param>
        /// <param name="defaultValue">
        /// The returned value in case the requested
        /// property does not exist in <see cref="Properties"/>.
        /// </param>
        /// <param name="comparisonType"> Optional parameter; the type of comparison used to compare the strings. </param>
        /// <returns> A string representation of the value of the requested <see cref="DelftIniProperty"/>. </returns>
        /// <remarks>
        /// If multiple properties exist with the requested name, only the value of the
        /// first property will be returned.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="comparisonType"/> is not defined.
        /// </exception>
        public string GetPropertyValue(string name, string defaultValue = null, StringComparison comparisonType = StringComparison.Ordinal)
        {
            Ensure.IsDefined(comparisonType, nameof(comparisonType));

            DelftIniProperty property = Properties.GetByName(name, comparisonType);
            return property != null
                       ? property.Value
                       : defaultValue;
        }

        /// <summary>
        /// Returns all property values for a property with multiplicity > 1.
        /// </summary>
        /// <param name="name"> The name of the requested property. </param>
        /// <returns> String representations of the requested values. </returns>
        public IEnumerable<string> GetPropertyValues(string name)
        {
            return Properties.Where(p => p.Name == name).Select(p => p.Value);
        }

        /// <summary>
        /// Adds a collection of <see cref="DelftIniProperty"/> objects to this category.
        /// </summary>
        /// <param name="properties"> The properties to add. </param>
        public void AddProperties(IEnumerable<DelftIniProperty> properties)
        {
            delftIniProperties.AddRange(properties);
        }

        /// <summary>
        /// Adds a <see cref="DelftIniProperty"/> to this category.
        /// </summary>
        /// <param name="property"> The property to add. </param>
        public void AddProperty(DelftIniProperty property)
        {
            delftIniProperties.Add(property);
        }

        /// <summary>
        /// Adds a string-valued <see cref="DelftIniProperty"/> to this category with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        public void AddProperty(string name, string value, string comment = null)
        {
            delftIniProperties.Add(new DelftIniProperty(name, value, comment ?? ""));
        }

        /// <summary>
        /// Adds a date-valued <see cref="DelftIniProperty"/> to this category with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="time"> The property <see cref="DateTime"/> value. </param>
        /// <param name="comment"> The property comment. </param>
        /// <param name="format"> The string format for the property value. </param>
        public void AddProperty(string name, DateTime time, string comment = null,
                                string format = "yyyy-MM-dd HH:mm:ss")
        {
            AddProperty(name, time.ToString(format, CultureInfo.InvariantCulture), comment);
        }

        /// <summary>
        /// Adds a decimal-valued <see cref="DelftIniProperty"/> to this category with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        /// <param name="format"> The string format for the property value. </param>
        public void AddProperty(string name, double value, string comment = null, string format = "e7")
        {
            AddProperty(name, value.ToString(format, CultureInfo.InvariantCulture), comment);
        }

        /// <summary>
        /// Adds a integer-valued <see cref="DelftIniProperty"/> to this category with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        public void AddProperty(string name, int value, string comment = null)
        {
            AddProperty(name, value.ToString(CultureInfo.InvariantCulture), comment);
        }

        /// <summary>
        /// Sets the property value and comment of an existing <see cref="DelftIniProperty"/> with the
        /// requested property name. If the requested property does not exist, a new property is added
        /// with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        public void SetProperty(string name, string value, string comment = null)
        {
            DelftIniProperty property = delftIniProperties.FirstOrDefault(p => p.Name == name);
            if (property != null)
            {
                property.Value = value;
                property.Comment = comment;
            }
            else
            {
                AddProperty(name, value, comment);
            }
        }

        /// <summary>
        /// Sets the decimal property value and comment of an existing <see cref="DelftIniProperty"/> with the
        /// requested property name. If the requested property does not exist, a new property is added
        /// with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        /// <param name="format"> The string format for the property value. </param>
        public void SetProperty(string name, double value, string comment = null, string format = "e7")
        {
            SetProperty(name, value.ToString(format, CultureInfo.InvariantCulture), comment);
        }

        /// <summary>
        /// Removes all properties from this category that satisfy the condition that is
        /// defined in the argument.
        /// </summary>
        /// <param name="condition"> The removal condition. </param>
        public void RemoveAllPropertiesWhere(Func<DelftIniProperty, bool> condition)
        {
            delftIniProperties.RemoveAllWhere(condition);
        }

        /// <summary>
        /// Override to add the <seealso cref="Name"/>.
        /// </summary>
        /// <returns>Base.ToString and a the <seealso cref="Name"/> of the <seealso cref="DelftIniCategory"/> </returns>
        public override string ToString()
        {
            return base.ToString() + $" ( {Name} )";
        }
    }
}