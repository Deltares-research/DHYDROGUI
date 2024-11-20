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
                    throw new InvalidOperationException($"{initialConditionQuantityString} is not a valid initial condition quantity.");
            }
        }

        /// <summary>
        /// Converts a <see cref="InitialConditionQuantity"/> to a string.
        /// </summary>
        /// <param name="quantity">The <see cref="InitialConditionQuantity"/> to convert.</param>
        /// <returns>A string representing the quantity.</returns>
        /// <exception cref="InvalidOperationException">When an invalid quantity is provided.</exception>
        public static string ConvertInitialConditionQuantityToString(
            InitialConditionQuantity quantity)
        {
            switch (quantity)
            {
                case InitialConditionQuantity.WaterLevel:
                    return "Water level";
                case InitialConditionQuantity.WaterDepth:
                    return "Water depth";
                default:
                    throw new InvalidOperationException($"{quantity} is not a valid initial condition quantity.");
            }
        }
    }
}