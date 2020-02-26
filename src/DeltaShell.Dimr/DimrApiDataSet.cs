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
        public const string DimrDllName = "dimr_dll.dll";

        [Obsolete("No longer used, use the Dll instead.")]
        public const string DimrExeName = "dimr.exe";

        private const string standardBinFolderName = "bin";
        private const string standardScriptFolderName = "scripts";
        private const string shareFolderName = "share";

        private const string dimrFolderName = "dimr";
        private const string waveFolderName = "dwaves";
        private const string swanFolderName = "swan";
        private const string esmfFolderName = "esmf";
        private const string dflowfmFolderName = "dflowfm";
        private const string rtcToolsFolderName = "drtc";

        private const string ARCH = "x64";

        /// <summary>
        /// Gets the kernels directory.
        /// </summary>
        public static string KernelsDirectory => Path.Combine(Path.GetDirectoryName(typeof(DimrApi).Assembly.Location), "kernels");

        /// <summary>
        /// Gets the shared DLL path.
        /// </summary>
        public static string SharedDllPath => Path.Combine(KernelsDirectory, ARCH, shareFolderName, standardBinFolderName);

        /// <summary>
        /// Gets the DIMR DLL path.
        /// </summary>
        public static string DimrDllPath => Path.Combine(KernelsDirectory, ARCH, dimrFolderName, standardBinFolderName);

        /// <summary>
        /// Gets the dimr executable path.
        /// </summary>
        public static string DimrExePath => Path.Combine(KernelsDirectory, ARCH, dimrFolderName, standardBinFolderName);

        /// <summary>
        /// Gets the wave DLL path.
        /// </summary>
        public static string WaveDllPath => Path.Combine(KernelsDirectory, ARCH, waveFolderName, standardBinFolderName);

        /// <summary>
        /// Gets the wave executable path.
        /// </summary>
        public static string WaveExePath => Path.Combine(KernelsDirectory, ARCH, waveFolderName, standardBinFolderName);

        /// <summary>
        /// Gets the swan executable path.
        /// </summary>
        public static string SwanExePath => Path.Combine(KernelsDirectory, ARCH, swanFolderName, standardBinFolderName);

        /// <summary>
        /// Gets the swan script path.
        /// </summary>
        public static string SwanScriptPath => Path.Combine(KernelsDirectory, ARCH, swanFolderName, standardScriptFolderName);

        /// <summary>
        /// Gets the esmf executable path.
        /// </summary>
        public static string EsmfExePath => Path.Combine(KernelsDirectory, ARCH, esmfFolderName, standardBinFolderName);

        /// <summary>
        /// Gets the esmf script path.
        /// </summary>
        public static string EsmfScriptPath => Path.Combine(KernelsDirectory, ARCH, esmfFolderName, standardScriptFolderName);

        /// <summary>
        /// Gets the D-FLOW FM DLL path.
        /// </summary>
        public static string DFlowFmDllPath => Path.Combine(KernelsDirectory, ARCH, dflowfmFolderName, standardBinFolderName);

        /// <summary>
        /// Gets the RTC tools DLL path.
        /// </summary>
        public static string RtcToolsDllPath => Path.Combine(KernelsDirectory, ARCH, rtcToolsFolderName, standardBinFolderName);

        /// <summary>
        /// Add the DIMR shared dll path to the end of the PATH variable, if it has not been added yet.
        /// </summary>
        public static void SetSharedPath()
        {
            string path = Environment.GetEnvironmentVariable("PATH");

            if (path != null && path.Contains(SharedDllPath)) 
                return;

            if (path == null)
                path = "";
            else if (path.Length > 0 && path.Last() != ';')
                path += ";";

            path += SharedDllPath;

            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// The feedback level key
        /// </summary>
        public const string FeedbackLevelKey = "feedbackLevel";

        /// <summary>
        /// The logfile level key
        /// </summary>
        public const string LogFileLevelKey = "debugLevel";

        /// <summary>
        /// The log file level
        /// </summary>
        public static Level LogFileLevel { get; set; } = Level.None;

        /// <summary>
        /// The feedback level
        /// </summary>
        public static Level FeedbackLevel { get; set; } = Level.None;

        /// <summary>
        /// The DIMR fill value
        /// </summary>
        public const double DimrFillValue = -999000.0d;
    }
}