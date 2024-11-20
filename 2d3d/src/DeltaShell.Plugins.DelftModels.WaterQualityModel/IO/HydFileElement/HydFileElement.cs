using System;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.HydFileElement
{
    /// <summary>
    /// Represents a key-value element in a hydrodynamics file (.hyd file).
    /// </summary>
    /// <typeparam name="T"> Type of the property value. </typeparam>
    public interface IHydFileElement
    {
        /// <summary>
        /// Parses a piece of text in order to retrieve the relevant value from a .hyd file
        /// property.
        /// </summary>
        /// <param name="textToParse"> The text to parse. </param>
        /// <returns> This element, updated with the parsed value. </returns>
        /// <exception cref="FormatException">
        /// When <paramref name="textToParse"/> is not
        /// formatted correctly for it's value.
        /// </exception>
        IHydFileElement ParseValue(string textToParse);

        /// <summary>
        /// Sets the parsed value from <see cref="ParseValue"/> to a .hyd file data object.
        /// </summary>
        /// <param name="hydFileData"> The hyd file data. </param>
        void SetDataTo(HydFileData hydFileData);
    }
}