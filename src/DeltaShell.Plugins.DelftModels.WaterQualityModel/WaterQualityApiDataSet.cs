using System.IO;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel
{
    /// <summary>
    /// Defines the different paths used for the water quality kernels.
    /// </summary>
    public static class WaterQualityApiDataSet
    {
        /// <summary>
        /// Initializes the <see cref="WaterQualityApiDataSet"/> class.
        /// </summary>
        static WaterQualityApiDataSet()
        {
            DimrApiDataSet.AddKernelDirToPath();
        }

        /// <summary>
        /// The directory where the water quality plugin is located.
        /// </summary>
        private static string WaqPluginDirectory { get; } = Path.GetDirectoryName(typeof(WaterQualityApiDataSet).Assembly.Location);

        /// <summary>
        /// The base directory of the water quality kernel.
        /// </summary>
        private static string WaqKernelDirectory { get; } = Path.Combine(WaqPluginDirectory, "waq_kernel");

        /// <summary>
        /// Gets the directory of the water quality substances and process data.
        /// </summary>
        public static string WaqDataDirectory { get; } = Path.Combine(WaqKernelDirectory, "Data");
        
        /// <summary>
        /// Gets the directory that contains the default duflow process definition files.
        /// </summary>
        public static string WaqDuflowProcessDefinitionFilesDirectory { get; } = Path.Combine(WaqDataDirectory, "Default", "proc_def_duflow");
        
        /// <summary>
        /// Gets the full path of the duflow process library.
        /// </summary>
        public static string WaqDuflowDllPath { get; } = Path.Combine(WaqPluginDirectory, "x64", "duflow.dll");

        /// <summary>
        /// Gets the full path of the delwaq1 executable.
        /// </summary>
        public static string DelWaq1ExePath { get; } = Path.Combine(DimrApiDataSet.DelWaqExeDirectory, DimrApiDataSet.DelWaq1ExeName);

        /// <summary>
        /// Gets the full path of the delwaq2 executable.
        /// </summary>
        public static string DelWaq2ExePath { get; } = Path.Combine(DimrApiDataSet.DelWaqExeDirectory, DimrApiDataSet.DelWaq2ExeName);

        /// <summary>
        /// Gets the full path of the bloom substances file.
        /// </summary>
        public static string DelWaqBloomSpePath { get; } = Path.Combine(DimrApiDataSet.DelWaqResourcesDirectory, DimrApiDataSet.DelWaqBloomSpeName);

        /// <summary>
        /// Gets the directory that contains the default water quality process definition files.
        /// </summary>
        public static string DelWaqProcessDefinitionFilesDirectory { get; } = Path.Combine(DimrApiDataSet.DelWaqResourcesDirectory, "proc_def");
    }
}