using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// Contains information of a delft INI property.
    /// </summary>
    public struct DelftIniPropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelftIniPropertyInfo"/> class.
        /// </summary>
        /// <param name="category"> The category name of the property. </param>
        /// <param name="property"> The name of the property. </param>
        /// <param name="value"> The value of the property. </param>
        public DelftIniPropertyInfo(string category, string property, string value)
        {
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(property, nameof(property));
            Ensure.NotNull(value, nameof(value));

            Category = category;
            Property = property;
            Value = value;
        }

        /// <summary>
        /// The category name.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// The property name.
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// The property value.
        /// </summary>
        public string Value { get; }
    }
}