using DelftTools.Hydro;
using DHYDRO.Common.Logging;

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
        /// <param name="logHandler">Do not log to the static logger but to this handler to report all warnings in 1 report.</param>
        /// <returns>The corresponding <see cref="CompartmentShape"/></returns>
        public static CompartmentShape ConvertStringToCompartmentShape(string compartmentShapeString, ILogHandler logHandler)
        {
            switch (compartmentShapeString?.ToLower())
            {
                case "rnd":
                case "round":
                    return CompartmentShape.Round;
                case "rhk":
                case "rectangular":
                    return CompartmentShape.Rectangular;
                case "unknown":
                    return CompartmentShape.Unknown;
                default:
                    logHandler?.ReportWarningFormat(GWSW.Properties.Resources.Shape__0__is_not_a_valid_shape_Setting_shape_to_unknown, compartmentShapeString);
                    return CompartmentShape.Unknown;
            }
        }
    }
}