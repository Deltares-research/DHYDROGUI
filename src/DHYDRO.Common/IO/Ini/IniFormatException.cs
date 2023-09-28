using System;

namespace DHYDRO.Common.IO.Ini
{
    /// <summary>
    /// Represents an exception that is thrown when the INI text is not well formed.
    /// </summary>
    public sealed class IniFormatException : FormatException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IniFormatException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="line">The line of text where the error occurred.</param>
        /// <param name="lineNumber">The line number where the error occurred.</param>
        public IniFormatException(string message, string line, int lineNumber)
            : base(message)
        {
            Line = line;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Gets the line of text where the error occurred.
        /// </summary>
        public string Line { get; }

        /// <summary>
        /// Gets the line number where the error occurred.
        /// </summary>
        public int LineNumber { get; }
    }
}