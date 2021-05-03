using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.Utils;
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

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            var sobekPreFix = "tmp";
            var initialPath = GetFilePath(SobekFileNames.SobekBoundaryFileName);
            if (!File.Exists(initialPath))
            {
                log.WarnFormat("Import of boundary conditions skipped, file {0} does not exist.", initialPath);
                return;
            }

            var nodes = HydroNetwork.Nodes.ToDictionary(n => n.Name, n => n, StringComparer.InvariantCultureIgnoreCase);
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

                INode node;
                if (!nodes.TryGetValue(nodeId, out node) && nodeId.StartsWith(sobekPreFix) && !nodes.TryGetValue(nodeId.Substring(3), out node))
                {
                    log.WarnFormat("Can't find node {0} for boundary condition, node: {1}, skipping node ...", nodeId, condition.ID);
                    continue;
                }

                flowBoundaryConditionData.Feature = node;
                flowBoundaryConditionData.UseSalt = useSalt;
                flowBoundaryConditionData.UseTemperature = useTemperature;

                if (SobekType == DeltaShell.Sobek.Readers.SobekType.SobekRE && 
                    !nodes[nodeId].IsConnectedToMultipleBranches && 
                    condition.BoundaryType == SobekFlowBoundaryConditionType.Flow && 
                    nodes[nodeId].IncomingBranches.Count > 0)
                {
                    // RE defines positive flow along the branch, hence at the end of a branch a boundary has a positive Q, 
                    // this means a flow out of the system. If the direction of the branch is flipped, the same positive Q 
                    // suddenly means a flow into the system.
                    // In 2.12 (and DeltaShell) negative flow is always out of the system, and positive always into the system.
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
                waterFlowFMModel.ReplaceBoundaryCondition(flowBoundaryConditionData);
                flowBoundaryConditionData.UpdateManholeWithOutletData(node);
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
