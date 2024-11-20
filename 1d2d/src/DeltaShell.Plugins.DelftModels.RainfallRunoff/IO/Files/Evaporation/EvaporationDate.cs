using System;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation
{
    /// <summary>
    /// An evaporation date.
    /// This class is introduced to circumvent the problem that <see cref="DateTime"/> does not support 0 as a year.
    /// </summary>
    public readonly struct EvaporationDate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaporationDate"/> struct with <see cref="Year"/> set to 0.
        /// </summary>
        /// <param name="month"> The month. </param>
        /// <param name="day"> The day. </param>
        /// <remarks>
        /// No validation is performed on the input arguments and whether they form a valid date.
        /// </remarks>
        public EvaporationDate(int month, int day) : this(0, month, day) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaporationDate"/> struct.
        /// </summary>
        /// <param name="year"> The year. </param>
        /// <param name="month"> The month. </param>
        /// <param name="day"> The day. </param>
        /// <remarks>
        /// No validation is performed on the input arguments and whether they form a valid date.
        /// </remarks>
        internal EvaporationDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        /// <summary>
        /// The year.
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// The month.
        /// </summary>
        public int Month { get; }

        /// <summary>
        /// The day.
        /// </summary>
        public int Day { get; }
    }
}