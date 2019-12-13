using System.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileReaders.Roughness;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class FeatureFile1D2DReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FeatureFile1D2DReader));
        
        public static void Read1D2DFeatures(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            ReadCrossSectionFiles(targetMduFilePath, fmModel);
            ReadObservationPointsFiles(targetMduFilePath, fmModel);
            ReadStructuresFiles(targetMduFilePath, fmModel);
            ReadRoughnessFiles(targetMduFilePath, fmModel);
            ReadRetentionsFile(targetMduFilePath, fmModel);
        }

        private static void ReadRetentionsFile(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            string netFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, fmModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile));
            if (!File.Exists(netFilePath)) return;
            string storageNodeFilePath = MduFileHelper.GetSubfilePath(targetMduFilePath, fmModel.ModelDefinition.GetModelProperty(KnownProperties.StorageNodeFile));

            if (File.Exists(storageNodeFilePath))
                RetentionFileReader.ReadFile(storageNodeFilePath, fmModel.Network);

        }

        private static void ReadCrossSectionFiles(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            var crLocFile = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossLocFile).GetValueAsString();
            crLocFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, crLocFile);
            if (!File.Exists(crLocFile)) return;

            var crDefFile = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossDefFile).GetValueAsString();
            crDefFile =IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, crDefFile);
            if (!File.Exists(crDefFile)) return;

            CrossSectionFileReader.ReadFile(crLocFile,crDefFile, fmModel.Network);
        }
        private static void ReadObservationPointsFiles(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            var obsFiles = fmModel.ModelDefinition.GetModelProperty(KnownProperties.ObsFile).GetValueAsString();
            foreach (var obsFile in obsFiles.Split(' '))
            {
                var obsFileFullPath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, obsFile);
                if (!File.Exists(obsFileFullPath)) return;
                LocationFileReader.ReadFileObservationPointLocations(obsFileFullPath, fmModel.Network);
            }
            
        }

        private static void ReadStructuresFiles(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            var crDefFile = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossDefFile).GetValueAsString();
            crDefFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, crDefFile);
            
            var structureFile = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile).GetValueAsString();
            structureFile = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, structureFile);
            if (!File.Exists(structureFile)) return;

            StructureFileReader.ReadFile(structureFile, crDefFile , fmModel.Network);
        }

        private static void ReadRoughnessFiles(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            var directoryName = Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null) return;

            var frictionFileNames = fmModel.ModelDefinition.GetModelProperty(KnownProperties.FrictFile).GetValueAsString()?.Split(';');
            if(frictionFileNames == null || frictionFileNames.Length == 0) return;
            

            foreach (var frictionFileName in frictionFileNames)
            {
                var fileName = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, frictionFileName);
                if (!File.Exists(fileName)) return;
                RoughnessDataFileReader.ReadFile(fileName, fmModel.ModelDefinition.Network, fmModel.ModelDefinition.RoughnessSections);
            }
        }
    }
}