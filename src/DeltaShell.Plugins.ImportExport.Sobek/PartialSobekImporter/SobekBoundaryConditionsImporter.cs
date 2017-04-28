using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekBoundaryConditionsImporter: PartialSobekImporterBase
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
            var waterFlowModel1D = GetModel<WaterFlowModel1D>();
            var useSalt = waterFlowModel1D.UseSalt;
            var useTemperature = waterFlowModel1D.UseTemperature;

            var sobekBoundaryConditionReader = new SobekBoundaryConditionReader();
            var boundaryC = sobekBoundaryConditionReader.Read(initialPath);

            foreach (var condition in boundaryC)
            {
                var flowBoundaryConditionData = WaterFlowModel1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(condition);
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

                flowBoundaryConditionData.Feature = nodes[nodeId];
                flowBoundaryConditionData.UseSalt = useSalt;
                flowBoundaryConditionData.UseTemperature = useTemperature;

                if ((SobekType == SobekType.SobekRE) && (!nodes[nodeId].IsConnectedToMultipleBranches) && (condition.BoundaryType == SobekFlowBoundaryConditionType.Flow))
                {
                    // RE defines positive flow along the branch, hence at the end of a branch a boundary has a positive Q, 
                    // this means a flow out of the system. If the direction of the branch is flipped, the same positive Q 
                    // suddenly means a flow into the system.
                    // In 2.12 (and DeltaShell) negative flow is always out of the system, and positive always into the system.
                    if (nodes[nodeId].IncomingBranches.Count > 0)
                    {
                        // end node thus invert boundary
                        flowBoundaryConditionData.Flow *= -1.0;

                        if (flowBoundaryConditionData.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries ||
                            flowBoundaryConditionData.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable ||
                            flowBoundaryConditionData.DataType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries)
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

                waterFlowModel1D.ReplaceBoundaryCondition(flowBoundaryConditionData);
            }
        }
        
        private Dictionary<string, string> CreateLookUpDictionary()
        {
            string path = GetFilePath(SobekFileNames.SobekBoundaryConditionsLocationsFileName);
            if (!File.Exists(path))
            {
                return new Dictionary<string, string>();
            }

            var sobekBoundaryLocations = new SobekBoundaryLocationReader { SobekType =  SobekType }.Read(path).Where(b => b.SobekBoundaryLocationType == SobekBoundaryLocationType.Node);

            return sobekBoundaryLocations.ToDictionaryWithErrorDetails(path, l => l.Id, l => l.ConnectionId);
        }
    }
}
