using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.NetCdf;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Helper class for reading WAQ NetCDF files.
    /// </summary>
    public static class NetCdfFileReaderHelper
    {
        /// <summary>
        /// Parses the values of the time variable to <see cref="IEnumerable{DateTime}" />.
        /// </summary>
        /// <param name="file"> The <see cref="NetCdfFile" /> object that is used to read from the file. </param>
        /// <param name="timeVariableName"> Name of the time variable in the file. </param>
        /// <returns>The parsed <see cref="DateTime"/> objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> is <c>null</c>.</exception>
        public static IEnumerable<DateTime> GetDateTimes(NetCdfFile file, string timeVariableName)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (string.IsNullOrEmpty(timeVariableName))
            {
                throw new ArgumentException($"Argument '{nameof(timeVariableName)}' cannot be null or empty.");
            }

            NetCdfVariable timeVariable = file.GetVariableByName(timeVariableName);

            DateTime referenceDate = ParseReferenceDate(file, timeVariable);

            IEnumerable<int> timeVariableValues = file.Read(timeVariable).Cast<int>();

            return GetTimes(timeVariableValues, referenceDate);
        }

        private static DateTime ParseReferenceDate(NetCdfFile file, NetCdfVariable timeVariable)
        {
            const string dateTimeFormat = "yyyy-MM-dd hh:mm:ss";
            const string timeReferenceAttributeName = "units";
            const string secondsSinceString = "seconds since ";

            string timeReferenceAttributeValue = file.GetAttributeValue(timeVariable, timeReferenceAttributeName);

            string referenceDateString = timeReferenceAttributeValue
                                         .Substring(secondsSinceString.Length)
                                         .Substring(0, dateTimeFormat.Length);

            return DateTime.ParseExact(referenceDateString, dateTimeFormat, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None);
        }

        private static IEnumerable<DateTime> GetTimes(IEnumerable<int> timeValues, DateTime referenceDate)
        {
            return timeValues.Select(v => v > 1E34
                                              ? DateTime.MaxValue
                                              : referenceDate.AddSeconds(v));
        }
    }
}