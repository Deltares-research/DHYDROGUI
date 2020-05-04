using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileReaders.Roughness;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using System.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class FeatureFile1D2DReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FeatureFile1D2DReader));
        
        public static void Read1D2DFeatures(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network, IEventedList<RoughnessSection> roughnessSections, IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            ReadStructuresFiles(targetMduFilePath, modelDefinition, network);
            ReadCrossSectionFiles(targetMduFilePath, modelDefinition, network);
            ReadObservationPointsFiles(targetMduFilePath, modelDefinition, network);
            ReadRoughnessFiles(targetMduFilePath, modelDefinition, network, roughnessSections, channelFrictionDefinitions);
            ReadRetentionsFile(targetMduFilePath, modelDefinition, network);
        }

        private static void ReadRetentionsFile(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            string netFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, modelDefinition.GetModelProperty(KnownProperties.NetFile));
            if (!File.Exists(netFilePath)) return;
            string storageNodeFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, modelDefinition.GetModelProperty(KnownProperties.StorageNodeFile));

            if (File.Exists(storageNodeFilePath))
                RetentionFileReader.ReadFile(storageNodeFilePath, network);

        }

        private static void ReadCrossSectionFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var crLocFile = modelDefinition.GetModelProperty(KnownProperties.CrossLocFile).GetValueAsString();
            crLocFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, crLocFile);
            if (!File.Exists(crLocFile)) Log.Warn("No cross section location file provided");//return;

            var crDefFile = modelDefinition.GetModelProperty(KnownProperties.CrossDefFile).GetValueAsString();
            crDefFile =IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, crDefFile);
            if (!File.Exists(crDefFile)) return;

            CrossSectionFileReader.ReadFile(crLocFile,crDefFile, network);
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

        private static void ReadStructuresFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network)
        {
            var crDefFile = modelDefinition.GetModelProperty(KnownProperties.CrossDefFile).GetValueAsString();
            crDefFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, crDefFile);
            
            var structureFile = modelDefinition.GetModelProperty(KnownProperties.StructuresFile).GetValueAsString();
            structureFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, structureFile);
            if (!File.Exists(structureFile)) return;

            StructureFileReader.ReadFile(structureFile, crDefFile, network);
        }

        private static void ReadRoughnessFiles(string targetMduFilePath, WaterFlowFMModelDefinition modelDefinition, IHydroNetwork network, IEventedList<RoughnessSection> roughnessSections, IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            var directoryName = Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null) return;

            var roughnessFileNames = modelDefinition.GetModelProperty(KnownProperties.FrictFile).GetValueAsString()?.Split(';');
            if(roughnessFileNames == null || roughnessFileNames.Length == 0) return;

            // read lanes
            foreach (var roughnessFileName in roughnessFileNames)
            {
                var fileName = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, roughnessFileName);
                if (!File.Exists(fileName)) return;
                RoughnessDataFileReader.ReadFile(fileName, network, roughnessSections, channelFrictionDefinitions);
            }

            // read channels roughness
            var frictionFileName = Properties.Resources.Roughness_Main_Channels_Filename;
            var frictionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, frictionFileName);
            if (!File.Exists(frictionFilePath)) return;
            ChannelFrictionDefinitionFileReader.ReadFile(frictionFilePath, modelDefinition, network, channelFrictionDefinitions);
        }
    }
}