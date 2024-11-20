using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.FileWriters
{
    /// <summary>
    /// Writer for the <see cref="IEvaporationFile"/>.
    /// </summary>
    public sealed class EvaporationFileWriter
    {
        private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Writes the provided evaporation file.
        /// </summary>
        /// <param name="evaporationFile"> The evaporation file. </param>
        /// <param name="textWriter"> The text writer. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="evaporationFile"/> or <paramref name="textWriter"/> is <c>null</c>.
        /// </exception>
        public void Write(IEvaporationFile evaporationFile, TextWriter textWriter)
        {
            Ensure.NotNull(evaporationFile, nameof(evaporationFile));
            Ensure.NotNull(textWriter, nameof(textWriter));

            foreach (string line in GetEvaporationFileLines(evaporationFile))
            {
                textWriter.WriteLine(line);
            }
        }

        private static IEnumerable<string> GetEvaporationFileLines(IEvaporationFile evaporationFile)
        {
            IList<string> lines = GetHeaderLines(evaporationFile);

            foreach (KeyValuePair<EvaporationDate, double[]> evaporation in evaporationFile.Evaporation)
            {
                string line = GetEvaporationLine(evaporation.Key, evaporation.Value);

                lines.Add(line);
            }

            return lines;
        }

        private static string GetEvaporationLine(EvaporationDate evaporationDate, IEnumerable<double> evaporationValues)
        {
            var year = evaporationDate.Year.ToString("0000");
            var month = evaporationDate.Month.ToString("00");
            var day = evaporationDate.Day.ToString("00");

            string values = string.Join(" ", evaporationValues.Select(FormatDouble));

            return $"{year} {month} {day} {values}";
        }

        private static IList<string> GetHeaderLines(IEvaporationFile evaporationFile)
        {
            return evaporationFile.Header.Select(CommentOut).ToList();
        }

        private static string CommentOut(string l)
        {
            return "*" + l;
        }

        private static string FormatDouble(double value)
        {
            return value.ToString(culture);
        }
    }
}