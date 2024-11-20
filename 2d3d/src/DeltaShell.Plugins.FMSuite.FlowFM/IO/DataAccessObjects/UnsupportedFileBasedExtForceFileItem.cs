using System.Collections.Generic;
using System.IO;
using DHYDRO.Common.IO.ExtForce;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public class UnsupportedFileBasedExtForceFileItem : IUnsupportedFileBasedExtForceFileItem
    {
        public UnsupportedFileBasedExtForceFileItem(string path, ExtForceData parent)
        {
            Path = path;
            UnsupportedExtForceFileItem = parent;
        }

        public ExtForceData UnsupportedExtForceFileItem { get; set; }

        public string Path { get; set; }
        public IEnumerable<string> Paths { get; }
        public bool IsFileCritical => true;

        public bool IsOpen => Path != null;

        /// <summary>
        /// Make a copy of the file if it is located in the DeltaShell working directory
        /// </summary>
        public bool CopyFromWorkingDirectory { get; }

        public void CreateNew(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path);
            }

            Path = path;
        }

        public void Close()
        {
            Path = null;
        }

        public void Open(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("File {0} could not be found", path));
            }

            Path = path;
        }

        public void CopyTo(string destinationPath)
        {
            if (!File.Exists(Path))
            {
                return;
            }

            if (System.IO.Path.GetFullPath(Path) != System.IO.Path.GetFullPath(destinationPath))
            {
                File.Copy(Path, destinationPath, true);
            }
        }

        public void SwitchTo(string newPath)
        {
            Path = newPath;
        }

        public void Delete()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }

            Path = null;
        }
    }
}