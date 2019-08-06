using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Contains helper methods to read netCDF files from D-Water Quality
    /// </summary>
    public static class NcFileReaderHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NcFileReaderHelper));

        /// <summary>
        /// Parses the values of the time variable to <see cref="IEnumerable{DateTime}" />.
        /// </summary>
        /// <param name="file"> The <see cref="NetCdfFile" /> object that is used to read from the file. </param>
        /// <param name="timeVariableName"> Name of the time variable in the file. </param>
        /// <returns> </returns>
        public static IEnumerable<DateTime> GetDateTimes(NetCdfFile file, string timeVariableName)
        {
            NetCdfVariable timeVariable = file.GetVariableByName(timeVariableName);

            DateTime dateTime = ParseReferenceDate(file, timeVariable);

            return GetTimes(file.Read(timeVariable).Cast<int>(), dateTime);
        }

        private static DateTime ParseReferenceDate(NetCdfFile file, NetCdfVariable timeVariable)
        {
            const string dateTimeFormat = "yyyy-MM-dd hh:mm:ss";
            const string timeReferenceAttributeName = "units";
            const string secondsSinceStr = "seconds since ";

            string timeReferenceAttributeValue = file.GetAttributeValue(timeVariable, timeReferenceAttributeName);

            DateTime referenceDate = DateTime.Today;
            if (!timeReferenceAttributeValue.StartsWith(secondsSinceStr, StringComparison.Ordinal))
            {
                return referenceDate;
            }

            string referenceDateString = timeReferenceAttributeValue
                                         .Substring(secondsSinceStr.Length)
                                         .Substring(0, dateTimeFormat.Length);

            if (!DateTime.TryParseExact(referenceDateString, dateTimeFormat, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out referenceDate))
            {
                log.Warn(string.Format(Resources.DelwaqNcMapFileReader_Reference_date_could_not_be_parsed,
                                       referenceDate.ToString(dateTimeFormat), file.Path));
            }

            return referenceDate;
        }

        private static IEnumerable<DateTime> GetTimes(IEnumerable<int> timeValues, DateTime referenceDate)
        {
            return timeValues.Select(v => v > 1E34
                                              ? DateTime.MaxValue
                                              : referenceDate.AddSeconds(v));
        }
    }
}