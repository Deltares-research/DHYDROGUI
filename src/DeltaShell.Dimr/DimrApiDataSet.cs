using System;
using System.IO;
using System.Linq;
using BasicModelInterface;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// <see cref="DimrApiDataSet"/> defines the different paths used within DIMR,
    /// as well as the option to set the shared dll path.
    /// </summary>
    public static class DimrApiDataSet
    {
        /// <summary>
        /// The DIMR DLL name
        /// </summary>
        public const string DIMR_DLL_NAME = "dimr_dll.dll";
        [Obsolete]
        public const string DIMR_EXE_NAME = "dimr.exe";

        private const string STANDARD_BINFOLDER_NAME = "bin";
        private const string STANDARD_SCRIPTFOLDER_NAME = "scripts";
        private const string SHARE_FOLDER_NAME = "share";

        private const string DIMR_FOLDER_NAME = "dimr";
        private const string WAVE_FOLDER_NAME = "dwaves";
        private const string SWAN_FOLDER_NAME = "swan";
        private const string ESMF_FOLDER_NAME = "esmf";
        private const string DFLOWFM_FOLDER_NAME = "dflowfm";
        private const string RTCTOOLS_FOLDER_NAME = "drtc";

        private const string ARCH = "x64";

        /// <summary>
        /// Gets the kernels directory.
        /// </summary>
        public static string KernelsDirectory => Path.Combine(Path.GetDirectoryName(typeof(DimrApi).Assembly.Location), "kernels");

        /// <summary>
        /// Gets the shared DLL path.
        /// </summary>
        public static string SharedDllPath => Path.Combine(KernelsDirectory, ARCH, SHARE_FOLDER_NAME, STANDARD_BINFOLDER_NAME);

        /// <summary>
        /// Gets the DIMR DLL path.
        /// </summary>
        public static string DimrDllPath => Path.Combine(KernelsDirectory, ARCH, DIMR_FOLDER_NAME, STANDARD_BINFOLDER_NAME);

        /// <summary>
        /// Gets the dimr executable path.
        /// </summary>
        public static string DimrExePath => Path.Combine(KernelsDirectory, ARCH, DIMR_FOLDER_NAME, STANDARD_BINFOLDER_NAME);

        /// <summary>
        /// Gets the wave DLL path.
        /// </summary>
        public static string WaveDllPath => Path.Combine(KernelsDirectory, ARCH, WAVE_FOLDER_NAME, STANDARD_BINFOLDER_NAME);

        /// <summary>
        /// Gets the wave executable path.
        /// </summary>
        public static string WaveExePath => Path.Combine(KernelsDirectory, ARCH, WAVE_FOLDER_NAME, STANDARD_BINFOLDER_NAME);

        /// <summary>
        /// Gets the swan executable path.
        /// </summary>
        public static string SwanExePath => Path.Combine(KernelsDirectory, ARCH, SWAN_FOLDER_NAME, STANDARD_BINFOLDER_NAME);

        /// <summary>
        /// Gets the swan script path.
        /// </summary>
        public static string SwanScriptPath => Path.Combine(KernelsDirectory, ARCH, SWAN_FOLDER_NAME, STANDARD_SCRIPTFOLDER_NAME);

        /// <summary>
        /// Gets the esmf executable path.
        /// </summary>
        public static string EsmfExePath => Path.Combine(KernelsDirectory, ARCH, ESMF_FOLDER_NAME, STANDARD_BINFOLDER_NAME);

        /// <summary>
        /// Gets the esmf script path.
        /// </summary>
        public static string EsmfScriptPath => Path.Combine(KernelsDirectory, ARCH, ESMF_FOLDER_NAME, STANDARD_SCRIPTFOLDER_NAME);

        /// <summary>
        /// Gets the D-FLOW FM DLL path.
        /// </summary>
        public static string DFlowFmDllPath => Path.Combine(KernelsDirectory, ARCH, DFLOWFM_FOLDER_NAME, STANDARD_BINFOLDER_NAME);

        /// <summary>
        /// Gets the RTC tools DLL path.
        /// </summary>
        public static string RtcToolsDllPath => Path.Combine(KernelsDirectory, ARCH, RTCTOOLS_FOLDER_NAME, STANDARD_BINFOLDER_NAME);

        /// <summary>
        /// Add the DIMR shared dll path to the end of the PATH variable, if it has not been added yet.
        /// </summary>
        public static void SetSharedPath()
        {
            string path = Environment.GetEnvironmentVariable("PATH");

            if (path != null && path.Contains(SharedDllPath)) 
                return;

            if (path.Length > 0 && path.Last() != ';')
                path += ";";

            path += SharedDllPath;

            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// The feedback level key
        /// </summary>
        public const string FEEDBACKLEVELKEY = "feedbackLevel";

        /// <summary>
        /// The logfile level key
        /// </summary>
        public const string LOGFILELEVELKEY = "debugLevel";

        /// <summary>
        /// The log file level
        /// </summary>
        public static Level LogFileLevel = Level.None;

        /// <summary>
        /// The feedback level
        /// </summary>
        public static Level FeedbackLevel = Level.None;

        /// <summary>
        /// The DIMR fill value
        /// </summary>
        public const double DIMR_FILL_VALUE = -999000.0d;
    }
}