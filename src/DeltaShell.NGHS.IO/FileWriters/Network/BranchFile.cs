using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils.Extensions;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public static class BranchFile
    {
        private static readonly Version version = new Version(2, 0);
        
        public enum BranchType
        {
            Channel = 0, SewerConnection = 1, Pipe = 2
        }

        public static BranchProperties GetBranchProperties(this IBranch branch)
        {
            var branchProperties = new BranchProperties
            {
                Name = branch.Name,
                IsCustomLength = branch.IsLengthCustom
            };

            switch (branch)
            {
                case Channel _:
                    branchProperties.BranchType = BranchType.Channel;
                    break;
                case IPipe pipe:
                    SetSewerConnectionProperties(branchProperties, pipe);
                    branchProperties.BranchType = BranchType.Pipe;
                    branchProperties.Material = pipe.Material;
                    break;
                case ISewerConnection sewerConnection:
                    SetSewerConnectionProperties(branchProperties, sewerConnection);
                    break;
            }

            return branchProperties;
        }

        /// <summary>
        /// Writes the provided branches to the branches.gui file at the specified path.
        /// </summary>
        /// <param name="filePath"> The file path to the branches.gui file. </param>
        /// <param name="branches"> The branches to write. </param>
        /// <param name="delftIniWriter"> The Delft INI writer. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="branches"/> or <paramref name="delftIniWriter"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c> or white space.
        /// </exception>
        public static void Write(string filePath, IEnumerable<IBranch> branches, IDelftIniWriter delftIniWriter)
        {
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNull(branches, nameof(branches));
            Ensure.NotNull(delftIniWriter, nameof(delftIniWriter));
            
            var categories = new List<DelftIniCategory>
            {
                CreateGeneralCategory()
            };

            foreach (var branch in branches)
            {
                var properties = branch.GetBranchProperties();

                var iniCategory = new DelftIniCategory(NetworkRegion.BranchIniHeader);
                iniCategory.AddProperty(NetworkRegion.BranchId, properties.Name);
                
                iniCategory.AddProperty(NetworkRegion.BranchType, ((int)properties.BranchType).ToString());
                iniCategory.AddProperty(NetworkRegion.IsLengthCustom, properties.IsCustomLength);

                if (properties.BranchType == BranchType.Channel && !properties.IsCustomLength)
                {
                    // do not write channels without custom length (no special properties need to be saved)
                    continue;
                }

                if (properties.SourceCompartmentName != null)
                {
                    iniCategory.AddProperty(NetworkRegion.SourceCompartmentName, properties.SourceCompartmentName);
                }

                if (properties.TargetCompartmentName != null)
                {
                    iniCategory.AddProperty(NetworkRegion.TargetCompartmentName, properties.TargetCompartmentName);
                }

                if (properties.BranchType == BranchType.Pipe)
                {
                    iniCategory.AddProperty(NetworkRegion.BranchMaterial, (int) properties.Material);
                }

                categories.Add(iniCategory);
            }

            // write branch file
            delftIniWriter.WriteDelftIniFile(categories, filePath);
        }

        private static DelftIniCategory CreateGeneralCategory()
        {
            return GeneralRegionGenerator.GenerateGeneralRegion(
                version.Major,
                version.Minor,
                GeneralRegion.FileTypeName.Branches);
        }

        /// <summary>
        /// Reads the branch information from the branches.gui file.
        /// </summary>
        /// <param name="filePath"> The file path to the branches.gui file. </param>
        /// <param name="netFilePath"> The file path to the network file. </param>
        /// <param name="delftIniReader"> The Delft INI reader. </param>
        /// <param name="logHandler"> The log handler to log messages with. </param>
        /// <returns>
        /// A collection of the branch properties that were collected from file.
        /// Returns an empty collection when:
        /// <list type="bullet">
        /// <item><description> The file does not contain any branch categories. </description></item>
        /// <item><description> The file misses a fileVersion in the general category. </description></item>
        /// <item><description> The file contains an invalid fileVersion in the general category.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="delftIniReader"/> or <paramref name="logHandler"/> is <c>null</c>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="filePath"/> or <paramref name="netFilePath"/> is <c>null</c> or white space.
        /// </exception>
        public static IList<BranchProperties> Read(string filePath, string netFilePath, IDelftIniReader delftIniReader, ILogHandler logHandler)
        {
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNullOrWhiteSpace(netFilePath, nameof(netFilePath));
            Ensure.NotNull(delftIniReader, nameof(delftIniReader));
            Ensure.NotNull(logHandler, nameof(logHandler));
            
            IList<DelftIniCategory> categories = delftIniReader.ReadDelftIniFile(filePath);
            
            Dictionary<string, IEnumerable<DelftIniCategory>> groupedCategories = categories.ToGroupedDictionary(category => category.Name);
            if (groupedCategories.TryGetValue(GeneralRegion.IniHeader, out IEnumerable<DelftIniCategory> generalCategories))
            {
                if (!ValidateGeneralCategory(generalCategories.First(), logHandler))
                {
                    return new List<BranchProperties>();
                }
            }
            else
            {
                logHandler.ReportWarning(Resources.BranchesGui_file_does_not_contain_a_general_section);
            }
            
            if (!groupedCategories.TryGetValue(NetworkRegion.BranchIniHeader, out IEnumerable<DelftIniCategory> branchCategories))
            {
                return new List<BranchProperties>();
            }

            return ReadBranchProperties(branchCategories, netFilePath);
        }

        private static IList<BranchProperties> ReadBranchProperties(IEnumerable<DelftIniCategory> branchCategories, string netFilePath)
        {
            var propertiesPerBranch = new List<BranchProperties>();

            foreach (var category in branchCategories)
            {
                var branchProperties = new BranchProperties
                {
                    Name = category.ReadProperty<string>(NetworkRegion.BranchId.Key),
                    BranchType = category.GetEnumValueByKey<BranchType>(NetworkRegion.BranchType.Key),
                    IsCustomLength = category.ReadProperty<bool>(NetworkRegion.IsLengthCustom.Key, true),
                    /* WaterType = category.GetEnumValueByKey<SewerConnectionWaterType>(KnownPropertyNames.WaterType),*/
                    Material = category.GetEnumValueByKey<SewerProfileMapping.SewerProfileMaterial>(NetworkRegion.BranchMaterial.Key),
                    SourceCompartmentName = category.ReadProperty<string>(NetworkRegion.SourceCompartmentName.Key, true),
                    TargetCompartmentName = category.ReadProperty<string>(NetworkRegion.TargetCompartmentName.Key, true)
                };
                propertiesPerBranch.Add(branchProperties);
            }
            
            if (!File.Exists(netFilePath)) return propertiesPerBranch;
            var file = NetCdfFile.OpenExisting(netFilePath);
            try
            {
                var branchIds = file.GetVariableByName($"network_{UGridConstants.Naming.BranchIds}");
                if (branchIds == null) return propertiesPerBranch;
                var branchTypes = file.GetVariableByName($"network_{UGridConstants.Naming.BranchType}");
                if (branchTypes == null) return propertiesPerBranch;

                var branchIdValues = file.Read(branchIds)
                    .Cast<char[]>()
                    .SelectMany(s => s.Select((character, index) => new { character, index })
                                     .GroupBy(y => y.index / UGridFileHelper.IdsSize)
                                     .Select(y => new string(y.Select(z => z.character).ToArray()).Trim()))
                    .ToArray();
                var branchTypeValues = file.Read(branchTypes).Cast<int>().ToArray();
                if (branchIdValues.Length != branchTypeValues.Length) return propertiesPerBranch;
                for (int i = 0; i < branchIdValues.Length; i++)
                {
                    var branchProperty = propertiesPerBranch.FirstOrDefault(bp => bp.Name == branchIdValues[i]);
                    if (branchProperty == null) continue;
                    branchProperty.WaterType = ConvertBranchTypeToWaterType(branchTypeValues[i]);
                }

            }
            finally
            {
                file.Close();
            }

            return propertiesPerBranch;
        }

        private static bool ValidateGeneralCategory(IDelftIniCategory generalCategory, ILogHandler logHandler)
        {
            string fileVersionStr = generalCategory.GetPropertyValue(GeneralRegion.FileVersion.Key);

            if (string.IsNullOrWhiteSpace(fileVersionStr))
            {
                logHandler.ReportError(Resources.File_version_in_general_category_is_empty);
                return false;
            }

            if (!Version.TryParse(fileVersionStr, out Version fileVersion))
            {
                logHandler.ReportError(string.Format(Resources.File_version_in_general_category_is_invalid_0_, fileVersionStr));
                return false;
            }

            if (fileVersion != version)
            {
                logHandler.ReportError(string.Format(Resources.File_version_in_general_category_is_not_supported_0_, fileVersionStr));
                return false;
            }

            return true;
        }

        private static SewerConnectionWaterType ConvertBranchTypeToWaterType(int branchTypeValue)
        {

            switch (branchTypeValue)
            {
                case (int) Grid.BranchType.DryWeatherFlow:
                    return SewerConnectionWaterType.DryWater;
                case (int) Grid.BranchType.MixedFlow:
                    return SewerConnectionWaterType.Combined;
                case (int) Grid.BranchType.StormWaterFlow:
                    return SewerConnectionWaterType.StormWater;
                default:
                    return SewerConnectionWaterType.None;

                
            }
        }

        private static T GetEnumValueByKey<T>(this IDelftIniCategory category, string propertyKey)
        {
            return (T) Enum.Parse(typeof(T), category.ReadProperty<int>(propertyKey, true).ToString());
        }

        private static void SetSewerConnectionProperties(BranchProperties branchProperties, ISewerConnection pipe)
        {
            branchProperties.BranchType = BranchType.SewerConnection;
            branchProperties.WaterType = pipe.WaterType;
            branchProperties.SourceCompartmentName = pipe.SourceCompartmentName;
            branchProperties.TargetCompartmentName = pipe.TargetCompartmentName;
        }
    }
}
