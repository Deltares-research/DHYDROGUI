using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        #region Implementation of IFileBased

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

                if (filePath.StartsWith("$"))
                {
                    if (MduFilePath != null)
                    {
                        OnSave();
                    }
                }
            }
        }

        IEnumerable<string> IFileBased.Paths
        {
            get
            {
                yield return ((IFileBased) this).Path;
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
            // todo: delete mdu & stuff
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

                if (MduFile == null)
                {
                    return;
                }

                mduFile.Path = mduPath;
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

        #endregion

        #region Files, folders and paths

        private const string mduExtension = ".mdu";
        private const string InputDirectoryName = "input";
        private const string OutputDirectoryName = "output";
        private const string PrefixDelwaqDirectoryName = "DFM_DELWAQ_";
        private const string SnappedFeaturesDirectoryName = "snapped";

        private string currentOutputDirectoryPath;
        private string outputSnappedFeaturesPath;
        private string inputFolder;

        public Func<string> WorkingDirectoryPathFunc =
            () => Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory");

        public event PropertyChangedEventHandler OutputSnappedFeaturesPathPropertyChanged;

        protected void OnOutputSnappedFeaturesPathPropertyChanged(string name)
        {
            OutputSnappedFeaturesPathPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string DelwaqOutputDirectoryName => PrefixDelwaqDirectoryName + Name;

        /// <summary>
        /// For FM the working directory is the same as the working directory from the application,
        /// which can be set by the user by using the options dialog box and this one is never null.
        /// Due to this the DimrRunner will never set a new working directory in the temp folder and
        /// therefore the set should never be executed.
        /// For other plugins the explicit working directory will be used as working directory. However,
        /// this one can be null and then the DimrRunner will create a new working directory and the setter
        /// is then needed.
        /// </summary>
        public virtual string WorkingDirectoryPath
        {
            get => Path.Combine(WorkingDirectoryPathFunc(), Name);
            set => throw new NotSupportedException("The working directory for running the model is not set");
        }

        public string ModelDirectoryPath => Path.GetDirectoryName(Path.GetDirectoryName(MduFilePath));

        public string DelwaqOutputDirectoryPath { get; set; }

        public string WorkingOutputDirectoryPath =>
            Path.Combine(WorkingDirectoryPath, DirectoryName, OutputDirectoryName);

        public string PersistentOutputDirectoryPath => Path.Combine(ModelDirectoryPath, OutputDirectoryName);

        public string HydFilePath
        {
            get
            {
                string modelName = Path.GetFileNameWithoutExtension(MduFilePath);
                return Path.Combine(WorkingDirectoryPath, DelwaqOutputDirectoryName, $"{modelName}.hyd");
            }
        }

        public string MduSavePath => GetMduPathFromDeltaShellPath(RecursivelyGetModelDirectoryPathFromMduFile());

        private string RecursivelyGetModelDirectoryPathFromMduFile()
        {
            if (string.IsNullOrEmpty(MduFilePath))
            {
                return Name;
            }

            var modelDir = new DirectoryInfo(MduFilePath);
            while (modelDir != null && modelDir.Name != Name)
            {
                modelDir = modelDir.Parent;
            }

            return modelDir?.Parent == null // should never happen, unless the file-based repository is corrupted
                       ? Path.GetDirectoryName(
                           Path.GetDirectoryName(MduFilePath)) // default behaviour (e.g. model renamed)
                       : modelDir.FullName;
        }

        public string HisSavePath
        {
            get
            {
                if (ModelDefinition == null)
                {
                    return null;
                }

                if (ModelDefinition.ModelName.Equals(Name))
                {
                    return HisFilePath;
                }

                return Name + WaterFlowFMModelDefinition.HisFileExtension;
            }
        }

        public string MapSavePath
        {
            get
            {
                if (ModelDefinition == null)
                {
                    return null;
                }

                if (ModelDefinition.ModelName.Equals(Name))
                {
                    return MapFilePath;
                }

                return Name + WaterFlowFMModelDefinition.MapFileExtension;
            }
        }

        public string ClassMapSavePath
        {
            get
            {
                if (ModelDefinition == null)
                {
                    return null;
                }

                if (ModelDefinition.ModelName.Equals(Name))
                {
                    return ClassMapFilePath;
                }

                return Name + WaterFlowFMModelDefinition.ClassMapFileExtension;
            }
        }

        public string ExtFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.ExtForceFile))
                {
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                                                        ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile));
                }

                return null;
            }
        }

        public string BndExtFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.BndExtForceFile))
                {
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                                                        ModelDefinition.GetModelProperty(
                                                            KnownProperties.BndExtForceFile));
                }

                return null;
            }
        }

        public string NetFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.NetFile))
                {
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                                                        ModelDefinition.GetModelProperty(KnownProperties.NetFile));
                }

                return null;
            }
        }

        public string MorFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.MorFile))
                {
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                                                        ModelDefinition.GetModelProperty(KnownProperties.MorFile));
                }

                return null;
            }
        }

        public string SedFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.SedFile))
                {
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                                                        ModelDefinition.GetModelProperty(KnownProperties.SedFile));
                }

                return null;
            }
        }

        public string OutputSnappedFeaturesPath
        {
            get => outputSnappedFeaturesPath;
            set
            {
                if (outputSnappedFeaturesPath == value)
                {
                    return;
                }

                outputSnappedFeaturesPath = value;

                OnOutputSnappedFeaturesPathPropertyChanged(TypeUtils.GetMemberName(() => OutputSnappedFeaturesPath));
            }
        }

        //Do not remove, is used by python code
        public string ComFilePath =>
            !string.IsNullOrEmpty(MduFilePath)
                ? Path.Combine(WorkingDirectoryPath, ModelDefinition.RelativeComFilePath)
                : null;

        private string HisFilePath =>
            !string.IsNullOrEmpty(MduFilePath)
                ? Path.Combine(PersistentOutputDirectoryPath, ModelDefinition.HisFileName)
                : null;

        private string MapFilePath =>
            !string.IsNullOrEmpty(MduFilePath)
                ? Path.Combine(PersistentOutputDirectoryPath, ModelDefinition.MapFileName)
                : null;

        private string ClassMapFilePath =>
            !string.IsNullOrEmpty(MduFilePath)
                ? Path.Combine(PersistentOutputDirectoryPath, ModelDefinition.ClassMapFileName)
                : null;

        private string GetMduPathFromDeltaShellPath(string path, string subFoldersFromModelFolder = "input")
        {
            string directoryName = path != null
                                       ? Path.GetDirectoryName(path) ?? ""
                                       : "";

            return Path.Combine(directoryName, Name, subFoldersFromModelFolder, Name + mduExtension);
        }

        public IEnumerable<KeyValuePair<WaterFlowFMProperty, string>> SubFiles
        {
            get
            {
                if (ModelDefinition == null)
                {
                    yield break;
                }

                string modelDefinitionName = ModelDefinition.ModelName;

                var modelNameBasedFiles = new Dictionary<string, string>
                {
                    {KnownProperties.NetFile, NetFile.FullExtension},
                    {KnownProperties.ExtForceFile, ExtForceFile.Extension},
                    {KnownProperties.BndExtForceFile, ExtForceFile.Extension},
                    {KnownProperties.LandBoundaryFile, MduFile.LandBoundariesExtension},
                    {KnownProperties.ThinDamFile, MduFile.ThinDamExtension},
                    {KnownProperties.FixedWeirFile, MduFile.FixedWeirExtension},
                    {KnownProperties.StructuresFile, MduFile.StructuresExtension},
                    {KnownProperties.ObsFile, MduFile.ObsExtension},
                    {KnownProperties.ObsCrsFile, MduFile.ObsCrossExtension},
                    {KnownProperties.DryPointsFile, MduFile.DryPointExtension}
                };

                foreach (KeyValuePair<string, string> pair in modelNameBasedFiles)
                {
                    WaterFlowFMProperty property = ModelDefinition.GetModelProperty(pair.Key);
                    string propertyValue = property.GetValueAsString();
                    if (pair.Key != KnownProperties.NetFile && pair.Key != KnownProperties.ExtForceFile &&
                        string.IsNullOrEmpty(propertyValue)) //skip default (empty) paths
                    {
                        continue;
                    }

                    string currentFileName = Path.GetFileName(propertyValue);
                    if (modelDefinitionName == null ||
                        (modelDefinitionName + pair.Value).Equals(currentFileName,
                                                                  StringComparison.InvariantCultureIgnoreCase) &&
                        pair.Key != KnownProperties.NetFile)
                    {
                        propertyValue = Name + pair.Value;
                    }

                    yield return new KeyValuePair<WaterFlowFMProperty, string>(property, propertyValue);
                }
            }
        }

        #endregion
    }
}