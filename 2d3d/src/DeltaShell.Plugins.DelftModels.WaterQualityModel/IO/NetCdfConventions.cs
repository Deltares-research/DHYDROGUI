namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Class containing NetCDF conventions, according to Climate and Forecast (CF), UGRID or Deltares standards.
    /// </summary>
    internal static class NetCdfConventions
    {
        /// <summary>
        /// Class containing standard names for quantity variables used by D-Water Quality.
        /// Adheres to Climate and Forecast (CF) conventions.
        /// </summary>
        public static class StandardNames
        {
            /// <summary>
            /// Gets the standard name for the time quantity.
            /// </summary>
            public static string Time => "time";
        }

        /// <summary>
        /// Class containing standard names for quantity variables used by D-Water Quality.
        /// Adheres to Climate and Forecast (CF), UGRID and Deltares conventions.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            ///  Gets the name of the face dimension attribute.
            /// </summary>
            public static string FaceDimension => "face_dimension";

            /// <summary>
            ///  Gets the name of the fill value attribute.
            /// </summary>
            public static string FillValue => "_FillValue";

            /// <summary>
            ///  Gets the name of the delwaq attribute.
            /// </summary>
            public static string DelwaqName => "delwaq_name";

            /// <summary>
            ///  Gets the name of the standard name attribute.
            /// </summary>
            public static string StandardName => "standard_name";

            /// <summary>
            ///  Gets the name of the (global) conventions attribute.
            /// </summary>
            public static string Conventions => "Conventions";

            /// <summary>
            /// Gets the name of the units attribute.
            /// </summary>
            public static string Units = "units";
        }
    }

}
