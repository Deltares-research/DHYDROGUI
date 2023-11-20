using System.Text;

namespace DHYDRO.Common.IO.Ini.Configuration
{
    /// <summary>
    /// Represents the configuration for parsing INI data.
    /// </summary>
    public sealed class IniParseConfiguration
    {
        /// <summary>
        /// Gets or sets the encoding used for parsing INI data from a stream.
        /// </summary>
        /// <remarks>
        /// The default value is UTF-8 without byte order mark.
        /// </remarks>
        public Encoding Encoding { get; set; } = new UTF8Encoding(false, true);
        
        /// <summary>
        /// Gets or sets a value indicating whether property keys with whitespaces are allowed during parsing.
        /// </summary>
        /// <remarks>
        /// The default value is <c>false</c>.
        /// </remarks>
        public bool AllowPropertyKeysWithSpaces { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether duplicate sections are allowed during parsing.
        /// </summary>
        /// <remarks>
        /// The default value is <c>true</c>.
        /// </remarks>
        public bool AllowDuplicateSections { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether duplicate properties within a section are allowed during parsing.
        /// </summary>
        /// <remarks>
        /// The default value is <c>true</c>.
        /// </remarks>
        public bool AllowDuplicateProperties { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether multiline property values are allowed during parsing.
        /// </summary>
        /// <remarks>
        /// The default value is <c>false</c>.
        /// </remarks>
        public bool AllowMultiLineValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the special property value delimiter should be cleaned during parsing.
        /// </summary>
        /// <remarks>
        /// The default value is <c>false</c>.
        /// </remarks>
        public bool CleanDelimitedValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether property and section comments are parsed.
        /// </summary>
        /// <remarks>
        /// The default value is <c>true</c>.
        /// </remarks>
        public bool ParseComments { get; set; } = true;
    }
}