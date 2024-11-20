using System;

namespace DeltaShell.NGHS.Utils
{
    public static class DoubleExtensions
    {
        private const int DefaultNumberOfDigits = 6;

        /// <summary>
        /// Truncate a double to a specified number of digits  
        /// </summary>
        /// <example>
        /// var myDoubleVal = 1.258;
        /// myDoubleVal.TruncateByDigits(2) results in 1.25 and not to 1.26 (which will be rounding the double)
        /// </example>
        /// <param name="number">Number to truncate.</param>
        /// <param name="digits">Number of digits to truncate the double to.</param>
        /// <returns>Truncated double</returns>
        public static double TruncateByDigits(this double number, int digits = DefaultNumberOfDigits)
        {
            var truncationNumber = Math.Pow(10, digits);
            return Math.Floor(number * truncationNumber) / truncationNumber;
        }
    }
}