using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.NetCdf;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils.Extensions;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public sealed class BranchFile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BranchFile));
        private static readonly Version version = new Version(2, 0);

        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchFile"/> class.
        /// </summary>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="fileSystem"/> is <c>null</c>.</exception>
        public BranchFile(IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));
            
            this.fileSystem = fileSystem;
        }

        public enum BranchType
        {
            Channel = 0, SewerConnection = 1, Pipe = 2
        }

        /// <summary>
        /// Writes the provided branches to the branches.gui file at the specified path.
        /// </summary>
        /// <param name="filePath"> The file path to the branches.gui file. </param>
        /// <param name="branches"> The branches to write. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="branches"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c> or white space.
        /// </exception>
        public void Write(string filePath, IEnumerable<IBranch> branches)
        {
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNull(branches, nameof(branches));

            var iniData = new IniData();
            iniData.AddSection(CreateGeneralIniSection());

            foreach (var branch in branches)
            {
                var properties = GetBranchProperties(branch);

                var iniSection = new IniSection(NetworkRegion.BranchIniHeader);
                iniSection.AddPropertyFromConfiguration(NetworkRegion.BranchId, properties.Name);
                
                iniSection.AddPropertyFromConfiguration(NetworkRegion.BranchType, ((int)properties.BranchType).ToString());
                iniSection.AddPropertyFromConfiguration(NetworkRegion.IsLengthCustom, properties.IsCustomLength);

                if (properties.BranchType == BranchType.Channel && !properties.IsCustomLength)
                {
                    // do not write channels without custom length (no special properties need to be saved)
                    continue;
                }

                if (properties.SourceCompartmentName != null)
                {
                    iniSection.AddPropertyFromConfiguration(NetworkRegion.SourceCompartmentName, properties.SourceCompartmentName);
                }

                if (properties.TargetCompartmentName != null)
                {
                    iniSection.AddPropertyFromConfiguration(NetworkRegion.TargetCompartmentName, properties.TargetCompartmentName);
                }

                if (properties.BranchType == BranchType.Pipe)
                {
                    iniSection.AddPropertyFromConfiguration(NetworkRegion.BranchMaterial, (int) properties.Material);
                }

                iniData.AddSection(iniSection);
            }

            WriteIniFile(iniData, filePath);
        }
        
        private static IniSection CreateGeneralIniSection()
        {
            return GeneralRegionGenerator.GenerateGeneralRegion(
                version.Major,
                version.Minor,
                GeneralRegion.FileTypeName.Branches);
        }
        
        public static BranchProperties GetBranchProperties(IBranch branch)
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

        private void WriteIniFile(IniData iniData, string path)
        {
            log.InfoFormat(Resources.BranchFile_WriteIniFile_Writing_branches_to__0__, path);
            
            using (FileSystemStream fileStream = fileSystem.File.Open(path, FileMode.Create))
            {
                var formatter = new IniFormatter { Configuration = { PropertyIndentationLevel = 4 } };
                formatter.Format(iniData, fileStream);
            }
        }

        /// <summary>
        /// Reads the branch information from the branches.gui file.
        /// </summary>
        /// <param name="filePath"> The file path to the branches.gui file. </param>
        /// <param name="netFilePath"> The file path to the network file. </param>
        /// <param name="logHandler"> The log handler to log messages with. </param>
        /// <returns>
        /// A collection of the branch properties that were collected from file.
        /// Returns an empty collection when:
        /// <list type="bullet">
        /// <item><description> The file does not contain any branch INI sections. </description></item>
        /// <item><description> The file misses a fileVersion in the general INI section. </description></item>
        /// <item><description> The file contains an invalid fileVersion in the general INI section.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="filePath"/> or <paramref name="netFilePath"/> is <c>null</c> or white space.
        /// </exception>
        public IList<BranchProperties> Read(string filePath, string netFilePath, ILogHandler logHandler)
        {
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNullOrWhiteSpace(netFilePath, nameof(netFilePath));
            Ensure.NotNull(logHandler, nameof(logHandler));
            
            IniData iniData = ReadIniFile(filePath);
            
            var groupedIniSections = iniData.Sections.ToGroupedDictionary(iniSection => iniSection.Name);
            
            if (groupedIniSections.TryGetValue(GeneralRegion.IniHeader, out IEnumerable<IniSection> generalIniSections))
            {
                if (!ValidateGeneralIniSection(generalIniSections.First(), logHandler))
                {
                    return new List<BranchProperties>();
                }
            }
            else
            {
                logHandler.ReportWarning(Resources.BranchesGui_file_does_not_contain_a_general_section);
            }
            
            if (!groupedIniSections.TryGetValue(NetworkRegion.BranchIniHeader, out IEnumerable<IniSection> branchIniSections))
            {
                return new List<BranchProperties>();
            }

            return ReadBranchProperties(branchIniSections, netFilePath).Values.ToList();
        }
        
        private IniData ReadIniFile(string path)
        {
            log.InfoFormat(Resources.BranchFile_ReadIniFile_Reading_branches_from__0__, path);
            
            using (FileSystemStream fileStream = fileSystem.File.OpenRead(path))
            {
                var parser = new IniParser();
                return parser.Parse(fileStream);
            }
        }

        private IDictionary<string, BranchProperties> ReadBranchProperties(IEnumerable<IniSection> branchIniSections, string netFilePath)
        {
            var propertiesPerBranch = new Dictionary<string, BranchProperties>();

            foreach (var iniSection in branchIniSections)
            {
                var branchProperties = new BranchProperties
                {
                    Name = iniSection.ReadProperty<string>(NetworkRegion.BranchId.Key),
                    BranchType = iniSection.GetPropertyValue<BranchType>(NetworkRegion.BranchType.Key),
                    IsCustomLength = iniSection.ReadProperty<bool>(NetworkRegion.IsLengthCustom.Key, true),
                    Material = iniSection.GetPropertyValue<SewerProfileMapping.SewerProfileMaterial>(NetworkRegion.BranchMaterial.Key),
                    SourceCompartmentName = iniSection.ReadProperty<string>(NetworkRegion.SourceCompartmentName.Key, true),
                    TargetCompartmentName = iniSection.ReadProperty<string>(NetworkRegion.TargetCompartmentName.Key, true)
                };
                propertiesPerBranch.Add(branchProperties.Name, branchProperties);
            }
            
            if (!fileSystem.File.Exists(netFilePath)) return propertiesPerBranch;
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
                                     .GroupBy(y => y.index / UGridFile.IDS_SIZE)
                                     .Select(y => new string(y.Select(z => z.character).ToArray()).Trim()))
                    .ToArray();
                var branchTypeValues = file.Read(branchTypes).Cast<int>().ToArray();
                if (branchIdValues.Length != branchTypeValues.Length) return propertiesPerBranch;
                for (int i = 0; i < branchIdValues.Length; i++)
                {
                    if (propertiesPerBranch.TryGetValue(branchIdValues[i], out BranchProperties branchProperty))
                    {
                        branchProperty.WaterType = ConvertBranchTypeToWaterType(branchTypeValues[i]);
                    }
                }

            }
            finally
            {
                file.Close();
            }

            return propertiesPerBranch;
        }

        private static bool ValidateGeneralIniSection(IniSection generalIniSection, ILogHandler logHandler)
        {
            string fileVersionStr = generalIniSection.GetPropertyValueWithOptionalDefaultValue(GeneralRegion.FileVersion.Key);

            if (string.IsNullOrWhiteSpace(fileVersionStr))
            {
                logHandler.ReportError(Resources.File_version_in_general_section_is_empty);
                return false;
            }

            if (!Version.TryParse(fileVersionStr, out Version fileVersion))
            {
                logHandler.ReportError(string.Format(Resources.File_version_in_general_section_is_invalid_0_, fileVersionStr));
                return false;
            }

            if (fileVersion != version)
            {
                logHandler.ReportError(string.Format(Resources.File_version_in_general_section_is_not_supported_0_, fileVersionStr));
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

        private static void SetSewerConnectionProperties(BranchProperties branchProperties, ISewerConnection pipe)
        {
            branchProperties.BranchType = BranchType.SewerConnection;
            branchProperties.WaterType = pipe.WaterType;
            branchProperties.SourceCompartmentName = pipe.SourceCompartmentName;
            branchProperties.TargetCompartmentName = pipe.TargetCompartmentName;
        }
    }
}
