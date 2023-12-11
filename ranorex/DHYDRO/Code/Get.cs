using System;
using System.IO;
using System.Reflection;

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
        public static string DataSourcesPath => Path.Combine(ExecutableDirectory, "DataSources");
        
        /// <summary>
        /// Gets the path to the Resources folder.
        /// </summary>
        public static string ResourcesPath => Path.Combine(ExecutableDirectory, "Resources");

        /// <summary>
        /// Gets the path to the DHYDRO executable folder.
        /// </summary>
        private static string ExecutableDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}