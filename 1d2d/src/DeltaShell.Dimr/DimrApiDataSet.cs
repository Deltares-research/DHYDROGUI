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
        /// The file name of the D-Flow FM kernel library.
        /// </summary>
        public const string DFlowFmDllName = "dflowfm.dll";

        /// <summary>
        /// The file name of the Rainfall Runoff kernel library.
        /// </summary>
        public const string RrDllName = "rr_dll.dll";

        /// <summary>
        /// The file name of the Real-Time Control tools kernel library.
        /// </summary>
        public const string RtcToolsDllName = "FBCTools_BMI.dll";

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
        private static string KernelsShareDirectory { get; } = Path.Combine(KernelsDirectory, "share");

        /// <summary>
        /// The directory that contains the DIMR library.
        /// </summary>
        public static string DimrDllDirectory => KernelsLibDirectory;

        /// <summary>
        /// The directory that contains the D-FlowFM kernel library.
        /// </summary>
        public static string DFlowFmDllDirectory => KernelsLibDirectory;

        /// <summary>
        /// The directory that contains the Rainfall Runoff kernel library.
        /// </summary>
        public static string RrDllDirectory => KernelsLibDirectory;

        /// <summary>
        /// The directory that contains the Real-Time Control tools kernel library.
        /// </summary>
        public static string RtcToolsDllDirectory => KernelsLibDirectory;

        /// <summary>
        /// The directory that contains the Real-Time Control schema definitions.
        /// </summary>
        public static string RtcXsdDirectory { get; } = Path.Combine(KernelsShareDirectory, "drtc");

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