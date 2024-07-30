using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Helpers.CopyHandlers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        private CacheFile cacheFile;
        private string outputSnappedFeaturesPath;

        /// <summary>
        /// Gets the MDU file reader/writer.
        /// </summary>
        public MduFile MduFile { get; } = new MduFile();

        /// <summary>
        /// Gets or sets the location of the MDU file.
        /// </summary>
        public virtual string MduFilePath { get; set; }

        public string MduSavePath 
            => GetMduPathFromDeltaShellPath(RecursivelyGetModelDirectoryPathFromMduFile());

        /// <summary>
        /// Gets the base directory containing model files and folders.
        /// </summary>
        public string ModelDirectoryPath 
            => Path.GetDirectoryName(Path.GetDirectoryName(MduFilePath));

        /// <summary>
        /// Gets the directory where the results of a model run are stored.
        /// </summary>
        public string PersistentOutputDirectoryPath 
            => Path.Combine(ModelDirectoryPath, DirectoryNameConstants.OutputDirectoryName);
        
        /// <summary>
        /// Gets the .cache file associated with the model.
        /// </summary>
        public CacheFile CacheFile 
            => cacheFile ?? (cacheFile = new CacheFile(this, new OverwriteCopyHandler()));
        
        /// <summary>
        /// Gets or sets the delegate for retrieving the working directory path.
        /// The default value is <see cref="DefaultModelSettings.DefaultDeltaShellWorkingDirectory"/>
        /// </summary>
        public Func<string> WorkingDirectoryPathFunc { get; set; } = () => Path.Combine(DefaultModelSettings.DefaultDeltaShellWorkingDirectory);

        /// <summary>
        /// For FM the working directory is the same as the working directory from the application,
        /// which can be set by the user by using the options dialog box and this one is never null.
        /// Due to this the DimrRunner will never set a new working directory in the temp folder and
        /// therefore the set should never be executed.
        /// For other plugins the explicit working directory will be used as working directory. However,
        /// this one can be null and then the DimrRunner will create a new working directory and the setter
        /// is then needed.
        /// </summary>
        public virtual string WorkingDirectory
            => Path.Combine(WorkingDirectoryPathFunc(), Name);
        
        public string WorkingOutputDirectoryPath 
            => Path.Combine(WorkingDirectory, DirectoryName, DirectoryNameConstants.OutputDirectoryName);

        public string DelwaqOutputDirectoryName => FileConstants.PrefixDelwaqDirectoryName + Name;
        
        public string DelwaqOutputDirectoryPath { get; set; }

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
                                                        ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile));
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

        private string MapFilePath
        {
            get
            {
                return !string.IsNullOrEmpty(MduFilePath)
                           ? Path.Combine(Path.GetDirectoryName(MduFilePath), ModelDefinition.RelativeMapFilePath)
                           : null;
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

        private string HisFilePath
        {
            get
            {
                return !string.IsNullOrEmpty(MduFilePath)
                           ? Path.Combine(Path.GetDirectoryName(MduFilePath), ModelDefinition.RelativeHisFilePath)
                           : null;
            }
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

                return Name + "_his.nc";
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

                return Name + "_map.nc";
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
        
        public string HydFilePath
        {
            get
            {
                var projectName = Path.GetFileNameWithoutExtension(MduFilePath);
                return Path.Combine(WorkingDirectory, string.Format("DFM_DELWAQ_{0}", projectName),String.Format("{0}.hyd", projectName));
            }
        }

        public bool HydFileOutput { get; set; } // always on ??

        public event PropertyChangedEventHandler OutputSnappedFeaturesPathPropertyChanged;

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
                    { KnownProperties.NetFile, NetFile.FullExtension },
                    { KnownProperties.ExtForceFile, ExtForceFile.Extension },
                    { KnownProperties.BndExtForceFile, ExtForceFile.Extension },
                    { KnownProperties.LandBoundaryFile, MduFile.LandBoundariesExtension },
                    { KnownProperties.ThinDamFile, MduFile.ThinDamExtension },
                    { KnownProperties.FixedWeirFile, MduFile.FixedWeirExtension },
                    { KnownProperties.StructuresFile, MduFile.StructuresExtension },
                    { KnownProperties.ObsFile, MduFile.ObsExtension },
                    { KnownProperties.ObsCrsFile, MduFile.ObsCrossExtension },
                    { KnownProperties.DryPointsFile, MduFile.DryPointExtension }
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
                        ((modelDefinitionName + pair.Value).Equals(currentFileName, StringComparison.InvariantCultureIgnoreCase) && pair.Key != KnownProperties.NetFile))
                    {
                        propertyValue = Name + pair.Value;
                    }

                    yield return new KeyValuePair<WaterFlowFMProperty, string>(property, propertyValue);
                }
            }
        }

        protected void OnOutputSnappedFeaturesPathPropertyChanged(string name)
        {
            OutputSnappedFeaturesPathPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string GetMduPathFromDeltaShellPath(string path, string subFoldersFromModelFolder = DirectoryNameConstants.InputDirectoryName)
        {
            var directoryName = path != null
                                    ? Path.GetDirectoryName(path) ?? ""
                                    : "";

            // dsproj_data/<model name>/<model name>.mdu
            return Path.Combine(directoryName, Name, subFoldersFromModelFolder, Name + ".mdu");
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
    }
}