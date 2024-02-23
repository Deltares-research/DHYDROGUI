using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization
{
    /// <summary>
    /// Parser for an initial or parameter INI section from the initial field file.
    /// </summary>
    public static class InitialFieldParser
    {
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
        public static InitialField Parse(IniSection section)
        {
            Ensure.NotNull(section, nameof(section));
            EnsureValidSection(section);

            var initialField = new InitialField
            {
                Quantity = section.GetPropertyValue(InitialFieldFileConstants.Keys.Quantity, InitialFieldQuantity.None),
                DataFile = section.GetPropertyValue(InitialFieldFileConstants.Keys.DataFile),
                DataFileType = section.GetPropertyValue(InitialFieldFileConstants.Keys.DataFileType, InitialFieldDataFileType.None),
                InterpolationMethod = section.GetPropertyValue(InitialFieldFileConstants.Keys.InterpolationMethod, InitialFieldInterpolationMethod.None),
                Operand = section.GetPropertyValue(InitialFieldFileConstants.Keys.Operand, InitialFieldOperand.Override),
                AveragingType = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingType, InitialFieldAveragingType.Mean),
                AveragingRelSize = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingRelSize, 1.01),
                AveragingNumMin = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingNumMin, 1),
                AveragingPercentile = section.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingPercentile, 0.0),
                ExtrapolationMethod = section.GetPropertyValue(InitialFieldFileConstants.Keys.ExtrapolationMethod, false),
                LocationType = section.GetPropertyValue(InitialFieldFileConstants.Keys.LocationType, InitialFieldLocationType.All),
                Value = section.GetPropertyValue(InitialFieldFileConstants.Keys.Value, double.NaN)
            };

            return initialField;
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
    }
}