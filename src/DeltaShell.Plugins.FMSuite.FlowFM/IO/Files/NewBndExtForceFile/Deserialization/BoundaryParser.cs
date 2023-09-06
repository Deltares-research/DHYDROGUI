using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization
{
    /// <summary>
    /// Parser for a boundary INI section from the external forcing file (*_bnd.ext).
    /// </summary>
    public sealed class BoundaryParser
    {
        /// <summary>
        /// Parse the INI section from the boundary external forcing file to a data access object.
        /// INI sections with the header "boundary" can be parsed.
        /// If values from the INI section are <c>null</c> or empty they will be set as <c>null</c> on the data access object.
        /// </summary>
        /// <param name="section"> The INI section from the boundary external forcing file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="section"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// A <see cref="BoundaryDTO"/> data access object that contains the parsed data of the boundary section.
        /// </returns>
        public BoundaryDTO Parse(IniSection section)
        {
            Ensure.NotNull(section, nameof(section));

            string quantity = ParseQuantity(section);
            string locationFile = ParseLocationFile(section);
            IEnumerable<string> forcingFiles = ParseForcingFiles(section);
            double? returnTime = ParseReturnTime(section);

            return new BoundaryDTO(quantity, locationFile, forcingFiles, returnTime);
        }

        private static string ParseQuantity(IniSection section)
        {
            string quantity = section.GetPropertyValueOrDefault(BndExtForceFileConstants.QuantityKey);
            return HasValue(quantity) ? quantity : null;
        }

        private static string ParseLocationFile(IniSection section)
        {
            string locationFile = section.GetPropertyValueOrDefault(BndExtForceFileConstants.LocationFileKey);
            return HasValue(locationFile) ? locationFile : null;
        }

        private static IEnumerable<string> ParseForcingFiles(IniSection section)
        {
            return section.GetAllProperties(BndExtForceFileConstants.ForcingFileKey)
                          .Select(p => p.Value)
                          .Where(HasValue);
        }

        private static double? ParseReturnTime(IniSection section)
        {
            string returnTimeString = section.GetPropertyValueOrDefault(BndExtForceFileConstants.ThatcherHarlemanTimeLagKey);
            double? returnTime = ParseReturnTime(returnTimeString);
            return returnTime;
        }

        private static double? ParseReturnTime(string returnTime)
        {
            if (returnTime.TryParseToDouble(out double convertedReturnTime))
            {
                return convertedReturnTime;
            }

            return null;
        }

        private static bool HasValue(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}