using System;
using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Helpers
{
    /// <summary>
    /// Interface for representation of a category in a .ini file.
    /// </summary>
    public interface IDelftIniCategory
    {
        /// <summary>
        /// The category name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The properties that belong to the category.
        /// </summary>
        IList<IDelftIniProperty> Properties { get; set; }

        /// <summary>
        /// The line number where this category was read in the file.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets the property value as a string.
        /// </summary>
        /// <param name="name"> The name of the requested property. </param>
        /// <param name="defaultValue"> The returned value in case the requested
        /// property does not exist in <see cref="Properties"/>. </param>
        /// <returns> A string representation of the value of the requested <see cref="DelftIniProperty"/>. </returns>
        /// <remarks> If multiple properties exist with the requested name, only the value of the
        /// first property will be returned. </remarks>
        string GetPropertyValue(string name, string defaultValue = null);

        /// <summary>
        /// Returns all property values for a property with multiplicity > 1.
        /// </summary>
        /// <param name="name"> The name of the requested property. </param>
        /// <returns> String representations of the requested values. </returns>
        IEnumerable<string> GetPropertyValues(string name);

        /// <summary>
        /// Adds a string-valued <see cref="DelftIniProperty"/> to this category with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        void AddProperty(string name, string value, string comment = null);

        /// <summary>
        /// Adds a date-valued <see cref="DelftIniProperty"/> to this category with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="time"> The property <see cref="DateTime"/> value. </param>
        /// <param name="comment"> The property comment. </param>
        /// <param name="format"> The string format for the property value. </param>
        void AddProperty(string name, DateTime time, string comment = null, string format = "yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// Adds a decimal-valued <see cref="DelftIniProperty"/> to this category with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        /// <param name="format"> The string format for the property value. </param>
        void AddProperty(string name, double value,  string comment = null, string format = "e7");

        /// <summary>
        /// Adds a integer-valued <see cref="DelftIniProperty"/> to this category with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        void AddProperty(string name, int value, string comment = null);

        /// <summary>
        /// Sets the property value and comment of an existing <see cref="DelftIniProperty"/> with the
        /// requested property name. If the requested property does not exist, a new property is added
        /// with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        void SetProperty(string name, string value, string comment = null);

        /// <summary>
        /// Sets the decimal property value and comment of an existing <see cref="DelftIniProperty"/> with the
        /// requested property name. If the requested property does not exist, a new property is added
        /// with the given values.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <param name="comment"> The property comment. </param>
        /// <param name="format"> The string format for the property value. </param>
        void SetProperty(string name, double value, string comment = null, string format = null);
    }
}