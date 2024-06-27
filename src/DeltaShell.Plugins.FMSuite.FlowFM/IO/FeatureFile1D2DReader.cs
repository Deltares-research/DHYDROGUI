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
        /// <param name="network"> The hydro network. </param>
        /// <param name="roughnessSections"> The roughness sections. </param>
        /// <param name="channelFrictionDefinitions"> The channel friction definitions. </param>
        /// <param name="channelInitialConditionDefinitions"> The channel initial condition definitions. </param>
        /// <param name="report1DOr2DFeatureRead">Function to report progress on</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public void Read1D2DFeatures(string mduFilePath,
                                            WaterFlowFMModelDefinition modelDefinition,
                                            IHydroNetwork network,
                                            IEventedList<RoughnessSection> roughnessSections,
                                            IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions,
                                            IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions,
                                            Action<string> report1DOr2DFeatureRead = null)
        {
            Ensure.NotNull(mduFilePath, nameof(mduFilePath));
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));
            Ensure.NotNull(network, nameof(network));
            Ensure.NotNull(roughnessSections, nameof(roughnessSections));
            Ensure.NotNull(channelFrictionDefinitions, nameof(channelFrictionDefinitions));
            Ensure.NotNull(channelInitialConditionDefinitions, nameof(channelInitialConditionDefinitions));

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_routes);
            ReadRoutesFile(mduFilePath, network);

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_cross_section_definitions);
            ICrossSectionDefinition[] definitions = ReadCrossSectionFiles(mduFilePath, modelDefinition, network, channelFrictionDefinitions);

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_structures); 
            ReadStructuresFiles(mduFilePath, modelDefinition, network, definitions);

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_observation_points); 
            ReadObservationPointsFiles(mduFilePath, modelDefinition, network);

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_roughness); 
            ReadRoughnessFiles(mduFilePath, modelDefinition, network, roughnessSections, channelFrictionDefinitions);
            
            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_retentions);
            var retentions= ReadRetentionsFromFile(mduFilePath, modelDefinition, network);
            foreach (IRetention retention in retentions)
            {
                retention.Branch.BranchFeatures.Add(retention);
            }

            report1DOr2DFeatureRead?.Invoke(Resources.FeatureFile1D2DReader_Read1D2DFeatures_Reading_initial_conditions); 
            ReadInitialConditionFiles(mduFilePath, modelDefinition, network, channelInitialConditionDefinitions);
        }

        private IEnumerable<IRetention> ReadRetentionsFromFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            string netFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, modelDefinition.GetModelProperty(KnownProperties.NetFile));
            if (!File.Exists(netFilePath))
            {
                return Enumerable.Empty<IRetention>();
            }

            string storageNodeFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, modelDefinition.GetModelProperty(KnownProperties.StorageNodeFile));
            if (!File.Exists(storageNodeFilePath))
            {
                return Enumerable.Empty<IRetention>();
            }

            var iniSections = new IniReader().ReadIniFile(storageNodeFilePath);
            if (iniSections.Count == 0)
            {
                log.Warn(string.Format(Resources.FeatureFile1D2DReader_ReadRetentionsFile_Could_not_read_file__0__properly__it_seems_empty, storageNodeFilePath));
                return Enumerable.Empty<IRetention>();
            }

            return RetentionSectionParser.ParseIniSections(iniSections, network);
        }

        private ICrossSectionDefinition[] ReadCrossSectionFiles(
            string targetMduFilePath,
            WaterFlowFMModelDefinition modelDefinition,
            IHydroNetwork network,
            IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            var crLocFile = modelDefinition.GetModelProperty(KnownProperties.CrossLocFile).GetValueAsString();
            crLocFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, crLocFile);

            var crDefFile = modelDefinition.GetModelProperty(KnownProperties.CrossDefFile).GetValueAsString();
            crDefFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, crDefFile);
            if (!File.Exists(crDefFile))
            {
                return Array.Empty<ICrossSectionDefinition>();
            }

            return CrossSectionFileReader.ReadFile(crLocFile, crDefFile, network, channelFrictionDefinitions);
        }
        private void ReadObservationPointsFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var obsFiles = modelDefinition.GetModelProperty(KnownProperties.ObsFile).GetValueAsString();
            foreach (var obsFile in obsFiles.SplitOnEmptySpace())
            {
                var obsFileFullPath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, obsFile);
                if (!File.Exists(obsFileFullPath))
                {
                    return;
                }
                LocationFileReader.ReadFileObservationPointLocations(obsFileFullPath, network);
            }

        }

        private void ReadRoutesFile(string targetMduFilePath, IHydroNetwork network)
        {
            var routesFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, RoutesFile.RoutesFileName);
            if (!File.Exists(routesFile))
            {
                return;
            }

            RoutesFile.Read(routesFile, network);
        }

        private void ReadStructuresFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
            IHydroNetwork network, ICrossSectionDefinition[] crossSectionDefinitions)
        {
            string structureFile = modelDefinition.GetModelProperty(KnownProperties.StructuresFile).GetValueAsString();
            var referenceDateTime = modelDefinition.GetReferenceDateAsDateTime();

            structureFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, structureFile);
            if (!File.Exists(structureFile))
            {
                return;
            }

            var reader = new StructureFileReader(new FileSystem());
            reader.ReadFile(structureFile, crossSectionDefinitions, network, referenceDateTime);
        }

        private void ReadRoughnessFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network, IEventedList<RoughnessSection> roughnessSections, IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            var roughnessFileNames = modelDefinition.GetModelProperty(KnownProperties.FrictFile).GetValueAsString()?.Split(' ', ';');
            if (roughnessFileNames == null || roughnessFileNames.Length == 0)
            {
                return;
            }
            var frictionFileName = Resources.Roughness_Main_Channels_Filename;

            // read lanes
            foreach (var roughnessFileName in roughnessFileNames)
            {
                if (roughnessFileName == frictionFileName)
                {
                    continue;
                }
                var fileName = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, roughnessFileName);
                if (!File.Exists(fileName))
                {
                    return;
                }

                var roughnessReader = new RoughnessDataFileReader(new FileSystem());
                
                roughnessReader.ReadFile(fileName, network, roughnessSections);
            }

            // read channels roughness
            var frictionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, frictionFileName);
            if (!File.Exists(frictionFilePath))
            {
                return;
            }
            ChannelFrictionDefinitionFileReader.ReadFile(frictionFilePath, modelDefinition, network, channelFrictionDefinitions);
        }

        private void ReadInitialConditionFiles(string targetMduFilePath,
            WaterFlowFMModelDefinition modelDefinition,
            IHydroNetwork network,
            IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions)
        {
            var initialConditionFilename = modelDefinition.GetModelProperty(KnownProperties.IniFieldFile).GetValueAsString();
            if (string.IsNullOrWhiteSpace(initialConditionFilename))
            {
                log.Warn(string.Format(Resources.FeatureFile1D2DReader_ReadInitialConditionFiles_No_initial_condition_filename_found_in_the_mdu_file_skipping_reading_initial_conditions));
                return;
            }
            var initialConditionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, initialConditionFilename);

            // read initialFields.ini
            var initialFieldFileData = initialFieldFile.Read(initialConditionFilePath, targetMduFilePath, modelDefinition);

            // read Initial<quantity>.ini
            string initialConditionQuantityFileName = GetChannelInitialConditionDefinitionFileName(initialFieldFileData);
            if (string.IsNullOrEmpty(initialConditionQuantityFileName))
            {
                return;
            }
            
            string initialConditionQuantityFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, initialConditionQuantityFileName);

            IEventedList<IBranch> branches = network.Branches;
            Dictionary<string, IBranch> branchDictionary = branches.ToDictionary(b => b.Name, b => b, StringComparer.InvariantCultureIgnoreCase);

            ChannelInitialConditionDefinitionFileReader.ReadFile(
                initialConditionQuantityFilePath,
                modelDefinition,
                branchDictionary,
                channelInitialConditionDefinitions);
        }
        
        private string GetChannelInitialConditionDefinitionFileName(InitialFieldFileData initialFieldFileData)
        {
            var initialConditionIniSections = initialFieldFileData.AllFields.Where(x => x.LocationType == InitialFieldLocationType.OneD ||
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