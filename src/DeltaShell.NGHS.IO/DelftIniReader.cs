using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.IO.Properties;

namespace DeltaShell.NGHS.IO
{
    /// <summary>
    /// Reader for Delft Ini-files.
    /// </summary>
    public class DelftIniReader : NGHSFileBase, IDelftIniReader
    {
        /// <summary>
        /// Regular expression for a key/value/comment line, where key is a string without white-spaces,
        /// value can be anything and an optional comment
        /// starting with the '#' character.
        /// </summary>
        private const string keyValueCommentPattern = @"^\s*(?<key>[^=\s]+)\s*=\s*(?<value>[^#]*)(#(?<comment>.*))?$";

        /// <inheritdoc cref="IDelftIniReader"/>
        public IList<DelftIniCategory> ReadDelftIniFile(Stream stream, string filePath)
        {
            OpenInputFile(stream);
            InputFilePath = filePath;

            var content = new List<DelftIniCategory>();
            try
            {
                string line;
                DelftIniCategory currentCategory = null;
                string categoryName = null;
                while ((line = GetNextLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue; // Skip white-space characters.
                    }

                    if (IsNewCategory(line, ref categoryName))
                    {
                        currentCategory = new DelftIniCategory(categoryName, LineNumber);
                        content.Add(currentCategory);
                        continue;
                    }

                    if (currentCategory == null)
                    {
                        continue;
                    }

                    string[] fields = GetKeyValueComment(line);
                    var delftIniProperty = new DelftIniProperty(fields[0], fields[1], fields[2], LineNumber);
                    currentCategory.AddProperty(delftIniProperty);
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
                throw new FormatException(string.Format(Resources.DelftIniReader_GetKeyValueComment_Invalid_key_value_comment_line_on_line__0__in_file__1_,
                                                        LineNumber, InputFilePath));
            }

            result[0] = matches[0].Groups["key"].Value.Trim();
            result[1] = matches[0].Groups["value"].Value.Trim();
            result[2] = matches[0].Groups["comment"].Value.Trim(); // Returns "" if comment group not matched

            return result;
        }

        /// <summary>
        /// Reads the line and checks if it represents a new category/group.
        /// </summary>
        /// <param name="line">Line to be interpreted.</param>
        /// <param name="newCategory">Set to the name of the category/group, when returning true.</param>
        /// <returns>True if the line represents a new category/group; False otherwise.</returns>
        /// <exception cref="FormatException">When an invalid category/group line was encountered.</exception>
        private bool IsNewCategory(string line, ref string newCategory)
        {
            if (line.StartsWith("["))
            {
                // group line
                int endIndex = line.LastIndexOf("]", StringComparison.Ordinal);
                if (endIndex < 3)
                {
                    throw new FormatException(string.Format(Resources.DelftIniReader_Invalid_group_on_line__0__in_file__1_, LineNumber, InputFilePath));
                }

                newCategory = line.Substring(1, endIndex - 1).Trim();
                return true;
            }

            return false;
        }
    }
}