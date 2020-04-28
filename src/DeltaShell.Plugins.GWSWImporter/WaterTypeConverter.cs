using System;
using DelftTools.Hydro;

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
        public static SewerConnectionWaterType ConvertStringToSewerConnectionWaterType(string waterTypeString)
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
                default:
                    return SewerConnectionWaterType.None;
            }
        }
    }
}