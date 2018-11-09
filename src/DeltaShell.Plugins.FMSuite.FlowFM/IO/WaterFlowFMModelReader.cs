using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
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
            var fmReaderData = new WaterFlowFMModelReaderData();

            // read MDU file
            var flowFmModelDefinition = new WaterFlowFMModelDefinition(mduPath, modelName);
            var area = new HydroArea();

            var mduFile = new MduFile();
            mduFile.Read(mduPath, flowFmModelDefinition, area, new List<ModelFeatureCoordinateData<FixedWeir>>());
            fmReaderData.ModelDefinition = flowFmModelDefinition;
            fmReaderData.Area = area;

            // Read network from netCDF file
            var netFileProperty = fmReaderData.ModelDefinition.GetModelProperty(KnownProperties.NetFile);
            if (string.IsNullOrEmpty(netFileProperty.GetValueAsString()))
            {
                netFileProperty.Value = modelName + NetFile.FullExtension;
            }

            var netFilePath = Path.Combine(Path.GetDirectoryName(mduPath), netFileProperty.GetValueAsString());
            fmReaderData.NetworkDataModel = UGridToNetworkAdapter.ReadNetworkDataModelFromUGrid(netFilePath);

            // Read nodes file
            var nodeFileProperty = fmReaderData.ModelDefinition.GetModelProperty(KnownProperties.NodeFile);
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, nodeFileProperty.GetValueAsString());
            if (File.Exists(nodeFilePath)) fmReaderData.PropertiesPerCompartment = NodeFile.Read(nodeFilePath);

            // Read branches file (GUI properties only)
            var branchFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            if (File.Exists(branchFilePath)) fmReaderData.PropertiesPerBranch = BranchFile.Read(branchFilePath);
            
            // Read network discretization
            fmReaderData.NetworkDiscretisationDataModel = UGridToNetworkAdapter.LoadNetworkDiscretisationDataModel(netFilePath);

            return fmReaderData;
        }
    }
}
