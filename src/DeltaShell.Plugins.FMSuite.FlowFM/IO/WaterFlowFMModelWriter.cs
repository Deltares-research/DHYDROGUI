using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelWriter
    {
        public static void Write(WaterFlowFMModel model, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true)
        {
            WriteMduFile(model, switchTo, writeExtForcings, writeFeatures);
            WriteUGridFile(model);
            WriteMorSedFilesIfNeeded(model);
        }

        private static void WriteMduFile(WaterFlowFMModel model, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true)
        {
            var mduFile = new MduFile();
            mduFile.Write(model.MduFilePath, model.ModelDefinition, model.Area, model.FixedWeirsProperties, switchTo, writeExtForcings, writeFeatures);
        }

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

            var nodesFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(netFilePath, UGridToNetworkAdapter.NodeFileName);
            if (nodesFilePath != null) NodeFile.Write(model.Network.Manholes.SelectMany(m => m.Compartments).ToList(), nodesFilePath);

            var branchesFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(netFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            if (branchesFilePath != null) BranchFile.Write(model.Network.Branches, branchesFilePath);

            //write PropertiesPerBranch separate
            UGridToNetworkAdapter.SaveNetworkDiscretisation(model.NetworkDiscretization, netFilePath);
        }
    }
}
