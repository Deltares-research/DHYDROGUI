using System.IO;
using System.Reflection;

namespace DHYDRO.Code
{
    /// <summary>
    /// Provides file utilities.
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Create the directory with the provided <paramref name="path"/> if it not exists.
        /// </summary>
        /// <param name="path"> The path of the directory to create. </param>
        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Create the parent directory of the provided <paramref name="path"/>.
        /// </summary>
        /// <param name="path"> The path to the file or directory to create the parent directory of. </param>
        public static void CreateParentDirectory(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        /// <summary>
        /// Gets the path to the DHYDRO executable folder.
        /// </summary>
        private static string ExecutableDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Gets the absolute path for the specified path.
        /// If the specified path is relative, it combines it with the executable directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to be converted to an absolute path.</param>
        /// <returns>The absolute path of the specified path.</returns>
        public static string GetAbsolutePath(string path)
        {
            return GetAbsolutePath(ExecutableDirectory, path);
        }

        /// <summary>
        /// Gets the absolute path for the specified path, based on the provided base path.
        /// If the specified path is relative, it combines it with the base path.
        /// </summary>
        /// <param name="basePath">The base path to be combined with the relative path.</param>
        /// <param name="path">The relative or absolute path to be converted to an absolute path.</param>
        /// <returns>The absolute path of the specified path.</returns>
        public static string GetAbsolutePath(string basePath, string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(basePath, path);
            }

            return Path.GetFullPath(path);
        }
    }
}