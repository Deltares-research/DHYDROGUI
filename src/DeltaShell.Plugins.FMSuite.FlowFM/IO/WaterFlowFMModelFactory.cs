using DeltaShell.NGHS.IO.Grid;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelFactory
    {
        public static WaterFlowFMModel CreateModelFromReaderData(WaterFlowFMModelReaderData fmReaderData)
        {
            var fmModel = new WaterFlowFMModel();
            CreateNetworkAndDiscretization(fmReaderData, fmModel);
            return fmModel;
        }

        private static void CreateNetworkAndDiscretization(WaterFlowFMModelReaderData fmReaderData, IWaterFlowFMModel fmModel)
        {
            fmModel.Network = NetworkDiscretisationFactory.CreateHydroNetwork(fmReaderData.NetworkDataModel, fmReaderData.PropertiesPerBranch, fmReaderData.PropertiesPerCompartment);
            fmModel.NetworkDiscretization = NetworkDiscretisationFactory.CreateNetworkDiscretisation(fmModel.Network, fmReaderData.NetworkDiscretisationDataModel);
        }
    }
}
