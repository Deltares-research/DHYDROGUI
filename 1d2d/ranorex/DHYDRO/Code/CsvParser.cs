using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DHYDRO.Code
{
    /// <summary>
    /// A CSV parser.
    /// </summary>
    public static class CsvParser
    {
        /// <summary>
        /// Parses a CSV file.
        /// </summary>
        /// <param name="filePath"> The full file path to the CSV file. </param>
        /// <param name="delimiter"> The delimiter on which to split the columns. </param>
        /// <returns> A collection of string (rows) which each a collection of strings (columns). </returns>
        public static IEnumerable<string[]> Parse(string filePath, char delimiter = ';')
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line.SplitOn(delimiter);
                }
            }
        }

        /// <summary>
        /// Splits the specified string on the delimiter and trims off the empty spaces.
        /// </summary>
        /// <param name="value"> The original string to be split. </param>
        /// <param name="delimiter"> THe delimiter on which to split the string. </param>
        /// <returns> An array of the split parts. </returns>
        private static string[] SplitOn(this string value, char delimiter)
        {
            return value.Split(delimiter).Select(v => v.Trim()).ToArray();
        }
    }
}