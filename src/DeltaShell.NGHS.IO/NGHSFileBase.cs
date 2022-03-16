using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;

namespace DeltaShell.NGHS.IO
{
    public abstract class NGHSFileBase
    {
        private const char MergeCharacter = '\'';
        protected readonly Dictionary<string, List<string>> commentBlocks;

        private readonly List<List<string>> headingCommentBlocks;
        private readonly List<string> commentLineStartsToBeIgnored;
        protected StreamReader reader;
        protected StreamWriter writer;
        protected List<string> currentCommentBlock;
        protected string storedNextInputLine;
        protected string storedNextOutputLine;
        private bool fileContentHasStarted;
        private CultureInfo storedCurrentCulture;

        /// <summary>
        /// Constructor, not external forcings file by default.
        /// </summary>
        protected NGHSFileBase()
        {
            headingCommentBlocks = new List<List<string>>();
            commentBlocks = new Dictionary<string, List<string>>();
            commentLineStartsToBeIgnored = new List<string>();
        }

        /// <summary>
        /// Retrieves the file path to another specified file that
        /// resides in the same folder as another.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="absolutePath"/> or <paramref name="relativePath"/> contains one or
        /// more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="absolutePath"/> or <paramref name="relativePath"/> is null.</exception>
        public static string GetOtherFilePathInSameDirectory(string absolutePath, string relativePath)
        {
            if (FileUtils.PathIsRelative(relativePath))
            {
                return Path.Combine(Path.GetDirectoryName(absolutePath), relativePath);
            }

            return relativePath;
        }

        protected int LineNumber { get; set; }

        protected string InputFilePath { get; set; }

        protected string OutputFilePath { get; private set; }

        protected string CurrentLine { get; private set; }

        protected virtual bool ExcludeEqualsIdentifier => true;

        /// <summary>
        /// Opens a FM suite related file.
        /// </summary>
        /// <param name="filePath">Path to the file being referenced.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is an empty string ("").</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="IOException">
        /// <paramref name="filePath"/> includes an incorrect or invalid syntax for file name,
        /// directory name, or volume label.
        /// </exception>
        [Obsolete("2019-10-22: Please use the OpenInputFile(Stream stream).")]
        protected void OpenInputFile(string filePath)
        {
            InputFilePath = filePath;
            reader = new StreamReader(filePath);
            fileContentHasStarted = false;
            LineNumber = 0;
        }

        protected void OpenInputFile(Stream stream)
        {
            reader = new StreamReader(stream);
            fileContentHasStarted = false;
            LineNumber = 0;
        }

        protected void CloseInputFile()
        {
            reader.Close();
        }

