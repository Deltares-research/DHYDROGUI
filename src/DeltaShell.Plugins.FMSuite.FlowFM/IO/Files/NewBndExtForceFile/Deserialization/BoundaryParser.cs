using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization
{
    /// <summary>
    /// Parser for a boundary Delft INI category from the external forcing file (*_bnd.ext).
    /// </summary>
    public sealed class BoundaryParser
    {
        /// <summary>
        /// Parse the Delft INI category from the boundary external forcing file to a data access object.
        /// Delft INI categories with the header "boundary" can be parsed.
        /// If values from the Delft INI category are <c>null</c> or empty they will be set as <c>null</c> on the data access
        /// object.
        /// </summary>
        /// <param name="delftIniCategory"> The Delft INI category from the boundary external forcing file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="delftIniCategory"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// A <see cref="BoundaryDTO"/> data access object that contains the parsed data of the boundary category.
        /// </returns>
        public BoundaryDTO Parse(DelftIniCategory delftIniCategory)
        {
            Ensure.NotNull(delftIniCategory, nameof(delftIniCategory));

            string quantity = ParseQuantity(delftIniCategory);
            string locationFile = ParseLocationFile(delftIniCategory);
            IEnumerable<string> forcingFiles = ParseForcingFiles(delftIniCategory);
            double? returnTime = ParseReturnTime(delftIniCategory);

            return new BoundaryDTO(quantity, locationFile, forcingFiles, returnTime);
        }

        private static string ParseQuantity(DelftIniCategory delftIniCategory)
        {
            string quantity = delftIniCategory.GetPropertyValue(BndExtForceFileConstants.QuantityKey);
            return HasValue(quantity) ? quantity : null;
        }

        private static string ParseLocationFile(DelftIniCategory delftIniCategory)
        {
            string locationFile = delftIniCategory.GetPropertyValue(BndExtForceFileConstants.LocationFileKey);
            return HasValue(locationFile) ? locationFile : null;
        }

        private static IEnumerable<string> ParseForcingFiles(DelftIniCategory delftIniCategory)
        {
            return delftIniCategory.GetPropertyValues(BndExtForceFileConstants.ForcingFileKey)
                                   .Where(HasValue);
        }

        private static double? ParseReturnTime(DelftIniCategory delftIniCategory)
        {
            string returnTimeString = delftIniCategory.GetPropertyValue(BndExtForceFileConstants.ThatcherHarlemanTimeLagKey);
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