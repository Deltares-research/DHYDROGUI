namespace DHYDRO.Code
{
    /// <summary>
    /// Class to retrieve data.
    /// </summary>
    public static class Get
    {
        /// <summary>
        /// Gets the path to the DataSources folder.
        /// </summary>
        public static string DataSourcesDirectory => FileUtils.GetAbsolutePath("DataSources");

        /// <summary>
        /// Gets the path to the Resources folder.
        /// </summary>
        public static string ResourcesDirectory => FileUtils.GetAbsolutePath("Resources");

        /// <summary>
        /// Gets the input directory path used by the currently running Test Suite.
        /// </summary>
        public static string InputDirectory => FileUtils.GetAbsolutePath(Current.GetParameter("InputDirectory"));

        /// <summary>
        /// Gets the output directory path used by the currently running Test Suite.
        /// </summary>
        public static string OutputDirectory => FileUtils.GetAbsolutePath(Current.GetParameter("OutputDirectory"));
    }
}