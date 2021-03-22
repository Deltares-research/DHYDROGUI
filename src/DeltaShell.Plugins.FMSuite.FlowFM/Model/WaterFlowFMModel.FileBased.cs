using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;

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
            OnAddedToProject(GetMduPathFromDeltaShellPath(path));
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
            string mduPath = GetMduPathFromDeltaShellPath(destinationPath);

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

            string expectedMduPath = GetMduPathFromDeltaShellPath(newPath);
            var mduFileInfo = new FileInfo(expectedMduPath);
            if (!mduFileInfo.Exists && mduFileInfo.Directory?.Parent != null)
            {
                // [D3DFMIQ-450] Backwards compatibility: Older Models may not have 'input' folder
                string legacyMduPath = Path.Combine(mduFileInfo.Directory.Parent.FullName, mduFileInfo.Name);

                if (File.Exists(legacyMduPath))
                {
                    OnSwitchTo(legacyMduPath);
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
            else // else: switch from existing: only change path
            {
                MduFilePath = mduPath;

                if (MduFile == null)
                {
                    return;
                }

                MduFile.Path = mduPath;
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

            foreach (IUnsupportedFileBasedExtForceFileItem notUsedExtForceFileItem in
                UnsupportedFileBasedExtForceFileItems)
            {
                string newPath = Path.Combine(Path.GetDirectoryName(ExtFilePath),
                                              Path.GetFileName(notUsedExtForceFileItem.Path));
                notUsedExtForceFileItem.SwitchTo(newPath);
            }
        }

        private void OnAddedToProject(string mduPath)
        {
            MduFilePath = mduPath;
            ExportTo(MduFilePath);
        }
    }
}