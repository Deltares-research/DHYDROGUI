using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.Ini
{
    /// <summary>
    /// Represents a section with properties in an INI file.
    /// </summary>
    /// <remarks>
    /// This class encapsulates a single section in an INI file, containing properties with key-value pairs.
    /// <para/>
    /// It is allowed to add multiple properties with the same key within a section.
    /// When using methods like <see cref="FindProperty"/> and <see cref="AddOrUpdateProperty{T}"/>, the first property found
    /// with the specified key is operated upon.
    /// <para/>
    /// Property keys are compared in a case-insensitive manner.
    /// </remarks>
    [DebuggerDisplay("{Name}")]
    public sealed class IniSection : IEquatable<IniSection>
    {
        private readonly List<IniProperty> properties;
        private readonly List<string> comments;

        /// <summary>
        /// Initializes a new instance of the <see cref="IniSection"/> class.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <exception cref="ArgumentException">When <paramref name="name"/> is <c>null</c> or empty.</exception>
        public IniSection(string name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            properties = new List<IniProperty>();
            comments = new List<string>();

            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IniSection"/> class with the same values as the specified section.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="other">The section to copy values from.</param>
        /// <exception cref="ArgumentException">When <paramref name="name"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="other"/> is <c>null</c>.</exception>
        public IniSection(string name, IniSection other)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));
            Ensure.NotNull(other, nameof(other));

            properties = other.Properties
                              .Select(p => new IniProperty(p.Key, p))
                              .ToList();

            comments = other.Comments.ToList();

            Name = name;
            LineNumber = other.LineNumber;
        }

        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the line number where the section is located. The default value is 0.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets the properties within the section.
        /// </summary>
        public IEnumerable<IniProperty> Properties => properties;

        /// <summary>
        /// Gets the comments associated with the section.
        /// </summary>
        public IEnumerable<string> Comments => comments;

        /// <summary>
        /// Gets the number of properties within the section.
        /// </summary>
        public int PropertyCount => properties.Count;

        /// <summary>
        /// Gets the number of comments associated with the section.
        /// </summary>
        public int CommentCount => comments.Count;

        /// <summary>
        /// Adds a new property with the specified key and value to the section.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <typeparam name="T">The type of the value, must implement <see cref="IConvertible"/>.</typeparam>
        /// <returns>The added <see cref="IniProperty"/> instance.</returns>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        public IniProperty AddProperty<T>(string key, T value)
            where T : IConvertible
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            var property = IniProperty.Create(key, value);
            properties.Add(property);

            return property;
        }

        /// <summary>
        /// Adds a property to the section.
        /// </summary>
        /// <param name="property">The property to add.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="property"/> is <c>null</c>.</exception>
        public void AddProperty(IniProperty property)
        {
            Ensure.NotNull(property, nameof(property));

            properties.Add(property);
        }

        /// <summary>
        /// Adds a collection of properties to the section.
        /// </summary>
        /// <param name="propertiesToAdd">The properties to add.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="propertiesToAdd"/> is <c>null</c>.</exception>
        public void AddMultipleProperties(IEnumerable<IniProperty> propertiesToAdd)
        {
            Ensure.NotNull(propertiesToAdd, nameof(propertiesToAdd));

            properties.AddRange(propertiesToAdd);
        }

        /// <summary>
        /// Adds a new property with the specified key and value to the section, or updates the value of the first property
        /// found with the specified key.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <typeparam name="T">The type of the value, must implement <see cref="IConvertible"/>.</typeparam>
        /// <returns>The added or updated <see cref="IniProperty"/> instance.</returns>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        public IniProperty AddOrUpdateProperty<T>(string key, T value)
            where T : IConvertible
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            IniProperty property = FindProperty(key);

            if (property != null)
            {
                property.SetConvertedValue(value);
                return property;
            }
            else
            {
                return AddProperty(key, value);
            }
        }

        /// <summary>
        /// Returns whether the section contains a property with the specified key.
        /// </summary>
        /// <param name="key">The key of the property to locate in the section.</param>
        /// <returns><c>true</c> if a property with the specified key is found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <c>null</c> or empty.</exception>
        public bool ContainsProperty(string key)
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            return FindProperty(key) != null;
        }

        /// <summary>
        /// Searches for a property in the section with the specified key, and returns the first occurrence.
        /// </summary>
        /// <param name="key">The key of the property to retrieve.</param>
        /// <returns>The first property in the section that matches the specified key, or <c>null</c> if not found.</returns>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        public IniProperty FindProperty(string key)
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            return properties.Find(
                p => p.IsKeyEqualTo(key));
        }

        /// <summary>
        /// Gets all properties in the section with the specified key.
        /// </summary>
        /// <param name="key">The key of the properties to retrieve.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all properties with the specified key.</returns>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        public IEnumerable<IniProperty> GetAllProperties(string key)
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            return properties.Where(
                p => p.IsKeyEqualTo(key));
        }

        /// <summary>
        /// Gets the value of the first property found in the section with the specified key, or returns a default value if
        /// the property is not found.
        /// </summary>
        /// <param name="key">The key of the property to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the property is not found. Default value is <c>null</c>.</param>
        /// <returns>
        /// The value of the first property with the specified key if found; otherwise, the <paramref name="defaultValue"/>.
        /// </returns>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        public string GetPropertyValue(string key, string defaultValue = null)
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            IniProperty property = FindProperty(key);

            return property?.Value ?? defaultValue;
        }

        /// <summary>
        /// Gets the converted value of the first property found in the section with the specified key, or returns a default value
        /// if the property is not found.
        /// </summary>
        /// <param name="key">The key of the property to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the property is not found. Default value is <c>default(T)</c>.</param>
        /// <typeparam name="T">The target type to convert the property value to. Must implement <see cref="IConvertible"/>.</typeparam>
        /// <returns>
        /// The value of the first property with the specified key if found and successfully converted;
        /// otherwise the <paramref name="defaultValue"/>.
        /// </returns>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        public T GetPropertyValue<T>(string key, T defaultValue = default)
            where T : IConvertible
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            IniProperty property = FindProperty(key);

            if (property == null)
            {
                return defaultValue;
            }

            return property.TryGetConvertedValue(out T convertedValue)
                       ? convertedValue
                       : defaultValue;
        }

        /// <summary>
        /// Removes the specified property from the section.
        /// </summary>
        /// <param name="property">The property to remove.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="property"/> is <c>null</c>.</exception>
        /// <remarks>Returns silently if the property was not found in the section.</remarks>
        public void RemoveProperty(IniProperty property)
        {
            Ensure.NotNull(property, nameof(property));

            properties.Remove(property);
        }

        /// <summary>
        /// Removes all properties with the specified key from the section.
        /// </summary>
        /// <param name="key">The key of properties to remove.</param>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        /// <remarks>Returns silently if no property with specified key was found in the section.</remarks>
        public void RemoveAllProperties(string key)
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            RemoveAllProperties(p => p.IsKeyEqualTo(key));
        }

        /// <summary>
        /// Removes all properties from the section that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="predicate">A delegate that defines the conditions of the properties to remove.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="predicate"/> is <c>null</c>.</exception>
        /// <remarks>Returns silently if no property matched the conditions defined by the specified predicate.</remarks>
        public void RemoveAllProperties(Predicate<IniProperty> predicate)
        {
            Ensure.NotNull(predicate, nameof(predicate));

            properties.RemoveAll(predicate);
        }

        /// <summary>
        /// Clears all properties from the section.
        /// </summary>
        public void ClearProperties()
        {
            properties.Clear();
        }

        /// <summary>
        /// Renames all properties with the specified old key to the new key in the section.
        /// </summary>
        /// <param name="oldKey">The key of the properties to be renamed.</param>
        /// <param name="newKey">The key to assign to the properties.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldKey"/> or <paramref name="newKey"/> is <c>null</c> or empty.
        /// </exception>
        /// <remarks>Returns silently if no property with specified key was found in the section.</remarks>
        public void RenameProperties(string oldKey, string newKey)
        {
            Ensure.NotNullOrEmpty(oldKey, nameof(oldKey));
            Ensure.NotNullOrEmpty(newKey, nameof(newKey));

            for (var i = 0; i < properties.Count; i++)
            {
                if (properties[i].IsKeyEqualTo(oldKey))
                {
                    properties[i] = new IniProperty(newKey, properties[i]);
                }
            }
        }

        /// <summary>
        /// Adds a comment line to the section's comments.
        /// </summary>
        /// <param name="comment">The comment line to add.</param>
        /// <exception cref="ArgumentException">When <paramref name="comment"/> is <c>null</c> or empty.</exception>
        public void AddComment(string comment)
        {
            Ensure.NotNull(comment, nameof(comment));

            comments.Add(comment);
        }

        /// <summary>
        /// Adds a collection of comment lines to the section's comments.
        /// </summary>
        /// <param name="commentsToAdd">The comment lines to add.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="commentsToAdd"/> is <c>null</c>.</exception>
        public void AddMultipleComments(IEnumerable<string> commentsToAdd)
        {
            Ensure.NotNull(commentsToAdd, nameof(commentsToAdd));

            comments.AddRange(commentsToAdd);
        }

        /// <summary>
        /// Removes the specified comment line from the section's comments.
        /// </summary>
        /// <param name="comment">The comment line to remove.</param>
        /// <exception cref="ArgumentException">When <paramref name="comment"/> is <c>null</c> or empty.</exception>
        /// <remarks>Returns silently if the comment was not found in the section's comments.</remarks>
        public void RemoveComment(string comment)
        {
            Ensure.NotNullOrEmpty(comment, nameof(comment));

            comments.Remove(comment);
        }

        /// <summary>
        /// Clears all comments associated with the section.
        /// </summary>
        public void ClearComments()
        {
            comments.Clear();
        }

        /// <summary>
        /// Returns whether the name of the section is equal to the specified name.
        /// </summary>
        /// <param name="name">The name to compare against.</param>
        /// <returns><c>true</c> if the names are equal; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">When <paramref name="name"/> is <c>null</c> or empty.</exception>
        public bool IsNameEqualTo(string name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            return Name.EqualsCaseInsensitive(name);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) ||
                   (obj is IniSection other && Equals(other));
        }

        /// <inheritdoc/>
        public bool Equals(IniSection other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var comparison = StringComparison.InvariantCultureIgnoreCase;
            StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;

            return string.Equals(Name, other.Name, comparison) &&
                   Equals(LineNumber, other.LineNumber) &&
                   Properties.SequenceEqual(other.Properties) &&
                   Comments.SequenceEqual(other.comments, comparer);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;

            return comparer.GetHashCode(Name);
        }
    }
}