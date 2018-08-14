using System.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.IO;
using DeltaShell.Plugins.SharpMapGis.ImportExport;

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
            var modelName = Path.GetFileNameWithoutExtension(mduPath);
            var readerData = new WaterFlowFMModelReaderData();

            // read MDU file
            var flowFmModelDefinition = new WaterFlowFMModelDefinition(mduPath, modelName);
            var mduFile = new MduFile();
            mduFile.Read(mduPath, ref flowFmModelDefinition);
            readerData.ModelDefinition = flowFmModelDefinition;


            // Read network from netCDF file
            var netFileProperty = readerData.ModelDefinition.GetModelProperty(KnownProperties.NetFile);
            if (string.IsNullOrEmpty(netFileProperty.GetValueAsString()))
            {
                netFileProperty.Value = modelName + NetFile.FullExtension;
            }

            var netFilePath = Path.Combine(Path.GetDirectoryName(mduPath), netFileProperty.GetValueAsString());
            readerData.NetworkDataModel = UGridToNetworkAdapter.ReadNetworkDataModelFromUGrid(netFilePath);

            // Read nodes file
            var nodeFileProperty = readerData.ModelDefinition.GetModelProperty(KnownProperties.NodeFile);
            var nodeFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(netFilePath, nodeFileProperty.GetValueAsString());
            if (File.Exists(nodeFilePath)) readerData.PropertiesPerCompartment = NodeFile.Read(nodeFilePath);

            // Read branches file (GUI properties only)
            var branchFilePath = UGridToNetworkAdapter.GetFilePathToLocationInSameDirectory(netFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            if (File.Exists(branchFilePath)) readerData.PropertiesPerBranch = BranchFile.Read(branchFilePath);
            
            // Read network discretization
            readerData.NetworkDiscretisationDataModel = UGridToNetworkAdapter.LoadNetworkDiscretisationDataModel(netFilePath);

            return readerData;
        }
    }
}
