using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation
{
    public class EvaporationDateComparer : IComparer<EvaporationDate>
    {
        /// <summary>
        /// Compares this instance to another evaporation data instance.
        /// </summary>
        /// <param name="x">The first evaporation date.</param>
        /// <param name="y"> The second evaporation date. </param>
        /// <returns>
        /// -1 if this instance is precedes the other instance;
        /// 0 if this instance is the same as the other instance;
        /// 1 if this instance is follows the other instance.
        /// </returns>
        public int Compare(EvaporationDate x, EvaporationDate y)
        {
            int yearComparison = x.Year.CompareTo(y.Year);
            if (yearComparison != 0)
            {
                return yearComparison;
            }

            int monthComparison = x.Month.CompareTo(y.Month);
            return monthComparison != 0
                       ? monthComparison
                       : x.Day.CompareTo(y.Day);
        }
    }
}