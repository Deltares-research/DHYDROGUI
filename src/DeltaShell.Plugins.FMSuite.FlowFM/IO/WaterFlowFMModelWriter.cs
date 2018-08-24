using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelWriter
    {
        public static void Write(WaterFlowFMModel model, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true)
        {
            //TODO: Refactor MduFile class such that outcommented code her can be used for writing the Mdu file and other files that are now written in that class 
            //PrepareModelDefinitionForWriting(model);
            //WriteMduFile(model);

            var mduFile = new MduFile();
            mduFile.Write(model.MduFilePath, model.ModelDefinition, model.Area, model.FixedWeirsProperties, switchTo, writeExtForcings, writeFeatures, model.DisableFlowNodeRenumbering);
            WriteUGridFile(model);
            WriteMorSedFilesIfNeeded(model);
        }

        //private static void PrepareModelDefinitionForWriting(IWaterFlowFMModel model)
        //{
        //    if (model.Network.Manholes.Any())
        //        model.ModelDefinition.SetModelProperty(KnownProperties.NodeFile, "nodeFile.ini");
        //}

        //private static void WriteMduFile(WaterFlowFMModel model)
        //{
        //    var mduFile = new MduFile();
        //    mduFile.Write(model.MduFilePath, model.ModelDefinition);
        //}

        private static void WriteMorSedFilesIfNeeded(WaterFlowFMModel model)
        {
            if (!model.UseMorSed) return;

            var morPath = Path.ChangeExtension(model.MduFilePath, "mor");
            MorphologyFile.Save(morPath, model.ModelDefinition);

            var sedPath = Path.ChangeExtension(model.MduFilePath, "sed");
            SedimentFile.Save(sedPath, model);
        }

        private static void WriteUGridFile(WaterFlowFMModel model)
        {
            var netFilePath = model.NetFilePath;

            var metaData = new UGridGlobalMetaData(model.Name, "1.1", "2.1"); // last two arguments should be retrieved from the FlowFMApplicationPlugin
            UGridToNetworkAdapter.SaveNetwork(model.Network, netFilePath, metaData);

            var nodesFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(netFilePath, model.ModelDefinition.GetModelProperty(KnownProperties.NodeFile).GetValueAsString());
            if (nodesFilePath != null) NodeFile.Write(model.Network.Manholes.SelectMany(m => m.Compartments).ToList(), nodesFilePath);

            var branchesFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(netFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            if (branchesFilePath != null) BranchFile.Write(model.Network.Branches, branchesFilePath);

            //write PropertiesPerBranch separate
            UGridToNetworkAdapter.SaveNetworkDiscretisation(model.NetworkDiscretization, netFilePath);
        }
    }
}
