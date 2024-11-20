namespace DHYDRO.Common.IO.ExtForce
{
    /// <summary>
    /// Provides constant values used in external forcing files.
    /// </summary>
    public static class ExtForceFileConstants
    {
        /// <summary>
        /// The external forcing file delimiters.
        /// </summary>
        public static class Delimiters
        {
            /// <summary>
            /// The character used to denote comment blocks in the external forcing file.
            /// </summary>
            public const char CommentBlock = '*';

            /// <summary>
            /// The characters used to denote inline comments in the external forcing file.
            /// </summary>
            public static readonly char[] InlineComments = { '#', '!' };

            /// <summary>
            /// The character used to assign values to properties in the external forcing file.
            /// </summary>
            public const char Assignment = '=';
        }

        /// <summary>
        /// The external forcing property keys.
        /// </summary>
        public static class Keys
        {
            /// <summary>
            /// The key used to identify the quantity.
            /// </summary>
            public const string Quantity = "QUANTITY";

            /// <summary>
            /// The key used to identify a disabled quantity.
            /// </summary>
            public const string DisabledQuantity = "DISABLED_QUANTITY";

            /// <summary>
            /// The keys representing unsupported quantities.
            /// </summary>
            public static readonly string[] UnsupportedQuantities = { "WUANTITY", "_UANTITY" };

            /// <summary>
            /// The key used to identify the file name.
            /// </summary>
            public const string FileName = "FILENAME";

            /// <summary>
            /// The key used to identify the variable name.
            /// </summary>
            public const string VariableName = "VARNAME";

            /// <summary>
            /// The key used to identify the file type.
            /// </summary>
            public const string FileType = "FILETYPE";

            /// <summary>
            /// The key used to identify the interpolation method.
            /// </summary>
            public const string Method = "METHOD";

            /// <summary>
            /// The key used to identify the operand.
            /// </summary>
            public const string Operand = "OPERAND";

            /// <summary>
            /// The key used to identify the value.
            /// </summary>
            public const string Value = "VALUE";

            /// <summary>
            /// The key used to identify the conversion factor.
            /// </summary>
            public const string Factor = "FACTOR";

            /// <summary>
            /// The key used to identify the offset.
            /// </summary>
            public const string Offset = "OFFSET";

            /// <summary>
            /// The key used to identify the friction type.
            /// </summary>
            public const string FrictionType = "IFRCTYP";

            /// <summary>
            /// The key used to identify the averaging type.
            /// </summary>
            public const string AveragingType = "AVERAGINGTYPE";

            /// <summary>
            /// The key used to identify the relative search cell size.
            /// </summary>
            public const string RelativeSearchCellSize = "RELATIVESEARCHCELLSIZE";

            /// <summary>
            /// The key used to identify the minimum sample points.
            /// </summary>
            public const string MinSamplePoints = "MINSAMPLEPOINTS";

            /// <summary>
            /// The key used to identify the extrapolation tolerance.
            /// </summary>
            public const string ExtrapolationTolerance = "EXTRAPOLTOL";

            /// <summary>
            /// The key used to identify the area.
            /// </summary>
            public const string Area = "AREA";
        }

        /// <summary>
        /// Type of external forcing data file.
        /// </summary>
        public static class FileTypes
        {
            /// <summary>
            /// Time series data.
            /// </summary>
            public const int Uniform = 1;

            /// <summary>
            /// Time series magnitude and direction data.
            /// </summary>
            public const int UniMagDir = 2;

            /// <summary>
            /// Spatially varying weather data.
            /// </summary>
            public const int SVWP = 3;

            /// <summary>
            /// Esri ArcInfo grid data.
            /// </summary>
            public const int ArcInfo = 4;

            /// <summary>
            /// Spiderweb data (cyclones).
            /// </summary>
            public const int SpiderWeb = 5;

            /// <summary>
            /// Curvilinear data.
            /// </summary>
            public const int Curvi = 6;

            /// <summary>
            /// Triangulation data.
            /// </summary>
            public const int Triangulation = 7;

            /// <summary>
            /// Triangulation magnitude and direction data.
            /// </summary>
            public const int TriangulationMagDir = 8;

            /// <summary>
            /// Polyline data.
            /// </summary>
            public const int PolyTim = 9;

            /// <summary>
            /// Polygon data.
            /// </summary>
            public const int InsidePolygon = 10;

            /// <summary>
            /// NetCDF grid data.
            /// </summary>
            public const int NcGrid = 11;

            /// <summary>
            /// GeoTIFF data.
            /// </summary>
            public const int GeoTiff = 12;
            
            /// <summary>
            /// NetCDF flow data.
            /// </summary>
            public const int NcFlow = 12;

            /// <summary>
            /// NetCDF wave data.
            /// </summary>
            public const int NcWave = 13;
        }

        /// <summary>
        /// Type of interpolation method.
        /// </summary>
        public static class Methods
        {
            /// <summary>
            /// No method defined.
            /// </summary>
            public const int None = 0;

            /// <summary>
            /// Interpolate space and time (getval), keep  2 meteo fields in memory.
            /// </summary>
            public const int SpaceAndTimeKeepMeteoFields = 1;

            /// <summary>
            /// First interpolate space (update), next interpolate time, keep 2 flow fields in memory.
            /// </summary>
            public const int SpaceAndTimeKeepFlowFields = 2;

            /// <summary>
            /// Save weight factors, interpolate space and time (getval), keep 2 pointer- and weight sets in memory.
            /// </summary>
            public const int SpaceAndTimeSaveWeights = 3;

            /// <summary>
            /// Only spatial, inside polygon.
            /// </summary>
            public const int InsidePolygon = 4;

            /// <summary>
            /// Only spatial, triangulation.
            /// </summary>
            public const int Triangulation = 5;

            /// <summary>
            /// Only spatial, averaging.
            /// </summary>
            public const int Averaging = 6;

            /// <summary>
            /// Only spatial, index triangulation.
            /// </summary>
            public const int IndexTriangulation = 7;

            /// <summary>
            /// Only spatial, smoothing.
            /// </summary>
            public const int Smoothing = 8;

            /// <summary>
            /// Only spatial, internal diffusion.
            /// </summary>
            public const int InternalDiffusion = 9;

            /// <summary>
            /// Only initial vertical profiles.
            /// </summary>
            public const int VerticalProfiles = 10;
        }

        /// <summary>
        /// Type of external forcing operand describing how the data is combined with existing data for the same quantity.
        /// </summary>
        public static class Operands
        {
            /// <summary>
            /// Override at all points.
            /// </summary>
            public const string Override = "O";

            /// <summary>
            /// Apply only if no value specified previously.
            /// </summary>
            public const string Append = "A";

            /// <summary>
            /// Add to previously specified value.
            /// </summary>
            public const string Add = "+";

            /// <summary>
            /// Multiplies the existing values by the provided values.
            /// </summary>
            public const string Multiply = "*";

            /// <summary>
            /// Takes the maximum of the existing values and the provided values.
            /// </summary>
            public const string Maximum = "X";

            /// <summary>
            /// Takes the minimum of the existing values and the provided values.
            /// </summary>
            public const string Minimum = "N";
        }

        /// <summary>
        /// Type of averaging.
        /// </summary>
        public static class AveragingTypes
        {
            /// <summary>
            /// The arithmetic mean.
            /// </summary>
            public const int SimpleAveraging = 1;

            /// <summary>
            /// The closest Point.
            /// </summary>
            public const int ClosestPoint = 2;

            /// <summary>
            /// The maximum value.
            /// </summary>
            public const int MaximumValue = 3;

            /// <summary>
            /// The minimum value.
            /// </summary>
            public const int MinimumValue = 4;

            /// <summary>
            /// The inverse-distance-weighted mean.
            /// </summary>
            public const int InverseWeightedDistance = 5;

            /// <summary>
            /// The minimum of the absolute values.
            /// </summary>
            public const int MinAbs = 6;

            /// <summary>
            /// The nearest neighbors in k-dimensional space.
            /// </summary>
            public const int KdTree = 7;
        }
    }
}