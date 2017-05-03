using System;
using System.IO;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridApiDataSet
    {
        public class UGridAttributeConstants
        {
            public class LocationValues
            {
                public const string Face = "face";
                public const string Edge = "edge";
                public const string Node = "node";
                public const string Volume = "volume";
            }

            public class Names
            {
                public const string Location = "location";    
            }
        }

        
        public class GridConstants
        {
            public const int MAXDIMS = 6;
            public const int MAXSTRLEN = 255; // Must be equal to MAXSTRLEN in io_netcdf.dll (kernel)
            
            public const int NUMBER_OF_NODES_ON_A_EDGE = 2;
           
            public const int IONC_NOERR = 0;
            public const int IONC_GENERAL_FATAL_ERR = -1000;
            public const int IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR = -1001;

            public const double UG_CONV_MIN_VERSION = 1.0d;

            public const string UG_CONV_CF = "CF-1.6";
            public const string UG_CONV_UGRID = "UGRID-1.0";
        }

        public enum DataSetConventions
        {
            IONC_CONV_NULL = 0,//Dataset conventions not yet detected
            IONC_CONV_CF = 1,
            IONC_CONV_UGRID = 2,//Dataset based on UGRID-conventions
            IONC_CONV_SGRID = 4,//Dataset based on SGRID-conventions
            IONC_CONV_OTHER = -99,//Dataset based on unknown or unsupported conventions (user should fall back to NetCDF native API calls)
            IONC_CONV_TEST = -111111 //Dataset Id for testeing
        }

        public enum NetcdfOpenMode
        {
            nf90_nowrite        = 0,         
            nf90_write          = 1,         
            nf90_clobber        = 0,         
            nf90_noclobber      = 4,         
            nf90_fill           = 0,         
            nf90_nofill         = 256,       
            nf90_64bit_offset   = 512, 
            nf90_lock           = 1024,      
            nf90_share          = 2048 
        }

        public enum Locations
        {
            UG_LOC_NONE = 0,
            UG_LOC_NODE = 1,
            UG_LOC_EDGE = 2,
            UG_LOC_FACE = 4,
            UG_LOC_VOL  = 8,
            UG_LOC_ALL2D = UG_LOC_NODE + UG_LOC_EDGE + UG_LOC_FACE 
        }

        public const string GRIDDLL_NAME = "io_netcdf.dll";

        public static string DllDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(typeof(UGridApi).Assembly.Location), "Kernels");
            }
        }

        public static string DllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", GRIDDLL_NAME); }
        }
    }
}