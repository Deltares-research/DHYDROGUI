using System.IO;
using System.Linq;
using BasicModelInterface;
using DeltaShell.NGHS.Common;

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

        /// <summary>
        /// The feedback level key
        /// </summary>
        public const string FeedbackLevelKey = "feedbackLevel";

        /// <summary>
        /// The logfile level key
        /// </summary>
        public const string LogFileLevelKey = "debugLevel";

        private const string standardBinFolderName = "bin";
        private const string standardScriptFolderName = "scripts";
        private const string shareFolderName = "share";

        private const string dimrFolderName = "dimr";
        private const string waveFolderName = "dwaves";
        private const string swanFolderName = "swan";
        private const string esmfFolderName = "esmf";
        private const string dflowfmFolderName = "dflowfm";
        private const string rtcToolsFolderName = "drtc";

        private const string rrFolderName = "drr";
        private const string iterative1D2DFolderName = "dflow1d2d";
        
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
        /// Gets the RR DLL path.
        /// </summary>
        public static string RrDllPath => Path.Combine(KernelsDirectory, "x64", rrFolderName, standardBinFolderName);

        /// <summary>
        /// Gets the iterative 1D2D DLL path.
        /// </summary>
        public static string Iterative1D2DDllPath => Path.Combine(KernelsDirectory, "x64", iterative1D2DFolderName, standardBinFolderName);

        /// <summary>
        /// The log file level
        /// </summary>
        public static Level LogFileLevel { get; set; } = Level.None;

        /// <summary>
        /// The feedback level
        /// </summary>
        public static Level FeedbackLevel { get; set; } = Level.None;

        /// <summary>
        /// Add the DIMR shared dll path to the end of the PATH variable, if it has not been added yet.
        /// </summary>
        public static void SetSharedPath()
        {
            SetSharedPath(new SystemEnvironment());
        }

        /// <summary>
        /// Add the DIMR shared dll path to the end of the PATH variable, if it has not been added yet
        /// using the specified environment.
        /// </summary>
        /// <param name="environment">The environment to interact with.</param>
        public static void SetSharedPath(IEnvironment environment)
        {
            string path = environment.GetVariable(EnvironmentConstants.PathKey) ?? "";

            if (path.Contains(SharedDllPath))
            {
                return;
            }

            path = path.Any()
                       ? string.Join(";", SharedDllPath, path)
                       : SharedDllPath;

            environment.SetVariable(EnvironmentConstants.PathKey, path);
        }
    }
}