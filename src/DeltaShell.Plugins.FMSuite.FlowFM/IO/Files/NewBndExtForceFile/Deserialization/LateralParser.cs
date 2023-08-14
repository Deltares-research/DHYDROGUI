using System;
using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization
{
    /// <summary>
    /// Parser for a lateral Delft INI category from the external forcing file (*_bnd.ext).
    /// </summary>
    public sealed class LateralParser
    {
        private const StringComparison ignoreCase = StringComparison.InvariantCultureIgnoreCase;
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Initialize a new instance of the <see cref="LateralParser"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        public LateralParser(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Parse the Delft INI category from the boundary external forcing file to a data access object.
        /// Delft INI categories with the header "lateral" can be parsed.
        /// If values from the Delft INI category are <c>null</c> or empty they will be set as <c>null</c> on the data access
        /// object.
        /// </summary>
        /// <param name="delftIniCategory"> The Delft INI category from the boundary external forcing file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="delftIniCategory"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// A <see cref="LateralDTO"/> data access object that contains the parsed data of the lateral category.
        /// </returns>
        public LateralDTO Parse(DelftIniCategory delftIniCategory)
        {
            Ensure.NotNull(delftIniCategory, nameof(delftIniCategory));

            string id = ParseId(delftIniCategory);
            string name = ParseName(delftIniCategory);
            LateralForcingType type = ParseType(delftIniCategory);
            LateralLocationType locationType = ParseLocationType(delftIniCategory);
            int? numCoordinates = ParseNumCoordinates(delftIniCategory);
            IEnumerable<double> xCoordinates = ParseXCoordinates(delftIniCategory);
            IEnumerable<double> yCoordinates = ParseYCoordinates(delftIniCategory);
            Steerable discharge = ParseDischarge(delftIniCategory);

            return new LateralDTO(id, name, type, locationType, numCoordinates, xCoordinates, yCoordinates, discharge);
        }

        private Steerable ParseDischarge(DelftIniCategory delftIniCategory)
        {
            string discharge = Retrieve(delftIniCategory, BndExtForceFileConstants.DischargeKey);

            if (!HasValue(discharge))
            {
                return null;
            }

            if (IsScalar(discharge, out double doubleValue))
            {
                return new Steerable
                {
                    Mode = SteerableMode.ConstantValue,
                    ConstantValue = doubleValue
                };
            }

            if (IsRealTime(discharge))
            {
                return new Steerable { Mode = SteerableMode.External };
            }

            if (IsTimeSeriesFile(discharge))
            {
                return new Steerable
                {
                    Mode = SteerableMode.TimeSeries,
                    TimeSeriesFilename = discharge
                };
            }

            logHandler.ReportError(string.Format(Resources.Discharge_value_0_could_not_be_parsed_Line_1_, discharge, delftIniCategory.LineNumber));
            return null;
        }

        private static bool IsScalar(string discharge, out double doubleValue) =>
            discharge.TryParseToDouble(out doubleValue);

        private static bool IsRealTime(string discharge) =>
            discharge.EqualsCaseInsensitive(BndExtForceFileConstants.RealTimeValue);

        private static string ParseId(DelftIniCategory delftIniCategory)
        {
            string id = Retrieve(delftIniCategory, BndExtForceFileConstants.IdKey);
            return HasValue(id) ? id : null;
        }

        private static string ParseName(DelftIniCategory delftIniCategory)
        {
            string name = Retrieve(delftIniCategory, BndExtForceFileConstants.NameKey);
            return HasValue(name) ? name : null;
        }

        private static LateralForcingType ParseType(DelftIniCategory delftIniCategory)
        {
            string typeStr = Retrieve(delftIniCategory, BndExtForceFileConstants.TypeKey);
            if (!HasValue(typeStr))
            {
                return LateralForcingType.None;
            }

            if (EnumConversion.TryGetFromDescription(typeStr, out LateralForcingType type))
            {
                return type;
            }

            return LateralForcingType.Unsupported;
        }

        private static LateralLocationType ParseLocationType(DelftIniCategory delftIniCategory)
        {
            string locationTypeStr = Retrieve(delftIniCategory, BndExtForceFileConstants.LocationTypeKey);
            if (!HasValue(locationTypeStr))
            {
                return LateralLocationType.None;
            }

            if (EnumConversion.TryGetFromDescription(locationTypeStr, out LateralLocationType locationType))
            {
                return locationType;
            }

            return LateralLocationType.Unsupported;
        }

        private static int? ParseNumCoordinates(DelftIniCategory delftIniCategory)
        {
            string numCoordinatesStr = Retrieve(delftIniCategory, BndExtForceFileConstants.NumCoordinatesKey);
            if (int.TryParse(numCoordinatesStr, out int numCoordinates))
            {
                return numCoordinates;
            }

            return null;
        }

        private IEnumerable<double> ParseXCoordinates(DelftIniCategory delftIniCategory) =>
            ParseDoubles(delftIniCategory, BndExtForceFileConstants.XCoordinatesKey);

        private IEnumerable<double> ParseYCoordinates(DelftIniCategory delftIniCategory) =>
            ParseDoubles(delftIniCategory, BndExtForceFileConstants.YCoordinatesKey);

        private IEnumerable<double> ParseDoubles(DelftIniCategory delftIniCategory, string propertyKey)
        {
            string strValues = Retrieve(delftIniCategory, propertyKey);
            if (!HasValue(strValues))
            {
                return null;
            }

            var doubles = new List<double>();

            foreach (string strValue in strValues.SplitOnEmptySpace())
            {
                if (!strValue.TryParseToDouble(out double doubleValue))
                {
                    logHandler.ReportError(string.Format(Resources._0_could_not_be_parsed_to_a_double_for_property_1_Line_2_, strValue, propertyKey, delftIniCategory.LineNumber));
                    continue;
                }

                doubles.Add(doubleValue);
            }

            return doubles;
        }

        private static bool IsTimeSeriesFile(string value) =>
            value.EndsWith(".bc", ignoreCase);

        private static bool HasValue(string value) => !string.IsNullOrWhiteSpace(value);

        private static string Retrieve(DelftIniCategory category, string property) =>
            category.GetPropertyValue(property, comparisonType: ignoreCase);
    }
}