        /// <summary>
        /// Opens the file writer to a given destination.
        /// </summary>
        /// <param name="filePath">File path to write to.</param>
        /// <exception cref="UnauthorizedAccessException">Access is denied</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="filePath"/> is an empty string ("") or contains the name of a
        /// system device (com1, com2, and so on).
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260
        /// characters.
        /// </exception>
        /// <exception cref="IOException">
        /// path includes an incorrect or invalid syntax for file name, directory name, or volume
        /// label syntax.
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        protected void OpenOutputFile(string filePath)
        {
            storedCurrentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            OutputFilePath = filePath;
            string directory = Path.GetDirectoryName(OutputFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            writer = new StreamWriter(OutputFilePath);
            fileContentHasStarted = false;
            LineNumber = 0;
        }

        protected void CloseOutputFile()
        {
            Thread.CurrentThread.CurrentCulture = storedCurrentCulture;
            writer.Close();
        }

        /// <summary>
        /// Reads the file up to the next data line, processing all comments and empty lines along the way.
        /// </summary>
        /// <returns>The next data line, or null when at the end of the file.</returns>
        /// <remarks>
        /// This method does not necessarily return the actual next line in the file.
        /// <see cref="LineNumber"/> is available to keep track of the actual line number.
        /// </remarks>
        /// <exception cref="InvalidOperationException">When file hasn't been opened yet.</exception>
        /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        protected string GetNextLine()
        {
            if (reader == null)
            {
                throw new InvalidOperationException("Input file not opened for reading: " + (InputFilePath ?? "(no file)"));
            }

            if (storedNextInputLine != null)
            {
                string nextLine = storedNextInputLine;
                storedNextInputLine = null;
                return nextLine;
            }

            LineNumber++;
            CurrentLine = reader.ReadLine();

            while (CurrentLine != null)
            {
                if (TryGetLine(out string nextLine1))
                {
                    return nextLine1;
                }
            }

            return null;
        }

        private bool TryGetLine(out string line)
        {
            string trimmedLine = CurrentLine.Trim();

            if (CheckAndProcessCommentInputLine(CurrentLine, trimmedLine))
            {
                CurrentLine = GetNextLine();
            }
            else if (CheckAndProcessEmptyInputLine(CurrentLine, trimmedLine))
            {
                CurrentLine = GetNextLine();
            }
            else
            {
                string nextLine = trimmedLine.Replace('\t', ' '); // avoid having to parse or split on tabs
                if (currentCommentBlock != null)
                {
                    if (!fileContentHasStarted)
                    {
                        headingCommentBlocks.Add(currentCommentBlock);
                        currentCommentBlock = null;
                    }
                    else
                    {
                        CreateCommonBlock();
                    }

                    currentCommentBlock = null;
                }

                fileContentHasStarted = true;
                line = nextLine;
                return true;
            }

            line = null;
            return false;
        }

        protected virtual void CreateCommonBlock()
        {
            string contentIdentifier = CreateContentIdentifier(CurrentLine);
            if (!commentBlocks.ContainsKey(contentIdentifier))
            {
                commentBlocks.Add(contentIdentifier, currentCommentBlock);
            }
        }

        protected static IEnumerable<string> SplitLine(string inputLine)
        {
            Ensure.NotNull(inputLine, nameof(inputLine));

            string trimmedInputLine = inputLine.Trim();

            int endOfFieldIndex = GetEndOfFieldIndex(trimmedInputLine);

            while (endOfFieldIndex > 0)
            {
                yield return trimmedInputLine.Substring(0, endOfFieldIndex).Trim(MergeCharacter);

                trimmedInputLine = trimmedInputLine.Substring(endOfFieldIndex).TrimStart();

                endOfFieldIndex = GetEndOfFieldIndex(trimmedInputLine);
            }
        }

        /// <summary>
        /// Parses a string for a double in <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <param name="lineField">String representation of the number.</param>
        /// <param name="errorMessageKey">Optional: Additional description on value.</param>
        /// <returns>The value.</returns>
        /// <exception cref="FormatException">When <paramref name="lineField"/> does not represent a double.</exception>
        protected double GetDouble(string lineField, string errorMessageKey = null)
        {
            double x;
            if (!double.TryParse(lineField, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x))
            {
                throw new FormatException(string.Format("Invalid {0} line {1} in file {2}",
                                                        errorMessageKey != null ? errorMessageKey + " on " : "", LineNumber, InputFilePath));
            }

            return x;
        }

        /// <summary>
        /// Parses a string for an int in <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <param name="lineField">String representation of the number.</param>
        /// <param name="errorMessageKey">Optional: Additional description on value.</param>
        /// <returns>The value.</returns>
        /// <exception cref="FormatException">When <paramref name="lineField"/> does not represent an integer.</exception>
        protected int GetInt(string lineField, string errorMessageKey = null)
        {
            int xAsInt;
            if (int.TryParse(lineField, out xAsInt))
            {
                return xAsInt;
            }

            double xAsDouble;
            if (double.TryParse(lineField, NumberStyles.Any, CultureInfo.InvariantCulture, out xAsDouble))
            {
                double xAsDoubleFloored = Math.Floor(xAsDouble);
                if (xAsDouble - xAsDoubleFloored < 1e-12)
                {
                    // valid int, (accidentally) written as double
                    return (int)xAsDoubleFloored;
                }
            }

            throw new FormatException(string.Format("Invalid {0} line {1} in file {2}",
                                                    errorMessageKey != null ? errorMessageKey + " on " : "", LineNumber, InputFilePath));
        }

        /// <summary>
        /// Write a line of text to file.
        /// </summary>
        /// <param name="line">Line of text to be written (should not be null).</param>
        /// <exception cref="InvalidOperationException">When calling this before <see cref="OpenOutputFile"/> has been called.</exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        protected void WriteLine(string line)
        {
            if (writer == null)
            {
                throw new InvalidOperationException("Output file not opened for writing: " + (OutputFilePath ?? "(no file)"));
            }

            LineNumber++;
            if (CheckAndProcessOutputCommentLines(line))
            {
                writer.WriteLine(line);
            }
        }

        protected virtual bool WriteCommentBlock(string line, bool doWriteLine)
        {
            string contentIdentifier = CreateContentIdentifier(line);
            if (commentBlocks.ContainsKey(contentIdentifier))
            {
                foreach (string commentLine in commentBlocks[contentIdentifier])
                {
                    writer.WriteLine(commentLine);
                }
            }

            return doWriteLine;
        }

        protected virtual string CreateContentIdentifier(string line)
        {
            if (line == null)
            {
                return string.Empty;
            }

            var i = 0;
            var contentIdentifier = new char[line.Length];
            foreach (char c in line)
            {
                if (c == ' ' || c == '\t')
                {
                    continue;
                }

                if (ExcludeEqualsIdentifier && c == '=')
                {
                    break;
                }

                if (c == '#' || c == '!' || c == '*')
                {
                    break;
                }

                contentIdentifier[i++] = c;
            }

            return new string(contentIdentifier, 0, i);
        }

        private static int GetEndOfFieldIndex(string line)
        {
            if (line.Length == 0)
            {
                return 0;
            }

            if (line[0] == MergeCharacter)
            {
                for (var i = 1; i < line.Length; ++i)
                {
                    if (line[i] == MergeCharacter)
                    {
                        return i + 1;
                    }
                }

                return line.Length;
            }

            for (var i = 0; i < line.Length; ++i)
            {
                if (char.IsWhiteSpace(line, i))
                {
                    return i;
                }
            }

            return line.Length;
        }

        private bool CheckAndProcessCommentInputLine(string line, string trimmedLine)
        {
            if (trimmedLine.Length == 0)
            {
                return false;
            }

            char firstChar = trimmedLine[0];
            if (firstChar == '#' ||
                firstChar == '!' ||
                firstChar == '*' ||
                firstChar == ':') // part of header in external forcings file
            {
                if (commentLineStartsToBeIgnored.Any(trimmedLine.StartsWith))
                {
                    return true; // comment line indeed, but not to be stored
                }

                if (currentCommentBlock == null)
                {
                    currentCommentBlock = new List<string>();
                }

                currentCommentBlock.Add(line);
                return true;
            }

            return false;
        }

        private bool CheckAndProcessEmptyInputLine(string line, string trimmedLine)
        {
            if (trimmedLine.Length == 0)
            {
                if (currentCommentBlock != null)
                {
                    if (!fileContentHasStarted)
                    {
                        headingCommentBlocks.Add(currentCommentBlock);
                        currentCommentBlock = null;
                    }
                    else
                    {
                        currentCommentBlock.Add(line);
                    }
                }

                return true;
            }

            return false;
        }

        private bool CheckAndProcessOutputCommentLines(string line)
        {
            var doWriteLine = true;
            if (!fileContentHasStarted)
            {
                foreach (List<string> headingCommentBlock in headingCommentBlocks)
                {
                    foreach (string commentLine in headingCommentBlock)
                    {
                        writer.WriteLine(commentLine);
                    }

                    if (!string.IsNullOrEmpty(line))
                    {
                        writer.WriteLine("*");
                    }
                }

                fileContentHasStarted = true;
            }
            else
            {
                doWriteLine = WriteCommentBlock(line, doWriteLine);
            }

            return doWriteLine;
        }
    }
}