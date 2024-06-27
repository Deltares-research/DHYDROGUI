using System;
using System.Collections.Generic;
using System.IO;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.NGHS.Common.IO
{
    /// <summary>
    /// A manager that keeps a collection of files and
    /// contains methods to perform actions with the files.
    /// </summary>
    /// <seealso cref="IFilesManager"/>
    public class FilesManager : IFilesManager
    {
        private readonly IList<Tuple<FileInfo, Action<string>>> fileData = new List<Tuple<FileInfo, Action<string>>>();

        public void Add(string filePath, Action<string> switchToAction)
        {
            Ensure.NotNull(filePath, nameof(filePath));

            fileData.Add(new Tuple<FileInfo, Action<string>>(new FileInfo(filePath),
                                                             switchToAction));
        }

        public void CopyTo(string targetPath, ILogHandler logHandler, bool switchTo)
        {
            Ensure.NotNull(targetPath, nameof(targetPath));
            Ensure.NotNull(logHandler, nameof(logHandler));

            var dirInfo = new DirectoryInfo(targetPath);

            foreach (Tuple<FileInfo, Action<string>> data in fileData)
            {
                FileInfo file = data.Item1;
                if (!file.Exists)
                {
                    logHandler.ReportError($"Could not find file at '{file.FullName}'.");
                    continue;
                }

                var targetFile = new FileInfo(Path.Combine(dirInfo.FullName, file.Name));
                if (file.FullName.Equals(targetFile.FullName))
                {
                    continue;
                }

                file.CopyTo(targetFile.FullName, true);

                if (switchTo)
                {
                    data.Item2?.Invoke(targetFile.FullName);
                }
            }
        }
    }
}