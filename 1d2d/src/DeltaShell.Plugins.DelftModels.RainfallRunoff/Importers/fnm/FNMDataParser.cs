using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers.fnm
{
    /// <summary>
    /// <see cref="FnmDataParser"/> is responsible for parsing a stream to a
    /// <see cref="FnmData"/> object.
    /// </summary>
    public static class FnmDataParser
    {
        /// <summary>
        /// Parse the provided <paramref name="stream"/> into a <see cref="FnmData"/> object.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed <see cref="FnmData"/> object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is <c>null</c>.
        /// </exception>
        public static FnmData Parse(StreamReader stream)
        {
            Ensure.NotNull(stream, nameof(stream));

            string[] data = stream.ReadLines()
                                  .Where(line => line.StartsWith("'"))
                                  .Select(ToValue)
                                  .ToArray();
            return new FnmData(data);
        }

        private static string ToValue(string line)
        {
            string trimmedLine = line.Trim();
            // The value is stored written as '...', as such we know that
            // the first is a ' after trimming, and we look at the second
            int index = trimmedLine.IndexOf("'", 1, StringComparison.InvariantCulture);
            return trimmedLine.Substring(1, index - 1);
        }

        private static IEnumerable<string> ReadLines(this StreamReader stream)
        {
            while (!stream.EndOfStream)
            {
                yield return stream.ReadLine();
            }
        }
    }
}