using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.Extensions;

namespace DHYDRO.Common.IO.ExtForce
{
    /// <summary>
    /// Provides a parser for the external forcings file (*.ext).
    /// </summary>
    public sealed class ExtForceFileParser
    {
        private readonly Encoding utf8NoBom;
        private readonly List<string> blockComments;

        private ExtForceFileData extForceFileData;
        private ExtForceData currentForcing;

        private string currentLine;
        private int lineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtForceFileParser"/> class.
        /// </summary>
        public ExtForceFileParser()
        {
            utf8NoBom = new UTF8Encoding(false);
            blockComments = new List<string>();
        }

        /// <summary>
        /// Parses formatted text from the specified string to an external forcings data object.
        /// </summary>
        /// <param name="str">The formatted text to parse.</param>
        /// <returns>An <see cref="ExtForceFileData"/> object containing the parsed external forcings data.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="str"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">When the text has an invalid format.</exception>
        public ExtForceFileData Parse(string str)
        {
            Ensure.NotNull(str, nameof(str));

            using (var stringReader = new StringReader(str))
            {
                return Parse(stringReader);
            }
        }

        /// <summary>
        /// Parses formatted text from the specified stream to an external forcings data object.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> from which to read the formatted text.</param>
        /// <returns>An <see cref="ExtForceFileData"/> object containing the parsed external forcings data.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="stream"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">When the text has an invalid format.</exception>
        public ExtForceFileData Parse(Stream stream)
        {
            Ensure.NotNull(stream, nameof(stream));

            using (var streamReader = new StreamReader(stream, utf8NoBom, true, 1024, true))
            {
                return Parse(streamReader);
            }
        }

        /// <summary>
        /// Parses formatted text from the specified reader to an external forcings data object.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> from which to read the INI-formatted text.</param>
        /// <returns>An <see cref="ExtForceFileData"/> object containing the parsed external forcings data.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="reader"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">When the text has an invalid format.</exception>
        public ExtForceFileData Parse(TextReader reader)
        {
            Ensure.NotNull(reader, nameof(reader));

            InitializeParsingContext();

            while ((currentLine = reader.ReadLine()) != null)
            {
                lineNumber++;

                FixInvalidHeaderLine();
                CleanCurrentLine();
                ParseCurrentLine();
            }

            return extForceFileData;
        }

        private void InitializeParsingContext()
        {
            extForceFileData = new ExtForceFileData();
            currentForcing = null;
            blockComments.Clear();
            lineNumber = 0;
        }

        private void FixInvalidHeaderLine()
        {
            const string invalidHeaderLine = "              :";
            const string validHeaderLine = "*             :";

            if (currentLine.StartsWith(invalidHeaderLine))
            {
                currentLine = currentLine.Replace(invalidHeaderLine, validHeaderLine);
            }
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

            if (IsCommentBlockLine())
            {
                ParseCommentBlockLine();
            }
            else if (IsPropertyLine())
            {
                ParsePropertyLine();
            }
            else
            {
                throw new FormatException($"Error on line {lineNumber}: invalid formatted text.");
            }
        }

        private bool IsEmptyLine()
        {
            return string.IsNullOrEmpty(currentLine);
        }

        private bool IsCommentBlockLine()
        {
            return GetKnownCommentDelimiters().Any(currentLine.StartsWith);
        }

        private void ParseCommentBlockLine()
        {
            int commentIndex = GetKnownCommentDelimiters().Max(currentLine.IndexOf);
            string comment = currentLine.Substring(commentIndex + 1);

            blockComments.Add(comment);
        }

        private static IEnumerable<char> GetKnownCommentDelimiters()
        {
            return ExtForceFileConstants.Delimiters.InlineComments.Concat(new[] { ExtForceFileConstants.Delimiters.CommentBlock });
        }

        private bool IsPropertyLine()
        {
            return currentLine.Contains(ExtForceFileConstants.Delimiters.Assignment);
        }

        private void ParsePropertyLine()
        {
            int assignmentIndex = currentLine.IndexOf(ExtForceFileConstants.Delimiters.Assignment);
            string key = currentLine.Substring(0, assignmentIndex).Trim().ToUpper();
            ValidatePropertyKey(key);

            int commentIndex = ExtForceFileConstants.Delimiters.InlineComments.Max(currentLine.IndexOf);
            int valueStartIndex = assignmentIndex + 1;

            string value = commentIndex > assignmentIndex
                               ? currentLine.Substring(valueStartIndex, commentIndex - valueStartIndex).Trim()
                               : currentLine.Substring(valueStartIndex).Trim();

            if (IsNewForcing(key))
            {
                ValidatePropertyValue(value);
                AddNewForcing(key, value);
            }
            else
            {
                SetPropertyValue(key, value);
            }
        }

