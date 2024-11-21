using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Provides methods to convert between INI data and new style external forcings file data.
    /// </summary>
    public static class BndExtForceFileDataConverter
    {
        /// <summary>
        /// Converts INI data to external forcings file data.
        /// </summary>
        /// <param name="iniData">The INI data object to convert to external forcings file data.</param>
        /// <returns>A new <see cref="BndExtForceFileData"/> object containing the converted INI data.</returns>
        /// <exception cref="System.ArgumentNullException">When <paramref name="iniData"/> is <c>null</c>.</exception>
        public static BndExtForceFileData ToExtForceFileData(this IniData iniData)
        {
            Ensure.NotNull(iniData, nameof(iniData));

            var extForceFileData = new BndExtForceFileData();

            foreach (IniSection section in iniData.Sections)
            {
                if (section.IsNameEqualTo(BndExtForceFileConstants.Headers.General))
                {
                    extForceFileData.FileInfo = section.ToFileInfo();
                }
                else if (section.IsNameEqualTo(BndExtForceFileConstants.Headers.Boundary))
                {
                    extForceFileData.AddBoundaryForcing(section.ToBoundaryData());
                }
                else if (section.IsNameEqualTo(BndExtForceFileConstants.Headers.Lateral))
                {
                    extForceFileData.AddLateralForcing(section.ToLateralData());
                }
                else if (section.IsNameEqualTo(BndExtForceFileConstants.Headers.Meteo))
                {
                    extForceFileData.AddMeteoForcing(section.ToMeteoData());
                }
            }

            return extForceFileData;
        }

        /// <summary>
        /// Converts external forcings file data to INI data.
        /// </summary>
        /// <param name="extForceFileData">The external forcings file data to convert to INI data.</param>
        /// <returns>A new <see cref="IniData"/> object containing the converted external forcings file data.</returns>
        /// <exception cref="System.ArgumentNullException">When <paramref name="extForceFileData"/> is <c>null</c>.</exception>
        public static IniData ToIniData(this BndExtForceFileData extForceFileData)
        {
            Ensure.NotNull(extForceFileData, nameof(extForceFileData));

            var iniData = new IniData();
            iniData.AddSection(extForceFileData.FileInfo.ToIniSection());

            foreach (BndExtForceBoundaryData boundaryData in extForceFileData.BoundaryForcings)
            {
                iniData.AddSection(boundaryData.ToIniSection());
            }

            foreach (BndExtForceLateralData lateralData in extForceFileData.LateralForcings)
            {
                iniData.AddSection(lateralData.ToIniSection());
            }

            foreach (BndExtForceMeteoData meteoData in extForceFileData.MeteoForcings)
            {
                iniData.AddSection(meteoData.ToIniSection());
            }

            return iniData;
        }
    }
}