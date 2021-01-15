using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;

namespace DeltaShell.NGHS.Common.IO
{
    /// <summary>
    /// <see cref="CommonFileSystemActions"/> implements common actions related to the
    /// file system.
    /// </summary>
    public static class CommonFileSystemActions
    {
        /// <summary>
        /// Clears any files and subsequent empty subfolders of the folder at <paramref name="folderPath"/>
        /// except for the files defined in the <paramref name="filteredFilePathsSet"/>.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="filteredFilePathsSet">The set of (absolute) file paths which should be ignored when cleaning the folder.</param>
        /// <remarks>
        /// If <paramref name="folderPath"/> nothing is done.
        /// If <paramref name="filteredFilePathsSet"/> no file exceptions are taken into account.
        ///
        /// <paramref name="filteredFilePathsSet"/> is expected to contain absolute paths, relative paths are
        /// not supported, and will be deleted regardless. Any absolute path that is encountered in the provided
        /// <paramref name="folderPath"/> is ignored and will not be deleted.
        /// </remarks>
        public static void ClearFolder(string folderPath, ISet<string> filteredFilePathsSet)
        {
            if (folderPath == null) return;
            filteredFilePathsSet = filteredFilePathsSet ?? new HashSet<string>();

            var dirInfo = new DirectoryInfo(folderPath);

            RemoveFiles(dirInfo, filteredFilePathsSet);
            RemoveEmptySubDirectories(dirInfo);
        }

        private static void RemoveFiles(DirectoryInfo parentDirectoryInfo,
                                        ISet<string> filePathFilter) =>
            parentDirectoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                               .Where(fi => !filePathFilter.Contains(fi.FullName))
                               .ForEach(fi => fi.Delete());

        private static void RemoveEmptySubDirectories(DirectoryInfo parentDirectoryInfo) =>
            parentDirectoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories)
                               .Where(di => !di.EnumerateFiles().Any())
                               .ForEach(di => FileUtils.DeleteIfExists(di.FullName));
    }
}