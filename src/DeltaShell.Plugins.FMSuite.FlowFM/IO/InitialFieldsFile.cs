namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// This class contains constants used for the Initial Fields file.
    /// </summary>
    public static class InitialFieldsFile
    {
        /// <summary>
        /// The quantity values.
        /// </summary>
        public static class Quantity
        {
            /// <summary>
            /// The water level quantity.
            /// </summary>
            public const string WaterLevel = "waterlevel";
        
            /// <summary>
            /// The water depth quantity.
            /// </summary>
            public const string WaterDepth = "waterdepth";

            /// <summary>
            /// The bed level quantity.
            /// </summary>
            public const string BedLevel = "bedlevel";

            /// <summary>
            /// The infiltration quantity.
            /// </summary>
            public const string Infiltration = "InfiltrationCapacity";
        }


        /// <summary>
        /// The data file type values.
        /// </summary>
        public static class DataType
        {
            /// <summary>
            /// The GeoTIFF data file type.
            /// </summary>
            public const string GeoTiff = "GeoTIFF";

            /// <summary>
            /// The ArcInfo data file type.
            /// </summary>
            public const string ArcInfo = "arcinfo";

            /// <summary>
            /// The sample data file type.
            /// </summary>
            public const string Sample = "sample";

            /// <summary>
            /// The polygon data file type.
            /// </summary>
            public const string Polygon = "polygon";
        }
    }
}