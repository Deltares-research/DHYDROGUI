using System;
using Deltares.Infrastructure.Extensions;
using Deltares.Infrastructure.IO.Ini;
using Deltares.Infrastructure.IO.Ini.Converters;
using DHYDRO.Common.Properties;
using log4net;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Provides methods to convert between INI data and discharge data.
    /// </summary>
    internal static class BndExtForceDischargeDataConverter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BndExtForceDischargeDataConverter));

        /// <summary>
        /// Converts an INI property to discharge data.
        /// </summary>
        public static BndExtForceDischargeData ToDischargeData(this IniProperty property)
        {
            if (!property.HasValue())
            {
                return null;
            }

            if (IsRealTime(property))
            {
                return new BndExtForceDischargeData { DischargeType = BndExtForceDischargeType.External };
            }

            if (IsTimeSeriesFile(property))
            {
                return new BndExtForceDischargeData
                {
                    DischargeType = BndExtForceDischargeType.TimeVarying,
                    TimeSeriesFile = property.Value
                };
            }

            if (IsScalar(property, out double scalarValue))
            {
                return new BndExtForceDischargeData
                {
                    DischargeType = BndExtForceDischargeType.TimeConstant,
                    ScalarValue = scalarValue
                };
            }

            log.Error(string.Format(Resources.Unsupported_discharge_value_0_Line_1_, property.Value, property.LineNumber));
            return null;
        }

        private static bool IsScalar(IniProperty property, out double scalarValue)
            => property.TryGetConvertedValue(out scalarValue);

        private static bool IsRealTime(IniProperty property)
            => property.Value.EqualsCaseInsensitive(BndExtForceFileConstants.RealTimeValue);

        private static bool IsTimeSeriesFile(IniProperty property)
            => property.Value.EndsWith(".bc", StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Converts discharge data to an INI property.
        /// </summary>
        public static IniProperty ToIniProperty(this BndExtForceDischargeData dischargeData)
        {
            return new IniProperty(BndExtForceFileConstants.Keys.Discharge, GetDischargeValue(dischargeData));
        }

        private static string GetDischargeValue(BndExtForceDischargeData dischargeData)
        {
            switch (dischargeData.DischargeType)
            {
                case BndExtForceDischargeType.TimeConstant:
                    return IniValueConverter.ConvertToString(dischargeData.ScalarValue);
                case BndExtForceDischargeType.TimeVarying:
                    return dischargeData.TimeSeriesFile;
                case BndExtForceDischargeType.External:
                    return BndExtForceFileConstants.RealTimeValue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dischargeData), dischargeData.DischargeType, null);
            }
        }
    }
}