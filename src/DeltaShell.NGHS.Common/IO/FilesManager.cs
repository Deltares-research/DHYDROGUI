using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;

namespace DeltaShell.NGHS.Common.IO
{
    /// <summary>
    /// A manager that keeps a collection of files and
    /// contains methods to perform actions with the files.
    /// </summary>
    /// <seealso cref="IFilesManager"/>
    public class FilesManager : IFilesManager
    {
        private readonly IList<FileInfo> files = new List<FileInfo>();

        public void Add(string filePath)
        {
            Ensure.NotNull(filePath, nameof(filePath));

            var fileInfo = new FileInfo(filePath);
            if (files.Any(f => f.FullName.Equals(fileInfo.FullName)))
            {
                return;
            }

            files.Add(fileInfo);
        }

        public void CopyTo(string targetPath, ILogHandler logHandler = null)
        {
            Ensure.NotNull(targetPath, nameof(targetPath));

            var dirInfo = new DirectoryInfo(targetPath);

            foreach (FileInfo file in files)
            {
                if (!file.Exists)
                {
                    logHandler?.ReportWarning($"Could not find file at '{file.FullName}'.");
                    continue;
                }

                var targetFile = new FileInfo(Path.Combine(dirInfo.FullName, file.Name));
                if (targetFile.Exists)
                {
                    logHandler?.ReportWarning($"File already exists at '{targetFile.FullName}' and will be overwritten.");
                }

                file.CopyTo(targetFile.FullName, overwrite: true);
            }
        }
    }
}