using System;
using System.Collections.Generic;
using System.Globalization;
using DelftTools.Functions;
using DelftTools.Utils;
using DelftTools.Utils.NetCdf;

namespace DeltaShell.Plugins.FMSuite.Common.FunctionStores
{
    public abstract class FMNetCdfFileFunctionStore : ReadOnlyNetCdfFunctionStoreBase, INameable
    {
        private const string TimeDimensionName = "time";

        //nhib
        protected FMNetCdfFileFunctionStore() {}

        protected FMNetCdfFileFunctionStore(string ncPath) : base(ncPath) {}

        public string Name { get; set; }

        protected override IList<string> TimeDimensionNames => new[]
        {
            TimeDimensionName
        };

        protected override IList<string> TimeVariableNames => new[]
        {
            GetTimeVariableName(TimeDimensionName)
        };

        protected override string GetTimeVariableName(string dimName)
        {
            return "time";
        }
        protected override string ReadReferenceDateFromFile(string timeVariableName)
        {
            NetCdfVariable timeVariable = netCdfFile.GetVariableByName(timeVariableName);
            string timeReference = netCdfFile.GetAttributeValue(timeVariable, "units");

            const string secondsSinceStr = "seconds since ";

            var dateTime = new DateTime(1970, 1, 1); // assume epoch otherwise
            if (timeReference.StartsWith(secondsSinceStr))
            {
                string timeStr = timeReference.Substring(secondsSinceStr.Length);

                if (!DateTime.TryParseExact(timeStr, $"{dateTimeFormat} zzz",
                                            CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                    return base.ReadReferenceDateFromFile(timeVariableName); // for backward compatibility, i have no time for a better solution
            }

            var timeZoneOffSet = TimeZoneInfo.Local.GetUtcOffset(dateTime);
            dateTime = dateTime.Subtract(timeZoneOffSet);
            return dateTime.ToString(DateTimeFormatInfo.InvariantInfo.FullDateTimePattern, CultureInfo.InvariantCulture);
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
            string timeReference = netCdfFile.GetAttributeValue(timeVariable, "units");

            const string secondsSince = "seconds since ";
            var dateTimeFormatWithZone = $"{dateTimeFormat} zzz";

            if (!timeReference.StartsWith(secondsSince))
            {
                throw new ArgumentException("Could not parse time reference");
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
    }
}