using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public class DelftIniReader : NGHSFileBase, IDelftIniReader
    {
        private readonly Regex keyValueCommentRegex = new Regex(KeyValueCommentPattern);

        /// <summary>
        /// Regular expression for a key/value/comment line, where key is a string without white-spaces,
        /// value can be anything and an optional comment 
        /// starting with the '#' character.
        /// </summary>
        protected const string KeyValueCommentPattern = @"^\s*(?<key>[^=\s]+)\s*=\s*(?<value>[^#]*)(#(?<comment>.*))?$";

        /// <summary>
        /// Reads a Delft .ini format file.
        /// </summary>
        /// <param name="iniFile">File path to be read</param>
        /// <returns>All parsed .ini groups with key-value pairs and comments.</returns>
        /// <exception cref="ArgumentException"><paramref name="iniFile"/> is an empty string ("").</exception>
        /// <exception cref="ArgumentNullException"><paramref name="iniFile"/> is null.</exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="IOException"><paramref name="iniFile"/> includes an incorrect or invalid syntax for file name, directory name, or volume label.</exception>
        /// <exception cref="FormatException">When an invalid line was encountered.</exception>
        public IList<DelftIniCategory> ReadDelftIniFile(string iniFile)
        {
            var content = new List<DelftIniCategory>();

            OpenInputFile(iniFile);
            try
            {
                string line;
                DelftIniCategory currentCategory = null;
                string categoryName = null;
                while ((line = GetNextLine()) != null)
                {
                    line = line.Trim();
                    if(string.IsNullOrEmpty(line)) continue; // Skip white-space characters.

                    if (IsNewCategory(line, ref categoryName))
                    {
                        currentCategory = new DelftIniCategory(categoryName) {LineNumber = LineNumber};
                        content.Add(currentCategory);
                        continue;
                    }
                    if (currentCategory == null) continue;

                    ReadFields(line, currentCategory);
                }
            }
            finally
            {
                CloseInputFile();
            }

            return content;
        }

        /// <summary>
        /// Parses a line into a DelftIniProperty.
        /// </summary>
        /// <param name="line">Line to be parsed.</param>
        /// <param name="currentCategory">The current category.</param>
        protected virtual void ReadFields(string line, DelftIniCategory currentCategory)
        {
            var fields = GetKeyValueComment(line);
            currentCategory.Properties.Add(new DelftIniProperty
                {Name = fields[0], Value = fields[1], Comment = fields[2], LineNumber = LineNumber});
        }

        /// <summary>
        /// Parses a line expecting a key-value-comment pattern.
        /// </summary>
        /// <param name="line">Line to be parsed.</param>
        /// <returns>A size 3 array of strings, where first item is the key, second the value and third the comment.</returns>
        /// <exception cref="FormatException">When <paramref name="line"/> does not match to <see cref="KeyValueCommentPattern"/>.</exception>
        protected virtual string[] GetKeyValueComment(string line)
        {
            var result = new string[3];

            var matches = keyValueCommentRegex.Matches(line);
            if(matches.Count == 0) throw new FormatException(String.Format("Invalid key-value-comment line on line {0} in file {1}", 
                                                                           LineNumber, InputFilePath));

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
        protected bool IsNewCategory(string line, ref string newCategory)
        {
            if (line.StartsWith("["))
            {
                // group line
                int endIndex = line.LastIndexOf("]", StringComparison.Ordinal);
                if (endIndex < 3)
                {
                    throw new FormatException(String.Format("Invalid group on line {0} in file {1}", LineNumber, InputFilePath));
                }
                newCategory = line.Substring(1, endIndex - 1).Trim();
                return true;
            }
            return false;
        }
    }
}