using DelftTools.Hydro;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// Converts a water type string as found in GWSW files.
    /// to a valid <see cref="SewerConnectionWaterType"/>.
    /// </summary>
    public static class WaterTypeConverter
    {
        /// <summary>
        /// Converts a string to a <see cref="SewerConnectionWaterType"/>. />
        /// </summary>
        /// <param name="waterTypeString"></param>
        /// <returns>The corresponding <see cref="SewerConnectionWaterType"/>.</returns>
        public static SewerConnectionWaterType ConvertStringToSewerConnectionWaterType(string waterTypeString, ILogHandler logHandler)
        {
            switch (waterTypeString.ToLower())
            {
                case "gmd":
                case "combined":
                    return SewerConnectionWaterType.Combined;
                case "dwa":
                case "dry weather":
                    return SewerConnectionWaterType.DryWater;
                case "hwa":
                case "storm water":
                    return SewerConnectionWaterType.StormWater;
                case "nvt":
                case "none":
                    return SewerConnectionWaterType.None;
                default:
                    logHandler?.ReportWarningFormat(Properties.Resources.Water_type__0__is_not_a_valid_water_type_Setting_water_type_to_none, waterTypeString);
                    return SewerConnectionWaterType.None;
            }
        }
    }
}