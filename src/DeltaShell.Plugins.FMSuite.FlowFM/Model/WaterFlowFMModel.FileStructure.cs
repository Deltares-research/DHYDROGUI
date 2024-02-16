using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.CopyHandlers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        private string outputSnappedFeaturesPath;

        private CacheFile cacheFile = null;

        public event PropertyChangedEventHandler OutputSnappedFeaturesPathPropertyChanged;
        public Func<string> WorkingDirectoryPathFunc { get; set; } = () => Path.Combine(DefaultModelSettings.DefaultDeltaShellWorkingDirectory);

        /// <summary>
        /// Gets the mdu file.
        /// </summary>
        /// <value>
        /// The mdu file.
        /// </value>
        public MduFile MduFile { get; } = new MduFile();

        /// <summary>
        /// Gets the cache file.
        /// </summary>
        /// <value>
        /// The cache file.
        /// </value>
        public CacheFile CacheFile =>
            cacheFile ?? (cacheFile = new CacheFile(this, new OverwriteCopyHandler()));

        public string DelwaqOutputDirectoryName => FileConstants.PrefixDelwaqDirectoryName + Name;

        /// <summary>
        /// For FM the working directory is the same as the working directory from the application,
        /// which can be set by the user by using the options dialog box and this one is never null.
        /// Due to this the DimrRunner will never set a new working directory in the temp folder and
        /// therefore the set should never be executed.
        /// For other plugins the explicit working directory will be used as working directory. However,
        /// this one can be null and then the DimrRunner will create a new working directory and the setter
        /// is then needed.
        /// </summary>
        public virtual string WorkingDirectoryPath => Path.Combine(WorkingDirectoryPathFunc(), Name);

        public string ModelDirectoryPath => Path.GetDirectoryName(Path.GetDirectoryName(MduFilePath));

        public string DelwaqOutputDirectoryPath { get; set; }

        public string WorkingOutputDirectoryPath =>
            Path.Combine(WorkingDirectoryPath, DirectoryName, DirectoryNameConstants.OutputDirectoryName);

        public string PersistentOutputDirectoryPath => Path.Combine(ModelDirectoryPath, DirectoryNameConstants.OutputDirectoryName);

        public string MduSavePath => GetMduPathFromDeltaShellPath(RecursivelyGetModelDirectoryPathFromMduFile());

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
                    return !string.IsNullOrEmpty(MduFilePath)
                               ? Path.Combine(PersistentOutputDirectoryPath, ModelDefinition.HisFileName)
                               : null;
                }

                return Name + FileConstants.HisFileExtension;
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
                    return !string.IsNullOrEmpty(MduFilePath)
                               ? Path.Combine(PersistentOutputDirectoryPath, ModelDefinition.MapFileName)
                               : null;
                }

                return Name + FileConstants.MapFileExtension;
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
                    return !string.IsNullOrEmpty(MduFilePath)
                               ? Path.Combine(PersistentOutputDirectoryPath, ModelDefinition.ClassMapFileName)
                               : null;
                }

                return Name + FileConstants.ClassMapFileExtension;
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

                OnOutputSnappedFeaturesPathPropertyChanged(nameof(OutputSnappedFeaturesPath));
            }
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
                    {KnownProperties.ExtForceFile, FileConstants.ExternalForcingFileExtension},
                    {KnownProperties.BndExtForceFile, FileConstants.ExternalForcingFileExtension},
                    {KnownProperties.LandBoundaryFile, FileConstants.LandBoundaryFileExtension},
                    {KnownProperties.ThinDamFile, FileConstants.ThinDamPliFileExtension},
                    {KnownProperties.FixedWeirFile, FileConstants.FixedWeirPlizFileExtension},
                    {KnownProperties.StructuresFile, FileConstants.StructuresFileExtension},
                    {KnownProperties.ObsFile, FileConstants.ObsPointFileExtension},
                    {KnownProperties.ObsCrsFile, FileConstants.ObsCrossSectionPliFileExtension},
                    {KnownProperties.DryPointsFile, FileConstants.DryPointFileExtension}
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

        public virtual string KernelDirectoryLocation => DimrApiDataSet.DFlowFmDllPath;

        #region Implementation of IHydFileModel

        /// <summary>
        /// Path to the produced hyd file.
        /// </summary>
        /// <returns>
        /// Returns the expected absolute path of the hyd file when the directory has been set, otherwise returns an empty
        /// string.
        /// </returns>
        public string HydFilePath =>
            DelwaqOutputDirectoryPath != null
                ? Path.Combine(DelwaqOutputDirectoryPath, $"{Path.GetFileNameWithoutExtension(MduFilePath)}.hyd")
                : string.Empty;

        #endregion

        public virtual string MduFilePath { get; protected set; }

        protected void OnOutputSnappedFeaturesPathPropertyChanged(string name)
        {
            OutputSnappedFeaturesPathPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string RecursivelyGetModelDirectoryPathFromMduFile()
        {
            if (string.IsNullOrEmpty(MduFilePath))
            {
                return Name;
            }

            string modelDirectoryName = Path.GetFileNameWithoutExtension(MduFilePath);
            var modelDir = new DirectoryInfo(MduFilePath);
            while (modelDir != null && modelDir.Name != modelDirectoryName)
            {
                modelDir = modelDir.Parent;
            }

            return modelDir?.Parent == null // should never happen, unless the file-based repository is corrupted
                       ? Path.GetDirectoryName(
                           Path.GetDirectoryName(MduFilePath)) // default behaviour (e.g. model renamed)
                       : modelDir.FullName;
        }

        private string GetMduPathFromDeltaShellPath(string path, string subFoldersFromModelFolder = DirectoryNameConstants.InputDirectoryName)
        {
            string directoryName = path != null
                                       ? Path.GetDirectoryName(path) ?? ""
                                       : "";

            return Path.Combine(directoryName, Name, subFoldersFromModelFolder, Name + FileConstants.MduFileExtension);
        }

        private void RenameSubFilesIfApplicable()
        {
            foreach (KeyValuePair<WaterFlowFMProperty, string> subFile in SubFiles)
            {
                WaterFlowFMProperty waterFlowFMProperty = subFile.Key;

                if (waterFlowFMProperty.GetValueAsString().Equals(subFile.Value))
                {
                    continue;
                }

                if (waterFlowFMProperty.Equals(ModelDefinition.GetModelProperty(KnownProperties.NetFile)))
                {
                    string oldPath = NetFilePath;
                    waterFlowFMProperty.SetValueFromString(subFile.Value);
                    string newPath = NetFilePath;

                    if (!File.Exists(oldPath) ||
                        string.Equals(Path.GetFullPath(oldPath), Path.GetFullPath(newPath),
                                      StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    File.Copy(oldPath, newPath, true);
                    File.Delete(oldPath);
                }
                else
                {
                    waterFlowFMProperty.SetValueFromString(subFile.Value);
                }
            }

            ModelDefinition.ModelName = Name;
        }

        #region Implementation of IDimrModel

        public virtual string LibraryName => "dflowfm";
        public virtual string InputFile => Path.GetFileName(MduSavePath);
        public virtual string DirectoryName => "dflowfm";

        public virtual string GetExporterPath(string directoryName)
        {
            return Path.Combine(directoryName, InputFile == null ? Name + FileConstants.MduFileExtension : Path.GetFileName(InputFile));
        }

        public virtual string DimrExportDirectoryPath => WorkingDirectoryPath;

        public virtual string DimrModelRelativeWorkingDirectory => Path.Combine(DirectoryName, DirectoryNameConstants.InputDirectoryName);
        public virtual string DimrModelRelativeOutputDirectory => Path.Combine(DirectoryName, DirectoryNameConstants.OutputDirectoryName);

        #endregion Implementation of IDimrModel
    }
}