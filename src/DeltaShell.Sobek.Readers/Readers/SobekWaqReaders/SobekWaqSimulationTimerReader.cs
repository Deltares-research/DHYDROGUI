using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqSimulationTimerReader
    {
        # region Sobek212

        /// <summary>
        /// Read method for the Sobek 212 WAQ simulation timer file "DELWAQ1.INP", 
        /// which defines the simulation timer.
        /// </summary>
        /// <example>
        /// File "DELWAQ1.INP" has the following structure for simulation timer definitions:
        /// 
        ///    86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock
        ///                           15.70 ; integration option
        ///             1997/01/01-01:00:00 ; simulation starting time
        ///             1998/01/01-01:00:00 ; simulation end time
        ///                               0 ; timestep constant
        ///                       000010018 ; simulation timestep
        /// </example>
        /// <param name="filePath">The path to "DELWAQ1.INP"</param>
        /// <returns>A SobekWaqTimer object</returns>
        /// <exception cref="FormatException">Thrown when the text format of the file is invalid</exception>
        public static SobekWaqTimer ReadSimulationTimerFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseSimulationTimerFromSobek212(text);
        }

        private static SobekWaqTimer ParseSimulationTimerFromSobek212(string simulationTimerText)
        {
            var simulationTimerTextLines = GetSimulationTimerTextLines(simulationTimerText);
            var timeStepFormat = GetTimeStepFormat(simulationTimerTextLines.ElementAt(0));
            var startTime = GetDateTime(simulationTimerTextLines.ElementAt(2), "start time");
            var stopTime = GetDateTime(simulationTimerTextLines.ElementAt(3), "stop time");
            var timeStep = GetTimeStep(simulationTimerTextLines.ElementAt(5), timeStepFormat);

            return new SobekWaqTimer
                       {
                           StartTime = startTime, 
                           StopTime = stopTime, 
                           TimeStep = timeStep
                       };
        }

        private static IEnumerable<string> GetSimulationTimerTextLines(string simulationTimerText)
        {
            var simulationTimerTextLines = SobekWaqReaderHelper.GetTextLines(simulationTimerText);

            if (simulationTimerTextLines.Count() != 6)
            {
                throw new FormatException("no valid data was found");
            }

            return simulationTimerTextLines;
        }

        private static string GetTimeStepFormat(string timeStepFormatLine)
        {
            var timeStepValue = SobekWaqReaderHelper.GetTextBlock(timeStepFormatLine, "'", "'");

            if (timeStepValue.Length == 0)
            {
                throw new FormatException("no system clock format was found");
            }

            return timeStepValue.Replace("'", "");
        }

        private static DateTime GetDateTime(string dateTimeLine, string timeName)
        {
            var dateTimeRegex = new Regex(@"(\d{4})/(\d{2})/(\d{2})-(\d{2}):(\d{2}):(\d{2})", RegexOptions.Singleline);
            var dateTimeMatch = dateTimeRegex.Match(dateTimeLine);

            if (dateTimeMatch.Length == 0)
            {
                throw new FormatException(string.Format("no {0} was found", timeName));
            }

            return SobekWaqReaderHelper.ParseDateTime(dateTimeMatch.Value);
        }

        private static TimeSpan GetTimeStep(string timeStepLine, string timeStepFormat)
        {
            var timeStepRegex = new Regex(@"\d*");
            var timeStepMatch = timeStepRegex.Match(timeStepLine);

            if (timeStepMatch.Length == 0)
            {
                throw new FormatException("no time step was found");
            }

            return SobekWaqReaderHelper.ParseTimeStep(timeStepMatch.Value, timeStepFormat);
        }

        # endregion
    }
}
