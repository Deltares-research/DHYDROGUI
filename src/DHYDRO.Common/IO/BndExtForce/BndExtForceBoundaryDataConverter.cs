using System.Linq;
using Deltares.Infrastructure.IO.Ini;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Provides methods to convert between INI data and boundary data.
    /// </summary>
    internal static class BndExtForceBoundaryDataConverter
    {
        /// <summary>
        /// Converts an INI section to boundary data.
        /// </summary>
        public static BndExtForceBoundaryData ToBoundaryData(this IniSection section)
        {
            return new BndExtForceBoundaryData
            {
                LineNumber = section.LineNumber,
                Quantity = section.GetPropertyValue(BndExtForceFileConstants.Keys.Quantity),
                NodeId = section.GetPropertyValue(BndExtForceFileConstants.Keys.NodeId),
                LocationFile = section.GetPropertyValue(BndExtForceFileConstants.Keys.LocationFile),
                ForcingFiles = section.GetAllPropertyValues(BndExtForceFileConstants.Keys.ForcingFile),
                ReturnTime = section.GetPropertyValue(BndExtForceFileConstants.Keys.ReturnTime, double.NaN),
                TracerFallVelocity = section.GetPropertyValue(BndExtForceFileConstants.Keys.TracerFallVelocity, double.NaN),
                TracerDecayTime = section.GetPropertyValue(BndExtForceFileConstants.Keys.TracerDecayTime, double.NaN),
                FlowLinkWidth = section.GetPropertyValue(BndExtForceFileConstants.Keys.FlowLinkWidth, double.NaN),
                BedLevelDepth = section.GetPropertyValue(BndExtForceFileConstants.Keys.BedLevelDepth, double.NaN)
            };
        }

        /// <summary>
        /// Converts boundary data to an INI section.
        /// </summary>
        public static IniSection ToIniSection(this BndExtForceBoundaryData boundaryData)
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Boundary);

            section.AddPropertyIf(BndExtForceFileConstants.Keys.Quantity, boundaryData.Quantity, value => !string.IsNullOrEmpty(value));
            section.AddPropertyIf(BndExtForceFileConstants.Keys.NodeId, boundaryData.NodeId, value => !string.IsNullOrEmpty(value));
            section.AddPropertyIf(BndExtForceFileConstants.Keys.LocationFile, boundaryData.LocationFile, value => !string.IsNullOrEmpty(value));
            section.AddMultipleProperties(BndExtForceFileConstants.Keys.ForcingFile, boundaryData.ForcingFiles ?? Enumerable.Empty<string>());
            section.AddPropertyIf(BndExtForceFileConstants.Keys.ReturnTime, boundaryData.ReturnTime, value => !double.IsNaN(value));
            section.AddPropertyIf(BndExtForceFileConstants.Keys.TracerFallVelocity, boundaryData.TracerFallVelocity, value => !double.IsNaN(value));
            section.AddPropertyIf(BndExtForceFileConstants.Keys.TracerDecayTime, boundaryData.TracerDecayTime, value => !double.IsNaN(value));
            section.AddPropertyIf(BndExtForceFileConstants.Keys.FlowLinkWidth, boundaryData.FlowLinkWidth, value => !double.IsNaN(value));
            section.AddPropertyIf(BndExtForceFileConstants.Keys.BedLevelDepth, boundaryData.BedLevelDepth, value => !double.IsNaN(value));

            return section;
        }
    }
}