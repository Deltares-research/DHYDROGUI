using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public class WaterFlowModel1DModelMergeValidator
    {
        public ValidationReport Validate(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            return new ValidationReport(destinationModel.Name + " (Water Flow 1D Model)", new[]
                                                                   {
                                                                       ValidateConnection(destinationModel,sourceModel),
                                                                       ValidateUnmergable(sourceModel),
                                                                       ValidateCoordinateSystems(destinationModel, sourceModel),
                                                                       ValidateSalility(destinationModel, sourceModel),
                                                                       ValidateTemperature(destinationModel, sourceModel),
                                                                       ValidateHydroObjectsWillBeRenamed(destinationModel,sourceModel),
                                                                       ValidateChannels(destinationModel,sourceModel),
                                                                       ValidateInitialConditions(destinationModel, sourceModel)
                                                                   });

        }
        private static ValidationReport ValidateCoordinateSystems(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            var issues = new List<ValidationIssue>();
            if (destinationModel.Network.CoordinateSystem != null && !destinationModel.Network.CoordinateSystem.EqualsTo(sourceModel.Network.CoordinateSystem))
            {
                var destinationCoordinateSystemName = destinationModel.Network.CoordinateSystem != null
                    ? destinationModel.Network.CoordinateSystem.Name
                    : "<empty>";
                
                var sourceCoordinateSystemName = sourceModel.Network.CoordinateSystem != null
                    ? sourceModel.Network.CoordinateSystem.Name
                    : "<empty>";

                var issue = new ValidationIssue(destinationModel.Network.CoordinateSystem, ValidationSeverity.Error,
                string.Format(
                    "Cannot perform merge om models which have different coordinate systems. Source model has coordinate system: {0}. Destination model has coordinate system {1}, please change this to the source model coordinate system.",
                    sourceCoordinateSystemName, destinationCoordinateSystemName), destinationModel.Network.CoordinateSystem);
                issues.Add(issue);
            }
            var destinationCoordinateSystemIssues = ValidateCoordinateSystem(destinationModel).ToList();
            if (destinationCoordinateSystemIssues.Any())
            {
                issues = issues.Concat(destinationCoordinateSystemIssues).ToList();
            }
            var sourceCoordinateSystemIssues = ValidateCoordinateSystem(sourceModel).ToList();
            if (sourceCoordinateSystemIssues.Any())
            {
                issues = issues.Concat(sourceCoordinateSystemIssues).ToList();
            }

            return issues.Any() ? new ValidationReport("Coordinate system", issues) : new ValidationReport("Coordinate system", Enumerable.Empty<ValidationIssue>());
        }
        
        private static ValidationIssue CheckUpdateChannelToNewGeometry(INode destinationNode, INode sourceNode,IChannel channel)
        {
            var changeCoordinate = new Coordinate(destinationNode.Geometry.Coordinate.X, destinationNode.Geometry.Coordinate.Y);
            var channelGeometryCoordinates = channel.Source == sourceNode
                ? new List<Coordinate> {changeCoordinate}.Union(channel.Geometry.Coordinates.ToList()
                    .GetRange(1, channel.Geometry.Coordinates.Length - 1)).ToArray()
                : channel.Geometry.Coordinates.ToList()
                    .GetRange(0, channel.Geometry.Coordinates.Length - 1).ToList()
                    .Union(new List<Coordinate> { changeCoordinate }).ToArray();
            
            var issue = new ValidationIssue(channel, ValidationSeverity.Info,
                string.Format(
                    @"The geometry of channel : {0}, will be changed from {1} to {2} because the connecting nodes are not exactly at same location. The length of the channel will stay the same : {3} by setting the is custom length flag to true to keep the original source calculations the same.",
                    channel.Name,
                    string.Join<Coordinate>(",", channel.Geometry.Coordinates),
                    string.Join<Coordinate>(",", channelGeometryCoordinates),
                    channel.Length));
            return issue;
        }

        public static ValidationReport ValidateChannels(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel, sourceModel);
            var valIssues = new List<ValidationIssue>();
            
            foreach (var connectedNode in connectedNodes)
            {
                foreach (var sourceNode in connectedNode.Value)
                {
                    var issues = WaterFlowModel1DModelMergeHelper.FitSourceChannelsOnDestinationNode(connectedNode.Key, sourceNode, sourceModel, CheckUpdateChannelToNewGeometry);
                    if(issues != null)
                        valIssues.AddRange(issues);
                }
            }
            return valIssues.Count == 0 ? new ValidationReport("Validate Channels", Enumerable.Empty<ValidationIssue>()) : new ValidationReport("Validate Channels", valIssues);
        }

        public static ValidationReport ValidateInitialConditions(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            var valIssues = new List<ValidationIssue>();

            if (destinationModel.InitialConditionsType != sourceModel.InitialConditionsType)
            {
                string sourceModelInitialConditionType = sourceModel.InitialConditionsType.ToString();
                string destinationmodelInitialConditionType = destinationModel.InitialConditionsType.ToString();
                valIssues.Add(new ValidationIssue(sourceModel, ValidationSeverity.Warning,
                    string.Format(
                    @"The Initial Conditions type of the source model: {0} differs from the destination model: {1}. Initial Conditions will not be merged.",
                    sourceModelInitialConditionType,
                    destinationmodelInitialConditionType)));
            }

            return valIssues.Count == 0 ? new ValidationReport("Validate Initial Conditions", Enumerable.Empty<ValidationIssue>()) : new ValidationReport("Validate Initial Conditions", valIssues);
        }

        public static ValidationReport ValidateConnection(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            var connectedNodesList = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel, sourceModel);

            if (!connectedNodesList.Any())
            {
                var issue = new ValidationIssue(sourceModel, ValidationSeverity.Warning,
                    string.Format("Source model ({0}) has no connecting nodes with the destination model ({1}).", sourceModel.Name, destinationModel.Name), sourceModel.Network);
                return new ValidationReport("Connected Nodes", new[] { issue });
            }
            
            var issues = connectedNodesList.Select(connectedNodes => new ValidationIssue(connectedNodes.Key, ValidationSeverity.Info, string.Format("Node{0} {1} of source model ({2}) will be connected to node {3} of destination model {4}", connectedNodes.Value.Count > 1 ? "s" : "", string.Join(", ", connectedNodes.Value.Select(n => n.Name)), sourceModel.Name, connectedNodes.Key.Name, destinationModel.Name))).ToList();
            return new ValidationReport("Connected Nodes", issues);
        }

        public static ValidationReport ValidateSalility(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            if (sourceModel.UseSalt)
            {
                // Destination model does not have salinity enabled, so copy the settings from source model
                if (!destinationModel.UseSalt)
                {
                    var issue = new ValidationIssue(sourceModel, ValidationSeverity.Warning,
                        "The source model has salinity enabled, but the destination model does not. " +
                        "Salinity will be enabled in the destination model and Salt-related Model Properties copied over");

                    return new ValidationReport("Salinity", new[] {issue});
                }

                // Both models have salinity enabled, but destination model does not have F3 or F4 coverages - data will be lost!
                if (sourceModel.DispersionFormulationType != DispersionFormulationType.Constant &&
                    destinationModel.DispersionFormulationType == DispersionFormulationType.Constant)
                {
                    var issue = new ValidationIssue(sourceModel.DispersionFormulationType, ValidationSeverity.Warning,
                        string.Format("The source model has a Dispersion Formulation Type {0}, " +
                                      "whereas the destination model has Dispersion Formulation Type {1}. " +
                                      "F3 and F4 spatial data will be lost in the merge",
                                      sourceModel.DispersionFormulationType, 
                                      destinationModel.DispersionFormulationType));

                    return new ValidationReport("Salinity", new[] {issue});
                }
            }

            return new ValidationReport("Salinity", Enumerable.Empty<ValidationIssue>());
        }

        public static ValidationReport ValidateTemperature(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            if (sourceModel.UseTemperature)
            {
                // Destination model does not have temperature enabled, so copy the settings from source model
                if (!destinationModel.UseTemperature)
                {
                    var issue = new ValidationIssue(sourceModel, ValidationSeverity.Warning,
                        "The source model has temperature enabled, but the destination model does not. " +
                        "Temperature will be enabled in the destination model and Temperature-related Model Properties copied over");

                    return new ValidationReport("Temperature", new[] { issue });
                }

                // We deliberately avoid merge of time series.
                if (sourceModel.TemperatureModelType == TemperatureModelType.Composite)
                {
                    var issue = new ValidationIssue(sourceModel.TemperatureModelType, ValidationSeverity.Warning,
                        string.Format("The source model has a Temperature Model Type {0}, " +
                                      "its Meteo data will be lost in the merge",
                                      sourceModel.TemperatureModelType));

                    return new ValidationReport("Temperature", new[] { issue });
                }
            }

            return new ValidationReport("Salinity", Enumerable.Empty<ValidationIssue>());
        }

        private static ValidationReport ValidateUnmergable(WaterFlowModel1D sourceModel)
        {
            var issue1 = new ValidationIssue(sourceModel, ValidationSeverity.Warning,
                                            string.Format("All source model ({0}) settings like timeframes, parameters, etc will be lost in the merged model", sourceModel.Name));
            
            var issue2 = new ValidationIssue(sourceModel, ValidationSeverity.Warning,
                                            string.Format("All source model ({0}) restart states will be lost in the merged model", sourceModel.Name));

            if (sourceModel.OutputIsEmpty) return new ValidationReport("Default warnings", new[] {issue1, issue2});

            var issue3 = new ValidationIssue(sourceModel, ValidationSeverity.Warning,
                string.Format("All source model ({0}) output files will be lost in the merged model", sourceModel.Name));

            return new ValidationReport("Default warnings", new[] { issue1, issue2, issue3 });
        }

        public static IEnumerable<ValidationIssue> ValidateCoordinateSystem(WaterFlowModel1D target)
        {
            var coordinateSystem = target.Network.CoordinateSystem;
            ValidationIssue issue = null;

            if (coordinateSystem == null)
                issue = new ValidationIssue(target.Network, ValidationSeverity.Error,
                    string.Format("Cannot perform merge if no coordinate system is set. Please set coordinate system for model : {0}",target.Name), target.Network);
            
            else if (coordinateSystem.IsGeographic)
                issue = new ValidationIssue(coordinateSystem, ValidationSeverity.Error,
                    string.Format(
                        "Cannot perform merge in geographical coordinate system {0}",
                        coordinateSystem.Name), coordinateSystem);

            return issue != null ? new List<ValidationIssue>() {issue} : Enumerable.Empty<ValidationIssue>();
        }

        public static ValidationReport ValidateHydroObjectsWillBeRenamed(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            var hydroObjectInDestinationModel = destinationModel.Network.AllHydroObjects.OrderBy(ho => ho.GetEntityType().Name).ToList();
            var destinationNetworkElements = hydroObjectInDestinationModel.Select(hodest => hodest.Name).ToList();
            if (hydroObjectInDestinationModel.Count == 0) return new ValidationReport("Rename needed", Enumerable.Empty<ValidationIssue>());
            var valReports = new List<ValidationReport>();
            var valIssues = new List<ValidationIssue>();
            var currentType = string.Empty;
            foreach (var hydroObject in hydroObjectInDestinationModel)
            {
                if (currentType != hydroObject.GetEntityType().Name)
                {
                    if (!string.IsNullOrEmpty(currentType) && valIssues.Count > 0)
                    {
                        var subReport = new ValidationReport(currentType, valIssues);
                        valReports.Add(subReport);
                    }
                    currentType = hydroObject.GetEntityType().Name;
                    valIssues = new List<ValidationIssue>();
                }
                else
                {
                    continue;
                }
                
                var sourceNetworkElements = sourceModel.Network.AllHydroObjects.Where(ho => ho.GetType() == hydroObject.GetType() && destinationNetworkElements.Contains(ho.Name)).ToList();
                foreach (var sourceNetworkElement in sourceNetworkElements)
                {
                    var formattedNewName = WaterFlowModel1DModelMergeHelper.GenerateNewFormattedName(destinationNetworkElements, sourceNetworkElements.Select(n => n.Name).ToList(), sourceModel.Name, 0, sourceNetworkElement.Name);
                    var issue = new ValidationIssue(sourceNetworkElement, ValidationSeverity.Info, string.Format(@"From model : {0}; element : {1} ; with name: {2} will be renamed after the merge into model : {3} to  : {4}", sourceModel.Name, sourceNetworkElement.GetEntityType().Name, sourceNetworkElement.Name, destinationModel.Name, formattedNewName));
                    valIssues.Add(issue);
                }    
            }

            if (valIssues.Any()) {
                var subReport = new ValidationReport(currentType, valIssues);
                valReports.Add(subReport);
            }
            if (valReports.Count == 0) return new ValidationReport("Rename needed", Enumerable.Empty<ValidationIssue>());
            return new ValidationReport("Rename needed", valReports);
        }
        
        public static ValidationReport ValidateBoundaryConditionClearCheck(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            var connectedNodes = WaterFlowModel1DModelMergeHelper.ConnectedNodesList(destinationModel, sourceModel);
            if (!connectedNodes.Any()) 
            {
                // do we need this error??
                var noConnectedNodesIssue = new ValidationIssue(sourceModel, ValidationSeverity.Error,
                    string.Format("The source model : {0}, has no connected nodes. Can't check for boundary conditions.", sourceModel.Name));
                return new ValidationReport("Boundary Conditions", new[] { noConnectedNodesIssue });
            }
            
            var bcIssues = new List<ValidationIssue>();

            // first check if nodes with boundary conditions will be set to none in destination model:
            var destinationConnectedNodesBoundaryConditions = WaterFlowModel1DModelMergeHelper.connectedNodesBoundaryConditions(destinationModel, connectedNodes.Select(cn => cn.Key));

            foreach (var destinationConnectedNodesBoundaryCondition in destinationConnectedNodesBoundaryConditions)
            {
                // inform user that boundary condition in destination model will be changed
                var destinationConnectedNode = destinationConnectedNodesBoundaryCondition.Node;
                var connectedNode = connectedNodes.FirstOrDefault(cn => cn.Key == destinationConnectedNode);
                
                var issue = new ValidationIssue(destinationConnectedNode, ValidationSeverity.Warning,
                    string.Format( "The destination model : {0}, has {1} connected node{2} ({3}) with the sourcemodel : {4} with a boundary condition. " +
                                   "The boundary condition for node : {5} ({6}), in the destination model : {0}, will be set to {7} AFTER the merge.",
                        destinationModel.Name,
                        connectedNode.Value.Count > 1 ? string.Empty : "a",
                        connectedNode.Value.Count > 1 ? "s" : string.Empty,
                        string.Join(", ",
                            connectedNode.Value.Select(n =>
                                string.Format("{0}{1}",
                                    n.Name,
                                    sourceModel.BoundaryConditions.FirstOrDefault(bc => bc.Node == n) != null
                                        ? string.Format(" with bc : {0}",
                                            sourceModel.BoundaryConditions.First(bc => bc.Node == n).DataType.ToString())
                                        : string.Empty))),
                        sourceModel.Name,
                        destinationConnectedNode.Name,
                        destinationConnectedNodesBoundaryCondition.DataType,
                        WaterFlowModel1DBoundaryNodeDataType.None));
                bcIssues.Add(issue);
            }


            // now check if nodes with boundary conditions will be not be used anymore in the destination model:
            var sourceConnectedNodesBoundaryConditions = WaterFlowModel1DModelMergeHelper.connectedNodesBoundaryConditions(sourceModel, connectedNodes.SelectMany(cn => cn.Value).Distinct());

            foreach (var sourceConnectedNodesBoundaryCondition in sourceConnectedNodesBoundaryConditions)
            {
                // only inform user when bc connection in source model is not merged!
                var sourceConnectedNode = sourceConnectedNodesBoundaryCondition.Node;
                var connectedNode = connectedNodes.FirstOrDefault(cn => cn.Value.Contains(sourceConnectedNode));
                
                var issue = new ValidationIssue(sourceConnectedNode, ValidationSeverity.Info, 
                    string.Format("Node : {0}, in source model : {1}, will be merged with node : {2}, of the destination model : {3}. The boundary condition : {4} (of datatype: {5}), will NOT be merged into the destination model : {3}.", 
                        sourceConnectedNode.Name,
                        sourceModel.Name, 
                        connectedNode.Key.Name,
                        destinationModel.Name,
                        sourceConnectedNodesBoundaryCondition.Name,
                        sourceConnectedNodesBoundaryCondition.DataType));

                bcIssues.Add(issue);
            }
            
            return bcIssues.Count == 0 ? new ValidationReport("Boundary Conditions", Enumerable.Empty<ValidationIssue>()) : new ValidationReport("Boundary Conditions", bcIssues);
        }
    }
}