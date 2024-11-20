using System;
using System.IO;

namespace DeltaShell.NGHS.IO.Grid
{
    /// <inheritdoc/>
    public class UgridFileInfo : IUgridFileInfo
    {
        public UgridFileInfo(string path)
        {
            Path = path;
        }

        /// <inheritdoc/>
        public string Path { get; }

        /// <inheritdoc/>
        public bool IsValidPath()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                return false;
            }

            FileInfo fileInfo;
            try
            {
                fileInfo = new FileInfo(Path);
            }
            catch (Exception)
            {
                return false;
            }

            return fileInfo.Exists &&
                   fileInfo.Length != 0 &&
                   !string.IsNullOrEmpty(fileInfo.Name);
        }
    }
}