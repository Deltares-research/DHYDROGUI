using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.NetworkEditor;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelWriter
    {
        public static void Write(WaterFlowFMModel model)
        {
            WriteUGridFile(model);
        }

        private static void WriteUGridFile(WaterFlowFMModel model)
        {
            var ugridPath = model.NetFilePath;

            var metaData = new UGridGlobalMetaData(model.Name, "1.1", "2.1"); // last two arguments should be retrieved from the FlowFMApplicationPlugin
            UGridToNetworkAdapter.SaveNetwork(model.Network, ugridPath, metaData);
            UGridToNetworkAdapter.SaveNetworkDiscretisation(model.NetworkDiscretization, ugridPath);
        }
    }
}
