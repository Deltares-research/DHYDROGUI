namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Collection of known data in an initial field file, such as section headers and property keys.
    /// </summary>
    public static class InitialFieldFileConstants
    {
        /// <summary>
        /// The default file name.
        /// </summary>
        public const string DefaultFileName = "initialFields.ini";

        /// <summary>
        /// Section headers.
        /// </summary>
        public static class Headers
        {
            public const string General = "General";
            public const string Initial = "Initial";
            public const string Parameter = "Parameter";
        }

        /// <summary>
        /// Property keys.
        /// </summary>
        public static class Keys
        {
            public const string FileVersion = "fileVersion";
            public const string FileType = "fileType";
            public const string Quantity = "quantity";
            public const string DataFile = "dataFile";
            public const string DataFileType = "dataFileType";
            public const string InterpolationMethod = "interpolationMethod";
            public const string Operand = "operand";
            public const string AveragingType = "averagingType";
            public const string FrictionType = "ifrctyp";
            public const string AveragingRelSize = "averagingRelSize";
            public const string AveragingNumMin = "averagingNumMin";
            public const string AveragingPercentile = "averagingPercentile";
            public const string ExtrapolationMethod = "extrapolationMethod";
            public const string LocationType = "locationType";
            public const string Value = "value";
        }
    }
}