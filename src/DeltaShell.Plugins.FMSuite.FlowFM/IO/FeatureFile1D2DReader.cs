using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class FeatureFile1D2DReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FeatureFile1D2DReader));

        public const string NODE_FILE_NAME = "nodeFile.ini";
        public const string STRUCTURES_FILE_NAME = "structures.ini";

        public static void Read1D2DFeatures(string targetMduFilePath, WaterFlowFMModel fmModel)
        {
            ReadCrossSectionFiles(targetMduFilePath, fmModel);
            ReadStructuresFiles(targetMduFilePath, fmModel);
            //ReadRoughnessFiles(targetMduFilePath, fmModel);
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
            var directoryName = System.IO.Path.GetDirectoryName(targetMduFilePath);
            if (directoryName == null) return;

            var roughnessFileNames = fmModel.RoughnessSections.Select(GetRoughnessFilename);
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.FrictFile, string.Join(";", roughnessFileNames));

            foreach (var roughnessSection in fmModel.RoughnessSections)
            {
                var roughnessFileName = GetRoughnessFilename(roughnessSection);
                var roughnessFilePath = System.IO.Path.Combine(directoryName, roughnessFileName);

                FileWritingUtils.ThrowIfFileNotExists(roughnessFilePath, directoryName, p => RoughnessDataFileWriter.WriteFile(p, roughnessSection));
            }
        }

        private static string GetRoughnessFilename(RoughnessSection roughnessSection)
        {
            return "roughness-" + roughnessSection.Name + ".ini";
        }
    }
}