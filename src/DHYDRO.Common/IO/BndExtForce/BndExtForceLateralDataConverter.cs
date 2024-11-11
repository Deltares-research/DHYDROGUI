using System.Globalization;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Provides methods to convert between INI data and lateral data.
    /// </summary>
    internal static class BndExtForceLateralDataConverter
    {
        /// <summary>
        /// Converts an INI section to lateral data.
        /// </summary>
        public static BndExtForceLateralData ToLateralData(this IniSection section)
        {
            return new BndExtForceLateralData
            {
                LineNumber = section.LineNumber,
                Id = section.GetPropertyValue(BndExtForceFileConstants.Keys.Id),
                Name = section.GetPropertyValue(BndExtForceFileConstants.Keys.Name),
                LocationType = section.GetPropertyValue(BndExtForceFileConstants.Keys.LocationType, BndExtForceLocationType.All),
                NodeId = section.GetPropertyValue(BndExtForceFileConstants.Keys.NodeId),
                BranchId = section.GetPropertyValue(BndExtForceFileConstants.Keys.BranchId),
                Chainage = section.GetPropertyValue<double>(BndExtForceFileConstants.Keys.Chainage),
                NumCoordinates = section.GetPropertyValue<int>(BndExtForceFileConstants.Keys.NumCoordinates),
                XCoordinates = section.GetMultiValuePropertyValues<double>(BndExtForceFileConstants.Keys.XCoordinates),
                YCoordinates = section.GetMultiValuePropertyValues<double>(BndExtForceFileConstants.Keys.YCoordinates),
                LocationFile = section.GetPropertyValue(BndExtForceFileConstants.Keys.LocationFile),
                Discharge = section.FindProperty(BndExtForceFileConstants.Keys.Discharge)?.ToDischargeData()
            };
        }

        /// <summary>
        /// Converts lateral data to an INI section.
        /// </summary>
        public static IniSection ToIniSection(this BndExtForceLateralData lateralData)
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Lateral);

            section.AddProperty(BndExtForceFileConstants.Keys.Id, lateralData.Id);
            section.AddProperty(BndExtForceFileConstants.Keys.Name, lateralData.Name);
            section.AddPropertyIf(BndExtForceFileConstants.Keys.LocationType, lateralData.LocationType, _ => lateralData.NumCoordinates > 0);
            section.AddPropertyIf(BndExtForceFileConstants.Keys.NodeId, lateralData.NodeId, value => !string.IsNullOrEmpty(value));

            if (lateralData.Chainage > 0)
            {
                section.AddProperty(BndExtForceFileConstants.Keys.BranchId, lateralData.BranchId);
                section.AddProperty(BndExtForceFileConstants.Keys.Chainage, lateralData.Chainage);
            }

            if (lateralData.NumCoordinates > 0)
            {
                section.AddProperty(BndExtForceFileConstants.Keys.NumCoordinates, lateralData.NumCoordinates);
                section.AddMultiValueProperty(BndExtForceFileConstants.Keys.XCoordinates, lateralData.XCoordinates.Select(ToCoordinateString));
                section.AddMultiValueProperty(BndExtForceFileConstants.Keys.YCoordinates, lateralData.YCoordinates.Select(ToCoordinateString));
            }

            section.AddPropertyIf(BndExtForceFileConstants.Keys.LocationFile, lateralData.LocationFile, value => !string.IsNullOrEmpty(value));
            section.AddProperty(BndExtForceFileConstants.Keys.Discharge, lateralData.Discharge?.ToIniProperty()?.Value);

            return section;
        }

        private static string ToCoordinateString(double coordinate)
            => coordinate.ToString("F2", CultureInfo.InvariantCulture);
    }
}