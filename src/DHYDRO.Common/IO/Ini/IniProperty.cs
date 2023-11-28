using System;
using System.Diagnostics;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.Guards;
using DHYDRO.Common.IO.Ini.Converters;

namespace DHYDRO.Common.IO.Ini
{
    /// <summary>
    /// Represents a property with a key-value pair in an INI file.
    /// </summary>
    /// <remarks>
    /// This class encapsulates a key-value pair in an INI section.
    /// </remarks>
    [DebuggerDisplay("{Key}={Value}")]
    public sealed class IniProperty : IEquatable<IniProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IniProperty"/> class.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="value">The optional value of the property. The default is an empty string.</param>
        /// <param name="comment">The optional comment associated with the property. The default is an empty string.</param>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        public IniProperty(string key, string value = "", string comment = "")
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            Key = key;
            Value = value;
            Comment = comment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IniProperty"/> class with the same values as the specified property.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="other">The property to copy values from.</param>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="other"/> is <c>null</c>.</exception>
        public IniProperty(string key, IniProperty other)
        {
            Ensure.NotNullOrEmpty(key, nameof(key));
            Ensure.NotNull(other, nameof(other));

            Key = key;
            Value = other.Value;
            Comment = other.Comment;
            LineNumber = other.LineNumber;
        }

        /// <summary>
        /// Gets the key of the property.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets or sets the value of the property. The default value is an empty string.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a comment associated with the property. The default value is an empty string.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the line number where the property is located. The default value is 0.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="IniProperty"/> class with the specified key and value.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <typeparam name="T">The type of the value, must implement <see cref="IConvertible"/>.</typeparam>
        /// <returns>A new <see cref="IniProperty"/> instance.</returns>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        public static IniProperty Create<T>(string key, T value)
            where T : IConvertible
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            string formattedValue = IniValueConverter.ConvertToString(value);
            return new IniProperty(key, formattedValue);
        }

        /// <summary>
        /// Returns whether the property has a non-null and non-empty value.
        /// </summary>
        /// <returns><c>true</c> if the property has a value; otherwise, <c>false</c>.</returns>
        public bool HasValue()
        {
            return !string.IsNullOrEmpty(Value);
        }
        
        /// <summary>
        /// Returns whether the property has a non-null and non-empty comment.
        /// </summary>
        /// <returns><c>true</c> if the property has a comment; otherwise, <c>false</c>.</returns>
        public bool HasComment()
        {
            return !string.IsNullOrEmpty(Comment);
        }

        /// <summary>
        /// Tries to convert the property value to the specified type and retrieves the converted value.
        /// </summary>
        /// <param name="convertedValue">The converted value if the conversion succeeded; otherwise, the default value.</param>
        /// <typeparam name="T">The type to convert the value to, must implement <see cref="IConvertible"/>.</typeparam>
        /// <returns><c>true</c> if the conversion succeeded and the value was retrieved; otherwise, <c>false</c>.</returns>
        public bool TryGetValue<T>(out T convertedValue)
            where T : IConvertible
        {
            try
            {
                convertedValue = IniValueConverter.ConvertFromString<T>(Value);
                return true;
            }
            catch
            {
                convertedValue = default;
                return false;
            }
        }

        /// <summary>
        /// Sets the property value by converting the specified value to a string representation.
        /// </summary>
        /// <param name="value">The new value to set. Will be converted to a formatted string.</param>
        /// <typeparam name="T">The type of the value, must implement <see cref="IConvertible"/>.</typeparam>
        public void SetValue<T>(T value)
            where T : IConvertible
        {
            Value = IniValueConverter.ConvertToString(value);
        }

        /// <summary>
        /// Returns whether the key of the property is equal to the specified key.
        /// </summary>
        /// <param name="key">The key to compare against.</param>
        /// <returns><c>true</c> if the keys are equal; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentException">When <paramref name="key"/> is <c>null</c> or empty.</exception>
        public bool IsKeyEqualTo(string key)
        {
            Ensure.NotNullOrEmpty(key, nameof(key));

            return Key.EqualsCaseInsensitive(key);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) ||
                   (obj is IniProperty other && Equals(other));
        }

        /// <inheritdoc/>
        public bool Equals(IniProperty other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

            return string.Equals(Key, other.Key, comparison) &&
                   string.Equals(Value, other.Value, comparison) &&
                   string.Equals(Comment, other.Comment, comparison) &&
                   Equals(LineNumber, other.LineNumber);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;

            return comparer.GetHashCode(Key);
        }
    }
}