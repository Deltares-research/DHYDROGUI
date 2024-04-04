using System.IO;
using System.Linq;
using DeltaShell.NGHS.Common;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// Defines the different paths used within the DIMR set.
    /// </summary>
    public static class DimrApiDataSet
    {
        /// <summary>
        /// The file name of the DIMR library.
        /// </summary>
        public const string DimrDllName = "dimr.dll";

        /// <summary>
        /// </summary>
        public const string NetCdfDllName = "io_netcdf.dll";

        /// <summary>
        /// The file name of the D-Flow FM kernel library.
        /// </summary>
        public const string DFlowFmDllName = "dflowfm.dll";

        /// <summary>
        /// The file name of the Real-Time Control tools kernel library.
        /// </summary>
        public const string RtcToolsDllName = "FBCTools_BMI.dll";

        /// <summary>
        /// The file name of the Water Quality 1 executable.
        /// </summary>
        public const string DelWaq1ExeName = "delwaq1.exe";

        /// <summary>
        /// The file name of the Water Quality 2 executable.
        /// </summary>
        public const string DelWaq2ExeName = "delwaq2.exe";

        /// <summary>
        /// The file name of the bloom substances file.
        /// </summary>
        public const string DelWaqBloomSpeName = "bloom.spe";

        /// <summary>
        /// The directory where the DIMR API plugin is located.
        /// </summary>
        private static string DimrPluginDirectory { get; } = Path.GetDirectoryName(typeof(DimrApi).Assembly.Location);

        /// <summary>
        /// The base directory of the kernel files and folders.
        /// </summary>
        private static string KernelsDirectory { get; } = Path.Combine(DimrPluginDirectory, "kernels", "x64");

        /// <summary>
        /// The directory that contains the executables and run scripts for the kernels.
        /// </summary>
        public static string KernelsBinDirectory { get; } = Path.Combine(KernelsDirectory, "bin");

        /// <summary>
        /// The directory that contains the kernel libraries and third-party libraries.
        /// </summary>
        public static string KernelsLibDirectory { get; } = Path.Combine(KernelsDirectory, "lib");

        /// <summary>
        /// The directory that contains the kernel resource files.
        /// </summary>
        public static string KernelsShareDirectory { get; } = Path.Combine(KernelsDirectory, "share");

        /// <summary>
        /// The directory that contains the DIMR library.
        /// </summary>
        public static string DimrDllDirectory => KernelsLibDirectory;

        /// <summary>
        /// The directory that contains the D-FlowFM kernel library.
        /// </summary>
        public static string DFlowFmDllDirectory => KernelsLibDirectory;

        /// <summary>
        /// The directory that contains the Real-Time Control tools kernel library.
        /// </summary>
        public static string RtcToolsDllDirectory => KernelsLibDirectory;

        /// <summary>
        /// The directory that contains the water quality executables.
        /// </summary>
        public static string DelWaqExeDirectory => KernelsBinDirectory;

        /// <summary>
        /// The directory that contains the D-Waves executable.
        /// </summary>
        public static string WaveExeDirectory => KernelsBinDirectory;

        /// <summary>
        /// The directory that contains the Swan executable.
        /// </summary>
        public static string SwanExeDirectory => KernelsBinDirectory;

        /// <summary>
        /// The directory that contains the Esmf executable.
        /// </summary>
        public static string EsmfExeDirectory => KernelsBinDirectory;

        /// <summary>
        /// The directory that contains the Real-Time Control schema definitions.
        /// </summary>
        public static string RtcXsdDirectory { get; } = Path.Combine(KernelsShareDirectory, "drtc");

        /// <summary>
        /// The directory that contains the default water quality process definition files.
        /// </summary>
        public static string DelWaqResourcesDirectory { get; } = Path.Combine(KernelsShareDirectory, "delft3d");

        /// <summary>
        /// Add the kernel directory to the PATH variable, if not already present.
        /// </summary>
        public static void AddKernelDirToPath()
        {
            AddKernelDirToPath(new SystemEnvironment());
        }

        /// <summary>
        /// Add the kernel directory to the PATH variable, if not already present.
        /// </summary>
        /// <param name="environment">The environment to interact with.</param>
        public static void AddKernelDirToPath(IEnvironment environment)
        {
            string path = environment.GetVariable(EnvironmentConstants.PathKey) ?? "";

            if (path.Contains(KernelsLibDirectory))
            {
                return;
            }

            path = path.Any()
                       ? string.Join(";", KernelsLibDirectory, path)
                       : KernelsLibDirectory;

            environment.SetVariable(EnvironmentConstants.PathKey, path);
        }
    }
}