        private void ValidatePropertyKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new FormatException($"Error on line {lineNumber}: property key cannot be empty.");
            }
        }

        private void ValidatePropertyValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new FormatException($"Error on line {lineNumber}: property value cannot be empty.");
            }
        }

        private static bool IsNewForcing(string key)
        {
            return GetKnownQuantityKeys().Any(key.StartsWith);
        }

        private static IEnumerable<string> GetKnownQuantityKeys()
        {
            IEnumerable<string> supportedKeys = new[] { ExtForceFileConstants.Keys.Quantity, ExtForceFileConstants.Keys.DisabledQuantity };
            IEnumerable<string> unsupportedKeys = ExtForceFileConstants.Keys.UnsupportedQuantities;

            return supportedKeys.Concat(unsupportedKeys);
        }

        private void AddNewForcing(string key, string value)
        {
            currentForcing = new ExtForceData
            {
                LineNumber = lineNumber,
                Quantity = value,
                IsEnabled = key == ExtForceFileConstants.Keys.Quantity
            };

            blockComments.ForEach(c => currentForcing.AddComment(c));
            blockComments.Clear();

            extForceFileData.AddForcing(currentForcing);
        }

        private void SetPropertyValue(string key, string value)
        {
            if (currentForcing == null)
            {
                throw new FormatException(GetUnexpectedKeyMessage(key));
            }

            switch (key)
            {
                case ExtForceFileConstants.Keys.FileName:
                    SetFileName(value);
                    break;
                case ExtForceFileConstants.Keys.VariableName:
                    SetVarName(value);
                    break;
                case ExtForceFileConstants.Keys.FileType:
                    SetFileType(value);
                    break;
                case ExtForceFileConstants.Keys.Method:
                    SetMethod(value);
                    break;
                case ExtForceFileConstants.Keys.Operand:
                    SetOperand(value);
                    break;
                case ExtForceFileConstants.Keys.Value:
                    SetValue(value);
                    break;
                case ExtForceFileConstants.Keys.Factor:
                    SetFactor(value);
                    break;
                case ExtForceFileConstants.Keys.Offset:
                    SetOffset(value);
                    break;
                default:
                    AddModelData(key, value);
                    break;
            }
        }

        private void SetFileName(string value)
        {
            ValidatePropertyValue(value);

            currentForcing.FileName = value;
        }

        private void SetVarName(string value)
        {
            ValidatePropertyPosition(f => string.IsNullOrEmpty(currentForcing.FileName), ExtForceFileConstants.Keys.VariableName);

            currentForcing.VariableName = value;
        }

        private void SetFileType(string value)
        {
            ValidatePropertyValue(value);
            ValidatePropertyPosition(f => string.IsNullOrEmpty(currentForcing.FileName), ExtForceFileConstants.Keys.FileType);

            currentForcing.FileType = ConvertFromString<int>(value);
        }

        private void SetMethod(string value)
        {
            ValidatePropertyValue(value);
            ValidatePropertyPosition(f => f.FileType == null, ExtForceFileConstants.Keys.Method);

            currentForcing.Method = ConvertFromString<int>(value);

            // Backward compatibility: samples triangulation changed from 4 to 5 in #30984
            if (currentForcing.FileType == ExtForceFileConstants.FileTypes.Triangulation &&
                currentForcing.Method == ExtForceFileConstants.Methods.InsidePolygon)
            {
                currentForcing.Method = ExtForceFileConstants.Methods.Triangulation;
            }
        }

        private void SetOperand(string value)
        {
            ValidatePropertyValue(value);
            ValidatePropertyPosition(f => f.Method == null, ExtForceFileConstants.Keys.Operand);

            currentForcing.Operand = value;
        }

        private void SetValue(string value)
        {
            ValidatePropertyPosition(f => string.IsNullOrEmpty(f.Operand), ExtForceFileConstants.Keys.Value);

            currentForcing.Value = ConvertFromString<double>(value);
        }

        private void SetFactor(string value)
        {
            ValidatePropertyPosition(f => string.IsNullOrEmpty(f.Operand), ExtForceFileConstants.Keys.Factor);

            currentForcing.Factor = ConvertFromString<double>(value);
        }

        private void SetOffset(string value)
        {
            ValidatePropertyPosition(f => string.IsNullOrEmpty(f.Operand), ExtForceFileConstants.Keys.Offset);

            currentForcing.Offset = ConvertFromString<double>(value);
        }

        private void AddModelData(string key, string value)
        {
            ValidatePropertyPosition(f => string.IsNullOrEmpty(f.Operand), key);

            currentForcing.SetModelData(key, value);
        }

        private void ValidatePropertyPosition(Func<ExtForceData, bool> condition, string key)
        {
            if (condition(currentForcing))
            {
                throw new FormatException(GetUnexpectedKeyMessage(key));
            }
        }

        private T ConvertFromString<T>(string value) where T : IConvertible
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new FormatException(
                    $"Cannot convert value '{value}' to '{typeof(T).Name}'. Line: {lineNumber}.", e);
            }
        }

        private string GetUnexpectedKeyMessage(string key)
            => $"Unexpected property '{key}' encountered on line {lineNumber}.";
    }
}