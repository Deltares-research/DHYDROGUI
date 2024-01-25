using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.EnumOperations;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization
{
    /// <summary>
    /// Parser for an initial or parameter INI section from the initial field file.
    /// </summary>
    public sealed class InitialFieldParser
    {
        private readonly EnumParser enumParser;
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Initialize a new instance of the <see cref="InitialFieldParser"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        public InitialFieldParser(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));

            this.logHandler = logHandler;
            enumParser = new EnumParser();
        }

        /// <summary>
        /// Parse the INI section from the initial field file file to a <see cref="InitialField"/> data access object.
        /// INI sections with the header "initial" or "parameter" can be parsed.
        /// If values from the INI section are <c>null</c> or empty they will be set with a default value on the data access
        /// object.
        /// For required properties, this default value is not a valid value.
        /// </summary>
        /// <param name="section"> The INI section from the initial field file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="section"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="section"/> does not have header "Parameter" or "Initial".
        /// </exception>
        /// <returns>
        /// A <see cref="InitialField"/> data access object that contains the parsed data of the INI section.
        /// </returns>
        public InitialField Parse(IniSection section)
        {
            Ensure.NotNull(section, nameof(section));
            EnsureValidSection(section);

            var initialField = new InitialField
            {
                Quantity = ParseEnum(section, InitialFieldFileConstants.Keys.Quantity, InitialFieldQuantity.None),
                DataFile = section.GetPropertyValue(InitialFieldFileConstants.Keys.DataFile),
                DataFileType = ParseEnum(section, InitialFieldFileConstants.Keys.DataFileType, InitialFieldDataFileType.None),
                InterpolationMethod = ParseEnum(section, InitialFieldFileConstants.Keys.InterpolationMethod, InitialFieldInterpolationMethod.None),
                Operand = ParseEnum(section, InitialFieldFileConstants.Keys.Operand, InitialFieldOperand.Override),
                AveragingType = ParseEnum(section, InitialFieldFileConstants.Keys.AveragingType, InitialFieldAveragingType.Mean),
                AveragingRelSize = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingRelSize, 1.01),
                AveragingNumMin = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingNumMin, 1),
                AveragingPercentile = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingPercentile, 0.0),
                ExtrapolationMethod = ParseBool(section, InitialFieldFileConstants.Keys.ExtrapolationMethod, false),
                LocationType = ParseEnum(section, InitialFieldFileConstants.Keys.LocationType, InitialFieldLocationType.All),
                Value = section.GetPropertyValue(InitialFieldFileConstants.Keys.Value, double.NaN)
            };

            return initialField;
        }

        private TEnum ParseEnum<TEnum>(IniSection section, string key, TEnum defaultValue) where TEnum : Enum
        {
            string propertyValue = section.GetPropertyValue(key);
            if (enumParser.TryParseByDescription(propertyValue, defaultValue, out TEnum result))
            {
                return result;
            }

            ReportError(string.Format(Resources._0_could_not_be_parsed_to_a_valid_1_Valid_values_2_,
                                      propertyValue, key, EnumFormatter.GetFormattedDescriptions<TEnum>()), section.LineNumber);

            return defaultValue;
        }

        private bool ParseBool(IniSection section, string key, bool defaultValue)
        {
            string propertyValue = section.GetPropertyValue(key);
            if (TryGetConvertedBool(propertyValue, defaultValue, out bool convertedValue))
            {
                return convertedValue;
            }

            ReportError(string.Format(Resources._0_could_not_be_parsed_to_a_boolean_for_property_1_,
                                      propertyValue, key), section.LineNumber);

            return defaultValue;
        }

        private static bool TryGetConvertedBool(string value, bool defaultValue, out bool convertedValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                convertedValue = defaultValue;
                return true;
            }

            if (value.EqualsCaseInsensitive("yes"))
            {
                convertedValue = true;
                return true;
            }

            if (value.EqualsCaseInsensitive("no"))
            {
                convertedValue = false;
                return true;
            }

            convertedValue = false;
            return false;
        }

        private static void EnsureValidSection(IniSection section)
        {
            if (!CanParseIniSection(section))
            {
                throw new ArgumentException($"Cannot parse {nameof(section)}: header [Parameter] or [Initial] required.");
            }
        }

        private static bool CanParseIniSection(IniSection section)
        {
            return section.IsNameEqualTo(InitialFieldFileConstants.Headers.Initial) ||
                   section.IsNameEqualTo(InitialFieldFileConstants.Headers.Parameter);
        }

        private void ReportError(string message, int lineNumber)
        {
            logHandler.ReportError(string.Format(Resources._0_Line_1_, message, lineNumber));
        }
    }
}