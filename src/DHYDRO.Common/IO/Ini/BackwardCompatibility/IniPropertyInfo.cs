using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.Ini.BackwardCompatibility
{
    /// <summary>
    /// Contains information of an INI property.
    /// </summary>
    public struct IniPropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IniPropertyInfo"/> class.
        /// </summary>
        /// <param name="section"> The section name of the property. </param>
        /// <param name="property"> The name of the property. </param>
        /// <param name="value"> The value of the property. </param>
        public IniPropertyInfo(string section, string property, string value)
        {
            Ensure.NotNull(section, nameof(section));
            Ensure.NotNull(property, nameof(property));
            Ensure.NotNull(value, nameof(value));

            Section = section;
            Property = property;
            Value = value;
        }

        /// <summary>
        /// The section name.
        /// </summary>
        public string Section { get; }

        /// <summary>
        /// The property key.
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// The property value.
        /// </summary>
        public string Value { get; }
    }
}