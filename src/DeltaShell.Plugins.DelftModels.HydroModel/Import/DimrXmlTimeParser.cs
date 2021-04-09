using System;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Parser for the model times in the dimr configuration file.
    /// </summary>
    public static class DimrXmlTimeParser
    {
        /// <summary>
        /// Parses the time string to a <see cref="ModelTimers"/> object.
        /// </summary>
        /// <param name="refTime"> The reference time. </param>
        /// <param name="timeStr"> The time string from the configuration file. </param>
        /// <param name="logHandler"> The log handler. </param>
        /// <param name="timers">
        /// When this method returns, the parsed timers if successful; otherwise <c>null</c>. This parameter
        /// is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the string was parsed successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// False is returned and an error is logged  when the specified <paramref name="timeStr"/>:
        /// - is <c>null</c> or empty;
        /// - does not contain exactly 3 values;
        /// - contains values that cannot be parsed to doubles.
        /// </remarks>
        public static bool TryParse(DateTime refTime, string timeStr, ILogHandler logHandler, out ModelTimers timers)
        {
            timers = null;

            if (string.IsNullOrWhiteSpace(timeStr))
            {
                logHandler?.ReportError(Resources.DimrXmlTimeParser_The_time_element_should_not_be_empty);
                return false;
            }
            
            string[] times = timeStr.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            if (times.Length != 3)
            {
                logHandler?.ReportError(Resources.DimrXmlTimeParser_The_time_element_should_contain_three_timers);
                return false;
            }

            // Execute all parsing operations in order to gather all errors collected by the TryParseSeconds,
            // thus we do not short-circuit our boolean operations.
            if (TryParseSeconds(times[0], logHandler, out double startSeconds) &
                TryParseSeconds(times[1], logHandler, out double timeStepSeconds) &
                TryParseSeconds(times[2], logHandler, out double stopSeconds))
            {
                timers = new ModelTimers(refTime.AddSeconds(startSeconds),
                                         TimeSpan.FromSeconds(timeStepSeconds),
                                         refTime.AddSeconds(stopSeconds));
            }

            return timers != null;
        }

        private static bool TryParseSeconds(string secStr, ILogHandler logHandler, out double seconds)
        {
            seconds = default(double);
            
            if (double.TryParse(secStr, out double parsedSeconds))
            {
                seconds = parsedSeconds;
                return true;
            }

            logHandler?.ReportError(string.Format(Resources.DimrXmlTimeParser_not_a_valid_number_of_seconds, secStr));
            return false;
        }
    }
}