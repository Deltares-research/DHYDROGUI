namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    /// <summary>
    /// <see cref="BndExtForceFileConstants"/> contains constant values for the boundary external forcing file.
    /// </summary>
    public static class BndExtForceFileConstants
    {
        /// <summary>
        /// The boundary category key.
        /// </summary>
        public const string BoundaryBlockKey = "boundary";

        /// <summary>
        /// The lateral category key.
        /// </summary>
        public const string LateralBlockKey = "lateral";
        
        /// <summary>
        /// The general category key.
        /// </summary>
        public const string GeneralBlockKey = "general";
        
        /// <summary>
        /// The quantity property key.
        /// </summary>
        public const string QuantityKey = "quantity";

        /// <summary>
        /// The location file property key.
        /// </summary>
        public const string LocationFileKey = "locationFile";

        /// <summary>
        /// The forcing file property key.
        /// </summary>
        public const string ForcingFileKey = "forcingFile";

        /// <summary>
        /// The thatcher harleman time lag property key.
        /// </summary>
        public const string ThatcherHarlemanTimeLagKey = "returnTime";

        /// <summary>
        /// The open boundary tolerance property key.
        /// </summary>
        public const string OpenBoundaryToleranceKey = "OpenBoundaryTolerance";

        /// <summary>
        /// The id key.
        /// </summary>
        public const string IdKey = "id";
        
        /// <summary>
        /// The name key.
        /// </summary>
        public const string NameKey = "name";

        /// <summary>
        /// The type key.
        /// </summary>
        public const string TypeKey = "type";
        
        /// <summary>
        /// The location type key.
        /// </summary>
        public const string LocationTypeKey = "locationType";
        
        /// <summary>
        /// The number of coordinates key.
        /// </summary>
        public const string NumCoordinatesKey = "numCoordinates";
        
        /// <summary>
        /// The x-coordinates key.
        /// </summary>
        public const string XCoordinatesKey = "xCoordinates";
        
        /// <summary>
        /// The y-coordinates key.
        /// </summary>
        public const string YCoordinatesKey = "yCoordinates";

        /// <summary>
        /// The discharge key.
        /// </summary>
        public const string DischargeKey = "discharge";

        /// <summary>
        /// The realtime discharge value.
        /// </summary>
        public const string RealTimeValue = "realtime";

        /// <summary>
        /// The file version property key.
        /// </summary>
        public const string FileVersionKey = "fileVersion";
        
        /// <summary>
        /// The file type property key.
        /// </summary>
        public const string FileTypeKey = "fileType";
    }
}