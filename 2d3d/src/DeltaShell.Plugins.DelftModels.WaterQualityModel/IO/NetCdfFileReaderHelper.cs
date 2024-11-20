using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Utils.NetCdf;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Helper class for reading WAQ NetCDF files.
    /// </summary>
    public static class NetCdfFileReaderHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NetCdfFileReaderHelper));

        /// <summary>
        /// Parses the values of the time variable to <see cref="IEnumerable{DateTime}"/>.
        /// </summary>
        /// <param name="file"> The <see cref="NetCdfFile"/> object that is used to read from the file. </param>
        /// <returns>The parsed <see cref="DateTime"/> objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> is <c>null</c>.</exception>
        public static IEnumerable<DateTime> GetDateTimes(NetCdfFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!TryGetVariableByStandardName(file, NetCdfConventions.StandardNames.Time, out NetCdfVariable timeVariable))
            {
                log.ErrorFormat(Resources.NetCdfFileReaderHelper_GetDateTimes_Time_variable_not_found, NetCdfConventions.StandardNames.Time, file.Path);
                return Enumerable.Empty<DateTime>();
            }

            DateTime referenceDate = ParseReferenceDate(file, timeVariable);

            IEnumerable<int> timeVariableValues = file.Read(timeVariable).Cast<int>();

            return GetTimes(timeVariableValues, referenceDate);
        }

        /// <summary>
        /// Performs an action with a <see cref="NetCdfFile"/>.
        /// </summary>
        /// <typeparam name="T"> Return type of <paramref name="netCdfFunction"/>. </typeparam>
        /// <param name="path"> The path of the NetCdf file. </param>
        /// <param name="netCdfFunction"> The function that performs an action with a <see cref="NetCdfFile"/>. </param>
        /// <returns> Returns the result of the <paramref name="netCdfFunction"/>. </returns>
        /// <exception cref="FileNotFoundException"> Thrown when <paramref name="path"/> does not exist. </exception>
        public static T DoWithNetCdfFile<T>(string path, Func<NetCdfFile, T> netCdfFunction)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            NetCdfFile netCdfFile = null;
            try
            {
                netCdfFile = NetCdfFile.OpenExisting(path);
                return netCdfFunction(netCdfFile);
            }
            finally
            {
                netCdfFile?.Close();
            }
        }

        private static DateTime ParseReferenceDate(NetCdfFile file, NetCdfVariable timeVariable)
        {
            const string dateTimeFormat = "yyyy-MM-dd hh:mm:ss";
            const string secondsSinceString = "seconds since ";

            string timeReferenceAttributeValue = file.GetAttributeValue(timeVariable, NetCdfConventions.Attributes.Units);

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

        /// <summary>
        /// Retrieves the NetCDF variable with the specified standard name.
        /// The standard name, enforced by the CF (Climate and Forecast) convention, is used to identify the physical quantity.
        /// </summary>
        /// <param name="file"> The NetCDF file. </param>
        /// <param name="standardName"> The standard name to search for. </param>
        /// <param name="retrievedVariable"> The retrieved NetCDF variable with the specified standard name. </param>
        /// <returns>
        /// <c>true</c> if the file contains a NetCDF variable with the specified standard name; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="file"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="standardName"/> is <c>null</c> or white space.
        /// </exception>
        public static bool TryGetVariableByStandardName(INetCdfFile file, string standardName, out NetCdfVariable retrievedVariable)
        {
            Ensure.NotNull(file, nameof(file));
            Ensure.NotNullOrWhiteSpace(standardName, nameof(standardName));

            foreach (NetCdfVariable variable in file.GetVariables())
            {
                if (file.GetAttributeValue(variable, NetCdfConventions.Attributes.StandardName) != standardName)
                {
                    continue;
                }

                retrievedVariable = variable;
                return true;
            }

            retrievedVariable = null;
            return false;
        }
    }
}