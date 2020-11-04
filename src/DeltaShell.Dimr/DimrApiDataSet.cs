using System;
using System.IO;
using BasicModelInterface;

namespace DeltaShell.Dimr
{
    public static class DimrApiDataSet
    {
        public const string DIMR_DLL_NAME = "dimr_dll.dll";
        public const string DIMR_EXE_NAME = "dimr.exe";

        private const string STANDARD_BINFOLDER_NAME = "bin";
        private const string STANDARD_SCRIPTFOLDER_NAME = "scripts";
        private const string SHARE_FOLDER_NAME = "share";

        private const string DIMR_FOLDER_NAME = "dimr";
        private const string WAVE_FOLDER_NAME = "dwaves";
        private const string SWAN_FOLDER_NAME = "swan";
        private const string ESMF_FOLDER_NAME = "esmf";
        private const string DFLOWFM_FOLDER_NAME = "dflowfm";
        private const string CF_FOLDER_NAME = "dflow1d";
        private const string RTCTOOLS_FOLDER_NAME = "drtc";
        private const string RR_FOLDER_NAME = "drr";
        private const string ITERATIVE1D2D_FOLDER_NAME = "dflow1d2d";
        


        public static string KernelsDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(typeof(DimrApi).Assembly.Location), "kernels");
            }
        }

        public static string SharedDllPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", SHARE_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string DimrDllPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", DIMR_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string DimrExePath
        {
            get { return Path.Combine(KernelsDirectory, "x64", DIMR_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string WaveDllPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", WAVE_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string WaveExePath
        {
            get { return Path.Combine(KernelsDirectory, "x64", WAVE_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string SwanExePath
        {
            get { return Path.Combine(KernelsDirectory, "x64", SWAN_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string SwanScriptPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", SWAN_FOLDER_NAME, STANDARD_SCRIPTFOLDER_NAME); }
        }

        public static string EsmfExePath
        {
            get { return Path.Combine(KernelsDirectory, "x64", ESMF_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string EsmfScriptPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", ESMF_FOLDER_NAME, STANDARD_SCRIPTFOLDER_NAME); }
        }

        public static string DFlowFmDllPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", DFLOWFM_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string CfDllPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", CF_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string RtcToolsDllPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", RTCTOOLS_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string RrDllPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", RR_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
        }

        public static string Iterative1D2DDllPath
        {
            get { return Path.Combine(KernelsDirectory, "x64", ITERATIVE1D2D_FOLDER_NAME, STANDARD_BINFOLDER_NAME); }
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

        public static Level LogFileLevel = Level.Info;
        public static Level FeedbackLevel = Level.Fatal;
        
        public const double DIMR_FILL_VALUE = -999000.0d;
    }
}