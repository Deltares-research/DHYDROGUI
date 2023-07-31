using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    /// <summary>
    /// Class that helps with parsing data from a <see cref="BcQuantityData"/> object.
    /// </summary>
    public static class BcQuantityDataParsingHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BcQuantityDataParsingHelper));
        
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
                DateTime startDate = TryParseDate(dateString, locationName);

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
        
        private static DateTime TryParseDate(string dateString, string supportPointName)
        {
            bool dateParsed = DateTime.TryParseExact(dateString, new[]
                                                     {
                                                         "yyyy-MM-dd",
                                                         "yyyy-MM-dd HH:mm:ss",
                                                         "yyyy-MM-dd HH:mm:ss zzz"
                                                     },
                                                     CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal,
                                                     out DateTime startDate);

            if (!dateParsed)
            {
                throw new FormatException("Time format '" + dateString + "' in support point '" + supportPointName + "' is not supported by bc file parser");
            }

            CheckForTimezoneOffset(dateString, supportPointName);

            return startDate;
        }
        
        private static DateTime ConvertToTime(string offsetValue, long offsetFactor, DateTime startDate)
        {
            // 1 Tick is 100 nanoseconds, as such there are 10 million ticks in a second.
            const long nTicksPerSecond = 10000000;
            long nTicks = nTicksPerSecond * offsetFactor * Convert.ToInt64(double.Parse(offsetValue));
            return startDate + new TimeSpan(nTicks);
        }
        
        private static void CheckForTimezoneOffset(string dateString, string supportPointName)
        {
            bool offsetParsed = DateTimeOffset.TryParseExact(dateString, "yyyy-MM-dd HH:mm:ss zzz",
                                                             CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
                                                             out DateTimeOffset dateTimeOffset);

            if (offsetParsed && dateTimeOffset.Offset.Ticks != 0)
            {
                Log.Warn(string.Format(Resources.BcFileFlowBoundaryDataBuilder_Support_point__0__contains_time_zone_offset, supportPointName));
            }
        }
        
        private static void LogWarningParsePropertyFailed(BcBlockData dataBlock, string propertyName,
                                                          string propertyValue)
        {
            Log.Warn(
                $"File {dataBlock.FilePath}, block starting at line {dataBlock.LineNumber}: {propertyName} {propertyValue} could not be parsed; omitting dataBlock block.");
        }
    }
}