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
        public static CompartmentShape ConvertStringToCompartmentShape(string compartmentShapeString)
        {
            switch (compartmentShapeString.ToLower())
            {
                case "rnd":
                case "round":
                    return CompartmentShape.Round;
                case "rhk":
                case "rectangular":
                    return CompartmentShape.Rectangular;
                case "unknown":
                default:
                    return CompartmentShape.Unknown;
            }
        }
    }
}