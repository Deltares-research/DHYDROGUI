using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public abstract class FMNetCdfFileFunctionStore : ReadOnlyNetCdfFunctionStoreBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FMNetCdfFileFunctionStore));
        public const string UserFriendlyCategoryNameAttribute = "userfriendly_category_name";
        private const double defaultNetCdfDouble = 9.9692099683868690e+36;
        private const string timeDimensionName = "time";
        private const string timeVariableName = "time";

        /// <remarks>Needed for NHibernate</remarks>
        protected FMNetCdfFileFunctionStore() {}

        protected FMNetCdfFileFunctionStore(string ncPath) : base(ncPath) {}

        public string Name { get; set; }

        protected override IList<string> TimeDimensionNames => new[]
        {
            timeDimensionName
        };

        protected override IList<string> TimeVariableNames => new[]
        {
            GetTimeVariableName(timeDimensionName)
        };

        protected override string GetTimeVariableName(string dimName)
        {
            return "time";
        }

        protected override void UpdateFunctionsAfterPathSet()
        {
            base.UpdateFunctionsAfterPathSet();

            Name = "Output (" + System.IO.Path.GetFileName(Path) + ")";
        }

        /// <summary>
        /// Read reference date and time from NetCDF file.
        /// </summary>
        /// <param name="timeVariableName">
        /// The name of the time variable.
        /// </param>
        /// <returns> The reference date and time in UTC.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when string in NetCDF file does not start
        /// with "seconds since".
        /// </exception>
        protected override string ReadReferenceDateFromFile(string timeVariableName)
        {
            NetCdfVariable timeVariable = netCdfFile.GetVariableByName(timeVariableName);
            string timeReference = netCdfFile.GetAttributeValue(timeVariable, "units") ?? string.Empty;

            const string secondsSince = "seconds since ";
            var dateTimeFormatWithZone = $"{dateTimeFormat} zzz";

            if (!timeReference.StartsWith(secondsSince))
            {
                throw new ArgumentException(Resources.FMNetCdfFileFunctionStore_Could_not_parse_time_reference);
            }

            timeReference = timeReference.Substring(secondsSince.Length);

            if (!DateTime.TryParseExact(timeReference,
                                        dateTimeFormatWithZone,
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.AdjustToUniversal,
                                        out DateTime dateTime))
            {
                return base.ReadReferenceDateFromFile(timeVariableName);
            }

            return dateTime.ToString(DateTimeFormatInfo.InvariantInfo.FullDateTimePattern, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Validates whether there are time values and whether they do not contain default values.
        /// </summary>
        /// <returns>
        /// <c>false</c> if:
        /// - the file does not contain a time variable;
        /// - the time variable is empty;
        /// - the time variable contain default (missing) values.
        /// Otherwise, <c>true</c>.
        /// </returns>
        /// <remarks>
        /// This method was added because there can be situations where the time variable is invalid.
        /// This happens for example when a user cancels a model run while it was executing.
        /// </remarks>
        protected bool ValidateTimes()
        {
            NetCdfVariable variable = netCdfFile.GetVariableByName(timeVariableName);
            if (variable == null)
            {
                log.WarnFormat(Resources.FMNetCdfFileFunctionStore_WarningTimeVariableMissing, Path);
                return false;
            }

            IEnumerable<double> times = netCdfFile.Read(variable).Cast<double>();
            if (!times.Any())
            {
                log.WarnFormat(Resources.FMNetCdfFileFunctionStore_WarningTimeVariableEmpty, Path);
                return false;
            }

            if (times.Contains(defaultNetCdfDouble))
            {
                log.WarnFormat(Resources.FMNetCdfFileFunctionStore_WarningTimeVariableMissingValues, Path);
                return false;
            }

            return true;
        }
    }
}