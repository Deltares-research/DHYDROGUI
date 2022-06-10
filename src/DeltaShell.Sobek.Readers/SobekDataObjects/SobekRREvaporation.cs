using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Sobek.Readers.Properties;
using log4net;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    /// <summary>
    /// The data access object for Sobek Rainfall-Runoff evaporation data.
    /// </summary>
    public class SobekRREvaporation
    {
        private const int aLeapYear = 4;
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRREvaporation));

        /// <summary>
        /// Whether or not the evaporation data is from a long time average.
        /// </summary>
        public bool IsLongTimeAverage { get; set; }

        /// <summary>
        /// Gets the number of locations.
        /// </summary>
        public int NumberOfLocations { get; private set; }

        /// <summary>
        /// Whether or not the data is periodic.
        /// </summary>
        public bool IsPeriodic => IsLongTimeAverage;

        /// <summary>
        /// The evaporation data per date, with each date containing an evaporation value per location.
        /// </summary>
        public IDictionary<DateTime, double[]> Data { get; private set;  } = new SortedDictionary<DateTime, double[]>();

        /// <summary>
        /// Gets all the dates in the evaporation data.
        /// </summary>
        public ICollection<DateTime> Dates => Data.Keys;
        
        /// <summary>
        /// Adds a new entry to the evaporation data with the provided date.
        /// </summary>
        /// <param name="year"> The date year. </param>
        /// <param name="month"> The date month. </param>
        /// <param name="day"> The date day. </param>
        /// <param name="values"> The evaporation values. </param>
        /// <remarks>
        /// If <see cref="IsLongTimeAverage"/> of this instance is <c>true</c>,
        /// the year will be replaced with a "dummy" leap year.
        /// Does not add an entry and logs an error if:
        /// - The provided year, month and day do not define a valid date.
        /// - The number of values is zero.
        /// - The number of values does not equal the number of values of the first successful entry.
        /// </remarks>
        public void Add(int year, int month, int day, double[] values)
        {
            Ensure.NotNull(values, nameof(values));

            year = IsLongTimeAverage ? aLeapYear : year;

            if (!TryCreateDate(year, month, day, out DateTime date))
            {
                log.Error($"{year}/{month}/{day} is not a valid date.");
                return;
            }

            if (!values.Any())
            {
                log.Error($"{ToString(date)} should have at least 1 evaporation value.");
                return;
            }

            if (!Data.Any())
            {
                NumberOfLocations = values.Length;
            }
            else if (values.Length != NumberOfLocations)
            {
                string valuesStr = NumberOfLocations == 1 ? "value" : "values";
                log.Error($"{ToString(date)} should have {NumberOfLocations} evaporation {valuesStr}.");
                return;
            }

            Data[date] = values;
        }

        /// <summary>
        /// Gets the evaporation values per date by station index.
        /// </summary>
        /// <param name="index"> The station index. </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="index"/> is negative or equal to or higher than <see cref="NumberOfLocations"/>.
        /// </exception>
        /// <returns> An array of evaporation values for the station at the specified index.</returns>
        public IEnumerable<double> GetValuesByStationIndex(int index)
        {
            Ensure.NotNegative(index, nameof(index));
            if (index >= NumberOfLocations)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    string.Format(Resources.Exception_IndexEqualToOrHigherThanNumberOfLocations, nameof(index), index, nameof(NumberOfLocations), NumberOfLocations));
            }

            return Data.Values.Select(row => row[index]).ToArray();
        }

        private static string ToString(DateTime dateTime) => dateTime.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

        private static bool TryCreateDate(int year, int month, int day, out DateTime date)
        {
            date = default(DateTime);
            try
            {
                date = new DateTime(year, month, day);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        /// <summary>
        /// Converts data to evaporation data based on the long time average data between the provided dates.
        /// of <paramref name="startTime"/> and <paramref name="stopTime"/>.
        /// </summary>
        /// <param name="startTime"> The start time of the time frame. </param>
        /// <param name="stopTime"> The stop time of the time frame. </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="IsLongTimeAverage"/> of this instance is <c>false</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the the date of the <paramref name="stopTime"/> precedes the date of the <paramref name="startTime"/>.
        /// </exception>
        public void ToLongTimeAverage(DateTime startTime, DateTime stopTime)
        {
            if (!IsLongTimeAverage)
            {
                throw new InvalidOperationException("The evaporation should be long time average.");
            }

            DateTime startDate = startTime.Date;
            DateTime stopDate = stopTime.Date;

            if (startDate > stopDate)
            {
                throw new ArgumentOutOfRangeException(nameof(stopTime), @"The stop date should not precede the start date.");
            }

            Data = GetDataBetweenDates(startDate, stopDate);
        }

        private IDictionary<DateTime, double[]> GetDataBetweenDates(DateTime startDate, DateTime stopDate)
        {
            var data = new Dictionary<DateTime, double[]>();
            
            for (DateTime date = startDate; date <= stopDate; date = date.AddDays(1))
            {
                var lookupDate = new DateTime(aLeapYear, date.Month, date.Day);
                data[date] = Data[lookupDate];
            }

            return data;
        }
    }
}