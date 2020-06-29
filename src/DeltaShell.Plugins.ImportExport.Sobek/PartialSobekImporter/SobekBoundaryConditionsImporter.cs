using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekBoundaryConditionsImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekBoundaryConditionsImporter));

        public override string DisplayName
        {
            get { return "Boundary conditions (data)"; }
        }

        protected override void PartialImport()
        {
            var initialPath = GetFilePath(SobekFileNames.SobekBoundaryFileName);
            if (!File.Exists(initialPath))
            {
                log.WarnFormat("Import of boundary conditions skipped, file {0} does not exist.", initialPath);
                return;
            }

            var nodes = HydroNetwork.Nodes.ToDictionary(n => n.Name, n => n);
            var boundaryConditionToFeatureLookup = CreateLookUpDictionary();
            var waterFlowFMModel = GetModel<WaterFlowFMModel>();
            var useSalt = waterFlowFMModel.UseSalinity;
            var useTemperature = waterFlowFMModel.UseTemperature;

            var sobekBoundaryConditionReader = new SobekBoundaryConditionReader();
            var boundaryC = sobekBoundaryConditionReader.Read(initialPath);

            foreach (var condition in boundaryC)
            {
                var flowBoundaryConditionData = Model1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(condition);
                var nodeId = boundaryConditionToFeatureLookup.ContainsKey(condition.ID) ? boundaryConditionToFeatureLookup[condition.ID] : null;

                if (nodeId == null)
                {
                    log.WarnFormat("Can't find node for boundary condition {0}, skipping node ...", condition.ID);
                    continue;
                }

                if (!nodes.ContainsKey(nodeId))
                {
                    log.WarnFormat("Can't find node {0} for boundary condition, node: {1}, skipping node ...", nodeId, condition.ID);
                    continue;
                }

                var node = nodes[nodeId];
                flowBoundaryConditionData.Feature = node;
                flowBoundaryConditionData.UseSalt = useSalt;
                flowBoundaryConditionData.UseTemperature = useTemperature;

                if ((SobekType == DeltaShell.Sobek.Readers.SobekType.SobekRE) && (!nodes[nodeId].IsConnectedToMultipleBranches) && (condition.BoundaryType == SobekFlowBoundaryConditionType.Flow))
                {
                    // RE defines positive flow along the branch, hence at the end of a branch a boundary has a positive Q, 
                    // this means a flow out of the system. If the direction of the branch is flipped, the same positive Q 
                    // suddenly means a flow into the system.
                    // In 2.12 (and DeltaShell) negative flow is always out of the system, and positive always into the system.
                    if (nodes[nodeId].IncomingBranches.Count > 0)
                    {
                        // end node thus invert boundary
                        flowBoundaryConditionData.Flow *= -1.0;

                        if (flowBoundaryConditionData.DataType == Model1DBoundaryNodeDataType.FlowTimeSeries ||
                            flowBoundaryConditionData.DataType == Model1DBoundaryNodeDataType.FlowWaterLevelTable ||
                            flowBoundaryConditionData.DataType == Model1DBoundaryNodeDataType.WaterLevelTimeSeries)
                        {
                            var values = ((IMultiDimensionalArray<double>)flowBoundaryConditionData.Data.Components[0].Values).ToArray();
                            for (var i = 0; i < values.Length; i++)
                            {
                                values[i] *= -1.0;
                            }
                            flowBoundaryConditionData.Data.Components[0].SetValues(values);
                        }
                    }
                }
                waterFlowFMModel.ReplaceBoundaryCondition(flowBoundaryConditionData);
                UpdateManholeWithOutletData(node, flowBoundaryConditionData);
            }
        }

        private void UpdateManholeWithOutletData(INode node, Model1DBoundaryNodeData flowBoundaryConditionData)
        {
            var manhole = node as Manhole;
            if (manhole != null && flowBoundaryConditionData.DataType == Model1DBoundaryNodeDataType.WaterLevelConstant)
            {
                //var outletCandidate = manhole.GetOutletCandidate(); // is not working. incomming branches are not set, but should be the method
                var outletCandidate = manhole.Compartments.LastOrDefault();
                if (outletCandidate != null)
                {
                    var outlet = manhole.UpdateCompartmentToOutletCompartment(outletCandidate);
                    outlet.SurfaceWaterLevel = flowBoundaryConditionData.WaterLevel;
                }
            }
        }

        private Dictionary<string, string> CreateLookUpDictionary()
        {
            string path = GetFilePath(SobekFileNames.SobekBoundaryConditionsLocationsFileName);
            if (!File.Exists(path))
            {
                return new Dictionary<string, string>();
            }

            var sobekBoundaryLocations = new SobekBoundaryLocationReader { SobekType = SobekType }.Read(path).Where(b => b.SobekBoundaryLocationType == SobekBoundaryLocationType.Node);

            return sobekBoundaryLocations.ToDictionaryWithErrorDetails(path, l => l.Id, l => l.ConnectionId);
        }
    }
}
