using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    /// <summary>
    /// Reader for diagnostics files (*.dia).
    /// </summary>
    public static class DiaFileReader
    {
        public static string Read(string diaFilePath)
        {
            return File.ReadAllText(diaFilePath);
        }

        /// <summary>
        /// Collects and returns all error messages from a given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream"> The stream object </param>
        /// <returns> A collection of error messages. </returns>
        /// <remarks>
        /// 1. A line containing the text 'ERROR' is seen as an error message.
        /// 2. Error messages can be split up on multiple consecutive lines. They will be read as one error message.
        ///    An error message stops when the next line starts with the character '*'.
        /// </remarks>
        public static IEnumerable<string> GetAllErrorMessages(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line) || !line.Contains("ERROR"))
                    {
                        continue;
                    }

                    string errorLine = line;
                    while((line = reader.ReadLine()) != null && !line.StartsWith("*"))
                    {
                        errorLine += line;
                    }

                    yield return errorLine;
                }
            }
        }
    }
}