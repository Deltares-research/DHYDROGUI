using System.IO;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils
{
    public static class DelwaqFileStructureHelper
    {
        public const string DELWAQ1_EXE = "delwaq1.exe";
        public const string DELWAQ2_EXE = "delwaq2.exe";
        private const string delwaq_kernel = "dwaq";
        private const string delwaq_plugin = "waq_kernel";

        static DelwaqFileStructureHelper()
        {
            DimrApiDataSet.SetSharedPath();
        }

        /// <summary>
        /// Gets the delwaq root folder file-path.
        /// </summary>
        public static string GetDelwaqKernelMainFolderPath()
        {
            return DWaqDllPath;
        }

        public static string GetDelwaqKernelPluginFolderPath()
        {
            string assemblyFolder = Path.GetDirectoryName(typeof(DelwaqFileStructureHelper).Assembly.Location);
            return Path.Combine(assemblyFolder, delwaq_plugin);
        }

        /// <summary>
        /// Gets the folder file-path where the delwaq binaries can be found.
        /// </summary>
        public static string GetDelwaqBinariesFolderPath()
        {
            return Path.Combine(GetDelwaqKernelMainFolderPath(), "bin");
        }

        /// <summary>
        /// Gets the delwaq1 executable path.
        /// </summary>
        public static string GetDelwaq1ExePath()
        {
            return Path.Combine(GetDelwaqBinariesFolderPath(), DELWAQ1_EXE);
        }

        /// <summary>
        /// Gets the delwaq2 executable path.
        /// </summary>
        public static string GetDelwaq2ExePath()
        {
            return Path.Combine(GetDelwaqBinariesFolderPath(), DELWAQ2_EXE);
        }

        /// <summary>
        /// Gets the delwaq substance and process data folder path.
        /// </summary>
        public static string GetDelwaqDataFolderPath()
        {
            return Path.Combine(GetDelwaqKernelPluginFolderPath(), "Data");
        }

        public static string GetDelwaqDataDefaultFolderPath()
        {
            return Path.Combine(GetDelwaqKernelMainFolderPath(), "Default");
        }

        private static string DWaqDllPath => Path.Combine(DimrApiDataSet.KernelsDirectory, "x64", delwaq_kernel);
    }
}