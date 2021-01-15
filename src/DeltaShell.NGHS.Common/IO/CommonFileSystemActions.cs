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
        /// except for the files defined in the <paramref name="fileExceptions"/>.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="fileExceptions">The file exceptions.</param>
        /// <remarks>
        /// If <paramref name="folderPath"/> nothing is done.
        /// If <paramref name="fileExceptions"/> no file exceptions are taken into account.
        /// </remarks>
        public static void ClearFolderWithFileExceptions(string folderPath, ISet<string> fileExceptions)
        {
            if (folderPath == null) return;
            fileExceptions = fileExceptions ?? new HashSet<string>();

            var dirInfo = new DirectoryInfo(folderPath);

            RemoveFiles(dirInfo, fileExceptions);
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