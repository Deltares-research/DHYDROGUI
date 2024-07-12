using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Deltares.Infrastructure.API.Guards;
using log4net;

namespace DHYDRO.Common.IO.ExtForce
{
    /// <summary>
    /// Represent one external forcing from the external forcings file.
    /// </summary>
    public sealed class ExtForceData
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExtForceData));

        private readonly List<string> comments;
        private readonly Dictionary<string, string> modelData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtForceData"/> class.
        /// </summary>
        public ExtForceData()
        {
            comments = new List<string>();
            modelData = new Dictionary<string, string>(
                StringComparer.InvariantCultureIgnoreCase);
            
            Comments = new ReadOnlyCollection<string>(comments);
            ModelData = new ReadOnlyDictionary<string, string>(modelData);
        }

        /// <summary>
        /// Gets the comments associated with this external forcing.
        /// </summary>
        public IReadOnlyCollection<string> Comments { get; }

        /// <summary>
        /// Gets the model data as key-value pairs.
        /// </summary>
        public IReadOnlyDictionary<string, string> ModelData { get; }

        /// <summary>
        /// The line number where the external forcing is located.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Whether this forcing is enabled. The default value is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// A forcing is considered disabled if the <see cref="Quantity"/> property contains either
        /// <see cref="ExtForceFileConstants.Keys.UnsupportedQuantities"/> or
        /// <see cref="ExtForceFileConstants.Keys.DisabledQuantity"/>.
        /// </remarks>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The name of the quantity.
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// The name of the file associated with this forcing.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The variable name used in the filename associated with this forcing. This is an optional setting.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// The parent directory of the data file.
        /// </summary>
        public string ParentDirectory { get; set; }

        /// <summary>
        /// The type of the data file.
        /// </summary>
        public int? FileType { get; set; }

        /// <summary>
        /// The type of interpolation operation method.
        /// </summary>
        public int? Method { get; set; }

        /// <summary>
        /// The type of operand; how the data is combined with existing data for this quantity.
        /// </summary>
        public string Operand { get; set; }

        /// <summary>
        /// Custom coefficients for transformation. This is an optional setting.
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// The conversion factor. This is an optional setting.
        /// </summary>
        public double? Factor { get; set; }

        /// <summary>
        /// The offset value. This is an optional setting.
        /// </summary>
        public double? Offset { get; set; }

        /// <summary>
        /// Adds a comment to the list of comments.
        /// </summary>
        /// <param name="comment">The comment to add.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="comment"/> is <c>null</c>.</exception>
        public void AddComment(string comment)
        {
            Ensure.NotNull(comment, nameof(comment));

            comments.Add(comment);
        }

        /// <summary>
        /// Gets the model data value for the specified key.
        /// </summary>
        /// <param name="key">The key of the model data to get.</param>
        /// <returns>The model data value with the specified key.</returns>
        /// <exception cref="System.ArgumentException">When <paramref name="key"/> is <c>null</c>, empty or whitespace.</exception>
        /// <exception cref="KeyNotFoundException">When the model data does not contain the specified <paramref name="key"/>.</exception>
        public string GetModelData(string key)
        {
            Ensure.NotNullOrWhiteSpace(key, nameof(key));

            return modelData[key];
        }

        /// <summary>
        /// Tries to get and convert the model data value for the specified key.
        /// </summary>
        /// <param name="key">The key of the model data to get.</param>
        /// <param name="convertedValue">The converted value if the conversion succeeded; otherwise, the default value.</param>
        /// <typeparam name="T">The type to convert the value to, must implement <see cref="IConvertible"/>.</typeparam>
        /// <returns><c>true</c> if the value was retrieved and the conversion succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentException">When <paramref name="key"/> is <c>null</c>, empty or whitespace.</exception>
        public bool TryGetModelData<T>(string key, out T convertedValue)
            where T : IConvertible
        {
            Ensure.NotNullOrWhiteSpace(key, nameof(key));

            if (!modelData.TryGetValue(key, out string value))
            {
                convertedValue = default;
                return false;
            }

            try
            {
                convertedValue = (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                log.ErrorFormat("Property '{0}' cannot be converted to a {1} for value: '{2}'. Forcing line: {3}.", key, typeof(T).Name, value, LineNumber);
                convertedValue = default;
                return false;
            }
        }

        /// <summary>
        /// Adds or updates a key-value pair in the model data.
        /// </summary>
        /// <param name="key">The key of the value to add or update.</param>
        /// <param name="value">The value to add or update.</param>
        /// <exception cref="System.ArgumentException">When <paramref name="key"/> is <c>null</c>, empty or whitespace.</exception>
        public void SetModelData(string key, string value)
        {
            Ensure.NotNullOrWhiteSpace(key, nameof(key));

            modelData[key] = value;
        }

        /// <summary>
        /// Adds or updates a key-value pair in the model data, converting the value to a string.
        /// </summary>
        /// <param name="key">The key of the value to add or update.</param>
        /// <param name="value">The value to add or update.</param>
        /// <typeparam name="T">The type to convert the value from, must implement <see cref="IConvertible"/>.</typeparam>
        /// <exception cref="System.ArgumentException">When <paramref name="key"/> is <c>null</c>, empty or whitespace.</exception>
        public void SetModelData<T>(string key, T value)
            where T : IConvertible
        {
            Ensure.NotNullOrWhiteSpace(key, nameof(key));

            modelData[key] = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines whether the model data contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the model data.</param>
        /// <returns>true if the model data contains an element with the specified key; otherwise, false.</returns>
        /// <exception cref="System.ArgumentException">When <paramref name="key"/> is <c>null</c>, empty or whitespace.</exception>
        public bool ContainsModelData(string key)
        {
            Ensure.NotNullOrWhiteSpace(key, nameof(key));

            return modelData.ContainsKey(key);
        }
    }
}