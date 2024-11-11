using Deltares.Infrastructure.IO.Ini;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Provides methods to convert between INI data and external forcings file info.
    /// </summary>
    internal static class BndExtForceFileInfoConverter
    {
        /// <summary>
        /// Converts an INI section to external forcings file info.
        /// </summary>
        public static BndExtForceFileInfo ToFileInfo(this IniSection section)
        {
            return new BndExtForceFileInfo
            {
                FileVersion = section.GetPropertyValue(BndExtForceFileConstants.Keys.FileVersion),
                FileType = section.GetPropertyValue(BndExtForceFileConstants.Keys.FileType)
            };
        }

        /// <summary>
        /// Converts external forcings file info to an INI section.
        /// </summary>
        public static IniSection ToIniSection(this BndExtForceFileInfo fileInfo)
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.General);
            section.AddProperty(BndExtForceFileConstants.Keys.FileVersion, fileInfo.FileVersion);
            section.AddProperty(BndExtForceFileConstants.Keys.FileType, fileInfo.FileType);
            return section;
        }
    }
}