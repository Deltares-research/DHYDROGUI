using Deltares.Infrastructure.IO.Ini;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Provides methods to convert between INI data and meteo data.
    /// </summary>
    internal static class BndExtForceMeteoDataConverter
    {
        /// <summary>
        /// Converts an INI section to meteo data.
        /// </summary>
        public static BndExtForceMeteoData ToMeteoData(this IniSection section)
        {
            return new BndExtForceMeteoData
            {
                LineNumber = section.LineNumber,
                Quantity = section.GetPropertyValue(BndExtForceFileConstants.Keys.Quantity),
                ForcingFile = section.GetPropertyValue(BndExtForceFileConstants.Keys.ForcingFile),
                ForcingFileType = section.GetPropertyValue<BndExtForceDataFileType>(BndExtForceFileConstants.Keys.ForcingFileType),
                TargetMaskFile = section.GetPropertyValue(BndExtForceFileConstants.Keys.TargetMaskFile),
                TargetMaskInvert = section.GetPropertyValue<bool>(BndExtForceFileConstants.Keys.TargetMaskInvert),
                InterpolationMethod = section.GetPropertyValue<BndExtForceInterpolationMethod>(BndExtForceFileConstants.Keys.InterpolationMethod),
                Operand = section.GetPropertyValue<BndExtForceOperand>(BndExtForceFileConstants.Keys.Operand)
            };
        }

        /// <summary>
        /// Converts meteo data to an INI section.
        /// </summary>
        public static IniSection ToIniSection(this BndExtForceMeteoData meteoData)
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Meteo);

            section.AddProperty(BndExtForceFileConstants.Keys.Quantity, meteoData.Quantity);
            section.AddProperty(BndExtForceFileConstants.Keys.ForcingFile, meteoData.ForcingFile);
            section.AddProperty(BndExtForceFileConstants.Keys.ForcingFileType, meteoData.ForcingFileType);

            if (!string.IsNullOrEmpty(meteoData.TargetMaskFile))
            {
                section.AddProperty(BndExtForceFileConstants.Keys.TargetMaskFile, meteoData.TargetMaskFile);
                section.AddProperty(BndExtForceFileConstants.Keys.TargetMaskInvert, meteoData.TargetMaskInvert ? "yes" : "no");
            }

            section.AddProperty(BndExtForceFileConstants.Keys.InterpolationMethod, meteoData.InterpolationMethod);
            section.AddProperty(BndExtForceFileConstants.Keys.Operand, meteoData.Operand);

            return section;
        }
    }
}