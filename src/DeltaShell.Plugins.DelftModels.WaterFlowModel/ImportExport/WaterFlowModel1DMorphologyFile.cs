using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Data;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class WaterFlowModel1DMorphologyFile : EditableObjectUnique<long>, IFileBased, ICloneable
    {
        public virtual void CreateNew(string path)
        {
            
        }

        public virtual void Close()
        {
            throw new NotImplementedException();
        }

        public virtual void Open(string path)
        {
            throw new NotImplementedException();
        }

        public virtual void CopyTo(string destinationPath)
        {
            if (!File.Exists(Path) || Equals(Path, destinationPath))
            {
                return;
            }

            // create directory if not exists
            var destinationFileInfo = new FileInfo(destinationPath);
            FileUtils.CreateDirectoryIfNotExists(destinationFileInfo.DirectoryName);

            FileUtils.CopyFile(Path, destinationPath);
        }

        public virtual void SwitchTo(string newPath)
        {
            Path = newPath;
        }

        public virtual void Delete()
        {
            FileUtils.DeleteIfExists(Path);
        }

        public virtual string Path { get; set; }

        public virtual IEnumerable<string> Paths
        {
            get { return new[] { Path }; }
            set { }
        }
        public virtual bool IsFileCritical { get; set; }
        public virtual bool IsOpen { get; set; }
        public virtual bool CopyFromWorkingDirectory { get; } = false;

        public virtual object Clone()
        {
            return new WaterFlowModel1DMorphologyFile { Path = Path };
        }
    }
}
