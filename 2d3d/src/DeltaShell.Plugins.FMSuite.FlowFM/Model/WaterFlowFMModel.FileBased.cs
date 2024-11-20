using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        string IFileBased.Path
        {
            get => filePath;
            set
            {
                if (filePath == value)
                {
                    return;
                }

                filePath = value;

                if (filePath == null)
                {
                    return;
                }

                if (filePath.StartsWith("$") && MduFilePath != null)
                {
                    OnSave();
                }
            }
        }

        IEnumerable<string> IFileBased.Paths
        {
            get
            {
                yield return ((IFileBased)this).Path;
            }
        }

        private string filePath;
        private bool isOpen;

        public bool IsFileCritical => true;

        bool IFileBased.IsOpen => isOpen;

        void IFileBased.CreateNew(string path)
        {
            OnAddedToProject(GetMduSavePath(path));
            filePath = path;
            isOpen = true;
        }

        void IFileBased.Close()
        {
            isOpen = false;
        }

        void IFileBased.Open(string path)
        {
            isOpen = true;
        }

        void IFileBased.CopyTo(string destinationPath)
        {
            string mduPath = GetMduSavePath(destinationPath);

            string dirName = Path.GetDirectoryName(mduPath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            RenameSubFilesIfApplicable();
            ExportTo(mduPath, false);
        }

        /// <summary>
        /// Relocate to reconnects the item to the given path. Does NOT perform copyTo.
        /// </summary>
        void IFileBased.SwitchTo(string newPath)
        {
            filePath = newPath;

            string expectedMduPath = GetMduSavePath(newPath);
            
            var mduFileInfo = new FileInfo(expectedMduPath);
            if (!mduFileInfo.Exists && mduFileInfo.Directory?.Parent != null)
            {
                // Older models may not have an 'input' folder or the MDU file might be located in a subdirectory
                string modelDirectory = mduFileInfo.Directory.Parent.FullName;
                string foundMduPath = Directory.GetFiles(modelDirectory, "*.mdu", SearchOption.AllDirectories).FirstOrDefault();
                
                if (File.Exists(foundMduPath))
                {
                    OnSwitchTo(foundMduPath);
                    return;
                }
            }

            OnSwitchTo(expectedMduPath);
        }

        void IFileBased.Delete()
        {
            // There currently does not occur any delete when the WaterFlowFMModel is 
            // deleted as a FileBased item.
        }

        private void OnSwitchTo(string mduPath)
        {
            if (MduFilePath == null) // switch from nothing: load
            {
                LoadFromMdu(mduPath);
            }
            else // switch from existing: only change path
            {
                MduFilePath = mduPath;
            }
        }

        private void OnAddedToProject(string mduPath)
        {
            ExportTo(mduPath);
        }
    }
}