using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileReaders.Roughness;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class FeatureFile1D2DReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FeatureFile1D2DReader));

        public static void Read1D2DFeatures(string targetMduFilePath,
            WaterFlowFMModelDefinition modelDefinition,
            IHydroNetwork network,
            IEventedList<RoughnessSection> roughnessSections,
            IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions,
            IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions)
        {
            ReadRoutesFile(targetMduFilePath, network);
            var definitions = ReadCrossSectionFiles(targetMduFilePath, modelDefinition, network, channelFrictionDefinitions);
            ReadStructuresFiles(targetMduFilePath, modelDefinition, network, definitions);
            ReadObservationPointsFiles(targetMduFilePath, modelDefinition, network);
            ReadRoughnessFiles(targetMduFilePath, modelDefinition, network, roughnessSections, channelFrictionDefinitions);
            ReadRetentionsFile(targetMduFilePath, modelDefinition, network);
            ReadInitialConditionFiles(targetMduFilePath, modelDefinition, network, channelInitialConditionDefinitions);
        }

        private static void ReadRetentionsFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            string netFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, modelDefinition.GetModelProperty(KnownProperties.NetFile));
            if (!File.Exists(netFilePath)) return;
            string storageNodeFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, modelDefinition.GetModelProperty(KnownProperties.StorageNodeFile));

            if (File.Exists(storageNodeFilePath))
                RetentionFileReader.ReadFile(storageNodeFilePath, network);

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
                return new ICrossSectionDefinition[0];

//            var channelFrictionDefinitionPerChannelLookup = channelFrictionDefinitions.ToDictionary(cfd => cfd.Channel);

            return CrossSectionFileReader.ReadFile(crLocFile, crDefFile, network, channelFrictionDefinitions);
        }
        private static void ReadObservationPointsFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var obsFiles = modelDefinition.GetModelProperty(KnownProperties.ObsFile).GetValueAsString();
            foreach (var obsFile in obsFiles.Split(' '))
            {
                var obsFileFullPath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, obsFile);
                if (!File.Exists(obsFileFullPath)) return;
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
            var structureFile = modelDefinition.GetModelProperty(KnownProperties.StructuresFile).GetValueAsString();
            structureFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, structureFile);
            if (!File.Exists(structureFile)) return;

            StructureFileReader.ReadFile(structureFile, crossSectionDefinitions, network);
        }

        private static void ReadRoughnessFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network, IEventedList<RoughnessSection> roughnessSections, IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            var directoryName = Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null) return;

            var roughnessFileNames = modelDefinition.GetModelProperty(KnownProperties.FrictFile).GetValueAsString()?.Split(';');
            if (roughnessFileNames == null || roughnessFileNames.Length == 0) return;
            var frictionFileName = Properties.Resources.Roughness_Main_Channels_Filename;

            // read lanes
            foreach (var roughnessFileName in roughnessFileNames)
            {
                if (roughnessFileName == frictionFileName) continue;
                var fileName = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, roughnessFileName);
                if (!File.Exists(fileName)) return;
                RoughnessDataFileReader.ReadFile(fileName, network, roughnessSections);
            }

            // read channels roughness
            var frictionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, frictionFileName);
            if (!File.Exists(frictionFilePath)) return;
            ChannelFrictionDefinitionFileReader.ReadFile(frictionFilePath, modelDefinition, network, channelFrictionDefinitions);
        }

        private static void ReadInitialConditionFiles(string targetMduFilePath,
            WaterFlowFMModelDefinition modelDefinition,
            IHydroNetwork network,
            IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions)
        {
            var directoryName = Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null) return;

            var initialConditionFilename = modelDefinition.GetModelProperty(KnownProperties.IniFieldFile).GetValueAsString();
            if (string.IsNullOrWhiteSpace(initialConditionFilename))
            {
                Log.Warn(string.Format(Properties.Resources.FeatureFile1D2DReader_ReadInitialConditionFiles_No_initial_condition_filename_found_in_the_mdu_file_skipping_reading_initial_conditions));
                return;
            }
            var initialConditionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, initialConditionFilename);

            // read initialFields.ini
            (InitialConditionQuantity quantity, string filename) initialConditionTuple =
                InitialConditionInitialFieldsFileReader.ReadFile(initialConditionFilePath, modelDefinition);


            // read Initial<quantity>.ini
            if (channelInitialConditionDefinitions == null || !channelInitialConditionDefinitions.Any()) return;
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