using System;
using System.IO;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.Ini
{
    /// <summary>
    /// Reader for INI files.
    /// </summary>
    public class IniReader : NGHSFileBase, IIniReader
    {
        /// <summary>
        /// Regular expression for a key/value/comment line, where key is a string without white-spaces,
        /// value can be anything and an optional comment
        /// starting with the '#' character.
        /// </summary>
        private const string keyValueCommentPattern = @"^\s*(?<key>[^=\s]+)\s*=\s*(?<value>[^#]*)(#(?<comment>.*))?$";

        /// <inheritdoc cref="IIniReader"/>
        public IniData ReadIniFile(Stream stream, string filePath)
        {
            OpenInputFile(stream);
            InputFilePath = filePath;

            var content = new IniData();
            try
            {
                string line;
                IniSection currentSection = null;
                string sectionName = null;
                while ((line = GetNextLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue; // Skip white-space characters.
                    }

                    if (IsNewSection(line, ref sectionName))
                    {
                        currentSection = new IniSection(sectionName) {LineNumber = LineNumber};
                        content.AddSection(currentSection);
                        continue;
                    }

                    if (currentSection == null)
                    {
                        continue;
                    }

                    string[] fields = GetKeyValueComment(line);
                    var iniProperty = new IniProperty(fields[0], fields[1]) {Comment = fields[2], LineNumber = LineNumber};
                    currentSection.AddProperty(iniProperty);
                }
            }
            finally
            {
                CloseInputFile();
            }

            return content;
        }

        /// <summary>
        /// Parses a line expecting a key-value-comment pattern.
        /// </summary>
        /// <param name="lineContent">Line to be parsed.</param>
        /// <returns>A size 3 array of strings, where first item is the key, second the value and third the comment.</returns>
        /// <exception cref="FormatException">
        /// When <paramref name="lineContent"/> does not match to
        /// <see cref="keyValueCommentPattern"/>.
        /// </exception>
        protected virtual string[] GetKeyValueComment(string lineContent)
        {
            var result = new string[3];

            MatchCollection matches = RegularExpression.GetMatches(keyValueCommentPattern, lineContent);
            if (matches.Count == 0)
            {
                throw new FormatException(string.Format(Resources.IniReader_GetKeyValueComment_Invalid_key_value_comment_line_on_line__0__in_file__1_,
                                                        LineNumber, InputFilePath));
            }

            result[0] = matches[0].Groups["key"].Value.Trim();
            result[1] = matches[0].Groups["value"].Value.Trim();
            result[2] = matches[0].Groups["comment"].Value.Trim(); // Returns "" if comment group not matched

            return result;
        }

        /// <summary>
        /// Reads the line and checks if it represents a new section.
        /// </summary>
        /// <param name="line">Line to be interpreted.</param>
        /// <param name="newSection">Set to the name of the section, when returning true.</param>
        /// <returns>True if the line represents a new section; False otherwise.</returns>
        /// <exception cref="FormatException">When an invalid section line was encountered.</exception>
        private bool IsNewSection(string line, ref string newSection)
        {
            if (line.StartsWith("["))
            {
                // group line
                int endIndex = line.LastIndexOf("]", StringComparison.Ordinal);
                if (endIndex < 3)
                {
                    throw new FormatException(string.Format(Resources.IniReader_Invalid_group_on_line__0__in_file__1_, LineNumber, InputFilePath));
                }

                newSection = line.Substring(1, endIndex - 1).Trim();
                return true;
            }

            return false;
        }
    }
}