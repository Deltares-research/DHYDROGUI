using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO;
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
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Reader for 1D2D features responsible for reading the feature files and setting the data on the models.
    /// </summary>
    public static class FeatureFile1D2DReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FeatureFile1D2DReader));

        /// <summary>
        /// Reads the features and sets the data on the given models.
        /// </summary>
        /// <param name="mduFilePath"> The MDU file path. </param>
        /// <param name="modelDefinition"> The loaded model definition. </param>
        /// <param name="network"> The hydro network. </param>
        /// <param name="roughnessSections"> The roughness sections. </param>
        /// <param name="channelFrictionDefinitions"> The channel friction definitions. </param>
        /// <param name="channelInitialConditionDefinitions"> The channel initial condition definitions. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public static void Read1D2DFeatures(string mduFilePath,
                                            WaterFlowFMModelDefinition modelDefinition,
                                            IHydroNetwork network,
                                            IEventedList<RoughnessSection> roughnessSections,
                                            IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions,
                                            IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions)
        {
            Ensure.NotNull(mduFilePath, nameof(mduFilePath));
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));
            Ensure.NotNull(network, nameof(network));
            Ensure.NotNull(roughnessSections, nameof(roughnessSections));
            Ensure.NotNull(channelFrictionDefinitions, nameof(channelFrictionDefinitions));
            Ensure.NotNull(channelInitialConditionDefinitions, nameof(channelInitialConditionDefinitions));
            
            ReadRoutesFile(mduFilePath, network);
            ICrossSectionDefinition[] definitions = ReadCrossSectionFiles(mduFilePath, modelDefinition, network, channelFrictionDefinitions);
            ReadStructuresFiles(mduFilePath, modelDefinition, network, definitions);
            ReadObservationPointsFiles(mduFilePath, modelDefinition, network);
            ReadRoughnessFiles(mduFilePath, modelDefinition, network, roughnessSections, channelFrictionDefinitions);

            var retentions= ReadRetentionsFromFile(mduFilePath, modelDefinition, network);
            foreach (IRetention retention in retentions)
            {
                retention.Branch.BranchFeatures.Add(retention);
            }

            ReadInitialConditionFiles(mduFilePath, modelDefinition, network, channelInitialConditionDefinitions);
        }

        private static IEnumerable<IRetention> ReadRetentionsFromFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
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

            var categories = new DelftIniReader().ReadDelftIniFile(storageNodeFilePath);
            if (categories.Count == 0)
            {
                log.Warn(string.Format(Resources.FeatureFile1D2DReader_ReadRetentionsFile_Could_not_read_file__0__properly__it_seems_empty, storageNodeFilePath));
                return Enumerable.Empty<IRetention>();
            }

            return RetentionCategoryParser.ParseCategories(categories, network);
        }

        private static ICrossSectionDefinition[] ReadCrossSectionFiles(
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
        private static void ReadObservationPointsFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
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

        private static void ReadRoutesFile(string targetMduFilePath, IHydroNetwork network)
        {
            var routesFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, RoutesFile.RoutesFileName);
            if (!File.Exists(routesFile))
            {
                return;
            }

            RoutesFile.Read(routesFile, network);
        }

        private static void ReadStructuresFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition,
            IHydroNetwork network, ICrossSectionDefinition[] crossSectionDefinitions)
        {
            string structureFile = modelDefinition.GetModelProperty(KnownProperties.StructuresFile).GetValueAsString();
            var referenceDateTime = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            structureFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, structureFile);
            if (!File.Exists(structureFile))
            {
                return;
            }

            StructureFileReader.ReadFile(structureFile, crossSectionDefinitions, network, referenceDateTime);
        }

        private static void ReadRoughnessFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network, IEventedList<RoughnessSection> roughnessSections, IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            var roughnessFileNames = modelDefinition.GetModelProperty(KnownProperties.FrictFile).GetValueAsString()?.Split(';');
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
                RoughnessDataFileReader.ReadFile(fileName, network, roughnessSections);
            }

            // read channels roughness
            var frictionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, frictionFileName);
            if (!File.Exists(frictionFilePath))
            {
                return;
            }
            ChannelFrictionDefinitionFileReader.ReadFile(frictionFilePath, modelDefinition, network, channelFrictionDefinitions);
        }

        private static void ReadInitialConditionFiles(string targetMduFilePath,
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
            (InitialConditionQuantity quantity, string filename) initialConditionTuple =
                InitialConditionInitialFieldsFileReader.ReadFile(initialConditionFilePath, modelDefinition);


            // read Initial<quantity>.ini
            var initialConditionQuantityFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, initialConditionTuple.filename);

            var branches = network.Branches;
            var branchDictionary = branches.ToDictionary(b => b.Name, b => b, StringComparer.InvariantCultureIgnoreCase);

            ChannelInitialConditionDefinitionFileReader.ReadFile(
                initialConditionQuantityFilePath,
                modelDefinition,
                branchDictionary,
                channelInitialConditionDefinitions);
        }
    }
}