namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// Constants used in UGrid
    /// </summary>
    public static class UGridConstants
    {
        /// <summary>
        /// Error code to indicate no error
        /// </summary>
        public const int NoErrorCode = 0;

        /// <summary>
        /// Error code for a generic fatal error 
        /// </summary>
        public const int GeneralFatalErrorCode = -1000;

        /// <summary>
        /// Default naming conventions in UGrid
        /// </summary>
        public static class Naming
        {
            public const string Altitude = "altitude";
            public const string NodeZ = "node_z";
            public const string FaceZ = "face_z";
            public const string Meter = "m";
            public const string BranchIds = "branch_id";
            public const string BranchType = "branch_type";

            public const string LocationAttributeName = "location";
            public const string FaceLocationAttributeName = "face";
            public const string EdgeLocationAttributeName = "edge";
            public const string NodeLocationAttributeName = "node";
            public const string VolumeLocationAttributeName = "volume";
        }

    }
}
