using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils.Extensions;

using TimeUnits = DeltaShell.NGHS.IO.FileWriters.Boundary.BoundaryRegion.UnitStrings;

namespace DeltaShell.NGHS.IO.FileReaders
{
    /// <summary>
    /// <see cref="BcSectionParser"/> is a helper class for parsing strings from Bc files.
    /// </summary>
    public class BcSectionParser : IBcSectionParser
    {
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Creates a new <see cref="BcSectionParser"/>.
        /// </summary>
        /// <param name="logHandler">The log handler used in this parser. </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logHandler"/> is <c>null</c>.</exception>
        public BcSectionParser(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));

            this.logHandler = logHandler;
        }

        public void CompleteFunction<T>(IFunction function, IEnumerable<T> argumentValues, IEnumerable<double> values, string periodic)
        {
            Ensure.NotNull(function, nameof(function));
            Ensure.NotNull(argumentValues, nameof(argumentValues));
            Ensure.NotNull(values, nameof(values));

            function.Clear();
            IVariable argument = function.Arguments[0];

            bool isAutoSorted = argument.IsAutoSorted;
            argument.IsAutoSorted = false;
            argument.SetValues(argumentValues);
            function.SetValues(values);
            argument.IsAutoSorted = isAutoSorted;

            argument.ExtrapolationType = periodic.EqualsCaseInsensitive("true")
                                             ? ExtrapolationType.Periodic
                                             : ExtrapolationType.Linear;
        }

        public bool TryParseDateTimes(IEnumerable<string> values, string unitValue, int lineNumber, out IEnumerable<DateTime> dateTimes)
        {
            Ensure.NotNull(values, nameof(values));
            Ensure.NotNull(unitValue, nameof(unitValue));

            dateTimes = null;

            if (!TryParseDoubles(values, lineNumber, out IEnumerable<double> doubles) ||
                !TryGetReferenceTime(unitValue, out DateTime referenceTime, lineNumber))
            {
                return false;
            }

            if (unitValue.Contains(TimeUnits.TimeSeconds))
            {
                dateTimes = doubles.Select(referenceTime.AddSeconds);
                return true;
            }

            if (unitValue.Contains(TimeUnits.TimeMinutes))
            {
                dateTimes = doubles.Select(referenceTime.AddMinutes);
                return true;
            }

            if (unitValue.Contains(TimeUnits.TimeHours))
            {
                dateTimes = doubles.Select(referenceTime.AddHours);
                return true;
            }
            
            logHandler.ReportError(string.Format(Resources.BcSectionParser_TryParseDateTimes_Cannot_interpret___0____see_section_on_line__1__, unitValue, lineNumber));
            return false;
        }

        private bool TryGetReferenceTime(string unitValue, out DateTime referenceTime, int lineNumber)
        {
            const string since = "since";
            int sinceIndex = unitValue.IndexOf(since, StringComparison.InvariantCultureIgnoreCase);
            string dateTimeString = unitValue.Substring(sinceIndex + since.Length).Trim();

            bool canParse = DateTime.TryParseExact(dateTimeString,
                                                   TimeUnits.TimeFormat,
                                                   CultureInfo.InvariantCulture, DateTimeStyles.None, out referenceTime);
            if (!canParse)
            {
                logHandler.ReportError(string.Format(Resources.BcSectionParser_TryGetReferenceTime_Cannot_parse___0___to_a_date_time__see_section_on_line__1__, dateTimeString, lineNumber));
                referenceTime = DateTime.MinValue;
            }
            return canParse;
        }

        public bool TryParseDoubles(IEnumerable<string> stringValues, int lineNumber, out IEnumerable<double> doubles)
        {
            Ensure.NotNull(stringValues, nameof(stringValues));
            
            var doubleValues = new List<double>();

            foreach (string stringValue in stringValues)
            {
                if (!TryParseDouble(stringValue, lineNumber, out double doubleValue))
                {
                    doubles = null;
                    return false;
                }

                doubleValues.Add(doubleValue);
            }

            doubles = doubleValues;
            return true;
        }

        private bool TryParseDouble(string doubleString, int lineNumber, out double doubleVal)
        {
            const NumberStyles numberStyle = NumberStyles.AllowLeadingWhite |
                                             NumberStyles.AllowTrailingWhite |
                                             NumberStyles.AllowLeadingSign |
                                             NumberStyles.AllowDecimalPoint |
                                             NumberStyles.AllowThousands |
                                             NumberStyles.AllowExponent;

            bool canParse = double.TryParse(doubleString, numberStyle, CultureInfo.InvariantCulture, out doubleVal);
            if (!canParse)
            {
                logHandler.ReportError(string.Format(Resources.BcSectionParser_TryParseDouble_Cannot_parse___0___to_a_double__see_section_on_line__1__, doubleString, lineNumber));
            }
            return canParse;
        }

        public double CreateConstant(IList<IBcQuantityData> table, int lineNumber)
        {
            Ensure.NotNull(table, nameof(table));

            if (!TableHasValue(table))
            {
                logHandler.ReportError(string.Format(Resources.BcSectionParser_Table_on_line__0___does_not_contain_any_values, lineNumber));

                return 0;
            }
            
            return TryParseDouble(table[0].Values[0], lineNumber, out double value) ? value : 0;
        }

        private static bool TableHasValue(IList<IBcQuantityData> table)
        {
            return table.Any() && table[0]?.Values != null && table[0].Values.Any();
        }
    }
}