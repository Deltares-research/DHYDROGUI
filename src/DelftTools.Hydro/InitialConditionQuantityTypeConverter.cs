using System;

namespace DelftTools.Hydro
{
    
    /// <summary>
    /// Converts a string to <see cref="InitialConditionQuantity"/>.
    /// </summary>
    public static class InitialConditionQuantityTypeConverter
    {
        /// <summary>
        /// Converts a string to a <see cref="InitialConditionQuantity"/>.
        /// </summary>
        /// <param name="initialConditionQuantityString"></param>
        /// <returns>A <see cref="InitialConditionQuantity"/></returns>
        /// <exception cref="InvalidOperationException">When an invalid string is provided.</exception>
        public static InitialConditionQuantity ConvertStringToInitialConditionQuantity(
            string initialConditionQuantityString)
        {
            switch (initialConditionQuantityString.ToLower())
            {
                case "waterlevel":
                    return InitialConditionQuantity.WaterLevel;
                case "waterdepth":
                    return InitialConditionQuantity.WaterDepth;
                default:
                    throw new InvalidOperationException($"{initialConditionQuantityString} is not a valid initial condition quantity");
            }
        }
    }
}