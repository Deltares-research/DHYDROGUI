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
        /// Gets the full path of the delwaq executable.
        /// </summary>
        public static string DelWaqExePath { get; } = Path.Combine(DimrApiDataSet.DelWaqExeDirectory, DimrApiDataSet.DelWaqExeName);

        /// <summary>
        /// Gets the full path of the bloom substances file.
        /// </summary>
        public static string DelWaqBloomSpePath { get; } = Path.Combine(DimrApiDataSet.DelWaqResourcesDirectory, DimrApiDataSet.DelWaqBloomSpeName);

        /// <summary>
        /// Gets the full path of the default water quality process definition files (without *.dat/*.def extension).
        /// </summary>
        public static string DelWaqProcessDefinitionFilesPath { get; } = Path.Combine(DimrApiDataSet.DelWaqResourcesDirectory, Path.GetFileNameWithoutExtension(DimrApiDataSet.DelWaqProcDefName));
        
        /// <summary>
        /// The directory that contains the default water quality substances files.
        /// </summary>
        public static string DelWaqSubstanceFilesDirectory => DimrApiDataSet.DelWaqSubstancesDirectory;
    }
}