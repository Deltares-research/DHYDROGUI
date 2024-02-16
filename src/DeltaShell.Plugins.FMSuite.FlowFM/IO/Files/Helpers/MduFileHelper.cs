using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    public static class MduFileHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MduFileHelper));

        public static string GetSubfilePath(string mduFilePath, WaterFlowFMProperty property)
        {
            if (IsFileValued(property))
            {
                string fileName = property.GetValueAsString();
                return GetCombinedPath(mduFilePath, fileName);
            }

            return null;
        }

        public static IList<string> GetMultipleSubfilePath(string mduFilePath, WaterFlowFMProperty property)
        {
            var filePathList = new List<string>();
            if (IsMultipleFileValued(property))
            {
                var valueList = property.Value as IList<string>; /* We made sure is not null in the above check */
                return valueList
                       .Select(fileName => GetCombinedPath(mduFilePath, fileName))
                       .Where(fp => !string.IsNullOrEmpty(fp)).ToList();
            }

            /*If it's not multiple file valued at least get one value if possible.*/
            string simpleFilePath = GetSubfilePath(mduFilePath, property);
            if (simpleFilePath != null)
            {
                filePathList.Add(simpleFilePath);
            }

            return filePathList.Select(fp => fp.Replace("/", @"\")).ToList();
        }

        /// <summary>
        /// Checks if the filePath is a valid filePath for writing a feature file. Whenever filePath contains
        /// a navigation to a parent folder, then it is considered as not valid and this method will return false.
        /// </summary>
        /// <param name="filePath"> The filePath as a string to check. </param>
        /// <returns> </returns>
        public static bool IsValidFilePath(string filePath, string mduFilePath)
        {
            var wrongValues = new List<string>();

            if (Path.IsPathRooted(filePath)) // Absolute path
            {
                filePath = FileUtils.GetRelativePath(Path.GetDirectoryName(mduFilePath), filePath, true);
            }

            var regexSlashes = new Regex("/{3,}");
            Match matchSlashes = regexSlashes.Match(filePath);
            if (matchSlashes.Success)
            {
                wrongValues.Add(matchSlashes.Value);
            }

            var regexParentFolder = new Regex(@"\.{2,}");
            Match matchParentFolder = regexParentFolder.Match(filePath);
            if (matchParentFolder.Success)
            {
                wrongValues.Add(matchParentFolder.Value);
            }

            bool valid = wrongValues.Count == 0;
            if (!valid)
            {
                Log.WarnFormat(
                    "Features are not saved to file at \'{0}\', because the group name is invalid. Remove any occurences of \'" +
                    string.Join("\', \'", wrongValues) + "\' from these group names.", filePath);
            }

            return valid;
        }

        /// <summary>
        /// Updates the GroupName and IsDefaultGroup properties of all features where needed.
        /// </summary>
        /// <typeparam name="TFeat"> The class type of the features. </typeparam>
        /// <param name="features"> The features for which properties are possibly being changed. </param>
        /// <param name="extension"> The extension that belongs to files that are corresponding to type TFeat. </param>
        /// <param name="defaultGroupName"> The default group name, which is the name of the FM model that the features belong to. </param>
        public static void UpdateFeatures<TFeat>(IList<TFeat> features, string extension, string defaultGroupName)
        {
            UpdateIsDefaultGroupFlag(features, extension, defaultGroupName);
            UpdateDefaultNamedFeatures(features, extension, defaultGroupName);
            ReplaceUndesiredCharactersInGroupNames(features, extension, defaultGroupName);
        }

        /// <summary>
        /// Returns unique group names according to Windows. Windows does not make a distinction between uppercase and lowercase
        /// named
        /// files and directories. This method checks if a file with the same name already exists for all feature's group name. If
        /// so,
        /// all group names will be changed to the file's name
        /// </summary>
        /// <typeparam name="TFeat"> The class type of the features. </typeparam>
        /// <param name="targetMduFilePath"> The file path that points to the location of an FM model's MDU file. </param>
        /// <param name="features"> The features that are to be written. </param>
        /// <param name="extension"> The extension that corresponds to the type of features. </param>
        /// <param name="alternativeExtensions">
        /// If a feature file has an alternative extension that it can save to, then this
        /// value is not null.
        /// </param>
        /// <returns> Unique relative file paths, according to Windows. </returns>
        public static string[] GetUniqueFilePathsForWindows<TFeat>(string targetMduFilePath, IList<TFeat> features,
                                                                   string extension,
                                                                   params string[] alternativeExtensions)
        {
            string mduDirectory = Path.GetFullPath(Path.GetDirectoryName(targetMduFilePath));
            string[] groupNames = features.OfType<IGroupableFeature>().Select(f => f.GroupName.Replace(@"\", "/"))
                                          .Distinct().ToArray();

            // Checking for existing files in the project folder
            groupNames = groupNames.Select(gn =>
            {
                string filePath = Path.Combine(mduDirectory,
                                               GetFilePathWithExtension(gn, extension, alternativeExtensions));
                string existingFilePath;
                try
                {
                    existingFilePath = Directory.GetFiles(Path.GetDirectoryName(filePath))
                                                .FirstOrDefault(
                                                    fp => fp.ToLowerInvariant()
                                                            .EndsWith(filePath.Replace("/", @"\")
                                                                              .ToLowerInvariant()));
                }
                catch (DirectoryNotFoundException)
                {
                    return GetFilePathWithExtension(gn, extension, alternativeExtensions);
                }

                if (File.Exists(filePath) && Path.GetFileName(existingFilePath) != Path.GetFileName(filePath))
                {
                    // If a file already exists that only differs by capital letters, give a warning and return this file name as a group name.
                    Log.WarnFormat(Resources.MduFileHelper_GetUniqueFilePathsForWindows_File_Already_Exists,
                                   existingFilePath, gn);
                    return FileUtils.GetRelativePath(mduDirectory, existingFilePath, true);
                }

                return GetFilePathWithExtension(gn, extension, alternativeExtensions);
            }).Distinct().ToArray();

            groupNames = groupNames.Select(gn =>
                {
                    string fileExtension = Path.GetExtension(extension);
                    if (fileExtension != null && gn.EndsWith(fileExtension) &&
                        !gn.EndsWith(extension))
                    {
                        return gn.Replace(fileExtension, extension);
                    }

                    return gn;
                }
            ).Distinct().ToArray();

            // Whenever two or more group names are equal, differing only by capital letters, then log a warning message 
            // that features are written to another file.
            for (var i = 1; i < groupNames.Length; i++)
            {
                string leadingValue = groupNames.Take(i).Where(
                    v => v.ToLowerInvariant() == groupNames[i].ToLowerInvariant() &&
                         v != groupNames[i]).Distinct().FirstOrDefault();
                if (leadingValue != null)
                {
                    Log.WarnFormat(
                        Resources
                            .MduFileHelper_GetUniqueFilePathsForWindows_Features_With_Group_Name___Are_Written_To_File___,
                        groupNames[i],
                        Path.Combine(mduDirectory,
                                     GetFilePathWithExtension(leadingValue, extension, alternativeExtensions))
                            .Replace("/", @"\"));
                    groupNames[i] = leadingValue;
                }
            }

            return groupNames.Distinct().ToArray();
        }

        public static bool IsMultipleFileValued(WaterFlowFMProperty property)
        {
            if (property.PropertyDefinition.IsMultipleFile
                && property.PropertyDefinition.MduPropertyName.ToLower().EndsWith("file"))
            {
                if (property.PropertyDefinition.IsDefinedInSchema)
                {
                    return property.PropertyDefinition.DataType == typeof(IList<string>);
                }

                return property.Value is IList<string>;
            }

            return false;
        }

        public static bool IsFileValued(WaterFlowFMProperty property)
        {
            if (property.PropertyDefinition.IsDefinedInSchema)
            {
                return property.PropertyDefinition.IsFile;
            }

            return property.Value is string && property.PropertyDefinition.MduPropertyName.ToLower().EndsWith("file");
        }

        /// <summary>
        /// This method copies feature files at the given <paramref name="featureGroupNames"/> to locations in the mdu folder if
        /// the file paths point
        /// to a location outside of the mdu folder. In case a file path has '../' in its path, the path is replaced by its
        /// absolute path.
        /// The corresponding ModelProperty is updated for every file path change.
        /// If the target file already exists, the file is overwritten.
        /// </summary>
        /// <param name="featureGroupNames"> The group names of all features, retrieved from the mdu file of the FM Model. </param>
        /// <param name="mduFilePath"> The file path of the mdu file. </param>
        /// <param name="modelDefinition"> The model definition of the FM Model. </param>
        /// <param name="propertyKey"> The key that corresponds to the type of file that is being read. </param>
        public static void CopyFilesToMduFolderIfNeeded(IList<string> featureGroupNames,
                                                        string mduFilePath,
                                                        WaterFlowFMModelDefinition modelDefinition,
                                                        string propertyKey)
        {
            string mduDirectory = Path.GetDirectoryName(Path.GetFullPath(mduFilePath));
            for (var i = 0; i < featureGroupNames.Count; i++)
            {
                string filePath = Path.GetFullPath(Path.Combine(mduDirectory, featureGroupNames[i]));
                Match isOutsideMduFolderMatch = new Regex(@"\.{2,}").Match(FileUtils.GetRelativePath(mduDirectory, filePath, true));

                // File is situated inside mdu-folder or in a subfolder
                if (!isOutsideMduFolderMatch.Success || propertyKey == KnownProperties.StructuresFile)
                {
                    featureGroupNames[i] = filePath;
                }
                else // File is situated outside of mdu-folder
                {
                    string newFilePath = Path.Combine(mduDirectory, Path.GetFileName(filePath));
                    if (File.Exists(newFilePath))
                    {
                        Log.InfoFormat(Resources.MduFile_CopyFilesToProjectFolderIfNeeded_CopyingFileOverwritesFileThatAtNewLocation,
                                       filePath, newFilePath);
                    }
                    else
                    {
                        Log.InfoFormat(Resources.MduFile_CopyFilesToProjectFolderIfNeeded_CopiedFileFrom_0_to_1_BecauseTheFileExistedOutsideOfTheProjectFolder,
                                       filePath, newFilePath, modelDefinition.ModelName);
                    }

                    File.Copy(filePath, newFilePath, true);
                    featureGroupNames[i] = newFilePath;
                }
            }

            featureGroupNames.RemoveAllWhere(fp => fp == null);
            IEnumerable<string> relativePaths = featureGroupNames.Select(fp => FileUtils.GetRelativePath(mduDirectory, fp, true));
            modelDefinition.GetModelProperty(propertyKey).SetValueFromStrings(relativePaths);
        }

        private static void UpdateIsDefaultGroupFlag<TFeat>(IList<TFeat> features, string extension,
                                                            string defaultGroupName)
        {
            features.OfType<IGroupableFeature>()
                    .Where(f => f.IsDefaultGroup && !f.HasDefaultGroupName(extension, defaultGroupName))
                    .ForEach(f => f.IsDefaultGroup = false);
        }

        private static void UpdateDefaultNamedFeatures<TFeat>(IList<TFeat> features, string extension,
                                                              string defaultGroupName)
        {
            features.OfType<IGroupableFeature>()
                    .Where(f => string.IsNullOrEmpty(f.GroupName) || f.GroupName.Equals(defaultGroupName) ||
                                f.IsDefaultGroup)
                    .ForEach(f =>
                    {
                        f.GroupName = string.Concat(defaultGroupName, extension);
                        f.IsDefaultGroup = true;
                    });
        }

        private static void ReplaceUndesiredCharactersInGroupNames<TFeat>(
            IList<TFeat> features, string extension, string defaultGroupName)
        {
            features.OfType<IGroupableFeature>().Where(f => f.GroupName.Contains(" ") || f.GroupName.Contains(@"\"))
                    .ForEach(f =>
                    {
                        f.GroupName = f.GroupName.Replace(" ", "_").Replace(@"\", "/");
                        if (f.HasDefaultGroupName(extension, defaultGroupName))
                        {
                            f.IsDefaultGroup = true;
                        }
                    });
        }

        private static string GetFilePathWithExtension(string filePath, string extension,
                                                       params string[] alternativeExtensions)
        {
            if (extension != null && filePath.EndsWith(Path.GetExtension(extension))
                || alternativeExtensions != null && alternativeExtensions.Any(filePath.EndsWith))
            {
                return filePath;
            }

            return filePath + (extension != null && filePath.EndsWith(extension) ? string.Empty : extension);
        }

        internal static string GetCombinedPath(string mduFilePath, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                return FileUtils.PathIsRelative(fileName)
                           ? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(mduFilePath), fileName))
                           : fileName;
            }

            return null;
        }
    }
}