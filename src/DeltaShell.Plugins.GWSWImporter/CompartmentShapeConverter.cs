using System;
using DelftTools.Hydro;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// Converts compartment shape strings as found in the GWSW files
    /// to the correct <see cref="CompartmentShape"/>.
    /// </summary>
    public static class CompartmentShapeConverter
    {
        /// <summary>
        /// Converts a string to a <see cref="CompartmentShape"/>.
        /// </summary>
        /// <param name="compartmentShapeString">The string for a compartment shape.</param>
        /// <returns>The corresponding <see cref="CompartmentShape"/></returns>
        /// <exception cref="InvalidOperationException">When an unknown compartment shape string is provided.</exception>
        public static CompartmentShape ConvertStringToCompartmentShape(string compartmentShapeString)
        {
            switch (compartmentShapeString.ToLower())
            {
                case "rnd":
                    return CompartmentShape.Square;
                case "rhk":
                    return CompartmentShape.Rectangular;
                case "unknown":
                    return CompartmentShape.Unknown;
                default:
                    throw new InvalidOperationException($"{compartmentShapeString} is not a valid compartment shape.");
            }
        }
    }
}