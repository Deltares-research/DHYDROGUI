using DeltaShell.Plugins.NetworkEditor;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelFactory
    {
        public static WaterFlowFMModel CreateModelFromReaderData(WaterFlowFMModelReaderData fmReaderData)
        {
            var flowFmModel = new WaterFlowFMModel();
            CreateNetwork(fmReaderData, flowFmModel);
            return flowFmModel;
        }

        private static void CreateNetwork(WaterFlowFMModelReaderData fmReaderData, WaterFlowFMModel flowFmModel)
        {
            flowFmModel.Network = NetworkUGridDataModel.ReconstructHydroNetwork(fmReaderData.NetworkDataModel, fmReaderData.PropertiesPerBranch);
        }
    }
}
