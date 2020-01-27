using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelReader
    {
        public static WaterFlowFMModel Read(string mduPath)
        {
            var waterFlowFmModelReaderData = ReadFiles(mduPath);

            var waterFlowFmModel = new WaterFlowFMModel
            {
                Network = NetworkDiscretisationFactory.CreateHydroNetwork(waterFlowFmModelReaderData.NetworkDataModel,
                    waterFlowFmModelReaderData.PropertiesPerBranch, waterFlowFmModelReaderData.PropertiesPerCompartment)
            };
            
            waterFlowFmModel.NetworkDiscretization = NetworkDiscretisationFactory.CreateNetworkDiscretisation(waterFlowFmModel.Network, waterFlowFmModelReaderData.NetworkDiscretisationDataModel);
            
            return waterFlowFmModel;
        }
        
        private static WaterFlowFMModelReaderData ReadFiles(string mduPath)
        {
            var modelName = Path.GetFileNameWithoutExtension(mduPath);
            var fmReaderData = new WaterFlowFMModelReaderData();

            // read MDU file
            var flowFmModelDefinition = new WaterFlowFMModelDefinition(mduPath, modelName);
            var area = new HydroArea();
            var network = new HydroNetwork();
            var discretization = new Discretization();
            var boundaryConditions1D = new EventedList<Model1DBoundaryNodeData>();
            var lateralSourcesData = new EventedList<Model1DLateralSourceData>();
            

            var mduFile = new MduFile();
            mduFile.Read(mduPath, flowFmModelDefinition, area, network, discretization, boundaryConditions1D, lateralSourcesData, new List<ModelFeatureCoordinateData<FixedWeir>>());
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
            var nodeFileProperty = fmReaderData.ModelDefinition.GetModelProperty(KnownProperties.StorageNodeFile);
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, nodeFileProperty.GetValueAsString());
            if (File.Exists(nodeFilePath)) fmReaderData.PropertiesPerCompartment = NodeFile.Read(nodeFilePath);
            UpdateCompartmentsToOutletsBasedOnBoundaryData(network, boundaryConditions1D);

            // Read branches file (GUI properties only)
            var branchFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, UGridToNetworkAdapter.BranchGuiFileName);
            if (File.Exists(branchFilePath)) fmReaderData.PropertiesPerBranch = BranchFile.Read(branchFilePath, netFilePath);
            
            // Read network discretization
            fmReaderData.NetworkDiscretisationDataModel = UGridToNetworkAdapter.LoadNetworkDiscretisationDataModel(netFilePath);

            return fmReaderData;
        }

        private static void UpdateCompartmentsToOutletsBasedOnBoundaryData(HydroNetwork network, EventedList<Model1DBoundaryNodeData> boundaryConditions1D)
        {
            foreach(var bc in boundaryConditions1D)
            {
                var manhole = bc.Node as Manhole;
                if (manhole != null && bc.DataType == Model1DBoundaryNodeDataType.WaterLevelConstant)
                {

                }

            }
        }
    }
}
