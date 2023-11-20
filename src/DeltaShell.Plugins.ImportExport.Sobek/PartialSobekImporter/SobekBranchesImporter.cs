using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class  SobekBranchesImporter : PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekBranchesImporter));

        public override string DisplayName
        {
            get { return "Branches and nodes"; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            var networkNetterPath = GetFilePath(SobekFileNames.SobekNetworkNetterFileName);
            var nodesPath = GetFilePath(SobekFileNames.SobekNodeFileName);
            var networkPath = GetFilePath(SobekFileNames.SobekNetworkFileName);
            var networkBranchGeometryPath = GetFilePath(SobekFileNames.SobekNetworkBrancheGeometryFileName);

            IDictionary<string,int> nodeTypes = new Dictionary<string, int>();
            IDictionary<string, int> branchTypes = new Dictionary<string, int>();

            if (File.Exists(networkNetterPath))
            {
                nodeTypes = SobekNetworkNetterReader.ReadNodeTypes(networkNetterPath);
                branchTypes = SobekNetworkNetterReader.ReadBranchTypes(networkNetterPath);
            }
            else
            {
                Log.WarnFormat("Couldn't find file {0} for detailed branches and nodes information.", networkNetterPath);
            }

            Dictionary<string, Retention> nodeData = new Dictionary<string, Retention>();
            var nodeDataReader = new SobekRetentionsReader() {Sobek2Import = true};
            var retentions = nodeDataReader.Read(nodesPath).Cast<Retention>();
            string doubleRetentions = "";
            foreach (var retention in retentions)
            {
                if (nodeData.Keys.Contains(retention.Name))
                {
                    doubleRetentions += retention.Name + " ";
                }
                else
                {
                    nodeData.Add(retention.Name, retention);
                }
            }

            if (doubleRetentions.Length > 0)
            {
                string warning =
                    "The following retentions were multiple defined. Imported the first definition skipped the other(s):\n" +
                    doubleRetentions;
                Log.Warn(warning);
            }

            var createdNodes = new SobekNetworkNodeFileReader().Read(networkPath)
                .Concat(new SobekNetworkLinkageNodeFileReader().Read(networkPath).Cast<SobekNode>())
                .ToDictionaryWithErrorDetails(networkPath, n => n.ID, n => CreateHydroNodeOrManhole(n, nodeTypes, nodeData, HydroNetwork));

            var createdBranches = new SobekNetworkBranchFileReader().Read(networkPath)
                                .ToDictionaryWithErrorDetails(networkPath, b => b.TextID, b => CreateBranchOrPipe(b, createdNodes, branchTypes, HydroNetwork));

            ReadAndUpdateBranchGeometry(networkBranchGeometryPath, createdBranches, SobekType);

            var nodesLookup = HydroNetwork.Nodes.ToDictionary(n => n.Name);
            var branchesLookUp = HydroNetwork.Branches.ToDictionary(b => b.Name);

            TryToReconstructManholesWithCompartmentsForExternalSobekNodes(createdNodes, createdBranches, nodeTypes);
            TryToReconstructUrbanOutlets(createdNodes,createdBranches, nodeTypes);

            var fmModel = TryGetModel<WaterFlowFMModel>();

            fmModel.DoWithPropertySet(nameof(WaterFlowFMModel.DisableNetworkSynchronization), true, () =>
            {
                AddToNetwork(createdNodes, nodesLookup, HydroNetwork);
                AddToNetwork(createdBranches, branchesLookUp, nodesLookup, HydroNetwork);
                
                fmModel?.AddMissingNodeData(createdNodes.Values);
                fmModel?.AddMissingBranchData(createdBranches.Values);
            });

            ReadAndUpdateBranchOrderNumber(nodesPath, HydroNetwork, branchesLookUp);
        }

        private void TryToReconstructManholesWithCompartmentsForExternalSobekNodes(Dictionary<string, INode> createdNodes, Dictionary<string, IBranch> createdBranches, IDictionary<string, int> nodeTypes)
        {
            var sobekPreFix = "tmp";

            var externalStructureNames = nodeTypes
                .Where(nt => SobekNetworkNetterReader.IsExternalStructureNode(nt.Value))
                .Select(nt => nt.Key);

            foreach (var nodeName in externalStructureNames)
            {
                if (createdNodes.ContainsKey(nodeName) && 
                    createdNodes.ContainsKey(sobekPreFix + nodeName) &&
                    createdBranches.ContainsKey(sobekPreFix + nodeName))
                {
                    var manhole = createdNodes[nodeName] as Manhole;
                    if (manhole != null)
                    {
                        var compartmentExternalStructure = new OutletCompartment(sobekPreFix + nodeName);
                        var compartment = manhole.Compartments.FirstOrDefault(c => c.Name == nodeName);
                        if (compartment != null)
                        {
                            compartmentExternalStructure = new OutletCompartment(compartment);
                            compartmentExternalStructure.Name = sobekPreFix + nodeName;
                        }

                        //pipes are connected to the manholes, manhole geometry is the average of the compartments -> in this importer the pipes already have geometry, so do not change the geometry of the manhole
                        var x = manhole.XCoordinate;
                        var y = manhole.YCoordinate;
                        manhole.Compartments.Add(compartmentExternalStructure);
                        compartmentExternalStructure.Geometry = new Point(x,y);
                        manhole.Geometry = new Point(x, y);

                        var internalSewerConnection = new SewerConnection(sobekPreFix + nodeName)
                        {
                            SourceCompartment = compartment,
                            SourceCompartmentName = nodeName,
                            Source = manhole,
                            TargetCompartment = compartmentExternalStructure,
                            TargetCompartmentName = sobekPreFix + nodeName,
                            Target = manhole,
                        };

                        //update dictionaries
                        createdBranches[sobekPreFix + nodeName] = internalSewerConnection;
                        createdNodes.Remove(sobekPreFix + nodeName);

                    }
                }
            }
        }

        private void TryToReconstructUrbanOutlets(Dictionary<string, INode> createdNodes, Dictionary<string, IBranch> createdBranches, IDictionary<string, int> nodeTypes)
        {
            var outletCandidates = nodeTypes
                .Where(nt => SobekNetworkNetterReader.IsConnectionNode(nt.Value))
                .Select(nt => nt.Key);

            foreach (var nodeName in outletCandidates)
            {
                if (createdNodes.ContainsKey(nodeName))
                {
                    var createdNode = createdNodes[nodeName];
                    if (!(createdNode is Manhole))
                    {
                        var IsLinkedToSewerConnection = createdBranches.Any(vp => vp.Value is ISewerConnection && (vp.Value.Source?.Name == nodeName || vp.Value.Target?.Name == nodeName));
                        if (IsLinkedToSewerConnection)
                        {

                            createdNodes[nodeName] = CreateManholeWithOutlet(createdNode);
                            createdBranches.Where(vp => vp.Value is ISewerConnection && vp.Value.Source?.Name == nodeName).ForEach(vp => vp.Value.Source = createdNodes[nodeName]);
                            createdBranches.Where(vp => vp.Value is ISewerConnection && vp.Value.Target?.Name == nodeName).ForEach(vp => vp.Value.Target = createdNodes[nodeName]);
                        }

                    }
                }
            }
        }

        private static void AddToNetwork(IDictionary<string, IBranch> createdChannels, IDictionary<string, IBranch> branchesLookUp, IDictionary<string, INode> nodesLookup, INetwork hydroNetwork)
        {
            foreach (var channel in createdChannels.Values)
            {
                if(branchesLookUp.TryGetValue(channel.Name, out var existingBranch))
                {
                    existingBranch.Geometry = channel.Geometry;
                    existingBranch.Length = channel.Length;
                    existingBranch.IsLengthCustom = channel.IsLengthCustom;
                    existingBranch.Source = nodesLookup[channel.Source.Name];
                    existingBranch.Target = nodesLookup[channel.Target.Name];
                }
                else
                {
                    branchesLookUp[channel.Name] = channel;
                    hydroNetwork.Branches.Add(channel);
                }
            }
        }

        private static void AddToNetwork(IDictionary<string, INode> createdNodes, IDictionary<string, INode> nodesLookup, INetwork hydroNetwork)
        {
            foreach (var node in createdNodes.Values)
            {
                if (nodesLookup.TryGetValue(node.Name, out var existingNode))
                {
                    existingNode.Geometry = node.Geometry;
                }
                else
                {
                    nodesLookup[node.Name] = node;
                    hydroNetwork.Nodes.Add(node);
                }
            }
        }

        /// <summary>
        /// Updates branches that where read from network.tp with geometry as stored in network.cp
        /// </summary>
        private static void ReadAndUpdateBranchGeometry(string filePath, IDictionary<string, IBranch> createdChannels, SobekType sobekType)
        {
            ThrowWhenFileNotExist(filePath);

            //update geometry of the branch
            var branchGeometryLookUp = new SobekCurvePointReader().Read(filePath).ToDictionaryWithErrorDetails(filePath,g => g.BranchID);

            foreach (var branchId in createdChannels.Keys)
            {
                var branch = createdChannels[branchId] as IChannel;

                if (branch == null) continue;//pipe has a geometry based on source and target node, so skip

                var branchGeometry = branchGeometryLookUp.ContainsKey(branchId)
                                         ? branchGeometryLookUp[branchId]
                                         : null;

                branch.Geometry = GeometryHelper.CalculateGeometry(sobekType == SobekType.Sobek212, branchGeometry, branch.Source, branch.Target);

                if (!branch.IsLengthCustom) continue;

                var difference = Math.Abs(branch.Length - branch.Geometry.Length);
                if (difference < epsilon) continue;

                var percentage = branch.Geometry.Length > 0 ? 100 * (difference / branch.Geometry.Length) : 100.0;
                if (percentage < 0.01) continue;

                Log.WarnFormat(
                    "Branch {0} - {1} has been imported with a difference of {2} between length {3} and geometry length {4} ({5} %)",
                    branch.Name, branch.LongName, difference.ToString("G4"), branch.Length.ToString("G4"),
                    branch.Geometry.Length.ToString("G4"), percentage.ToString("N2"));
            }
        }
    
        private static IBranch CreateBranchOrPipe(SobekBranch sobekBranch, Dictionary<string, INode> createdNodes,
            IDictionary<string, int> branchTypes, INetwork hydroNetwork)
        {
            var fromNode = createdNodes[sobekBranch.StartNodeID];
            var toNode = createdNodes[sobekBranch.EndNodeID];

            var fromManhole = fromNode as Manhole;
            var toManhole = toNode as Manhole;

            if (branchTypes.ContainsKey(sobekBranch.TextID))
            {
                var branchType = branchTypes[sobekBranch.TextID];
                if (SobekNetworkNetterReader.IsPreasurePipe(branchType)) //is a preasure pipe / sewer connection 
                {
                    var sewerConnection = new SewerConnection(sobekBranch.TextID)
                    {
                        LongName = sobekBranch.Name,
                        Network = hydroNetwork,
                        IsLengthCustom = true,
                        Length = sobekBranch.Length,
                        Geometry = new LineString(
                            new[] {fromNode.Geometry.Coordinate, toNode.Geometry.Coordinate}),
                        Source = fromNode,
                        Target = toNode,
                        SourceCompartment = fromManhole?.Compartments?.FirstOrDefault(),
                        TargetCompartment = toManhole?.Compartments?.FirstOrDefault(),
                    };
                    return sewerConnection;
                }

                if (SobekNetworkNetterReader.IsPipe(branchType)) //is a pipe 
                {
                    var pipe = new Pipe
                    {
                        Name = sobekBranch.TextID,
                        LongName = sobekBranch.Name,
                        Network = hydroNetwork,
                        IsLengthCustom = true,
                        Length = sobekBranch.Length,
                        Geometry = new LineString(new[] {fromNode.Geometry.Coordinate, toNode.Geometry.Coordinate}),
                        Material = SewerProfileMapping.SewerProfileMaterial.Unknown,
                        Source = fromNode,
                        Target = toNode,
                        SourceCompartment = fromManhole?.Compartments?.FirstOrDefault(),
                        TargetCompartment = toManhole?.Compartments?.FirstOrDefault()
                    };

                    if (SobekNetworkNetterReader.IsDryWeatherPipe(branchType))
                    {
                        pipe.WaterType = SewerConnectionWaterType.DryWater;
                    }
                    else if (SobekNetworkNetterReader.IsStormWeatherPipe(branchType))
                    {
                        pipe.WaterType = SewerConnectionWaterType.StormWater;
                    }
                    else
                    {
                        pipe.WaterType = SewerConnectionWaterType.Combined;
                    }

                    return pipe;
                }
            }

            return new Channel(fromNode, toNode, sobekBranch.Length)
            {
                Name = sobekBranch.TextID,
                LongName = sobekBranch.Name,
                Network = hydroNetwork,
                IsLengthCustom = true
            };
        }

        private static INode CreateHydroNodeOrManhole(SobekNode sobekNode, IDictionary<string, int> nodeTypes,
            Dictionary<string, Retention> nodeData,
            INetwork hydroNetwork)
        {

            //manholes??
            if (nodeTypes.ContainsKey(sobekNode.ID) && 
                SobekNetworkNetterReader.IsManhole(nodeTypes[sobekNode.ID]))
            {
                var manhole = CreateManholeWithOneCompartment(sobekNode);

                if (nodeData.ContainsKey(sobekNode.ID))
                {
                    UpdateManholeWithNodeData(manhole, nodeData[sobekNode.ID]);
                }

                return manhole;
            }

            return new HydroNode
            {
                Name = sobekNode.ID,
                LongName = sobekNode.Name,
                Geometry = new Point(sobekNode.X, sobekNode.Y),
                Network = hydroNetwork
            };
        }

        private static IManhole CreateManholeWithOneCompartment(SobekNode sobekNode)
        {
            var compartment = new Compartment
            {
                Name = sobekNode.ID,
                ManholeWidth = 1,
                ManholeLength = 1,
                FloodableArea = 1,
                Geometry = new Point(sobekNode.X, sobekNode.Y)
            };
            var manhole = new Manhole(sobekNode.ID);
            manhole.Geometry = new Point(sobekNode.X, sobekNode.Y);
            manhole.Compartments.Add(compartment);
            compartment.ParentManhole = manhole;
            compartment.ParentManholeName = sobekNode.ID;

            return manhole;
        }

        private static IManhole CreateManholeWithOutlet(INode node)
        {
            var compartment = new OutletCompartment(node.Name)
            {
                Geometry = node.Geometry,
                ManholeWidth = 1,
                ManholeLength = 1,
                FloodableArea = 1
            };
            var manhole = new Manhole(node.Name);
            manhole.Geometry = node.Geometry;
            manhole.Compartments.Add(compartment);
            compartment.ParentManhole = manhole;
            compartment.ParentManholeName = node.Name;

            return manhole;
        }

        private static void UpdateManholeWithNodeData(IManhole manhole, Retention retention)
        {
            var dimension = 0.0;
            if (retention.StorageArea > 0.0) dimension = Math.Pow(retention.StorageArea, 0.5);

            var compartment = manhole.Compartments.FirstOrDefault(c => c.Name == manhole.Name);
            if (compartment != null)
            {
                compartment.Name = retention.Name;
                compartment.ManholeLength = dimension;
                compartment.ManholeWidth = dimension;
                compartment.Shape = CompartmentShape.Round;
                compartment.BottomLevel = retention.BedLevel;
                compartment.FloodableArea = retention.StreetStorageArea;
                compartment.SurfaceLevel = retention.StreetLevel;
                if (retention.UseTable)
                {
                    compartment.UseTable = true;
                    compartment.Storage = retention.Data;               
                }
            }
        }

        /// <summary>
        /// Updates ordernumber branches based on nodes.dat Sobek 212.4 - 213 files
        /// </summary>
        private static void ReadAndUpdateBranchOrderNumber(string filePath, INetwork hydroNetwork, IDictionary<string, IBranch> branchesLookup)
        {
            if (!File.Exists(filePath))
            {
                Log.ErrorFormat("Couldn't find file {0}.", filePath);
                return;
            }

            var nodesWithInterpolation = new SobekNodeFileReader().Read(filePath)
                                                        .Where(n => n.InterpolationOverNode)
                                                        .ToDictionaryWithErrorDetails(filePath, n => n.ID);

            var orderNumbers = hydroNetwork.Branches.Select(b => b.OrderNumber).Where(n => n > 0).ToList();
            var highestOrderNumber = orderNumbers.Any() ? orderNumbers.Max() : 0;

            var branchesPast = new HashSet<string>();

            foreach (var nodeId in nodesWithInterpolation.Keys)
            {
                var chainOfBranches = GetChainOfBranches(nodeId, branchesPast, nodesWithInterpolation, branchesLookup).ToList();
                if (chainOfBranches.Count <= 1) continue;

                highestOrderNumber++;
                foreach (var branch in chainOfBranches)
                {
                    branch.OrderNumber = highestOrderNumber;
                }
            }
        }

        private static IEnumerable<IBranch> GetChainOfBranches(string nodeId, HashSet<string> branchesPast, Dictionary<string, SobekNode> dicNodes, IDictionary<string, IBranch> dicBranches)
        {
            var node = dicNodes[nodeId];

            if (!branchesPast.Contains(node.InterpolationFrom) && dicBranches.ContainsKey(node.InterpolationFrom))
            {
                branchesPast.Add(node.InterpolationFrom);
                yield return dicBranches[node.InterpolationFrom];
                var previousNode = dicNodes.Values.FirstOrDefault(n => n.InterpolationTo == node.InterpolationFrom);
                if (previousNode != null)
                {
                    foreach (var branch in GetChainOfBranches(previousNode.ID, branchesPast, dicNodes, dicBranches))
                    {
                        yield return branch;
                    }
                }
            }

            if (!branchesPast.Contains(node.InterpolationTo) && dicBranches.ContainsKey(node.InterpolationTo))
            {
                branchesPast.Add(node.InterpolationTo);
                yield return dicBranches[node.InterpolationTo];

                var nextNode = dicNodes.Values.FirstOrDefault(n => n.InterpolationFrom == node.InterpolationTo);
                if (nextNode != null)
                {
                    foreach (var branch in GetChainOfBranches(nextNode.ID, branchesPast, dicNodes, dicBranches))
                    {
                        yield return branch;
                    }
                }
            }
        }
    }
}
