using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.Guards;
using DHYDRO.Common.IO.Ini.Configuration;

namespace DHYDRO.Common.IO.Ini
{
    /// <summary>
    /// Parses INI-formatted text to an INI data object.
    /// </summary>
    /// <remarks>
    /// The parsing behavior can be customized through the <see cref="Configuration"/> property, which specifies parsing
    /// options like whether duplicate section names, duplicate property keys and multi-line values are allowed.
    /// <para/>
    /// The INI file format can be customized through the <see cref="Scheme"/> property, which specifies the characters
    /// that define sections, properties and comments.
    /// </remarks>
    public sealed class IniParser
    {
        private readonly Encoding utf8NoBom;

        private IniScheme scheme;
        private IniParseConfiguration configuration;

        private IniData iniData;
        private IniSection currentSection;
        private IniProperty currentProperty;

        private List<string> commentsTemp;
        private HashSet<string> foundSections;
        private HashSet<string> foundProperties;

        private string currentLine;
        private int lineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="IniParser"/> class.
        /// </summary>
        public IniParser()
        {
            utf8NoBom = new UTF8Encoding(false);
            scheme = new IniScheme();
            configuration = new IniParseConfiguration();
        }

        /// <summary>
        /// Gets or sets the configuration that controls the INI parsing.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        public IniParseConfiguration Configuration
        {
            get => configuration;
            set
            {
                Ensure.NotNull(value, nameof(value));
                configuration = value;
            }
        }

        /// <summary>
        /// Gets or sets the scheme that defines the format of the INI file.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        public IniScheme Scheme
        {
            get => scheme;
            set
            {
                Ensure.NotNull(value, nameof(value));
                scheme = value;
            }
        }

        /// <summary>
        /// Parses INI-formatted text from the specified string to an INI data object.
        /// </summary>
        /// <param name="ini">The INI-formatted text to parse.</param>
        /// <returns>An <see cref="IniData"/> object containing the parsed INI data.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="ini"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">When the INI text has an invalid format.</exception>
        public IniData Parse(string ini)
        {
            Ensure.NotNull(ini, nameof(ini));

            using (var stringReader = new StringReader(ini))
            {
                return Parse(stringReader);
            }
        }

        /// <summary>
        /// Parses INI-formatted text from the specified stream to an INI data object.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> from which to read the INI-formatted text.</param>
        /// <returns>An <see cref="IniData"/> object containing the parsed INI data.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="stream"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">When the INI text has an invalid format.</exception>
        public IniData Parse(Stream stream)
        {
            Ensure.NotNull(stream, nameof(stream));

            using (var streamReader = new StreamReader(stream, utf8NoBom, true, 1024, true))
            {
                return Parse(streamReader);
            }
        }

        /// <summary>
        /// Parses INI-formatted text from the specified reader to an INI data object.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> from which to read the INI-formatted text.</param>
        /// <returns>An <see cref="IniData"/> object containing the parsed INI data.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="reader"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">When the INI text has an invalid format.</exception>
        public IniData Parse(TextReader reader)
        {
            Ensure.NotNull(reader, nameof(reader));

            InitializeParsingContext();

            while ((currentLine = reader.ReadLine()) != null)
            {
                lineNumber++;

                CleanCurrentLine();
                ParseCurrentLine();
            }

            return iniData;
        }

        private void InitializeParsingContext()
        {
            iniData = new IniData();
            commentsTemp = new List<string>();
            foundSections = new HashSet<string>();
            foundProperties = new HashSet<string>();
            currentSection = null;
            currentProperty = null;
            currentLine = null;
            lineNumber = 0;
        }

        private void CleanCurrentLine()
        {
            currentLine = currentLine.Replace('\t', ' ');
            currentLine = currentLine.Trim();
        }

        private void ParseCurrentLine()
        {
            if (IsEmptyLine())
            {
                return;
            }

            if (IsCommentLine())
            {
                ParseCommentLine();
            }
            else if (IsSectionLine())
            {
                ParseSectionLine();
            }
            else if (IsPropertyLine())
            {
                ParsePropertyLine();
            }
            else if (IsMultiLineValueLine())
            {
                ParseMultiLineValueLine();
            }
            else
            {
                throw new FormatException($"Error on line {lineNumber}: invalid INI-formatted text.");
            }
        }

        private bool IsEmptyLine()
        {
            return string.IsNullOrEmpty(currentLine);
        }

        private bool IsCommentLine()
        {
            return currentLine.StartsWith(Scheme.CommentDelimiter);
        }

        private void ParseCommentLine()
        {
            if (!Configuration.ParseComments)
            {
                return;
            }

            int commentIndex = currentLine.IndexOf(Scheme.CommentDelimiter);
            string comment = currentLine.Substring(commentIndex + 1).Trim();

            commentsTemp.Add(comment);
        }

