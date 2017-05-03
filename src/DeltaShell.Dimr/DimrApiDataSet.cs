using System;
using System.IO;

namespace DeltaShell.Dimr
{
    public static class DimrApiDataSet
    {
        public const string DIMRDLL_NAME = "dimr_dll.dll";
        public const string DIMREXE_NAME = "dimr.exe";

        public static string DllDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(typeof(DimrApi).Assembly.Location), "kernels");
            }
        }

        public static string DllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", DIMRDLL_NAME); }
        }

        public static string ExePath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", DIMREXE_NAME); }
        }

        [Flags]
        public enum DebugLevel
        {
            SILENT            = 1,
            ALWAYS            = 2,
            WARN              = 4,
            MAJOR             = 8,
            MINOR             = 16,
            DETAIL            = 16,
            CONFIG_MAJOR      = 32,
            ITER_MAJOR        = 64,
            DDMAPPER_MAJOR    = 128,
            RESERVED_9        = 256,
            RESERVED_10       = 512,
            RESERVED_11       = 1024,
            RESERVED_12       = 2048,
            RESERVED_13       = 4096,
            RESERVED_14       = 8192,
            RESERVED_15       = 16384,
            
            CONFIG_MINOR      = 32768,
            ITER_MINOR        = 65536,
            DDMAPPER_MINOR    = 131072,
            DD_SENDRECV       = 262144,
            DD_SEMAPHORE      = 524288,
            RESERVED_21       = 1048576,
            RESERVED_22       = 2097152,
            RESERVED_23       = 4194304,
            RESERVED_24       = 8388608,
            RESERVED_25       = 16777216,
            RESERVED_26       = 33554432,
            RESERVED_27       = 67108864,
            RESERVED_28       = 134217728,
            RESERVED_29       = 268435456,
            RESERVED_30       = 536870912,
            LOG_DETAIL        = 1073741824,
        }

    }
}