using System.IO;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class GridApiDataSet
    {
        public enum DataSetConventions
        {
            CONV_NULL = 0, //Dataset conventions not yet detected
            CONV_CF = 1,
            CONV_UGRID = 2,     //Dataset based on UGRID-conventions
            CONV_SGRID = 4,     //Dataset based on SGRID-conventions
            CONV_OTHER = -99,   //Dataset based on unknown or unsupported conventions (user should fall back to NetCDF native API calls)
            CONV_TEST = -111111 //Dataset Id for testing
        }

        public enum LocationType
        {
            UG_LOC_NONE = 0,
            UG_LOC_NODE = 1,
            UG_LOC_EDGE = 2,
            UG_LOC_FACE = 4,
            UG_LOC_VOL = 8,
            UG_LOC_ALL2D = UG_LOC_NODE + UG_LOC_EDGE + UG_LOC_FACE
        }

        public enum NetcdfOpenMode
        {
            nf90_nowrite = 0,
            nf90_write = 1,
            nf90_clobber = 0,
            nf90_noclobber = 4,
            nf90_fill = 0,
            nf90_nofill = 256,
            nf90_64bit_offset = 512,
            nf90_lock = 1024,
            nf90_share = 2048
        }

        public const string GRIDDLL_NAME = "io_netcdf.dll";

        public static class GridConstants
        {
            public const int MAXDIMS = 6;
            public const int MAXSTRLEN = 255; // Must be equal to MAXSTRLEN in io_netcdf.dll (kernel)

            public const int NUMBER_OF_NODES_ON_AN_EDGE = 2;

            public const int NOERR = 0;
            public const int GENERAL_FATAL_ERR = -1000;
            public const int GENERAL_ARRAY_LENGTH_FATAL_ERR = -1001;

            public const double UG_CONV_MIN_VERSION = 1.0d;

            public const string UG_CONV_CF = "CF-1.6";
            public const string UG_CONV_UGRID = "UGRID-1.0";

            public const int TESTING_ERROR = -9999;

            public const int NF90_DOUBLE = 6;
            public const double DEFAULT_FILL_VALUE = -999.0;
        }

        public static class UGridApiConstants
        {
            public const string Altitude = "altitude";
            public const string NodeZ = "node_z";
            public const string NetNodeZ = "NetNode_z";
            public const string FaceZ = "face_z";
            public const string M = "m";
            public const string DiscretizationPointIds = "node_ids";
        }

        public static class UGridAttributeConstants
        {
            public static class LocationValues
            {
                public const string Face = "face";
                public const string Edge = "edge";
                public const string Node = "node";
                public const string Volume = "volume";
            }

            public static class Names
            {
                public const string Location = "location";
            }
        }
    }
}