        private bool IsSectionLine()
        {
            return currentLine.StartsWith(Scheme.SectionStartDelimiter) && currentLine.Contains(Scheme.SectionEndDelimiter);
        }

        private void ParseSectionLine()
        {
            int sectionStartIndex = currentLine.IndexOf(Scheme.SectionStartDelimiter);
            int sectionEndIndex = currentLine.LastIndexOf(Scheme.SectionEndDelimiter);

            string sectionName = currentLine.Substring(sectionStartIndex + 1, sectionEndIndex - sectionStartIndex - 1).Trim();
            ValidateSectionName(sectionName);

            AddNewSection(sectionName);
        }

        private void ValidateSectionName(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                throw new FormatException($"Error on line {lineNumber}: section name cannot be empty.");
            }

            if (!Configuration.AllowDuplicateSections && !foundSections.Add(sectionName))
            {
                throw new FormatException($"Error on line {lineNumber}: duplicate section with name '{sectionName}'.");
            }

            if (!Configuration.AllowDuplicateProperties)
            {
                foundProperties.Clear();
            }
        }

        private void AddNewSection(string sectionName)
        {
            currentSection = new IniSection(sectionName) { LineNumber = lineNumber };
            currentSection.AddMultipleComments(commentsTemp);
            iniData.AddSection(currentSection);
            commentsTemp.Clear();
        }

        private bool IsPropertyLine()
        {
            return currentLine.Contains(Scheme.PropertyAssignmentDelimiter);
        }

        private void ParsePropertyLine()
        {
            if (currentSection == null)
            {
                throw new FormatException($"Error on line {lineNumber}: global properties are not allowed.");
            }

            int assignmentIndex = currentLine.IndexOf(Scheme.PropertyAssignmentDelimiter);
            string key = currentLine.Substring(0, assignmentIndex).Trim();
            ValidatePropertyKey(key);

            int commentIndex = currentLine.LastIndexOf(Scheme.CommentDelimiter);
            int valueStartIndex = assignmentIndex + 1;

            string value = commentIndex > assignmentIndex
                               ? currentLine.Substring(valueStartIndex, commentIndex - valueStartIndex).Trim()
                               : currentLine.Substring(valueStartIndex).Trim();
            value = CleanupMultiLineValue(value);
            value = CleanupSpecialValue(value);

            string comment = commentIndex != -1 && Configuration.ParseComments
                                 ? currentLine.Substring(commentIndex + 1).Trim()
                                 : string.Empty;

            AddNewProperty(key, value, comment);
        }

        private void ValidatePropertyKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new FormatException($"Error on line {lineNumber}: property key cannot be empty.");
            }

            if (!Configuration.AllowPropertyKeysWithSpaces && key.Contains(" "))
            {
                throw new FormatException($"Error on line {lineNumber}: property key cannot contain spaces.");
            }

            if (!Configuration.AllowDuplicateProperties && !foundProperties.Add(key))
            {
                throw new FormatException($"Error on line {lineNumber}: duplicate property with key '{key}'.");
            }
        }

        private string CleanupMultiLineValue(string value)
        {
            return Configuration.AllowMultiLineValues
                       ? value.TrimEnd(Scheme.MultiLineValueDelimiter, ' ')
                       : value;
        }

        private string CleanupSpecialValue(string value)
        {
            return Configuration.CleanDelimitedValues
                       ? value.Trim(Scheme.SpecialValueDelimiter, ' ')
                       : value;
        }

        private void AddNewProperty(string key, string value, string comment = "")
        {
            currentProperty = new IniProperty(key, value, comment) { LineNumber = lineNumber };
            currentSection.AddProperty(currentProperty);
            commentsTemp.Clear();
        }

        private bool IsMultiLineValueLine()
        {
            return Configuration.AllowMultiLineValues && !currentLine.Contains(Scheme.PropertyAssignmentDelimiter);
        }

        private void ParseMultiLineValueLine()
        {
            if (currentProperty == null)
            {
                throw new FormatException($"Error on line {lineNumber}: global property values are not allowed.");
            }

            int commentIndex = currentLine.LastIndexOf(Scheme.CommentDelimiter);

            string value = commentIndex != -1
                               ? currentLine.Substring(0, commentIndex).Trim()
                               : currentLine.Trim();
            value = CleanupMultiLineValue(value);

            string comment = commentIndex != -1 && Configuration.ParseComments
                                 ? currentLine.Substring(commentIndex + 1).Trim()
                                 : string.Empty;

            AppendValueAndComment(value, comment);
        }

        private void AppendValueAndComment(string value, string comment)
        {
            currentProperty.Value += $"{Environment.NewLine}{value}";
            currentProperty.Comment += $"{Environment.NewLine}{comment}";
            commentsTemp.Clear();
        }
    }
}