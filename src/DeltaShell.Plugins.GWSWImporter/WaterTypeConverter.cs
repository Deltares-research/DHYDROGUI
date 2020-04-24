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
        /// <exception cref="InvalidOperationException">When unknown string is provided.</exception>
        /// <returns>The corresponding <see cref="SewerConnectionWaterType"/>.</returns>
        public static SewerConnectionWaterType ConvertStringToSewerConnectionWaterType(string waterTypeString)
        {
            switch (waterTypeString.ToLower())
            {
                case "gmd":
                    return SewerConnectionWaterType.Combined;
                case "dwa":
                    return SewerConnectionWaterType.DryWater;
                case "hwa":
                    return SewerConnectionWaterType.StormWater;
                case "nvt":
                    return SewerConnectionWaterType.None;
                default:
                    throw new InvalidOperationException($"{waterTypeString} is not a valid water type.");
            }
        }
    }
}