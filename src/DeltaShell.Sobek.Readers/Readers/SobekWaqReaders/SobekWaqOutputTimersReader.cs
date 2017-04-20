using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqOutputTimersReader
    {
        # region Sobek212

        /// <summary>
        /// Read method for the Sobek 212 WAQ output timers file "DELWAQ2.INP", 
        /// which defines the output timers.
        /// </summary>
        /// <example>
        /// File "DELWAQ2.INP" has the following structure:
        /// 
        /// ; output control (see DELWAQ-manual)
        /// ; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss   dddhhmmss
        ///   1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for balance output
        ///   1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for map output
        ///   1997/01/01-01:00:00  1997/01/15-00:00:00   000020036 ;  start, stop and step for his output
        /// </example>
        /// <param name="filePath">The path to "DELWAQ2.INP"</param>
        /// <returns>An enumerable with three SobekWaqTimer objects (=> respectively a balance, map and his output timer)</returns>
        /// <exception cref="FormatException">Thrown when the text format of the file is invalid</exception>
        public static IEnumerable<SobekWaqTimer> ReadOutputTimersFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseOutputTimersFromSobek212(text);
        }

        private static IEnumerable<SobekWaqTimer> ParseOutputTimersFromSobek212(string outputTimersText)
        {
            var outputTimersTextLines = GetOutputTimersTextLines(outputTimersText);
            var balanceOutputTimer = GetOutputTimer(outputTimersTextLines.ElementAt(0), "balance");
            var mapOutputTimer = GetOutputTimer(outputTimersTextLines.ElementAt(1), "map");
            var hisOutputTimer = GetOutputTimer(outputTimersTextLines.ElementAt(2), "his");

            return new List<SobekWaqTimer> { balanceOutputTimer, mapOutputTimer, hisOutputTimer };
        }

        private static IEnumerable<string> GetOutputTimersTextLines(string outputTimersText)
        {
            var outputTimersTextLines = SobekWaqReaderHelper.GetTextLines(outputTimersText, ";");

            if (outputTimersTextLines.Count() != 3)
            {
                throw new FormatException("no valid data was found");
            }

            return outputTimersTextLines;
        }

        private static SobekWaqTimer GetOutputTimer(string outputTimersTextLine, string timerName)
        {
            var dateTimeRegex = new Regex(@"(\d{4})/(\d{2})/(\d{2})-(\d{2}):(\d{2}):(\d{2})", RegexOptions.Singleline);
            var dateTimeMatches = dateTimeRegex.Matches(outputTimersTextLine);

            if (dateTimeMatches.Count != 2)
            {
                throw new FormatException(string.Format("no start and/or stop time was found for the {0} output timer", timerName));
            }

            var timeStepRegex = new Regex(@"\d*");
            var timeStepMatch = timeStepRegex.Match(outputTimersTextLine.Replace(dateTimeMatches[0].Value, "").Replace(dateTimeMatches[1].Value, "").Replace(" ", ""));

            if (timeStepMatch.Length == 0)
            {
                throw new FormatException(string.Format("no time step was found for the {0} output timer", timerName));
            }

            return new SobekWaqTimer
                       {
                           StartTime = SobekWaqReaderHelper.ParseDateTime(dateTimeMatches[0].Value),
                           StopTime = SobekWaqReaderHelper.ParseDateTime(dateTimeMatches[1].Value),
                           TimeStep = SobekWaqReaderHelper.ParseTimeStep(timeStepMatch.Value, "DDDHHMMSS")
                       };
        }

        # endregion
    }
}
