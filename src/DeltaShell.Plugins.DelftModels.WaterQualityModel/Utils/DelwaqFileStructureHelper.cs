using System;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils
{
    public class DelwaqFileStructureHelper
    {
        private const string delwaq_kernel = "waq_kernel";
        public const string DELWAQ1_EXE = "delwaq1.exe";
        public const string DELWAQ2_EXE = "delwaq2.exe";
        public const string DELWAQ2LIB = "delwaq2_lib";
        public const string DELWAQ2LIB_DLL = "delwaq2_lib.dll";

        /// <summary>
        /// Gets the delwaq root folder file-path.
        /// </summary>
        public static string GetDelwaqKernelMainFolderPath()
        {
            string assemblyFolder = Path.GetDirectoryName(typeof(DelwaqFileStructureHelper).Assembly.Location);
            return Path.Combine(assemblyFolder, delwaq_kernel);
        }

        /// <summary>
        /// Gets the folder file-path where the delwaq binaries can be found.
        /// </summary>
        public static string GetDelwaqBinariesFolderPath()
        {
            return GetDelwaqBinariesFolderPath(Environment.Is64BitOperatingSystem);
        }

        private static string GetDelwaqBinariesFolderPath(bool isX64Machine)
        {
            string platformFolder = isX64Machine ? "x64" : "x86";

            return Path.Combine(GetDelwaqKernelMainFolderPath(), platformFolder);
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
            return Path.Combine(GetDelwaqKernelMainFolderPath(), "Data");
        }

        public static string GetDelwaqDataDefaultFolderPath()
        {
            return Path.Combine(GetDelwaqDataFolderPath(), "Default");
        }
    }
}