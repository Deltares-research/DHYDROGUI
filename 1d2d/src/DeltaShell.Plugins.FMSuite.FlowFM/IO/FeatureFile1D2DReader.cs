using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileReaders.Retention;
using DeltaShell.NGHS.IO.FileReaders.Roughness;
using DeltaShell.NGHS.IO.FileReaders.Structure;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.InitialField;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Reader for 1D2D features responsible for reading the feature files and setting the data on the models.
    /// </summary>
    public sealed class FeatureFile1D2DReader
    {
        private readonly ILog log = LogManager.GetLogger(typeof(FeatureFile1D2DReader));
        private readonly InitialFieldFile initialFieldFile;

        private WaterFlowFMModelDefinition modelDefinition;
        private IHydroNetwork hydroNetwork;
        private IEventedList<RoughnessSection> roughnessSections;
        private IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions;
        private IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions;
        private string mduFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureFile1D2DReader"/> class.
        /// </summary>
        /// <param name="initialFieldFile">Provides methods for reading and writing initial field files (*.ini).</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="initialFieldFile"/> is <c>null</c>.</exception>
        public FeatureFile1D2DReader(InitialFieldFile initialFieldFile)
        {
            Ensure.NotNull(initialFieldFile, nameof(initialFieldFile));
            this.initialFieldFile = initialFieldFile;
        }

        /// <summary>
        /// Reads the features and sets the data on the given models.
        /// </summary>
        /// <param name="mduFilePath"> The MDU file path. </param>
        /// <param name="modelDefinition"> The loaded model definition. </param>
        /// <param name="hydroNetwork"> The hydro network. </param>
        /// <param name="roughnessSections"> The roughness sections. </param>
        /// <param name="channelFrictionDefinitions"> The channel friction definitions. </param>
        /// <param name="channelInitialConditionDefinitions"> The channel initial condition definitions. </param>
        /// <param name="report1DOr2DFeatureRead">Function to report progress on</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <c>null</c>.</exception>
        public void Read1D2DFeatures(string mduFilePath,
                                     WaterFlowFMModelDefinition modelDefinition,
                                     IHydroNetwork hydroNetwork,
                                     IEventedList<RoughnessSection> roughnessSections,
                                     IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions,
                                     IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions,
                                     Action<string> report1DOr2DFeatureRead = null)
        {
            Ensure.NotNull(mduFilePath, nameof(mduFilePath));
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));
            Ensure.NotNull(hydroNetwork, nameof(hydroNetwork));
            Ensure.NotNull(roughnessSections, nameof(roughnessSections));
            Ensure.NotNull(channelFrictionDefinitions, nameof(channelFrictionDefinitions));
            Ensure.NotNull(channelInitialConditionDefinitions, nameof(channelInitialConditionDefinitions));

            this.mduFilePath = mduFilePath;
            this.modelDefinition = modelDefinition;
            this.hydroNetwork = hydroNetwork;
            this.roughnessSections = roughnessSections;
            this.channelFrictionDefinitions = channelFrictionDefinitions;
            this.channelInitialConditionDefinitions = channelInitialConditionDefinitions;

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_routes);
            ReadRoutesFile();

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_cross_section_definitions);
            ICrossSectionDefinition[] definitions = ReadCrossSectionFiles();

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_structures);
            ReadStructuresFiles(definitions);

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_observation_points);
            ReadObservationPointsFiles();

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_roughness);
            ReadRoughnessFiles();

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_retentions);
            IEnumerable<IRetention> retentions = ReadRetentionsFromFile();
            foreach (IRetention retention in retentions)
            {
                retention.Branch.BranchFeatures.Add(retention);
            }

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_initial_conditions);
            ReadInitialConditionFiles();
        }

        private IEnumerable<IRetention> ReadRetentionsFromFile()
        {
            string netFilePath = MduFileHelper.GetSubfilePath(mduFilePath, modelDefinition.GetModelProperty(KnownProperties.NetFile));
            if (!File.Exists(netFilePath))
            {
                return Enumerable.Empty<IRetention>();
            }

            string storageNodeFilePath = MduFileHelper.GetSubfilePath(mduFilePath, modelDefinition.GetModelProperty(KnownProperties.StorageNodeFile));
            if (!File.Exists(storageNodeFilePath))
            {
                return Enumerable.Empty<IRetention>();
            }

            IList<IniSection> iniSections = new IniReader().ReadIniFile(storageNodeFilePath);
            if (iniSections.Count == 0)
            {
                log.Warn(string.Format(Resources.FeatureFile1D2DReader_ReadRetentionsFile_Could_not_read_file__0__properly__it_seems_empty, storageNodeFilePath));
                return Enumerable.Empty<IRetention>();
            }

            return RetentionSectionParser.ParseIniSections(iniSections, hydroNetwork);
        }

        private ICrossSectionDefinition[] ReadCrossSectionFiles()
        {
            string crLocFile = modelDefinition.GetModelProperty(KnownProperties.CrossLocFile).GetValueAsString();
            crLocFile = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, crLocFile);

            string crDefFile = modelDefinition.GetModelProperty(KnownProperties.CrossDefFile).GetValueAsString();
            crDefFile = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, crDefFile);
            if (!File.Exists(crDefFile))
            {
                return Array.Empty<ICrossSectionDefinition>();
            }

            return CrossSectionFileReader.ReadFile(crLocFile, crDefFile, hydroNetwork, channelFrictionDefinitions);
        }

        private void ReadObservationPointsFiles()
        {
            string obsFiles = modelDefinition.GetModelProperty(KnownProperties.ObsFile).GetValueAsString();
            foreach (string obsFile in obsFiles.SplitOnEmptySpace())
            {
                string obsFileFullPath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, obsFile);
                if (!File.Exists(obsFileFullPath))
                {
                    return;
                }

                LocationFileReader.ReadFileObservationPointLocations(obsFileFullPath, hydroNetwork);
            }
        }

        private void ReadRoutesFile()
        {
            string routesFile = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, RoutesFile.RoutesFileName);
            if (!File.Exists(routesFile))
            {
                return;
            }

            RoutesFile.Read(routesFile, hydroNetwork);
        }

        private void ReadStructuresFiles(ICrossSectionDefinition[] crossSectionDefinitions)
        {
            WaterFlowFMProperty structureFileProperty = modelDefinition.GetModelProperty(KnownProperties.StructuresFile);

            string structureFilePath = MduFileHelper.GetSubfilePath(mduFilePath, structureFileProperty);
            if (!File.Exists(structureFilePath))
            {
                return;
            }

            var reader = new StructureFileReader(new FileSystem());
            reader.ReadFile(structureFilePath, crossSectionDefinitions, hydroNetwork, modelDefinition.GetReferenceDateAsDateTime());
        }

        private void ReadRoughnessFiles()
        {
            WaterFlowFMProperty roughnessFileProperty = modelDefinition.GetModelProperty(KnownProperties.FrictFile);
            IReadOnlyList<string> roughnessFilePaths = MduFileHelper.GetMultipleSubfilePath(mduFilePath, roughnessFileProperty).ToList();
            if (!roughnessFilePaths.Any())
            {
                return;
            }
            
            // read lanes
            foreach (string roughnessFilePath in roughnessFilePaths)
            {
                if (roughnessFilePath.ContainsCaseInsensitive(Resources.Roughness_Main_Channels_Filename))
                {
                    continue;
                }

                var roughnessReader = new RoughnessDataFileReader(new FileSystem());
                roughnessReader.ReadFile(roughnessFilePath, hydroNetwork, roughnessSections);
            }

            // read channels roughness
            string frictionFilePath = roughnessFilePaths.FirstOrDefault(x => x.ContainsCaseInsensitive(Resources.Roughness_Main_Channels_Filename));
            if (!File.Exists(frictionFilePath))
            {
                return;
            }

            ChannelFrictionDefinitionFileReader.ReadFile(frictionFilePath, modelDefinition, hydroNetwork, channelFrictionDefinitions);
        }

        private void ReadInitialConditionFiles()
        {
            WaterFlowFMProperty pathsRelativeToParentProperty = modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent);
            WaterFlowFMProperty initialFieldFileProperty = modelDefinition.GetModelProperty(KnownProperties.IniFieldFile);
            
            string initialConditionFilePath = MduFileHelper.GetSubfilePath(mduFilePath, initialFieldFileProperty);
            string parentFilePath = (bool)pathsRelativeToParentProperty.Value ? initialConditionFilePath : mduFilePath;

            if (string.IsNullOrWhiteSpace(initialConditionFilePath))
            {
                log.Warn(string.Format(Resources.FeatureFile1D2DReader_ReadInitialConditionFiles_No_initial_condition_filename_found_in_the_mdu_file_skipping_reading_initial_conditions));
                return;
            }

            // read initialFields.ini
            InitialFieldFileData initialFieldFileData = initialFieldFile.Read(initialConditionFilePath, parentFilePath, modelDefinition);

            // read Initial<quantity>.ini
            string initialConditionQuantityFileName = GetChannelInitialConditionDefinitionFileName(initialFieldFileData);
            if (string.IsNullOrEmpty(initialConditionQuantityFileName))
            {
                return;
            }

            string initialConditionQuantityFilePath = IoHelper.GetFilePathToLocationInSameDirectory(parentFilePath, initialConditionQuantityFileName);

            IEventedList<IBranch> branches = hydroNetwork.Branches;
            Dictionary<string, IBranch> branchDictionary = branches.ToDictionary(b => b.Name, b => b, StringComparer.InvariantCultureIgnoreCase);

            ChannelInitialConditionDefinitionFileReader.ReadFile(
                initialConditionQuantityFilePath,
                modelDefinition,
                branchDictionary,
                channelInitialConditionDefinitions);
        }

        private string GetChannelInitialConditionDefinitionFileName(InitialFieldFileData initialFieldFileData)
        {
            InitialFieldData[] initialConditionIniSections = initialFieldFileData.AllFields.Where(x => x.LocationType == InitialFieldLocationType.OneD ||
                                                                                                       x.DataFileType == InitialFieldDataFileType.OneDField)
                                                                                 .ToArray();
            if (!initialConditionIniSections.Any())
            {
                return string.Empty;
            }

            if (initialConditionIniSections.Length > 1)
            {
                log.Warn(Resources.Initial_Condition_Warning_Only_one_quantity_type_is_currently_supported_reading_the_first_and_ignoring_all_others);
            }

            InitialFieldData initialConditionIniSection = initialConditionIniSections.First();
            return initialConditionIniSection.DataFile;
        }
    }
}