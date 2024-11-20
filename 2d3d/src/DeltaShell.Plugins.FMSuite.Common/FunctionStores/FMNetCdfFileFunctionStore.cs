using System;
using System.Collections.Generic;
using System.Globalization;
using DelftTools.Functions;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.FunctionStores
{
    public abstract class FMNetCdfFileFunctionStore : ReadOnlyNetCdfFunctionStoreBase, IFMNetCdfFileFunctionStore
    {
        private const string timeDimensionName = "time";

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
            string timeReference = netCdfFile.GetAttributeValue(timeVariable, "units");

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
    }
}