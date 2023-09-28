using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DHYDRO.Common.Guards;
using DHYDRO.Common.IO.Ini.Configuration;

namespace DHYDRO.Common.IO.Ini
{
    /// <summary>
    /// Formats INI data to an INI-formatted string.
    /// </summary>
    /// <remarks>
    /// The formatting behavior can be customized through the <see cref="Configuration"/> property, which specifies
    /// formatting options like the property key/value width and indentation and whether properties without value
    /// should be written.
    /// <para/>
    /// The INI file format can be customized through the <see cref="Scheme"/> property, which specifies the characters
    /// that define sections, properties and comments.
    /// </remarks>
    public sealed class IniFormatter
    {
        private IniFormatConfiguration configuration;
        private IniScheme scheme;
        private TextWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="IniFormatter"/> class.
        /// </summary>
        public IniFormatter()
        {
            configuration = new IniFormatConfiguration();
            scheme = new IniScheme();
        }

        /// <summary>
        /// Gets or sets the configuration that controls the INI formatting.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        public IniFormatConfiguration Configuration
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
        /// Formats the specified INI data to an INI-formatted string.
        /// </summary>
        /// <param name="iniData">The <see cref="IniData"/> to format.</param>
        /// <returns>The formatted INI string.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="iniData"/> is <c>null</c>.</exception>
        public string Format(IniData iniData)
        {
            Ensure.NotNull(iniData, nameof(iniData));

            using (var stringWriter = new StringWriter())
            {
                Format(iniData, stringWriter);

                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Formats the specified INI data to an INI-formatted string and writes it to the specified stream.
        /// </summary>
        /// <param name="iniData">The <see cref="IniData"/> to format.</param>
        /// <param name="stream">The <see cref="Stream"/> to write the formatted INI data to.</param>
        /// <exception cref="ArgumentNullException">
        /// When <paramref name="iniData"/> or <paramref name="stream"/> is <c>null</c>.
        /// </exception>
        public void Format(IniData iniData, Stream stream)
        {
            Ensure.NotNull(iniData, nameof(iniData));
            Ensure.NotNull(stream, nameof(stream));

            using (var streamWriter = new StreamWriter(stream, Encoding.Default, 1024, true))
            {
                Format(iniData, streamWriter);
            }
        }

        /// <summary>
        /// Formats the specified INI data to an INI-formatted string and writes it to the specified writer.
        /// </summary>
        /// <param name="iniData">The <see cref="IniData"/> to format.</param>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write the formatted INI data to.</param>
        /// <exception cref="ArgumentNullException">
        /// When <paramref name="iniData"/> or <paramref name="textWriter"/> is <c>null</c>.
        /// </exception>
        public void Format(IniData iniData, TextWriter textWriter)
        {
            Ensure.NotNull(iniData, nameof(iniData));
            Ensure.NotNull(textWriter, nameof(textWriter));

            writer = textWriter;

            WriteIniData(iniData);
        }

        private void WriteIniData(IniData iniData)
        {
            WriteSections(iniData.Sections);
        }

        private void WriteSections(IEnumerable<IniSection> sections)
        {
            foreach (IniSection section in sections)
            {
                WriteSection(section);
                WriteNewLine();
            }
        }

        private void WriteSection(IniSection section)
        {
            if (Configuration.WriteComments && section.Comments.Any())
            {
                WriteComments(section.Comments);
                WriteNewLine();
            }

            writer.Write(Scheme.SectionStartDelimiter);
            writer.Write(section.Name);
            writer.Write(Scheme.SectionEndDelimiter);

            if (section.Properties.Any())
            {
                WriteNewLine();
                WriteProperties(section.Properties);
            }
        }

        private void WriteProperties(IEnumerable<IniProperty> properties)
        {
            foreach (IniProperty property in properties)
            {
                if (CanWriteProperty(property))
                {
                    WriteProperty(property);
                    WriteNewLine();
                }
            }
        }

        private bool CanWriteProperty(IniProperty property)
        {
            return property.HasValue() || Configuration.WritePropertyWithoutValue;
        }

        private void WriteProperty(IniProperty property)
        {
            writer.Write($"{{0,{-Configuration.PropertyIndentationLevel}}}", string.Empty);
            writer.Write($"{{0,{-Configuration.PropertyKeyWidth}}}", property.Key);
            writer.Write($" {Scheme.PropertyAssignmentDelimiter} ");
            writer.Write($"{{0,{-Configuration.PropertyValueWidth}}}", property.Value);

            if (Configuration.WriteComments && property.HasComment())
            {
                WriteComment(property.Comment);
            }
        }

        private void WriteComments(IEnumerable<string> comments)
        {
            foreach (string comment in comments)
            {
                WriteComment(comment);
                WriteNewLine();
            }
        }

        private void WriteComment(string comment)
        {
            writer.Write($"{Scheme.CommentDelimiter} {comment}");
        }

        private void WriteNewLine()
        {
            writer.Write(Configuration.NewLineString);
        }
    }
}