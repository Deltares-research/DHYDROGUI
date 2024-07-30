using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel : IFileBased
    {
        private string filePath;
        private bool isOpen;
        
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
            get { yield return ((IFileBased)this).Path; }
        }

        public bool IsFileCritical => true;

        bool IFileBased.IsOpen => isOpen;

        public bool CopyFromWorkingDirectory => false;

        void IFileBased.CreateNew(string path)
        {
            ExportTo(GetMduSavePath(path));
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
            //Currently no action, implementation will be based on decision of issue [FM1D2D-2112].
        }

        private void OnSwitchTo(string mduPath)
        {
            if (MduFilePath == null) // switch from nothing: load
            {
                OnLoad(mduPath);
            }
            else // else: switch from existing: only change path
            {
                MduFilePath = mduPath;
                SwitchFileBasedItems();
            }
        }

        private void SwitchFileBasedItems()
        {
            foreach (IFileBased windField in WindFields.OfType<IFileBased>())
            {
                string newPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), Path.GetFileName(windField.Path));
                windField.SwitchTo(newPath);
            }

            foreach (IUnsupportedFileBasedExtForceFileItem notUsedExtForceFileItem in UnsupportedFileBasedExtForceFileItems)
            {
                string newPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), Path.GetFileName(notUsedExtForceFileItem.Path));
                notUsedExtForceFileItem.SwitchTo(newPath);
            }
        }
    }
}