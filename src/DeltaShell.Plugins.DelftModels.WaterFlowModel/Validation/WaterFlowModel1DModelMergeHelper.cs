using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public static class WaterFlowModel1DModelMergeHelper
    {
        public const int metersInRange = 10;

        public static IEnumerable<KeyValuePair<INode, IList<INode>>> ConnectedNodesList(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            // As per requirement!
            if(destinationModel.Network.CoordinateSystem == null || sourceModel.Network.CoordinateSystem == null )
                return Enumerable.Empty <KeyValuePair<INode, IList<INode>>>() ; 

            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();

            ICoordinateTransformation transformSourceToDest = null;
            if (sourceModel.Network.CoordinateSystem != null && destinationModel.Network.CoordinateSystem != null)
                transformSourceToDest = Map.CoordinateSystemFactory.CreateTransformation(
                    sourceModel.Network.CoordinateSystem,
                    destinationModel.Network.CoordinateSystem);

            var destinationNetworkNodes = destinationModel.Network.Nodes;
            var sourceNetworkNodes = sourceModel.Network.Nodes;

            var coordinateSystem = destinationModel.Network.CoordinateSystem;
            var connectedNodesList = destinationNetworkNodes
                .Select(n =>
                {
                    var nodes = sourceNetworkNodes.Where(sn =>
                    {
                        if (coordinateSystem != null)
                            return new GeodeticDistance(coordinateSystem).Distance(n.Geometry.Coordinate,
                                ConvertToDestinationCoordinateSystem(sn.Geometry.Coordinate, transformSourceToDest)) <=
                                   metersInRange;
                        
                        var envelope = new Envelope(n.Geometry.Coordinate);
                        envelope.ExpandBy(metersInRange);
                        return envelope.Intersects(sn.Geometry.Coordinate);
                    }).Select(sn => new KeyValuePair<INode,double>(sn, new GeodeticDistance(coordinateSystem).Distance(n.Geometry.Coordinate, ConvertToDestinationCoordinateSystem(sn.Geometry.Coordinate, transformSourceToDest)))).ToList();
                    return new KeyValuePair<INode, IList<KeyValuePair<INode, double>>>(n, nodes);
                    
                })
                .Where(kvp => kvp.Value.Count != 0).SelectMany(cn => cn.Value, (p, sourceModelNode) => new { sourceNode = sourceModelNode.Key, distance = sourceModelNode.Value, destinationNode = p.Key }).ToList();
            
            // check if nodes from source model are coupled more than once
            var t = connectedNodesList.OrderBy(cn => cn.sourceNode).ThenBy(cn=> cn.distance).GroupBy(cn => cn.sourceNode).Select(g => g.First()).ToList();
            var t1 =
                t.Select(
                    cn =>
                    {
                        var value = t.Where(sn => sn.destinationNode == cn.destinationNode).Select(sn => sn.sourceNode).ToList();
                        return new KeyValuePair<INode, IList<INode>>(cn.destinationNode,value);
                    }).GroupBy(cn => cn.Key).Select(g => g.First()).ToList();
           
            return t1;
        }
     
        private static Coordinate ConvertToDestinationCoordinateSystem(Coordinate coordinate, ICoordinateTransformation transformSourceToDest)
        {
            if (transformSourceToDest == null) return coordinate;
            var sourceCoordinateInDestinationCoordinateSystem = transformSourceToDest.MathTransform.Transform(new[] { coordinate.X, coordinate.Y });
            return new Coordinate(sourceCoordinateInDestinationCoordinateSystem[0], sourceCoordinateInDestinationCoordinateSystem[1]);
        }

        public static void RenameAllNetworkElements(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            var hydroObjectInDestinationModel = destinationModel.Network.AllHydroObjects.OrderBy(ho => ho.GetEntityType().Name).ToList();
            if (hydroObjectInDestinationModel.Count == 0) return;
            foreach (var hydroObject in hydroObjectInDestinationModel)
            {
                RenameNetworkElement(hydroObject.GetEntityType(), destinationModel, sourceModel);
            }
        }

        public static void RenameNetworkElement(Type type, WaterFlowModel1D destinationModel1D, WaterFlowModel1D sourceModel1D)
        {
            var destinationNetworkElements = destinationModel1D.Network.AllHydroObjects.Where(ho => ho.GetType() == type).Select(ho => ho.Name).ToList(); 
            var sourceNetworkElements = sourceModel1D.Network.AllHydroObjects.Where(ho => ho.GetType() == type && destinationNetworkElements.Contains(ho.Name)).ToList();
            
            foreach (var sourceNetworkElement in sourceNetworkElements)
            {
                sourceNetworkElement.Name = GenerateNewFormattedName(destinationNetworkElements, sourceNetworkElements.Select(n => n.Name).ToList(), sourceModel1D.Name, 0, sourceNetworkElement.Name);
            }
        }

        public static string GenerateNewFormattedName(ICollection<string> destinationNetworkElements, IList<string> sourceNetworkElementsNames, string modelName, int index, string name)
        {
            if (name.StartsWith(modelName))
            {
                var prefixName = name.Split('_').FirstOrDefault();
                if (prefixName != null)
                {
                    name = name.Remove(0, prefixName.Length+1);
                    Regex re = new Regex(@"\d+");
                    Match m = re.Match(prefixName.Remove(0,modelName.Length));
                    if (m.Success)
                    {
                        if (Int32.TryParse(m.Value,out index))
                            index++;
                    }
                }
            }

            var newName = String.Format(@"{0}{1}_{2}", modelName, index, name);
            return destinationNetworkElements.Contains(newName) || sourceNetworkElementsNames.Contains(newName) ? GenerateNewFormattedName(destinationNetworkElements, sourceNetworkElementsNames, modelName, index + 1, newName) : newName;
        }

        public static IEnumerable<WaterFlowModel1DBoundaryNodeData> connectedNodesBoundaryConditions(WaterFlowModel1D model, IEnumerable<INode> connectedNodes)
        {
            return connectedNodes
                .Select(connectedNode => model.BoundaryConditions.FirstOrDefault(nodeBoundaryCondition => nodeBoundaryCondition.Node == connectedNode))
                .Where(nodeBoundaryCondition => nodeBoundaryCondition != null && nodeBoundaryCondition.DataType != WaterFlowModel1DBoundaryNodeDataType.None);
        }

        public static IList<ValidationIssue> FitSourceChannelsOnDestinationNode(INode destinationNode, INode sourceNode, WaterFlowModel1D sourceModel1D, Func<INode, INode, IChannel,ValidationIssue> HandleFitting = null)
        {
            const double checkPrecision = 0.0001;
            if (HandleFitting == null)
                HandleFitting = UpdateChannelToNewGeometry;

            if (Math.Abs(destinationNode.Geometry.Coordinate.X - sourceNode.Geometry.Coordinate.X) < checkPrecision
                && Math.Abs(destinationNode.Geometry.Coordinate.Y - sourceNode.Geometry.Coordinate.Y) < checkPrecision)
                return null;

            var channels = sourceModel1D.Network.Channels.Where(c => c.Source == sourceNode || c.Target == sourceNode).GroupBy(c => c).Select(g => g.First()).ToList();
            
            var validationIssues = channels.Select(channel => HandleFitting(destinationNode, sourceNode, channel)).Where(issue => issue != null).ToList();
            return validationIssues.Count == 0 ? null : validationIssues;
        }

        private static ValidationIssue UpdateChannelToNewGeometry(INode destinationNode, INode sourceNode, IChannel channel)
        {
            channel.IsLengthCustom = true;
            var changeCoordinate = new Coordinate(destinationNode.Geometry.Coordinate.X, destinationNode.Geometry.Coordinate.Y);
            var channelGeometryCoordinates = channel.Source == sourceNode
                ? new List<Coordinate> { changeCoordinate }
                    .Union(channel.Geometry.Coordinates.ToList()
                        .GetRange(1, channel.Geometry.Coordinates.Length - 1)).ToArray()
                : channel.Geometry.Coordinates.ToList()
                    .GetRange(0, channel.Geometry.Coordinates.Length - 1).ToList()
                    .Union(new List<Coordinate> { changeCoordinate }).ToArray();
            
            ChannelFromGisImporter.UpdateGeometry(channel, new LineString(channelGeometryCoordinates));
            return null;
        }

        public static void Merge(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            var connectedNodes = ConnectedNodesList(destinationModel, sourceModel);
            var connectedSourceNodes = connectedNodes.SelectMany(cn => cn.Value).ToList();
            

            //add all nodes which are not connected to the destination model
            foreach (var node in sourceModel.Network.Nodes.Where(node => !connectedSourceNodes.Contains(node)))
            {
                destinationModel.Network.Nodes.Add(node);

                // Remove default boundary conditions
                var existingBoundaryCondition = destinationModel.BoundaryConditions.FirstOrDefault(bc => bc.Node == node);
                if (existingBoundaryCondition != null) destinationModel.BoundaryConditions.Remove(existingBoundaryCondition);

                // Add source model boundary conditions
                var sourceBoundaryCondition = sourceModel.BoundaryConditions.FirstOrDefault(bc => bc.Node == node && bc.DataType != WaterFlowModel1DBoundaryNodeDataType.None);
                if (sourceBoundaryCondition != null)
                    destinationModel.BoundaryConditions.Add(sourceBoundaryCondition);
            }

            //add all channels which are not connected to the destination model
            foreach (var channel in sourceModel.Network.Channels.Where(channel => !connectedSourceNodes.Contains(channel.Source) && !connectedSourceNodes.Contains(channel.Target)))
            {
                destinationModel.Network.Branches.Add(channel);
            }


            var connectedBranches = new List<IChannel>();
            //add all channels which are connected to the destination model on sourcenode
            foreach (var channel in sourceModel.Network.Channels.Where(channel => connectedSourceNodes.Contains(channel.Source)))
            {
                var branch = channel;
                foreach (var connectedNode in connectedNodes.Where(connectedNode => connectedNode.Value.Contains(branch.Source)))
                {
                    FitSourceChannelsOnDestinationNode(connectedNode.Key, channel.Source, sourceModel);
                    channel.Source = connectedNode.Key;
                    connectedBranches.Add(channel);
                }
            }

            //add all channels which are connected to the destination model on targetnode
            foreach (var channel in sourceModel.Network.Channels.Where(channel => connectedSourceNodes.Contains(channel.Target)))
            {
                var branch = channel;
                foreach (var connectedNode in connectedNodes.Where(connectedNode => connectedNode.Value.Contains(branch.Target)))
                {
                    FitSourceChannelsOnDestinationNode(connectedNode.Key, channel.Target, sourceModel);
                    channel.Target = connectedNode.Key;
                    if (!connectedBranches.Contains(channel))
                        connectedBranches.Add(channel);
                }
            }
            foreach (var connectedBranch in connectedBranches)
            {
                destinationModel.Network.Branches.Add(connectedBranch);
            }

            // Add source model SharedCrossSectionDefinitions
            foreach (var sharedCrossSectionDefinition in sourceModel.Network.SharedCrossSectionDefinitions)
            {
                sharedCrossSectionDefinition.Name = String.Format(CultureInfo.InvariantCulture, "{0}_{1}", sourceModel.Name, sharedCrossSectionDefinition.Name);
                destinationModel.Network.SharedCrossSectionDefinitions.Add(sharedCrossSectionDefinition);
            }

            // rename computational Grid Point Names when needed
            foreach (var computationalGridPoint in sourceModel.NetworkDiscretization.Locations.Values.Where(computationalGridPoint => !computationalGridPoint.Name.StartsWith(computationalGridPoint.Branch.Name)))
            {
                computationalGridPoint.Name = String.Format(CultureInfo.InvariantCulture, "{0}_{1:0.000}", computationalGridPoint.Branch.Name, computationalGridPoint.Chainage);
            }
            // Add source model computational grid points
            destinationModel.NetworkDiscretization.Locations.AddValues(sourceModel.NetworkDiscretization.Locations.Values);

            // Add source model lateral source datas
            foreach (var sourceLateralSourceData in sourceModel.LateralSourceData)
            {
                var existingLateralSourceData = destinationModel.LateralSourceData.FirstOrDefault(lsd => lsd.Feature == sourceLateralSourceData.Feature);
                if (existingLateralSourceData != null) destinationModel.LateralSourceData.Remove(existingLateralSourceData);
                destinationModel.LateralSourceData.Add(sourceLateralSourceData);
            }

            // Add source model crossSectionTypes (used by roughness & cross section section)
            var existingCrossSectionTypeNames = destinationModel.Network.CrossSectionSectionTypes.Select(cst => cst.Name);
            var additionalCrossSectionTypes = sourceModel.Network.CrossSectionSectionTypes.Where(cst => !existingCrossSectionTypeNames.Contains(cst.Name));
            destinationModel.Network.CrossSectionSectionTypes.AddRange(additionalCrossSectionTypes);
            
            // relink CrossSectionDefinition and SharedCrossSectionDefinition Sections
            var sourceModelCrossSectionNames = sourceModel.Network.CrossSections.Select(cs => cs.Name).ToList();
            var crossSectionDefinitionSectionsToRelink = destinationModel.Network.CrossSections.Where(cs => sourceModelCrossSectionNames.Contains(cs.Name)).Select(cs => cs.Definition);

            var sourceModelSharedCrossSectionDefinitionNames = sourceModel.Network.SharedCrossSectionDefinitions.Select(scsd => scsd.Name).ToList();
            var sharedCrossSectionDefinitionSectionsToRelink = destinationModel.Network.SharedCrossSectionDefinitions.Where(scsd => sourceModelSharedCrossSectionDefinitionNames.Contains(scsd.Name));

            var totalCrossSectionsDefinitionSectionsToRelink = crossSectionDefinitionSectionsToRelink.Union(sharedCrossSectionDefinitionSectionsToRelink);

            foreach (var crossSectionDefinition in totalCrossSectionsDefinitionSectionsToRelink)
            {
                foreach (var section in crossSectionDefinition.Sections)
                {
                    var sectionType = destinationModel.Network.CrossSectionSectionTypes.FirstOrDefault(st => st.Name == section.SectionType.Name);
                    if (sectionType != null) // should never be null since we just added the missing CrossSectionDefinition Sections!
                        section.SectionType = sectionType;
                }
            }
            
            // Add source model roughness data
            foreach (var roughnessSection in sourceModel.RoughnessSections)
            {
                var existingRoughnessSection = destinationModel.RoughnessSections.FirstOrDefault(rs => rs.Name == roughnessSection.Name);
                if (existingRoughnessSection == null) destinationModel.RoughnessSections.Add(roughnessSection);
                else
                {
                    CopyCoverageData(existingRoughnessSection.RoughnessNetworkCoverage, roughnessSection.RoughnessNetworkCoverage);
                    existingRoughnessSection.CopyFunctionOfFromRoughnessSection(roughnessSection, sourceModel.Network);
                }
            }

            // Add source model initial conditions data
            if(sourceModel.InitialConditionsType == destinationModel.InitialConditionsType)
                CopyCoverageData(destinationModel.InitialConditions, sourceModel.InitialConditions);

            // Add source model initial waterflow data
            CopyCoverageData(destinationModel.InitialFlow, sourceModel.InitialFlow);

            // Add source model windshielding data
            CopyCoverageData(destinationModel.WindShielding, sourceModel.WindShielding);

            if (sourceModel.UseSalt)
            {
                if (!destinationModel.UseSalt) // if destination does not use salt, copy over the properties too
                {
                    destinationModel.UseSalt = true;
                    destinationModel.DispersionFormulationType = sourceModel.DispersionFormulationType;
                    destinationModel.SalinityPath = sourceModel.SalinityPath;

                    destinationModel.InitialSaltConcentration.Name = sourceModel.InitialSaltConcentration.Name;
                    destinationModel.InitialSaltConcentration.DefaultValue = sourceModel.InitialSaltConcentration.DefaultValue;
                    destinationModel.InitialSaltConcentration.Locations.InterpolationType = sourceModel.InitialSaltConcentration.Locations.InterpolationType;

                    destinationModel.DispersionCoverage.Name = sourceModel.DispersionCoverage.Name;
                    destinationModel.DispersionCoverage.DefaultValue = sourceModel.DispersionCoverage.DefaultValue;
                    destinationModel.DispersionCoverage.Locations.InterpolationType = sourceModel.DispersionCoverage.Locations.InterpolationType;
                }

                // Add source model initial salt concentration data
                CopyCoverageData(destinationModel.InitialSaltConcentration, sourceModel.InitialSaltConcentration);

                // Add source model dispersion coverage data
                CopyCoverageData(destinationModel.DispersionCoverage, sourceModel.DispersionCoverage);

                // If both models have non-constant Dispersion Formulation Types, copy the F3 and F4 covearge datas too
                if (sourceModel.DispersionFormulationType != DispersionFormulationType.Constant
                    && destinationModel.DispersionFormulationType != DispersionFormulationType.Constant)
                {
                    CopyCoverageData(destinationModel.DispersionF3Coverage, sourceModel.DispersionF3Coverage);
                    CopyCoverageData(destinationModel.DispersionF4Coverage, sourceModel.DispersionF4Coverage);
                }
            }

            if (sourceModel.UseTemperature)
            {
                if (!destinationModel.UseTemperature) // if destination does not use temp, copy over the properties too
                {
                    destinationModel.UseTemperature = true;

                    destinationModel.InitialTemperature.Name = sourceModel.InitialTemperature.Name;
                    destinationModel.InitialTemperature.DefaultValue = sourceModel.InitialTemperature.DefaultValue;
                    destinationModel.InitialTemperature.Locations.InterpolationType = sourceModel.InitialTemperature.Locations.InterpolationType;

                    destinationModel.TemperatureModelType = sourceModel.TemperatureModelType;
                    destinationModel.BackgroundTemperature = sourceModel.BackgroundTemperature;

                    destinationModel.SurfaceArea = sourceModel.SurfaceArea;
                    destinationModel.AtmosphericPressure = sourceModel.AtmosphericPressure;
                    destinationModel.DaltonNumber = sourceModel.DaltonNumber;
                    destinationModel.StantonNumber = sourceModel.StantonNumber;
                    destinationModel.HeatCapacityWater = sourceModel.HeatCapacityWater;

                    destinationModel.DensityType = sourceModel.DensityType;
                    destinationModel.Latitude = sourceModel.Latitude;
                    destinationModel.Longitude = sourceModel.Longitude;
                }
                
                // Add source model initial temperature data
                CopyCoverageData(destinationModel.InitialTemperature, sourceModel.InitialTemperature);
            }
        }

        private static void CopyCoverageData(IFunction destinationCoverage, IFunction sourceCoverage)
        {
            if (sourceCoverage.Arguments.Count != 1 || destinationCoverage.Arguments.Count != 1 ||
                sourceCoverage.Components.Count != destinationCoverage.Components.Count) return;

            var isAutoSorted = destinationCoverage.Arguments[0].IsAutoSorted;

            // Disable auto sorting
            if (isAutoSorted) destinationCoverage.Arguments[0].IsAutoSorted = false;

            // Add locations - will add default values
            foreach (var value in sourceCoverage.Arguments[0].Values)
            {
                destinationCoverage.Arguments[0].Values.Add(value);
            }

            // Reset auto sorting to original value
            if(isAutoSorted) destinationCoverage.Arguments[0].IsAutoSorted = true;
            
            // Update values
            for (var componentIndex = 0; componentIndex < sourceCoverage.Components.Count; componentIndex++)
            {
                var sourceCoverageValues = sourceCoverage.Components[componentIndex].Values;
                var destinationCoverageValues = destinationCoverage.Components[componentIndex].Values;

                for (var valueIndex = 0; valueIndex < sourceCoverageValues.Count; valueIndex++)
                {
                    var numExistingValues = destinationCoverageValues.Count - sourceCoverageValues.Count;
                    destinationCoverageValues[numExistingValues + valueIndex] = sourceCoverageValues[valueIndex];
                }
            }
        }
        
        public static void ClearBoundaryConditionsOnDestinationModel(WaterFlowModel1D destinationModel, WaterFlowModel1D sourceModel)
        {
            var connectedNodes = ConnectedNodesList(destinationModel, sourceModel);
            var destinationConnectedNodesBoundaryConditions = connectedNodesBoundaryConditions(destinationModel, connectedNodes.Select(cn => cn.Key));

            foreach (var destinationConnectedNodesBoundaryCondition in destinationConnectedNodesBoundaryConditions)
            {
                destinationConnectedNodesBoundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.None;
            }

        }
    }
}