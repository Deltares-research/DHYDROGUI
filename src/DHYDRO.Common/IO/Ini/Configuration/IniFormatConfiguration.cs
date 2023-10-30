using System;

namespace DHYDRO.Common.IO.Ini.Configuration
{
    /// <summary>
    /// Represents the configuration for formatting INI data.
    /// </summary>
    public sealed class IniFormatConfiguration
    {
        /// <summary>
        /// Gets or sets the string used for new lines in the formatted INI data.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="Environment.NewLine"/>.
        /// </remarks>
        public string NewLineString { get; set; } = Environment.NewLine;

        /// <summary>
        /// Gets or sets the level of indentation for properties in the formatted INI data.
        /// </summary>
        /// <remarks>
        /// The default value is <c>0</c>.
        /// </remarks>
        public uint PropertyIndentationLevel { get; set; }

        /// <summary>
        /// Gets or sets the width reserved for property keys in the formatted INI data.
        /// </summary>
        /// <remarks>
        /// This value excludes the single whitespace before the property assignment character. The default value is <c>21</c>.
        /// </remarks>
        public uint PropertyKeyWidth { get; set; } = 21;

        /// <summary>
        /// Gets or sets the width reserved for property values in the formatted INI data.
        /// </summary>
        /// <remarks>
        /// This value excludes the single whitespace after the property assignment character. The default value is <c>20</c>.
        /// </remarks>
        public uint PropertyValueWidth { get; set; } = 20;

        /// <summary>
        /// Gets or sets a value indicating whether section and property comments should be written to the formatted INI data.
        /// </summary>
        /// <remarks>
        /// The default value is <c>true</c>.
        /// </remarks>
        public bool WriteComments { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether properties without values should be written to the formatted INI data.
        /// </summary>
        /// <remarks>
        /// The default value is <c>false</c>.
        /// </remarks>
        public bool WritePropertyWithoutValue { get; set; }
    }
}