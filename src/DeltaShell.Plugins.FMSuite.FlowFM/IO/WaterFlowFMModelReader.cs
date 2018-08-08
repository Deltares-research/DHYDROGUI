using System.IO;
using DeltaShell.Plugins.NetworkEditor;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelReader
    {
        public static WaterFlowFMModel Read(string mduPath)
        {
            var waterFlowFmModelReaderData = ReadFiles(mduPath);
            var waterFlowFmModel = WaterFlowFMModelFactory.CreateModelFromReaderData(waterFlowFmModelReaderData);

            return waterFlowFmModel;
        }

        private static WaterFlowFMModelReaderData ReadFiles(string mduPath)
        {
            var ugridPath = Path.Combine(Path.GetDirectoryName(mduPath), "udgrid_net.nc");

            var waterFlowFmModelReaderData =  new WaterFlowFMModelReaderData();
            waterFlowFmModelReaderData.NetworkDataModel = ReadUGridFile(ugridPath);
        

            return waterFlowFmModelReaderData;
        }

        private static NetworkUGridDataModel ReadUGridFile(string ugridPath)
        {
            return UGridToNetworkAdapter.ReadUGridFile(ugridPath);
        }
    }
}
