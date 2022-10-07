using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Sobek.Readers.Properties;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    /// <summary>
    /// Reader for <see cref="SobekRREvaporation"/> data. 
    /// </summary>
    public class SobekRREvaporationReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRREvaporationReader));

        /// <summary>
        /// Reads the <see cref="SobekRREvaporation"/> from the specified <paramref name="stream"/>
        /// </summary>
        /// <param name="stream"> The stream. </param>
        /// <returns>
        /// The <see cref="SobekRREvaporation"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided <paramref name="stream"/> does not support reading.
        /// </exception>
        public SobekRREvaporation Read(Stream stream)
        {
            Ensure.NotNull(stream, nameof(stream));

            if (!stream.CanRead)
            {
                throw new InvalidOperationException(string.Format(Resources.SobekRREvaporationReader_Read_The_current__0__does_not_support_reading_, nameof(stream)));
            }

            var evaporation = new SobekRREvaporation();

            var lineIndex = 0;
            foreach (string line in ReadData(stream))
            {
                lineIndex++;

                if (line == string.Empty || line.StartsWith("*"))
                {
                    continue;
                }

                ParseLine(line, lineIndex, evaporation);
            }

            return evaporation;
        }

        private static void ParseLine(string line, int lineIndex, SobekRREvaporation evaporation)
        {
            string[] parts = line.SplitOnEmptySpace();

            string yearStr = parts[0];
            string monthStr = parts[1];
            string dayStr = parts[2];
            string[] evaporationStr = parts.Skip(3).ToArray();

            if (!int.TryParse(yearStr, out int year) ||
                !int.TryParse(monthStr, out int month) ||
                !int.TryParse(dayStr, out int day))
            {
                log.Error(string.Format(Resources.SobekRREvaporationReader_ParseLine_Line__0___Not_all_date_values_are_valid_integers_, lineIndex));
                return;
            }

            if (!TryCreateDoubles(evaporationStr, out double[] evaporationValues))
            {
                log.Error(string.Format(Resources.SobekRREvaporationReader_ParseLine_Line__0___Not_all_evaporation_values_are_valid_floating_point_numbers_, lineIndex));
                return;
            }

            evaporation.Add(year, month, day, evaporationValues);
        }

        private static bool TryCreateDoubles(string[] doubleStrs, out double[] doubles)
        {
            int n = doubleStrs.Length;
            doubles = new double[n];

            for (var i = 0; i < n; i++)
            {
                if (!double.TryParse(doubleStrs[i], NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleVal))
                {
                    return false;
                }

                doubles[i] = doubleVal;
            }

            return true;
        }

        private static IEnumerable<string> ReadData(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    yield return line.Trim();
                }
            }
        }
    }
}
