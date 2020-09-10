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

        protected override void UpdateFunctionsAfterPathSet()
        {
            base.UpdateFunctionsAfterPathSet();

            Name = "Output (" + System.IO.Path.GetFileName(Path) + ")";
        }

        protected override string ReadReferenceDateFromFile(string timeVariableName)
        {
            NetCdfVariable timeVariable = netCdfFile.GetVariableByName(timeVariableName);
            string timeReference = netCdfFile.GetAttributeValue(timeVariable, "units");

            const string secondsSince = "seconds since ";
            var dateTimeFormatWithZone = $"{dateTimeFormat} zzz";

            if (!timeReference.StartsWith(secondsSince))
            {
                throw new ArgumentException("Could_not_parse_time_reference");
            }

            timeReference = timeReference.Substring(secondsSince.Length);

            if (!DateTime.TryParseExact(timeReference,
                                        dateTimeFormatWithZone,
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out DateTime dateTime))
            {
                return base.ReadReferenceDateFromFile(timeVariableName);
            }

            TimeSpan timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
            dateTime = dateTime.Subtract(timeZoneOffset);

            return dateTime.ToString(DateTimeFormatInfo.InvariantInfo.FullDateTimePattern, CultureInfo.InvariantCulture);
        }
    }
}