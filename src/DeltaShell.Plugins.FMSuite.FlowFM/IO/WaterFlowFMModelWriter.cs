using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelWriter
    {

        // TODO: get rid of the optional parameters. Solve in a different way.
        public static void Write(WaterFlowFMModel model, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true)
        {
            var writerData = CreateWriterData(model);

            PrepareModelDefinitionForWriting(model);
            WriteMorSedFilesIfNeeded(model);
            WriteMduFile(model, switchTo, writeExtForcings, writeFeatures);
            WriteCrossSectionDefinitions(model);
            WriteCrossSectionLocation(model);
            WriteNodeFile(model);
            WriteBranchesGuiFile(model);
            WriteUGridFile(writerData);
        }

        private static WaterFlowFMModelWriterData CreateWriterData(WaterFlowFMModel model)
        {
            return new WaterFlowFMModelWriterData
            {
                ModelName = model.Name,
                FilePaths = new WaterFlowFMModelWriterData.FileNames
                {
                    NetFilePath = model.NetFilePath,
                },
                NetworkDataModel = new NetworkUGridDataModel(model.Network),
                NetworkDiscretisationDataModel = new NetworkDiscretisationUGridDataModel(model.NetworkDiscretization)
            };
        }

        private static void PrepareModelDefinitionForWriting(IWaterFlowFMModel model)
        {
            var network = model.Network;
            if (network.Manholes.Any())
                model.ModelDefinition.SetModelProperty(KnownProperties.NodeFile, "nodeFile.ini");
            if (network.CrossSections.Any() || network.Pipes.Any(p => p.CrossSectionDefinition != null))
                model.ModelDefinition.SetModelProperty(KnownProperties.CrossDefFile, "crsdef.ini");
            if (network.CrossSections.Any() || network.Pipes.Any(p => p.CrossSectionDefinition != null))
                model.ModelDefinition.SetModelProperty(KnownProperties.CrossLocFile, "crsloc.ini");
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
            var crossDefFileProperty = model.ModelDefinition.GetModelProperty(KnownProperties.CrossDefFile);
            var crosDefFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(model.NetFilePath, crossDefFileProperty.GetValueAsString());

            if(crosDefFilePath != null)
                FmCrossSectionDefinitionWriter.WriteFile(crosDefFilePath, model);
        }

        private static void WriteCrossSectionLocation(WaterFlowFMModel model)
        {
            var crossLocFileName = model.ModelDefinition.GetModelProperty(KnownProperties.CrossLocFile).GetValueAsString();
            var crossLocFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(model.NetFilePath, crossLocFileName);

            if (crossLocFilePath != null) CrossSectionLocationWriter.WriteFile(crossLocFilePath, model);
        }

        private static void WriteNodeFile(WaterFlowFMModel model)
        {
            var nodeFileProperty = model.ModelDefinition.GetModelProperty(KnownProperties.NodeFile);
            var nodesFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(model.NetFilePath, nodeFileProperty.GetValueAsString());
            if (nodesFilePath != null) NodeFile.Write(model.Network.Manholes.SelectMany(m => m.Compartments), nodesFilePath);
        }

        private static void WriteBranchesGuiFile(WaterFlowFMModel model)
        {
            var branchesFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(model.NetFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            if (branchesFilePath != null) BranchFile.Write(model.Network.Branches, branchesFilePath);
        }

        private static void WriteUGridFile(WaterFlowFMModelWriterData writerData)
        {
            var netFilePath = writerData.FilePaths.NetFilePath;

            var metaData = new UGridGlobalMetaData(writerData.ModelName, "1.1", "2.1"); // last two arguments should be retrieved from the FlowFMApplicationPlugin
            UGridToNetworkAdapter.SaveNetwork(netFilePath, writerData.NetworkDataModel, metaData);
            UGridToNetworkAdapter.SaveNetworkDiscretisation(writerData.NetworkDiscretisationDataModel, netFilePath);
        }
    }
}
