using System;
using System.IO;

namespace DeltaShell.Dimr
{
    public static class DimrApiDataSet
    {
        public const string DIMR_FOLDER_NAME = "dimr";
        public const string SHARED_FOLDER_NAME = "shared";
        public const string DIMR_BINFOLDER_NAME = "bin";
        public const string DIMR_DLL_NAME = "dimr_dll.dll";
        public const string DIMR_EXE_NAME = "dimr.exe";

        public static string DllDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(typeof(DimrApi).Assembly.Location), "kernels");
            }
        }

        public static string SharedDllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", SHARED_FOLDER_NAME); }
        }

        public static string DllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", DIMR_FOLDER_NAME, DIMR_BINFOLDER_NAME); }
        }

        public static string ExePath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", DIMR_FOLDER_NAME, DIMR_BINFOLDER_NAME); }
        }

        public static void SetSharedPath()
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            if (path != null && path.Contains(SharedDllPath)) return;

            path = SharedDllPath + ";" + path;
            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
        }

        public const string FEEDBACKLEVELKEY = "feedbackLevel";
        public const string LOGFILELEVELKEY = "debugLevel";

        public static DimrLoggingLevel LogFileLevel = DimrLoggingLevel.LOG_DETAIL;
        public static DimrLoggingLevel FeedbackLevel = DimrLoggingLevel.WARN;


        [Flags]
        public enum DebugLevel : long
        {
            SILENT            = 0L,
            ALWAYS            = 1L,
            WARN              = 2L,
            MAJOR             = 4L,
            MINOR             = 8L,
            DETAIL            = 16L,
            CONFIG_MAJOR      = 32L,
            ITER_MAJOR        = 64L,
            DDMAPPER_MAJOR    = 128L,
            RESERVED_9        = 256L,
            RESERVED_10       = 512L,
            RESERVED_11       = 1024L,
            RESERVED_12       = 2048L,
            RESERVED_13       = 4096L,
            RESERVED_14       = 8192L,
            RESERVED_15       = 16384L,
            
            CONFIG_MINOR      = 32768L,
            ITER_MINOR        = 65536L,
            DDMAPPER_MINOR    = 131072L,
            DD_SENDRECV       = 262144L,
            DD_SEMAPHORE      = 524288L,
            RESERVED_21       = 1048576L,
            RESERVED_22       = 2097152L,
            RESERVED_23       = 4194304L,
            RESERVED_24       = 8388608L,
            RESERVED_25       = 16777216L,
            RESERVED_26       = 33554432L,
            RESERVED_27       = 67108864L,
            RESERVED_28       = 134217728L,
            RESERVED_29       = 268435456L,
            RESERVED_30       = 536870912L,
            LOG_DETAIL        = 1073741824L,
        }

        public enum DimrLoggingLevel : long
        {
            ALWAYS     = DebugLevel.ALWAYS,
            WARN       = DebugLevel.ALWAYS + DebugLevel.WARN,
            MAJOR      = DebugLevel.ALWAYS + DebugLevel.WARN + DebugLevel.MAJOR,
            MINOR      = DebugLevel.ALWAYS + DebugLevel.WARN + DebugLevel.MAJOR + DebugLevel.MINOR,
            DETAIL     = DebugLevel.ALWAYS + DebugLevel.WARN + DebugLevel.MAJOR + DebugLevel.MINOR + DebugLevel.DETAIL,
            LOG_DETAIL = DebugLevel.LOG_DETAIL
        }

        public const double DIMR_FILL_VALUE = -999000.0d;
    }
}