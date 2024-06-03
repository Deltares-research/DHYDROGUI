using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.CopyHandlers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
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
        public string MduFilePath
        {
            get => MduFile.FilePath;
            set => MduFile.FilePath = value;
        }
        
        /// <summary>
        /// Gets the .cache file associated with the model.
        /// </summary>
        public CacheFile CacheFile => cacheFile ?? (cacheFile = new CacheFile(this, new OverwriteCopyHandler()));

        /// <summary>
        /// Gets or sets the delegate for retrieving the working directory path.
        /// The default value is <see cref="DefaultModelSettings.DefaultDeltaShellWorkingDirectory"/>
        /// </summary>
        public Func<string> WorkingDirectoryPathFunc { get; set; } = () => DefaultModelSettings.DefaultDeltaShellWorkingDirectory;

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

        public string DelwaqOutputDirectoryPath { get; set; }

        public string WorkingOutputDirectoryPath =>
            Path.Combine(WorkingDirectoryPath, DirectoryName, DirectoryNameConstants.OutputDirectoryName);
        
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
                               ? Path.Combine(GetModelOutputDirectory(), ModelDefinition.HisFileName)
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
                               ? Path.Combine(GetModelOutputDirectory(), ModelDefinition.MapFileName)
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
                               ? Path.Combine(GetModelOutputDirectory(), ModelDefinition.ClassMapFileName)
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
                        string newFileName = Name + pair.Value;
                        string directoryName = string.IsNullOrEmpty(propertyValue)
                                                   ? string.Empty
                                                   : Path.GetDirectoryName(propertyValue) ?? string.Empty;

                        propertyValue = Path.Combine(directoryName, newFileName);
                    }

                    yield return new KeyValuePair<WaterFlowFMProperty, string>(property, propertyValue);
                }
            }
        }
        
        public event PropertyChangedEventHandler OutputSnappedFeaturesPathPropertyChanged;

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

        protected void OnOutputSnappedFeaturesPathPropertyChanged(string name)
        {
            OutputSnappedFeaturesPathPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Retrieves the directory where the MDU file is located.
        /// </summary>
        /// <returns>A string representing the full path of the MDU directory.</returns>
        public string GetMduDirectory()
        {
            if (string.IsNullOrEmpty(MduFilePath))
            {
                return string.Empty;
            }
            
            return Path.GetDirectoryName(MduFilePath);
        }

        /// <summary>
        /// Retrieves the directory where the results of a model run are stored.
        /// </summary>
        /// <returns>A string representing the full path of the model's output directory.</returns>
        public string GetModelOutputDirectory()
        {
            return Path.Combine(GetModelDirectory(), DirectoryNameConstants.OutputDirectoryName);
        }

        /// <summary>
        /// Retrieves the base directory containing model files and folders.
        /// </summary>
        /// <returns>A string representing the full path of the model's base directory.</returns>
        /// <remarks>
        /// This method searches for relative file references upward in the directory structure first.
        /// If no matching directory is found, it attempts to locate a directory matching the name of the MDU file.
        /// If neither is defined, the directory of the MDU file is returned.
        /// </remarks>
        public string GetModelDirectory()
        {
            if (string.IsNullOrEmpty(MduFilePath))
            {
                return Name;
            }

            string modelDirectory = GetModelDirectoryFromMduFileReferences() ??
                                    GetModelDirectoryFromMduFilePath();

            if (string.IsNullOrEmpty(modelDirectory))
            {
                modelDirectory = GetMduDirectory();
            }

            return modelDirectory;
        }

        private string GetModelDirectoryFromMduFileReferences()
        {
            IEnumerable<string> fileLocations = modelDefinition.FileProperties.SelectMany(x => x.GetFileLocationValues());

            IDirectoryInfo modelDir = fileHierarchyResolver.GetBaseDirectoryFromFileReferences(MduFilePath, fileLocations);
            if (fileSystem.ArePathsEqual(modelDir.FullName, fileSystem.Path.GetDirectoryName(MduFilePath)))
            {
                return null;
            }

            if (modelDir.Name == DirectoryNameConstants.InputDirectoryName)
            {
                modelDir = modelDir.Parent;
            }
            
            return modelDir?.FullName;
        }
        
        private string GetModelDirectoryFromMduFilePath()
        {
            string modelDirName = Path.GetFileNameWithoutExtension(MduFilePath);

            string mduDir = GetMduDirectory();
            var modelDir = new DirectoryInfo(mduDir);

            while (modelDir != null)
            {
                if (modelDir.Name == modelDirName)
                {
                    return modelDir.FullName;
                }

                if (modelDir.Name == DirectoryNameConstants.InputDirectoryName)
                {
                    return modelDir.Parent?.FullName;
                }

                modelDir = modelDir.Parent;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the path for exporting the MDU file.
        /// </summary>
        /// <param name="baseDir">The directory where the MDU file will be exported.</param>
        /// <returns>A string representing the full path of the MDU file.</returns>
        public string GetMduExportPath(string baseDir)
        {
            string mduSubDir = GetMduSubDirectoryFromModelDirectory();
            
            return Path.Combine(baseDir ?? string.Empty, mduSubDir, InputFile);
        }
        
        /// <summary>
        /// Retrieves the path for saving the MDU file.
        /// </summary>
        /// <returns>A string representing the full path of the MDU file.</returns>
        public string GetMduSavePath()
        {
            return GetMduSavePath(GetModelDirectory());
        }

        private string GetMduSavePath(string fromPath)
        {
            string baseDir = Path.GetDirectoryName(fromPath) ?? string.Empty;
            string mduSubDir = GetMduSubDirectoryFromModelDirectory();

            return Path.Combine(baseDir, Name, DirectoryNameConstants.InputDirectoryName, mduSubDir, InputFile);
        }

        private string GetMduSubDirectoryFromModelDirectory()
        {
            if (string.IsNullOrEmpty(MduFilePath))
            {
                return string.Empty;
            }

            string relativeMduPath = FileUtils.GetRelativePath(GetModelDirectory(), MduFilePath);
            string inputDirPart = $@"{DirectoryNameConstants.InputDirectoryName}{Path.DirectorySeparatorChar}";

            if (relativeMduPath.StartsWith(inputDirPart))
            {
                relativeMduPath = relativeMduPath.Substring(inputDirPart.Length);
            }

            return Path.GetDirectoryName(relativeMduPath);
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