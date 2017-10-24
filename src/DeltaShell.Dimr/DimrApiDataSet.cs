using System;
using System.IO;
using BasicModelInterface;

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
            get { return Path.Combine(DllDirectory, "x64", SHARED_FOLDER_NAME); }
        }

        public static string DllPath
        {
            get { return Path.Combine(DllDirectory, "x64", DIMR_FOLDER_NAME, DIMR_BINFOLDER_NAME); }
        }

        public static string ExePath
        {
            get { return Path.Combine(DllDirectory, "x64", DIMR_FOLDER_NAME, DIMR_BINFOLDER_NAME); }
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

        public static Level LogFileLevel = Level.Debug;
        public static Level FeedbackLevel = Level.Error;
        
        public const double DIMR_FILL_VALUE = -999000.0d;
    }
}