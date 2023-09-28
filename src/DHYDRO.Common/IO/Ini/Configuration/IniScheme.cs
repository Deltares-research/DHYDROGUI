namespace DHYDRO.Common.IO.Ini.Configuration
{
    /// <summary>
    /// Defines the format of an INI file through customization of the characters
    /// that define sections, property value assignment and comments.
    /// </summary>
    /// <remarks>
    /// By default the various delimiters for the INI file are set to:
    /// <para/>
    /// '#' for one-line comments and inline comments<br/>
    /// '#' for delimiting a special value<br/>
    /// '[' ']' for delimiting a section<br/>
    /// '=' for property key / value pairs<br/>
    /// '\' for multi-line property values<br/>
    /// <example>
    /// An example of well formed data with the default values:
    /// <para/>
    /// # section comment line 1<br/>
    /// # section comment line 2<br/>
    /// [section]<br/>
    /// key1 = value1 # inline property comment<br/>
    /// key2 = value2 \<br/>
    /// value3 \<br/>
    /// value4 # inline property comment<br/>
    /// key3 = #value5# # inline property comment<br/>
    /// </example>
    /// </remarks>
    public sealed class IniScheme
    {
        /// <summary>
        /// Gets or sets the delimiter used to indicate comments in INI data.
        /// </summary>
        /// <remarks>
        /// The default value is '#'.
        /// </remarks>
        public char CommentDelimiter { get; set; } = '#';

        /// <summary>
        /// Gets or sets the delimiter used to indicate the start of sections in INI data.
        /// </summary>
        /// <remarks>
        /// The default value is '['.
        /// </remarks>
        public char SectionStartDelimiter { get; set; } = '[';

        /// <summary>
        /// Gets or sets the delimiter used to indicate the end of sections in INI data.
        /// </summary>
        /// <remarks>
        /// The default value is ']'.
        /// </remarks>
        public char SectionEndDelimiter { get; set; } = ']';

        /// <summary>
        /// Gets or sets the delimiter used to separate property keys and values in INI data.
        /// </summary>
        /// <remarks>
        /// The default value is '='.
        /// </remarks>
        public char PropertyAssignmentDelimiter { get; set; } = '=';
        
        /// <summary>
        /// Gets or sets the delimiter used to separate multiline property values in INI data.
        /// </summary>
        /// <remarks>
        /// When a property value spans multiple lines, this string is used to indicate the continuation
        /// of the value on subsequent lines. The default value is '\'.
        /// </remarks>
        public char MultiLineValueDelimiter { get; set; } = '\\';

        /// <summary>
        /// Gets or sets the start/end delimiter for special values in INI data.
        /// </summary>
        /// <remarks>
        /// The default value is '#'.
        /// </remarks>
        public char SpecialValueDelimiter { get; set; } = '#';
    }
}