using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelWriter
    {
        private static WaterFlowFMModelWriterData WriterData;

        // TODO: get rid of the optional parameters. Solve in a different way.
        public static void Write(WaterFlowFMModel model, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true)
        {
            PrepareModelDefinitionForWriting(model);
            WriterData = CreateWriterData(model);

            WriteMorSedFilesIfNeeded(model);
            WriteMduFile(model, switchTo, writeExtForcings, writeFeatures);
            WriteCrossSectionDefinitions(model);
            WriteCrossSectionLocation(model);
            WriteNodeFile(model);
            WriteBranchesGuiFile(model);
            WriteStructuresFile(model);
            //WriteRoughness(model);
            WriteUGridFile(WriterData);
        }

        //private static void WriteRoughness(WaterFlowFMModel model)
        //{
        //    var writtenRoughessFiles = new List<string>();

        //    foreach (var roughnessSection in model.RoughnessSections)
        //    {
        //        var filename = "roughness-" + roughnessSection.Name + ".ini";
        //        var roughnessFilename = Path.Combine(KnownProperties.RoughnessFile, filename);

        //        RoughnessDataFileWriter.
        //        ThrowIfFileNotExists(roughnessFilename, fileName.TargetPath, p => RoughnessDataFileWriter.WriteFile(p, roughnessSection));//Add subPath!!
        //        writtenRoughessFiles.Add(filename);
        //    }
        //    model.ModelDefinition.SetModelProperty(KnownProperties.RoughnessFile, string.Join(" ", writtenRoughessFiles));
        //}

        private static void PrepareModelDefinitionForWriting(IWaterFlowFMModel model)
        {
            var network = model.Network;
            var modelDefinition = model.ModelDefinition;
            if (network.Manholes.Any())
                modelDefinition.SetModelProperty(KnownProperties.NodeFile, "nodeFile.ini");
            if (network.CrossSections.Any() || network.Pipes.Any(p => p.CrossSectionDefinition != null))
            {
                modelDefinition.SetModelProperty(KnownProperties.CrossDefFile, "crsdef.ini");
                modelDefinition.SetModelProperty(KnownProperties.CrossLocFile, "crsloc.ini");
            }

            if (network.BranchFeatures.Any())
            {
                modelDefinition.SetModelProperty(KnownProperties.StructuresFile, "structures.ini");
            }
        }

        private static WaterFlowFMModelWriterData CreateWriterData(WaterFlowFMModel model)
        {
            return new WaterFlowFMModelWriterData
            {
                ModelName = model.Name,
                FilePaths = new WaterFlowFMModelWriterData.FileNames
                {
                    NetFilePath = model.NetFilePath,
                    CrossSectionDefinitionFilePath = GetAbsoluteFilePathFromModel(KnownProperties.CrossDefFile, model),
                    CrossSectionLocationFilePath = GetAbsoluteFilePathFromModel(KnownProperties.CrossLocFile, model),
                    NodeFilePath = GetAbsoluteFilePathFromModel(KnownProperties.NodeFile, model),
                    StructuresFilePath = GetAbsoluteFilePathFromModel(KnownProperties.StructuresFile, model)
                },
                NetworkDataModel = new NetworkUGridDataModel(model.Network),
                NetworkDiscretisationDataModel = new NetworkDiscretisationUGridDataModel(model.NetworkDiscretization)
            };
        }

        private static string GetAbsoluteFilePathFromModel(string key, WaterFlowFMModel model)
        {
            var fileProperty = model.ModelDefinition.GetModelProperty(key);
            var fileName = fileProperty.GetValueAsString();
            if (string.IsNullOrEmpty(fileName)) return string.Empty;

            var absolutePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(model.NetFilePath, fileName);
            return absolutePath;
        }

        private static void WriteMorSedFilesIfNeeded(WaterFlowFMModel model)
        {
            if (!model.UseMorSed) return;

            var morPath = Path.ChangeExtension(model.MduFilePath, "mor");
            MorphologyFile.Save(morPath, model.ModelDefinition);

            var sedPath = Path.ChangeExtension(model.MduFilePath, "sed");
            SedimentFile.Save(sedPath, model);
        }

        private static void WriteMduFile(WaterFlowFMModel model, bool switchTo, bool writeExtForcings, bool writeFeatures)
        {
            var mduFile = new MduFile();
            mduFile.Write(model.MduFilePath, model.ModelDefinition, model.Area, model.FixedWeirsProperties, switchTo, writeExtForcings, writeFeatures, model.DisableFlowNodeRenumbering);
        }

        private static void WriteCrossSectionDefinitions(WaterFlowFMModel model)
        {
            var filePath = WriterData.FilePaths.CrossSectionDefinitionFilePath;
            if (string.IsNullOrEmpty(filePath)) return;

            CrossSectionDefinitionFileWriter.WriteFile(filePath, model.Network, model.RoughnessSections);
        }

        private static void WriteCrossSectionLocation(WaterFlowFMModel model)
        {
            var filePath = WriterData.FilePaths.CrossSectionLocationFilePath;
            if (!string.IsNullOrEmpty(filePath))
                CrossSectionLocationWriter.WriteFile(filePath, model);
        }

        private static void WriteNodeFile(WaterFlowFMModel model)
        {
            var filePath = WriterData.FilePaths.NodeFilePath;
            if (!string.IsNullOrEmpty(filePath))
                NodeFile.Write(filePath, model.Network.Manholes.SelectMany(m => m.Compartments));
        }

        private static void WriteBranchesGuiFile(WaterFlowFMModel model)
        {
            var branchesFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(model.NetFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            if (branchesFilePath != null) BranchFile.Write(branchesFilePath, model.Network.Branches);
        }

        private static void WriteStructuresFile(WaterFlowFMModel model)
        {
            var filePath = WriterData.FilePaths.StructuresFilePath;

            if (!string.IsNullOrEmpty(filePath))
                StructureFileWriter.WriteFile(
                    filePath, 
                    model,
                    StructureFile.Generate2DStructureCategoriesFromFMModel);
        }

        private static void WriteUGridFile(WaterFlowFMModelWriterData writerData)
        {
            var netFilePath = writerData.FilePaths.NetFilePath;

            var metaData = new UGridGlobalMetaData(writerData.ModelName, "1.1", "2.1"); // last two arguments should be retrieved from the FlowFMApplicationPlugin
            UGridToNetworkAdapter.SaveNetwork(netFilePath, writerData.NetworkDataModel, metaData);
            UGridToNetworkAdapter.SaveNetworkDiscretisation(netFilePath, writerData.NetworkDiscretisationDataModel);
        }
    }
}
