using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    /// <summary>
    /// Class that helps with parsing data from a <see cref="BcQuantityData"/> object.
    /// </summary>
    public static class BcQuantityDataParsingHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BcQuantityDataParsingHelper));

        private const string dateFormat = "yyyy-MM-dd";
        private const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const string dateTimeTimeZoneFormat = "yyyy-MM-dd HH:mm:ss zzz";
        
        private static readonly string[] dateTimeFormats = { dateFormat, dateTimeFormat, dateTimeTimeZoneFormat };

        /// <summary>
        /// Parses the values of a time quantity/unit to a collection of <see cref="DateTime"/>.
        /// </summary>
        /// <param name="locationName"> The data location name. </param>
        /// <param name="quantityData"> The quantity data. </param>
        /// <returns>
        /// A collection of parsed <see cref="DateTime"/>.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown when the reference date, provided by the unit, cannot be parsed.
        /// </exception>
        public static IEnumerable<DateTime> ParseDateTimes(string locationName, BcQuantityData quantityData)
        {
            IEnumerable<string> stringValues = quantityData.Values;
            string format = quantityData.Unit;
            if (string.IsNullOrEmpty(format) || format == "-")
            {
                return stringValues.Select(s => DateTime.ParseExact(s, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            }

            List<string> splitFormat = format.Split().ToList();
            if (splitFormat[1] == "since")
            {
                string dateString = string.Join(" ", splitFormat.Skip(2));
                DateTime startDate = TryParseDate(dateString, locationName).DateTime;

                switch (splitFormat[0].ToLower())
                {
                    case "seconds":
                    {
                        return stringValues.Select(s => ConvertToTime(s, 1L, startDate));
                    }
                    case "minutes":
                    {
                        return stringValues.Select(s => ConvertToTime(s, 60L, startDate));
                    }
                    case "hours":
                    {
                        return stringValues.Select(s => ConvertToTime(s, 60L * 60L, startDate));
                    }
                    case "days":
                    {
                        return stringValues.Select(s => ConvertToTime(s, 60L * 60L * 24L, startDate));
                    }
                }
            }

            return Enumerable.Empty<DateTime>();
        }

        /// <summary>
        /// Parses the values of a time quantity/unit to a timezone as <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="timeFormat">Time format from which a timezone is parsed.</param>
        /// <param name="locationName">The data location name.</param>
        /// <returns>
        /// Timezone as <see cref="TimeSpan"/>, if an incorrect time format is given, <see cref="TimeSpan.Zero"/> is
        /// returned.
        /// </returns>
        /// <exception cref="FormatException">Thrown when the time format is correct but the date time in this format is incorrect.</exception>
        /// <example>An example of <paramref name="timeFormat"/> is <c>"seconds since 2000-01-01 00:00:00 +01:00"</c>.</example>
        public static TimeSpan ParseTimeZone(string timeFormat, string locationName)
        {
            if (IncorrectTimeUnitFormat(timeFormat))
            {
                return TimeSpan.Zero;
            }

            List<string> splitTimeFormat = timeFormat.Split().ToList();
            string dateString = string.Join(" ", splitTimeFormat.Skip(2));
            TimeSpan timeZone = TryParseDate(dateString, locationName).Offset;

            return timeZone;
        }

        /// <summary>
        /// Gets the time quantity unit string based on the provided reference date time and time zone.
        /// </summary>
        /// <param name="referenceTime"><see cref="DateTime"/> used for the unit.</param>
        /// <param name="timeZone"><see cref="TimeSpan"/> which is added as timezone.</param>
        /// <returns>Expected DateTime Unit.</returns>
        public static string GetDateTimeUnit(DateTime referenceTime, TimeSpan timeZone)
        {
            if (timeZone == TimeSpan.Zero)
            {
                return "seconds since " + referenceTime.ToString(dateTimeFormat);
            }

            var timeWithTimeZone = new DateTimeOffset(referenceTime, timeZone);
            return "seconds since " + timeWithTimeZone.ToString(dateTimeTimeZoneFormat);
        }

        public static InterpolationType ParseTimeInterpolationType(BcBlockData dataBlock)
        {
            if (string.IsNullOrEmpty(dataBlock.TimeInterpolationType) ||
                dataBlock.TimeInterpolationType.ToLower() == "linear")
            {
                return InterpolationType.Linear;
            }

            if (dataBlock.TimeInterpolationType.ToLower().Contains("block"))
            {
                return InterpolationType.Constant;
            }

            LogWarningParsePropertyFailed(dataBlock, "time interpolation type",
                                          dataBlock.TimeInterpolationType);
            throw new NotSupportedException($"Not able to map {dataBlock.TimeInterpolationType} to any valid type.");
        }

        private static bool IncorrectTimeUnitFormat(string format)
        {
            return string.IsNullOrEmpty(format) || format == "-" || !format.Contains("since");
        }

        private static DateTimeOffset TryParseDate(string dateString, string supportPointName)
        {

            bool dateParsed = DateTimeOffset.TryParseExact(dateString, dateTimeFormats,
                                                          CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
                                                          out DateTimeOffset dateTimeOffset);
            
            if (!dateParsed)
            {
                throw new FormatException("Time format '" + dateString + "' in support point '" + supportPointName + "' is not supported by bc file parser");
            }
            
            return dateTimeOffset;
        }

        private static DateTime ConvertToTime(string offsetValue, long offsetFactor, DateTime startDate)
        {
            // 1 Tick is 100 nanoseconds, as such there are 10 million ticks in a second.
            const long nTicksPerSecond = 10000000;
            long nTicks = nTicksPerSecond * offsetFactor * Convert.ToInt64(double.Parse(offsetValue));
            return startDate + new TimeSpan(nTicks);
        }

        private static void LogWarningParsePropertyFailed(BcBlockData dataBlock, string propertyName,
                                                          string propertyValue)
        {
            Log.Warn(
                $"File {dataBlock.FilePath}, block starting at line {dataBlock.LineNumber}: {propertyName} {propertyValue} could not be parsed; omitting dataBlock block.");
        }
    }
}