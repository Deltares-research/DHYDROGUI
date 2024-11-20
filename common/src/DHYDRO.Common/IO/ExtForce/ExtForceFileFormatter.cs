using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Deltares.Infrastructure.API.Guards;

namespace DHYDRO.Common.IO.ExtForce
{
    /// <summary>
    /// Formats external forcings data to a formatted string.
    /// </summary>
    public sealed class ExtForceFileFormatter
    {
        private readonly Encoding utf8NoBom;
        private TextWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtForceFileFormatter"/> class.
        /// </summary>
        public ExtForceFileFormatter()
        {
            utf8NoBom = new UTF8Encoding(false);
            writer = TextWriter.Null;
        }

        /// <summary>
        /// Formats the specified external forcings data to a formatted string.
        /// </summary>
        /// <param name="extForceFileData">The <see cref="ExtForceFileData"/> to format.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="System.ArgumentNullException">When <paramref name="extForceFileData"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">When one of the mandatory forcing fields is <c>null</c> or empty.</exception>
        public string Format(ExtForceFileData extForceFileData)
        {
            Ensure.NotNull(extForceFileData, nameof(extForceFileData));

            using (var stringWriter = new StringWriter())
            {
                Format(extForceFileData, stringWriter);

                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Formats the specified external forcings data to a formatted string and writes it to the specified stream.
        /// </summary>
        /// <param name="extForceFileData">The <see cref="ExtForceFileData"/> to format.</param>
        /// <param name="stream">The <see cref="Stream"/> to write the formatted data to.</param>
        /// <exception cref="System.ArgumentNullException">
        /// When <paramref name="extForceFileData"/> or <paramref name="stream"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">When one of the mandatory forcing fields is <c>null</c> or empty.</exception>
        public void Format(ExtForceFileData extForceFileData, Stream stream)
        {
            Ensure.NotNull(extForceFileData, nameof(extForceFileData));
            Ensure.NotNull(stream, nameof(stream));

            using (var streamWriter = new StreamWriter(stream, utf8NoBom, 1024, true))
            {
                Format(extForceFileData, streamWriter);
            }
        }

        /// <summary>
        /// Formats the specified external forcings data to a formatted string and writes it to the specified writer.
        /// </summary>
        /// <param name="extForceFileData">The <see cref="ExtForceFileData"/> to format.</param>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write the formatted data to.</param>
        /// <exception cref="System.ArgumentNullException">
        /// When <paramref name="extForceFileData"/> or <paramref name="textWriter"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">When one of the mandatory forcing fields is <c>null</c> or empty.</exception>
        public void Format(ExtForceFileData extForceFileData, TextWriter textWriter)
        {
            Ensure.NotNull(extForceFileData, nameof(extForceFileData));
            Ensure.NotNull(textWriter, nameof(textWriter));

            writer = textWriter;

            foreach (ExtForceData forcing in extForceFileData.Forcings)
            {
                WriteForcing(forcing);
                WriteNewLine();
            }
        }

        private void WriteForcing(ExtForceData forcing)
        {
            ValidateMandatoryFields(forcing);
            
            if (forcing.Comments.Any())
            {
                WriteComments(forcing.Comments);
                WriteNewLine();
            }

            WriteProperty(forcing.IsEnabled ? ExtForceFileConstants.Keys.Quantity : ExtForceFileConstants.Keys.DisabledQuantity, forcing.Quantity);
            WriteProperty(ExtForceFileConstants.Keys.FileName, forcing.FileName);

            WriteOptionalProperty(ExtForceFileConstants.Keys.VariableName, forcing.VariableName, variable => !string.IsNullOrEmpty(variable));

            WriteProperty(ExtForceFileConstants.Keys.FileType, forcing.FileType);
            WriteProperty(ExtForceFileConstants.Keys.Method, forcing.Method);
            WriteProperty(ExtForceFileConstants.Keys.Operand, forcing.Operand);

            WriteOptionalProperty(ExtForceFileConstants.Keys.Value, forcing.Value, value => value != null && !double.IsNaN(value.Value));
            WriteOptionalProperty(ExtForceFileConstants.Keys.Factor, forcing.Factor, factor => factor != null && !double.IsNaN(factor.Value));
            WriteOptionalProperty(ExtForceFileConstants.Keys.Offset, forcing.Offset, offset => offset != null && !double.IsNaN(offset.Value));

            WriteModelData(forcing.ModelData);
        }
        
        private static void ValidateMandatoryFields(ExtForceData forcing)
        {
            if (string.IsNullOrEmpty(forcing.Quantity) ||
                string.IsNullOrEmpty(forcing.FileName) ||
                string.IsNullOrEmpty(forcing.Operand) ||
                forcing.Method == null ||
                forcing.FileType == null)
            {
                throw new InvalidOperationException("Mandatory fields must be set.");
            }
        }

        private void WriteComments(IEnumerable<string> comments)
        {
            foreach (string comment in comments)
            {
                WriteComment(comment);
            }
        }

        private void WriteComment(string comment)
        {
            writer.WriteLine($"{ExtForceFileConstants.Delimiters.CommentBlock}{comment}");
        }

        private void WriteModelData(IReadOnlyDictionary<string, string> modelData)
        {
            foreach (KeyValuePair<string, string> keyValuePair in modelData)
            {
                WriteProperty(keyValuePair.Key, keyValuePair.Value);
            }
        }

        private void WriteOptionalProperty<T>(string key, T value, Func<T, bool> condition)
        {
            if (condition(value))
            {
                WriteProperty(key, value);
            }
        }

        private void WriteProperty<T>(string key, T value)
        {
            string formattedValue = string.Format(CultureInfo.InvariantCulture, "{0}", value);
            
            writer.WriteLine($"{key}{ExtForceFileConstants.Delimiters.Assignment}{formattedValue}");
        }

        private void WriteNewLine()
        {
            writer.WriteLine();
        }
    }
}